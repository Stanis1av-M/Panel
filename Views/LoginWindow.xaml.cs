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

        // 1. МЕТОД: Проверка на опасные SQL-команды (Пасхалка/Защита)
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

        // 2. МЕТОД: Проверка правильности формата почты
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

            // ВАЛИДАЦИЯ 
            
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
                        if (user.Role.Name == "Администратор" || user.Role.Name == "Менеджер")
                        {
                          
                            UserSession.CurrentUser = user;

                            MainWindow main = new MainWindow(user);
                            main.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Доступ запрещен...", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}