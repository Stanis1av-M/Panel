using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using panel.Models;

namespace Panel.Views.Pages
{
    public partial class SuppliesPage : Page
    {
        private User _currentUser;

        // Теперь конструктор принимает пользователя, чтобы знать, кто создает поставки
        public SuppliesPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadSupplies();
        }

        private void LoadSupplies()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    // Загружаем поставки вместе с данными о поставщиках
                    GridSupplies.ItemsSource = db.Supplies
                        .Include(s => s.Supplier)
                        .OrderByDescending(s => s.CreatedAt)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void BtnAddSupply_Click(object sender, RoutedEventArgs e)
        {
            // Открываем созданное нами ранее окно и передаем туда ID текущего пользователя
            SupplyEditWindow win = new SupplyEditWindow(_currentUser.UserId);
            win.Owner = Window.GetWindow(this);

            if (win.ShowDialog() == true)
            {
                LoadSupplies(); // Обновляем список, если поставка создана
            }
        }

        private void BtnCompleteSupply_Click(object sender, RoutedEventArgs e)
        {
            if (GridSupplies.SelectedItem is Supply selectedSupply)
            {
                if (selectedSupply.Status == "Завершено")
                {
                    MessageBox.Show("Эта поставка уже была завершена.", "Информация");
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите завершить поставку №{selectedSupply.SupplyId}?\n" +
                    "Товары из этой поставки будут добавлены на остатки склада.",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CompleteSupplyTransaction(selectedSupply.SupplyId);
                }
            }
            else
            {
                MessageBox.Show("Выберите поставку из списка!", "Внимание");
            }
        }

        private void CompleteSupplyTransaction(int supplyId)
        {
            using (var db = new AppDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var supply = db.Supplies
                            .Include(s => s.SupplyItems)
                            .FirstOrDefault(s => s.SupplyId == supplyId);

                        if (supply == null) return;

                        // Прибавляем количество каждого товара на склад
                        foreach (var item in supply.SupplyItems)
                        {
                            var product = db.Products.Find(item.ProductId);
                            if (product != null)
                            {
                                product.Stock += item.Quantity;
                            }
                        }

                        supply.Status = "Завершено";
                        supply.CompleteAt = DateTime.Now; // Указываем время завершения из твоей БД

                        db.SaveChanges();
                        transaction.Commit();

                        MessageBox.Show("Склад успешно обновлен!", "Успех");
                        LoadSupplies();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка транзакции: {ex.Message}");
                    }
                }
            }
        }

        private void BtnPriceHistory_Click(object sender, RoutedEventArgs e)
        {
            PriceHistoryWindow historyWin = new PriceHistoryWindow();
            historyWin.Owner = Window.GetWindow(this);
            historyWin.ShowDialog();
        }
    }
}