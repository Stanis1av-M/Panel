using Microsoft.EntityFrameworkCore;
using panel.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Panel.Views.Pages
{
    public partial class ProfilePage : Page
    {
        private AppDbContext _db = new AppDbContext();
        private User _user;

        public ProfilePage()
        {
            InitializeComponent();

            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show("Ошибка сессии. Пожалуйста, перезайдите.");
                return;
            }

            LoadUserData();
        }

        private void LoadUserData()
        {
            // Загружаем данные пользователя
            _user = _db.Users.Include(u => u.Role).FirstOrDefault(u => u.UserId == UserSession.CurrentUser.UserId);

            if (_user != null)
            {
                txtFullName.Text = _user.FullName;
                txtEmail.Text = _user.Email;
                txtPhone.Text = _user.Phone;
                txtUserRole.Text = _user.Role?.Name.ToUpper() ?? "ПОЛЬЗОВАТЕЛЬ";
            }
        }

        // 1. ИСПРАВЛЕНО: Валидация телефона (разрешаем цифры, скобки, тире, плюс, пробел)
        private void PhoneTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9\\+\\-\\(\\)\\s]");
            e.Handled = regex.IsMatch(e.Text);
        }

        // 2. ИСПРАВЛЕНО: Валидация вставки (Ctrl+V) для телефона
        private void PhoneTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (new Regex("[^0-9\\+\\-\\(\\)\\s]").IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
        }

        // 3. ДОБАВЛЕНО: Жесткое ограничение длины пароля (30 символов)
        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = sender as PasswordBox;
            if (pb != null && pb.Password.Length > 30)
            {
                // Как только длина превышает 30, мы просто "отрезаем" лишнее
                pb.Password = pb.Password.Substring(0, 30);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("ФИО не может быть пустым!", "Внимание");
                return;
            }

            try
            {
                _user.FullName = txtFullName.Text;
                _user.Phone = txtPhone.Text;

                if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    _user.Password = txtPassword.Password;
                }

                _db.SaveChanges();

                // Синхронизация с MainWindow
                UserSession.CurrentUser = _user;
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.UpdateHeaderInfo();
                }

                MessageBox.Show("Профиль успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }
    }
}