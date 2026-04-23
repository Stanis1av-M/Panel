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
    }
}