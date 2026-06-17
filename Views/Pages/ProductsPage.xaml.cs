using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using panel.Models;

namespace Panel.Views.Pages
{
    public partial class ProductsPage : Page
    {
        public ProductsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new AppDbContext())
            {
                GridProducts.ItemsSource = db.Products.AsNoTracking().Include(p => p.Category).ToList();
            }
        }

        private bool IsInputMalicious(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string[] bad = { "DROP ", "DELETE ", "TRUNCATE ", "UNION ", "--" };
            return bad.Any(b => input.ToUpper().Contains(b));
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = txtSearch.Text.Trim().ToLower();

            if (IsInputMalicious(search))
            {
                txtSearch.Clear();
                LoadData();
                return;
            }

            using (var db = new AppDbContext())
            {
                GridProducts.ItemsSource = db.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.Name.ToLower().Contains(search) || p.Article.ToLower().Contains(search))
                    .ToList();
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (new ProductEditWindow(null).ShowDialog() == true) LoadData();
        }

        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (GridProducts.SelectedItem is Product selected)
            {
                if (new ProductEditWindow(selected).ShowDialog() == true) LoadData();
            }
            else MessageBox.Show("Выберите товар.");
        }

        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (GridProducts.SelectedItem is not Product selected)
            {
                MessageBox.Show("Выберите товар.");
                return;
            }

            if (!selected.IsVisible)
            {
                MessageBox.Show("Этот товар уже удалён.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Удалить товар \"{selected.Name}\" из магазина?\nТовар будет скрыт от клиентов, но останется в базе (для истории заказов).",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new AppDbContext())
                {
                    var product = db.Products.First(p => p.ProductId == selected.ProductId);
                    product.IsVisible = false;
                    db.SaveChanges();
                }
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось удалить товар: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRestoreProduct_Click(object sender, RoutedEventArgs e)
        {
            if (GridProducts.SelectedItem is not Product selected)
            {
                MessageBox.Show("Выберите товар.");
                return;
            }

            if (selected.IsVisible)
            {
                MessageBox.Show("Этот товар уже активен.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var product = db.Products.First(p => p.ProductId == selected.ProductId);
                    product.IsVisible = true;
                    db.SaveChanges();
                }
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось восстановить товар: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}