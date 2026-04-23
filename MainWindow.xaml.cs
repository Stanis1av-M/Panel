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
            // чтобы ProfilePage мог его прочитать
            _currentUser = user;
            UserSession.CurrentUser = user;

            // 2. ОТОБРАЖЕНИЕ
            UpdateHeaderInfo();

            // 3. ОГРАНИЧЕНИЕ ПРАВ
            if (_currentUser.Role?.Name != "Администратор")
            {
                btnAdminPanel.Visibility = Visibility.Collapsed;
                btnAddUser.Visibility = Visibility.Collapsed; 
            }
        }

       
        public void UpdateHeaderInfo()
        {
            txtUserName.Text = _currentUser.FullName;
            txtUserRole.Text = _currentUser.Role?.Name ?? "Пользователь";
        }


        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainFrame.Navigate(new Pages.ProfilePage());
            }
            catch (Exception ex)
            {
                
                MessageBox.Show($"Ошибка перехода в профиль: {ex.Message}\n\n{ex.InnerException?.Message}",
                                "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ProductsPage());
        }

        private void btnSupplies_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Panel.Views.Pages.SuppliesPage(_currentUser));
        }

        private void BtnInventory_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Panel.Views.Pages.InventoryPage());
        }

        private void btnAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Panel.Views.Pages.DictionariesPage());
        }

        private void btnOrders_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                MainFrame.Navigate(new Panel.Views.Pages.OrdersPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Очищаем сессию при выходе
            UserSession.CurrentUser = null;

            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Panel.Views.Pages.AddUserPage());
        }
    }
}