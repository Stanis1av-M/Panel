using Microsoft.EntityFrameworkCore;
using panel.Models;
using System;
using System.Linq;
using System.Windows;
using System.Text.RegularExpressions;

namespace Panel.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private bool IsInputMalicious(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            string upperInput = input.ToUpper();
            string[] badWords = {
                "DROP DATABASE", "DROP TABLE", "DELETE FROM",
                "TRUNCATE TABLE", "ALTER TABLE", "UNION SELECT", "--"
            };

            foreach (var word in badWords)
            {
                if (upperInput.Contains(word)) return true;
            }
            return false;
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите Email и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsInputMalicious(email) || IsInputMalicious(password))
            {
                MessageBox.Show("Нельзя так делать ;)", "Атата!", MessageBoxButton.OK, MessageBoxImage.Stop);
                txtEmail.Clear();
                txtPassword.Clear();
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный формат Email (например, admin@world.com).", "Ошибка формата", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var user = db.Users
                                 .Include(u => u.Role)
                                 .FirstOrDefault(u => u.Email == email && u.Password == password);

                    if (user != null)
                    {
                        if (user.Ban)
                        {
                            MessageBox.Show("Этот аккаунт заблокирован. Обратитесь к администратору.", "Доступ запрещён",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            UserSession.CurrentUser = user;

                            MainWindow main = new MainWindow(user);
                            main.Show();
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Неверный Email или пароль.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ПЕРЕХОД НА ОКНО РЕГИСТРАЦИИ
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close(); // Закрываем окно входа
        }
    }
}