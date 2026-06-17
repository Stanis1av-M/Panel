using panel.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Panel.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        // 1. Безопасность: Проверка на SQL-инъекции
        private bool IsInputMalicious(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string upperInput = input.ToUpper();
            string[] badWords = { "DROP DATABASE", "DROP TABLE", "DELETE FROM", "TRUNCATE TABLE", "UNION SELECT", "--" };
            return badWords.Any(word => upperInput.Contains(word));
        }

        // 2. Валидация Email на корректность формата
        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        // 3. Валидация ФИО (только буквы, пробелы и дефисы, минимум 2 слова)
        private bool IsValidFullName(string name)
        {
            if (!Regex.IsMatch(name, @"^[a-zA-Zа-яА-ЯёЁ\s\-]+$")) return false;

            string[] words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length >= 2;
        }

        // 4. Валидация формата телефона
        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return true; // Телефон необязателен
            return Regex.IsMatch(phone, @"^\+?[0-9\s\-()]{10,20}$");
        }

        private void BtnSubmitRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Password;

            // 1. Проверка заполнения обязательных полей (на пустоту)
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Поля ФИО, Email и Пароль обязательны для заполнения.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. ЗАЩИТА ОТ ПЕРЕПОЛНЕНИЯ (Валидация максимальной длины - защита от вставки "Войны и мира")
            if (fullName.Length > 100)
            {
                MessageBox.Show("ФИО слишком длинное (максимум 100 символов).", "Ошибка длины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (email.Length > 80)
            {
                MessageBox.Show("Email слишком длинный (максимум 80 символов).", "Ошибка длины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (phone.Length > 20)
            {
                MessageBox.Show("Номер телефона слишком длинный (максимум 20 символов).", "Ошибка длины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length > 30)
            {
                MessageBox.Show("Пароль слишком длинный (максимум 30 символов).", "Ошибка длины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Проверка безопасности (SQL-защита)
            if (IsInputMalicious(fullName) || IsInputMalicious(email) || IsInputMalicious(phone) || IsInputMalicious(password))
            {
                MessageBox.Show("Обнаружены недопустимые системные спецсимволы.", "Ошибка безопасности", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            // 4. Валидация ФИО на формат букв
            if (!IsValidFullName(fullName))
            {
                MessageBox.Show("Пожалуйста, введите корректные ФИО.\nПример: Иванов Иван (минимум 2 слова, без цифр и знаков).", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 5. Валидация формата Email
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный адрес электронной почты (например, client@mail.ru).", "Ошибка формата", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 6. Валидация телефона (если он указан)
            if (!string.IsNullOrWhiteSpace(phone) && !IsValidPhone(phone))
            {
                MessageBox.Show("Введите корректный номер телефона.\nМинимум 10 цифр (разрешены цифры, пробелы, дефисы, скобки и знак +).", "Ошибка формата", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    // Проверяем, существует ли уже такая почта
                    var exists = db.Users.Any(u => u.Email == email);
                    if (exists)
                    {
                        MessageBox.Show("Пользователь с таким Email уже зарегистрирован в системе.", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Создаем сущность нового пользователя
                    User newUser = new User
                    {
                        RoleId = 4, // Роль: Клиент
                        FullName = fullName,
                        Email = email,
                        Phone = string.IsNullOrWhiteSpace(phone) ? string.Empty : phone,
                        Password = password,
                        RegistrationDate = DateTime.Now,
                        Ban = false
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация успешно завершена! Войдите под своими учетными данными.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Переходим на окно входа
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения в базу данных: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}