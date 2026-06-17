using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using panel.Models; // Твой namespace с моделями
using Panel.Views;  // Чтобы видеть AddReferenceWindow

namespace Panel.Views.Pages
{
    public partial class DictionariesPage : Page
    {
        // Контекст БД живет вместе со страницей
        private AppDbContext _db = new AppDbContext();
        private string _currentTable = "";

        public DictionariesPage()
        {
            InitializeComponent();
            this.Unloaded += (s, e) => _db.Dispose();
        }

        // 1. ИСПРАВЛЕНИЕ: Логика скрытия связей и настройки галочек
        private void GridData_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var type = e.PropertyType;

            // Скрываем все сложные объекты и коллекции (связи)
            if ((type.IsClass && type != typeof(string)) ||
                (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string)))
            {
                e.Cancel = true;
                return;
            }

            // Настройка чекбоксов (галочек)
            if (e.Column is DataGridCheckBoxColumn checkBoxColumn)
            {
                checkBoxColumn.IsReadOnly = false; // Разрешаем менять галочку
            }

            // Переименовываем заголовки для менеджера
            switch (e.PropertyName)
            {
                case "Name": e.Column.Header = "Наименование"; break;
                case "Country": e.Column.Header = "Страна"; break;
                case "Price": e.Column.Header = "Стоимость (руб.)"; break;
                case "IsActive": e.Column.Header = "Доступен"; break;
                case "ContactInfo": e.Column.Header = "Контакты"; break;
                case "Address": e.Column.Header = "Адрес"; break;
                default:
                    if (e.PropertyName.EndsWith("Id")) e.Column.Header = "ID";
                    break;
            }
        }

        // 2. ИСПРАВЛЕНИЕ: Вызов отдельного окна добавления
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentTable))
            {
                MessageBox.Show("Сначала выберите справочник в меню слева!");
                return;
            }

            // Открываем созданное ранее универсальное окно
            AddReferenceWindow addWin = new AddReferenceWindow(_currentTable);
            addWin.Owner = Window.GetWindow(this);

            if (addWin.ShowDialog() == true)
            {
                RefreshData(); // Обновляем таблицу после успешного добавления
            }
        }

        private void LstDictionaries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstDictionaries.SelectedItem is ListBoxItem selected)
            {
                _currentTable = selected.Tag.ToString();
                RefreshData();
            }
        }

        // 3. ИСПРАВЛЕНИЕ: Очистка контекста при обновлении
        private void RefreshData()
        {
            try
            {
                // Освобождаем предыдущий контекст перед пересозданием, чтобы не копить
                // открытые соединения с БД при каждом переключении справочника.
                _db.Dispose();
                _db = new AppDbContext();

                switch (_currentTable)
                {
                    case "Categories": GridData.ItemsSource = _db.Categories.ToList(); break;
                    case "Manufacturers": GridData.ItemsSource = _db.Manufacturers.ToList(); break;
                    case "Suppliers": GridData.ItemsSource = _db.Suppliers.ToList(); break;
                    case "Statuses": GridData.ItemsSource = _db.OrderStatuses.ToList(); break;
                    case "Delivery": GridData.ItemsSource = _db.DeliveryMethods.ToList(); break;
                    case "Payment": GridData.ItemsSource = _db.PaymentMethods.ToList(); break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочника: {ex.Message}");
            }
        }

        // Проверка перед сохранением: пустые названия и отрицательные цены не пропускаем в БД,
        // так как редактирование идёт прямо в ячейках DataGrid без отдельной формы.
        private bool ValidateBeforeSave(out string errorMessage)
        {
            errorMessage = "";

            switch (_currentTable)
            {
                case "Categories":
                    if (_db.Categories.Local.Any(c => string.IsNullOrWhiteSpace(c.Name)))
                    {
                        errorMessage = "Наименование категории не может быть пустым!";
                        return false;
                    }
                    break;

                case "Manufacturers":
                    if (_db.Manufacturers.Local.Any(m => string.IsNullOrWhiteSpace(m.Name)))
                    {
                        errorMessage = "Наименование производителя не может быть пустым!";
                        return false;
                    }
                    break;

                case "Suppliers":
                    if (_db.Suppliers.Local.Any(s => string.IsNullOrWhiteSpace(s.Name)))
                    {
                        errorMessage = "Наименование поставщика не может быть пустым!";
                        return false;
                    }
                    break;

                case "Statuses":
                    if (_db.OrderStatuses.Local.Any(s => string.IsNullOrWhiteSpace(s.Name)))
                    {
                        errorMessage = "Название статуса не может быть пустым!";
                        return false;
                    }
                    break;

                case "Delivery":
                    if (_db.DeliveryMethods.Local.Any(d => string.IsNullOrWhiteSpace(d.Name)))
                    {
                        errorMessage = "Название способа доставки не может быть пустым!";
                        return false;
                    }
                    if (_db.DeliveryMethods.Local.Any(d => d.Price < 0 || d.Price > 1000000))
                    {
                        errorMessage = "Стоимость доставки должна быть от 0 до 1 000 000 рублей!";
                        return false;
                    }
                    break;

                case "Payment":
                    if (_db.PaymentMethods.Local.Any(p => string.IsNullOrWhiteSpace(p.Name)))
                    {
                        errorMessage = "Название способа оплаты не может быть пустым!";
                        return false;
                    }
                    break;
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Обязательно подтверждаем правки в ячейках, иначе изменения могут не попасть в Local
            GridData.CommitEdit(DataGridEditingUnit.Row, true);

            if (!ValidateBeforeSave(out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _db.SaveChanges();
                MessageBox.Show("Изменения успешно сохранены!", "Успех");
                RefreshData();
            }
            catch (Exception ex)
            {
                // Глобальная обработка исключений по ТЗ
                MessageBox.Show($"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}",
                                "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                RefreshData();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = GridData.SelectedItem;
            if (selectedItem == null) return;

            if (MessageBox.Show("Удалить выбранную запись?", "Вопрос", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _db.Remove(selectedItem);
                    _db.SaveChanges();
                    RefreshData();
                }
                catch (Exception)
                {
                    // Контроль целостности по ТЗ
                    MessageBox.Show("Нельзя удалить запись, так как она связана с другими данными!", "Ошибка");
                    RefreshData();
                }
            }
        }
    }
}