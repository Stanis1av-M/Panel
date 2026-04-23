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
                var products = db.Products
                                 .Include(p => p.Category)
                                 .ToList();

                GridProducts.ItemsSource = products;
            }
        }

        // 1. Наш метод проверки на пакости (скопировали из окна логина)
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

        // 2. Обновленный метод поиска
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Берем текст из поля поиска
            string rawSearchText = txtSearch.Text;

            // ВАЛИДАЦИЯ: Проверка на опасные команды
            if (IsInputMalicious(rawSearchText))
            {
                MessageBox.Show("Нельзя так делать ;)", "Атата!", MessageBoxButton.OK, MessageBoxImage.Stop);

                // Очищаем поле поиска
                txtSearch.Clear();

                // Перезагружаем полную таблицу, так как поиск сброшен
                LoadData();
                return;
            }

            // Если всё чисто, переводим в нижний регистр для нормального поиска
            string searchText = rawSearchText.ToLower().Trim();

            using (var db = new AppDbContext())
            {
                var filtered = db.Products
                                 .Include(p => p.Category)
                                 .Where(p => p.Name.ToLower().Contains(searchText) || p.Article.ToLower().Contains(searchText))
                                 .ToList();

                GridProducts.ItemsSource = filtered;
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
          
            ProductEditWindow window = new ProductEditWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadData(); 
            }
        }

        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            
            if (GridProducts.SelectedItem is Product selectedProduct)
            {
               
                ProductEditWindow window = new ProductEditWindow(selectedProduct);
                if (window.ShowDialog() == true)
                {
                    LoadData(); 
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите товар в таблице.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}