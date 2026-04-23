using System;
using System.Windows;
using panel.Models;

namespace Panel.Views
{
    public partial class MainWindow : Window
    {
        private User _currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            txtUserName.Text = _currentUser.FullName;
            txtUserRole.Text = _currentUser.Role.Name;

            if (_currentUser.Role.Name != "Администратор")
            {
                btnAdminPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ProductsPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        // Исправленный метод навигации
        private void btnOrders_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                // Попробуем перейти на страницу
                MainFrame.Navigate(new Panel.Views.Pages.OrdersPage());
            }
            catch (Exception ex)
            {
                // Если OrdersPage "падает" при создании, мы увидим почему:
                MessageBox.Show($"Не удалось загрузить страницу заказов: {ex.Message}",
                                "Ошибка навигации", MessageBoxButton.OK, MessageBoxImage.Error);

                if (ex.InnerException != null)
                {
                    MessageBox.Show($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
        }

        private void btnSupplies_Click(object sender, RoutedEventArgs e)
        {
            // Передаем текущего пользователя в страницу
            MainFrame.Navigate(new Panel.Views.Pages.SuppliesPage(_currentUser));
        }

        private void BtnInventory_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Panel.Views.Pages.InventoryPage());
        }
    }
}