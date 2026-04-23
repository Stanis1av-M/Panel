using System;
using System.Text.RegularExpressions; // Для проверки цифр
using System.Windows;
using System.Windows.Input; // Для обработки ввода
using panel.Models;

namespace Panel.Views
{
    public partial class AddReferenceWindow : Window
    {
        private string _tableName;
        private AppDbContext _db = new AppDbContext();

        public AddReferenceWindow(string tableName)
        {
            InitializeComponent();
            _tableName = tableName;
            SetupUI();
        }

        private void SetupUI()
        {
            txtTitle.Text = $"НОВАЯ ЗАПИСЬ: {_tableName.ToUpper()}";

            // Показываем поля в зависимости от выбранного справочника
            if (_tableName == "Manufacturers") panelCountry.Visibility = Visibility.Visible;
            if (_tableName == "Suppliers") panelSupplier.Visibility = Visibility.Visible;
            if (_tableName == "Delivery") panelPrice.Visibility = Visibility.Visible;

            // Включаем панель с галочкой для таблиц, где есть активность
            if (_tableName == "Delivery" || _tableName == "Payment")
            {
                panelActive.Visibility = Visibility.Visible;
            }
        }

        // --- ВАЛИДАЦИЯ ЧИСЛОВОГО ВВОДА ---

        // Разрешаем только цифры (запрет букв при нажатии клавиш)
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]"); // Ищем всё, что НЕ цифра
            e.Handled = regex.IsMatch(e.Text); // Если нашли не цифру — блокируем ввод
        }

        // Запрет вставки текста с буквами (Ctrl+V)
        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (new Regex("[^0-9]").IsMatch(text))
                {
                    e.CancelCommand(); // Отменяем вставку, если в тексте буквы
                }
            }
            else { e.CancelCommand(); }
        }

        // --- СОХРАНЕНИЕ ---

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация: Наименование (обязательно для всех)
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Наименование не может быть пустым!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Валидация: Ограничение длины текста (миллион символов) для всех полей
            // Это дополнительная проверка перед отправкой в БД
            if (txtName.Text.Length > 1000000 || txtCountry.Text.Length > 1000000 ||
                txtContacts.Text.Length > 1000000 || txtAddress.Text.Length > 1000000)
            {
                MessageBox.Show("Превышено ограничение по количеству символов (макс. 1 000 000)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                switch (_tableName)
                {
                    case "Categories":
                        _db.Categories.Add(new Category { Name = txtName.Text });
                        break;

                    case "Manufacturers":
                        _db.Manufacturers.Add(new Manufacturer { Name = txtName.Text, Country = txtCountry.Text });
                        break;

                    case "Suppliers":
                        _db.Suppliers.Add(new Supplier
                        {
                            Name = txtName.Text,
                            ContactInfo = txtContacts.Text,
                            Address = txtAddress.Text
                        });
                        break;

                    case "Statuses":
                        _db.OrderStatuses.Add(new OrderStatus { Name = txtName.Text });
                        break;

                    case "Delivery":
                        if (!decimal.TryParse(txtPrice.Text, out decimal p))
                            throw new Exception("Введите корректную стоимость доставки (только числа)");

                        // ИСПРАВЛЕНО: Ограничение именно ЧИСЛА (не более 1 000 000)
                        if (p > 1000000)
                        {
                            MessageBox.Show("Стоимость доставки не может превышать 1 000 000 рублей!", "Превышение лимита", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        _db.DeliveryMethods.Add(new DeliveryMethod
                        {
                            Name = txtName.Text,
                            Price = p,
                            IsActive = chkIsActive.IsChecked ?? false
                        });
                        break;

                    case "Payment":
                        _db.PaymentMethods.Add(new PaymentMethod
                        {
                            Name = txtName.Text,
                            IsActive = chkIsActive.IsChecked ?? false
                        });
                        break;
                }

                _db.SaveChanges();
                this.DialogResult = true; // Закрываем и обновляем таблицу
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}