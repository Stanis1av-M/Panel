using System;
using System.Linq;
using System.Windows;
// ВАЖНО: Эти два using исправляют ошибки Include и DataGrid
using Microsoft.EntityFrameworkCore;
using panel.Models;

namespace Panel.Views
{
    // Проверь, чтобы здесь было : Window
    public partial class PriceHistoryWindow : Window
    {
        public PriceHistoryWindow()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    // Теперь .Include() будет работать благодаря Microsoft.EntityFrameworkCore
                    var history = db.PriceHistories
                                    .Include(ph => ph.Product)
                                    .OrderByDescending(ph => ph.ChangeAt)
                                    .ToList();

                    GridHistory.ItemsSource = history;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}");
            }
        }
    }
}