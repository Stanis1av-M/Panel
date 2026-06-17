using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore; // Для работы с базой данных
using panel.Models; // Подключаем ваши модели таблиц (AppDbContext, Product, Tour)

namespace Panel.Views
{
    public class ChatMessage
    {
        public bool IsUser { get; set; }
        public string Text { get; set; } = "";

        public string SenderLabel => IsUser ? "Вы" : "Нейросеть";

        public HorizontalAlignment BubbleAlignment => IsUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;

        public Brush BubbleBackground => IsUser
            ? new SolidColorBrush(Color.FromRgb(0xDC, 0xF0, 0xFF))
            : new SolidColorBrush(Color.FromRgb(0xF5, 0xF6, 0xFA));

        public Brush SenderColor => IsUser
            ? new SolidColorBrush(Color.FromRgb(0x00, 0xA8, 0xFF))
            : new SolidColorBrush(Color.FromRgb(0xA4, 0xB0, 0xBE));
    }

    public partial class AiChatWindow : Window
    {
        // История чата
        private readonly List<ChatMessage> _messages = new List<ChatMessage>();

        // ВНИМАНИЕ: ЗАМЕНИТЕ НА КЛЮЧ GOOGLE GEMINI (начинается на AIzaSy...)
        // Текущий ключ "AQ..." от Яндекса здесь работать НЕ БУДЕТ!
        private readonly string _apiKey = "ВСТАВЬТЕ_СЮДА_КЛЮЧ_ОТ_GOOGLE";

        private bool _isSending = false;

        public AiChatWindow()
        {
            InitializeComponent();

            // Приветственное сообщение от бота
            AddMessage(false, "Здравствуйте! Я ИИ-консультант магазина Apex. Я знаю всё о наших товарах и турах. Чем могу помочь?");
        }

        // ------------------------------------------------------------------
        // Интерфейс чата
        // ------------------------------------------------------------------

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                _ = SendMessageAsync();
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            _ = SendMessageAsync();
        }

        private async Task SendMessageAsync()
        {
            if (_isSending) return;

            string text = txtMessage.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(text)) return;

            if (text.Length > 4000)
            {
                MessageBox.Show("Сообщение слишком длинное.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddMessage(isUser: true, text: text);
            txtMessage.Clear();

            _isSending = true;
            btnSend.IsEnabled = false;

            try
            {
                // 1. Получаем данные из базы данных в формате JSON
                string dbContextJson = await GetStoreDataJsonAsync();

                // 2. Отправляем вопрос и данные в Google Gemini
                string reply = await SendToAiAsync(text, _apiKey, dbContextJson);

                AddMessage(isUser: false, text: reply);
            }
            catch (Exception ex)
            {
                AddMessage(isUser: false, text: $"Ошибка: {ex.Message}");
            }
            finally
            {
                _isSending = false;
                btnSend.IsEnabled = true;
            }
        }

        private void AddMessage(bool isUser, string text)
        {
            _messages.Add(new ChatMessage { IsUser = isUser, Text = text });
            ItemsChat.ItemsSource = null;
            ItemsChat.ItemsSource = _messages;
            ScrollChat.ScrollToEnd();
        }

        // ------------------------------------------------------------------
        // Работа с базой данных (выгрузка контекста)
        // ------------------------------------------------------------------

        /// <summary>
        /// Выгружает актуальные туры и товары из БД и конвертирует их в JSON.
        /// </summary>
        private async Task<string> GetStoreDataJsonAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    // Берем только видимые товары, которые есть в наличии
                    var products = await db.Products
                        .Where(p => p.IsVisible && p.Stock > 0)
                        .Select(p => new {
                            Категория = p.Category.Name,
                            Название = p.Name,
                            Цена = p.Price,
                            Остаток = p.Stock
                        })
                        .ToListAsync();

                    // Берем все туры
                    var tours = await db.Tours
                        .Select(t => new {
                            Название = t.Name,
                            Регион = t.Region,
                            Дней = t.DurationDays,
                            Описание = t.Description
                        })
                        .ToListAsync();

                    // Объединяем в один объект
                    var storeData = new
                    {
                        Товары_В_Наличии = products,
                        Доступные_Туры = tours
                    };

                    // Конвертируем в JSON с поддержкой русского языка
                    var options = new JsonSerializerOptions
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    return JsonSerializer.Serialize(storeData, options);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка БД: {ex.Message}");
                return "{ \"Ошибка\": \"Не удалось загрузить базу данных\" }";
            }
        }

        // ------------------------------------------------------------------
        // Отправка в Google Gemini
        // ------------------------------------------------------------------

        /// <summary>
        /// Запрос к Google Gemini с передачей JSON данных вашей базы.
        /// </summary>
        private async Task<string> SendToAiAsync(string userMessage, string apiKey, string dbJsonContext)
        {
            using var http = new HttpClient();
            string requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            // Настраиваем системную инструкцию: говорим ИИ, кто он такой и даем ему данные БД
            string systemPrompt =
                "Ты — вежливый менеджер-консультант магазина снаряжения и туров 'Apex'. " +
                "Отвечай на вопросы клиента ТОЛЬКО опираясь на следующие данные из нашей базы данных (в формате JSON). " +
                "Если клиент спрашивает о товаре или туре, которого нет в JSON, честно скажи, что у нас этого нет. " +
                "Не придумывай цены и товары из головы. Отвечай кратко и по делу.\n\n" +
                $"ДАННЫЕ БАЗЫ: {dbJsonContext}";

            // Формируем тело запроса для Google API
            var requestBody = new
            {
                // Системный промпт (правила для ИИ и JSON база)
                system_instruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                // Сообщение пользователя
                contents = new[]
                {
                    new {
                        role = "user",
                        parts = new[] { new { text = userMessage } }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await http.PostAsync(requestUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"({response.StatusCode}): Убедитесь, что вы используете ключ!\n{jsonResponse}";
                }

                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var candidates = doc.RootElement.GetProperty("candidates");

                if (candidates.GetArrayLength() > 0)
                {
                    string resultText = candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? "";

                    return resultText.Trim();
                }

                return "Нейросеть вернула пустой ответ.";
            }
            catch (Exception ex)
            {
                return $"Ошибка сети: {ex.Message}. Напоминаю: сервисы Google API заблокированы в РФ, требуется VPN.";
            }
        }
    }
}