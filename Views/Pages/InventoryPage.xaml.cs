using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using panel.Models;

namespace Panel.Views.Pages
{
    public partial class InventoryPage : Page
    {
        public InventoryPage()
        {
            InitializeComponent();

            // Жестко ограничиваем длину ввода в самом TextBox через код
            if (txtSearch != null) txtSearch.MaxLength = 50;

            RefreshInventory();
        }

        // --- ВАЛИДАЦИЯ: Защита от SQL-инъекций и вредоносного кода ---
        private bool IsInputMalicious(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            string upperInput = input.ToUpper();
            // Список ключевых слов, которые часто используются для атак
            string[] sqlKeywords = { "DROP", "DELETE", "TRUNCATE", "UPDATE", "INSERT", "ALTER", "--", "SELECT", "UNION", "EXEC" };

            return sqlKeywords.Any(word => upperInput.Contains(word));
        }

        private void RefreshInventory()
        {
            // ЗАЩИТА: Если элементы еще не инициализированы — выходим
            if (txtSearch == null || chkOnlyLowStock == null || GridInventory == null) return;

            string rawSearch = txtSearch.Text ?? "";

            // 1. ПРОВЕРКА ДЛИНЫ (не более 50 символов)
            if (rawSearch.Length > 50)
            {
                rawSearch = rawSearch.Substring(0, 50);
            }

            // 2. ПРОВЕРКА НА ВРЕДОНОСНОСТЬ
            if (IsInputMalicious(rawSearch))
            {
                // Если нашли подозрительные слова, очищаем поиск и выходим
                txtSearch.Text = "";
                return;
            }

            using (var db = new AppDbContext())
            {
                var query = db.Products.Include(p => p.Category).AsQueryable();

                // 3. ПОИСК (уже валидированный)
                string search = rawSearch.ToLower().Trim();
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Name.ToLower().Contains(search) ||
                                           p.Article.ToLower().Contains(search));
                }

                // 4. ФИЛЬТР: Только дефицит
                if (chkOnlyLowStock.IsChecked == true)
                {
                    query = query.Where(p => p.Stock < 5);
                }

                var list = query.ToList();

                // 5. ОБНОВЛЕНИЕ КАРТОЧЕК СТАТИСТИКИ
                // Считаем по всей базе, а не только по отфильтрованному списку
                txtTotalPositions.Text = db.Products.Count().ToString();
                txtTotalUnits.Text = db.Products.Sum(p => (int?)p.Stock ?? 0).ToString();
                txtLowStockCount.Text = db.Products.Count(p => p.Stock < 5).ToString();

                // 6. ПРИВЯЗКА ДАННЫХ
                GridInventory.ItemsSource = list.Select(p => new {
                    p.Article,
                    p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : "Нет категории",
                    p.Stock,
                    IsLowStock = p.Stock < 5
                }).ToList();
            }
        }

        private void FilterChanged(object sender, RoutedEventArgs e) => RefreshInventory();
        private void FilterChanged(object sender, TextChangedEventArgs e) => RefreshInventory();
    }
}