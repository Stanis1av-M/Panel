using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using panel.Models;

namespace Panel.Views.Pages
{
    public partial class OrdersPage : Page
    {
        private bool _isInitialized = false;

        public OrdersPage()
        {
            InitializeComponent();
            LoadInitialData();
            _isInitialized = true;
            RefreshOrders();
        }

        private void LoadInitialData()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    if (cmbStatus == null) return;

                    var statuses = db.OrderStatuses.Select(s => s.Name).ToList();
                    statuses.Insert(0, "Все статусы");

                    cmbStatus.ItemsSource = statuses;
                    cmbStatus.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- 1. ВАЛИДАЦИЯ (Защита от SQL-инъекций) ---
        private bool IsInputMalicious(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string upperInput = input.ToUpper();
            // Список опасных команд
            string[] badWords = { "DROP", "DELETE", "TRUNCATE", "UPDATE", "INSERT", "ALTER", "--", "SELECT" };
            return badWords.Any(word => upperInput.Contains(word));
        }

        private void RefreshOrders()
        {
            if (!_isInitialized || GridOrders == null || cmbStatus == null || cmbSort == null)
                return;

            string rawSearch = txtSearch.Text ?? "";

            // --- 2. ПРОВЕРКА ВАЛИДНОСТИ ПЕРЕД ЗАПРОСОМ ---
            if (IsInputMalicious(rawSearch))
            {
                MessageBox.Show("Использование системных команд в поиске запрещено!", "Система безопасности", MessageBoxButton.OK, MessageBoxImage.Stop);

                _isInitialized = false;
                txtSearch.Text = ""; // Очищаем поле
                _isInitialized = true;

                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var query = db.Orders
                                  .Include(o => o.User)
                                  .Include(o => o.OrderStatus)
                                  .AsQueryable();

                    // --- 3. РАСШИРЕННЫЙ ПОИСК (Имя, Почта, ID) ---
                    string search = rawSearch.ToLower().Trim();
                    if (!string.IsNullOrEmpty(search))
                    {
                        query = query.Where(o =>
                            (o.User != null && (
                                o.User.FullName.ToLower().Contains(search) ||
                                o.User.Email.ToLower().Contains(search) // Поиск по почте
                            )) ||
                            o.OrderId.ToString().Contains(search) // Поиск по номеру заказа
                        );
                    }

                    // Фильтрация
                    if (cmbStatus.SelectedItem != null && cmbStatus.SelectedItem.ToString() != "Все статусы")
                    {
                        string selectedStatus = cmbStatus.SelectedItem.ToString();
                        query = query.Where(o => o.OrderStatus != null && o.OrderStatus.Name == selectedStatus);
                    }

                    // Сортировка
                    switch (cmbSort.SelectedIndex)
                    {
                        case 0: query = query.OrderByDescending(o => o.CreatedAt); break;
                        case 1: query = query.OrderBy(o => o.CreatedAt); break;
                        case 2: query = query.OrderBy(o => o.TotalAmount); break;
                        case 3: query = query.OrderByDescending(o => o.TotalAmount); break;
                    }

                    GridOrders.ItemsSource = query.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления списка: {ex.Message}");
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            RefreshOrders();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _isInitialized = false;
            txtSearch.Text = "";
            cmbStatus.SelectedIndex = 0;
            cmbSort.SelectedIndex = 0;
            _isInitialized = true;
            RefreshOrders();
        }

        private void BtnUpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (GridOrders.SelectedItem is Order selectedOrder)
            {
                OrderStatusWindow statusWindow = new OrderStatusWindow(selectedOrder);
                statusWindow.Owner = Window.GetWindow(this);

                if (statusWindow.ShowDialog() == true)
                {
                    RefreshOrders();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заказ из списка.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}