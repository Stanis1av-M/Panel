using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using panel.Models;

namespace Panel.Views.Pages
{
    public partial class AddUserPage : Page
    {
        public AddUserPage()
        {
            InitializeComponent();
            LoadRoles();
        }

        // Загружаем список ролей из БД в выпадающий список
        private void LoadRoles()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    cmbRole.ItemsSource = db.Roles.ToList();
                    cmbRole.DisplayMemberPath = "Name";       // То, что видит пользователь
                    cmbRole.SelectedValuePath = "RoleId";     // То, что мы сохраним в базу
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }
        }

        // --- ВАЛИДАЦИЯ ИЗ ПРОФИЛЯ ---
        private void PhoneTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9\\+\\-\\(\\)\\s]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void PhoneTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (new Regex("[^0-9\\+\\-\\(\\)\\s]").IsMatch(text)) e.CancelCommand();
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = sender as PasswordBox;
            if (pb != null && pb.Password.Length > 30)
            {
                pb.Password = pb.Password.Substring(0, 30);
            }
        }

      
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка на пустоту обязательных полей
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password) ||
                cmbRole.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля (отмечены *).", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string email = txtEmail.Text.Trim();

            // 2. Проверка формата Email (как в окне логина)
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный формат Email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    
                    bool userExists = db.Users.Any(u => u.Email == email);
                    if (userExists)
                    {
                        MessageBox.Show("Пользователь с таким Email уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 4. Создаем нового пользователя
                    var newUser = new User
                    {
                        FullName = txtFullName.Text.Trim(),
                        Email = email,
                        Phone = txtPhone.Text.Trim(),
                        Password = txtPassword.Password, 
                        RoleId = (int)cmbRole.SelectedValue
                    };

                    // 5. Сохраняем в базу
                    db.Users.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show("Сотрудник успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    
                    txtFullName.Clear();
                    txtEmail.Clear();
                    txtPhone.Clear();
                    txtPassword.Clear();
                    cmbRole.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении в базу данных: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}