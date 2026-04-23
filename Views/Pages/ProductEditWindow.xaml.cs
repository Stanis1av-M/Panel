using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using panel.Models;

namespace Panel.Views
{
    public partial class ProductEditWindow : Window
    {
        private AppDbContext _db = new AppDbContext();
        private Product _currentProduct;
        private string _selectedImagePath = null;

        // Путь к твоей заглушке
        private readonly string _placeholderPath = "/images/products/NotFound.jpg";

        public ProductEditWindow(Product product = null)
        {
            InitializeComponent();

            cmbCategory.ItemsSource = _db.Categories.ToList();
            cmbManufacturer.ItemsSource = _db.Manufacturers.ToList();

            if (product != null)
            {
                _currentProduct = _db.Products.Find(product.ProductId);
                txtArticle.Text = _currentProduct.Article;
                txtName.Text = _currentProduct.Name;
                txtPrice.Text = _currentProduct.Price.ToString("F2");
                txtStock.Text = _currentProduct.Stock.ToString();

                cmbCategory.SelectedValue = _currentProduct.CategoryId;
                cmbManufacturer.SelectedValue = _currentProduct.ManufacturerId;

                // Загрузка фото с проверкой на существование
                LoadImagePreview(_currentProduct.ImageUrl);
            }
            else
            {
                _currentProduct = new Product();
                _currentProduct.CreatedAt = DateTime.Now;
                _currentProduct.SupplierId = 1;
                _currentProduct.Discount = 0;
                _currentProduct.IsVisible = true;
                _currentProduct.ImageUrl = _placeholderPath; // Ставим заглушку по умолчанию
                LoadImagePreview(_placeholderPath);
            }
        }

        // Метод для безопасного отображения картинки
        private void LoadImagePreview(string relativePath)
        {
            try
            {
                string path = string.IsNullOrEmpty(relativePath) ? _placeholderPath : relativePath;
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.TrimStart('/'));

                if (!File.Exists(fullPath))
                    fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _placeholderPath.TrimStart('/'));

                imgPreview.Source = new BitmapImage(new Uri(fullPath));
            }
            catch
            {
                // Если даже заглушка пропала, просто не выводим ничего
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnGenerateArticle_Click(object sender, RoutedEventArgs e)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string newArticle = new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            txtArticle.Text = newArticle;
        }

        private bool IsInputMalicious(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string upperInput = input.ToUpper();
            string[] badWords = { "DROP", "DELETE", "TRUNCATE", "UPDATE", "INSERT", "ALTER", "--", "SELECT" };
            return badWords.Any(word => upperInput.Contains(word));
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                imgPreview.Source = new BitmapImage(new Uri(_selectedImagePath));
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // === 1. ВАЛИДАЦИЯ И ЗАЩИТА ===

            if (IsInputMalicious(txtName.Text) || IsInputMalicious(txtArticle.Text))
            {
                MessageBox.Show("Нельзя так делать ;)", "Атата!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            // Название: до 50 символов
            if (string.IsNullOrWhiteSpace(txtName.Text) || txtName.Text.Length > 50)
            {
                MessageBox.Show("Название должно быть от 1 до 50 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Цена: макс 1 000 000
            if (!decimal.TryParse(txtPrice.Text, out decimal newPrice) || newPrice < 0 || newPrice > 1000000)
            {
                MessageBox.Show("Цена должна быть числом от 0 до 1 000 000!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Склад: макс 99
            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0 || stock > 99)
            {
                MessageBox.Show("Количество на складе должно быть от 0 до 99!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbCategory.SelectedValue == null || cmbManufacturer.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию и производителя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // === 2. СОХРАНЕНИЕ ===

            if (_currentProduct.ProductId > 0 && _currentProduct.Price != newPrice)
            {
                _db.PriceHistories.Add(new PriceHistory
                {
                    ProductId = _currentProduct.ProductId,
                    OldPrice = _currentProduct.Price,
                    NewPrice = newPrice,
                    ChangeAt = DateTime.Now
                });
            }

            _currentProduct.Article = txtArticle.Text;
            _currentProduct.Name = txtName.Text;
            _currentProduct.Price = newPrice;
            _currentProduct.Stock = stock;
            _currentProduct.CategoryId = (int)cmbCategory.SelectedValue;
            _currentProduct.ManufacturerId = (int)cmbManufacturer.SelectedValue;

            // Обработка картинки по ТЗ
            if (_selectedImagePath != null)
            {
                // ТЗ требует папку ProductImages
                string destFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductImages");
                if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

                string ext = Path.GetExtension(_selectedImagePath);
                string newFileName = Guid.NewGuid().ToString() + ext;
                string destFilePath = Path.Combine(destFolder, newFileName);

                File.Copy(_selectedImagePath, destFilePath, true);
                _currentProduct.ImageUrl = "/ProductImages/" + newFileName;
            }

            if (_currentProduct.ProductId == 0)
                _db.Products.Add(_currentProduct);

            try
            {
                _db.SaveChanges();
                MessageBox.Show("Товар успешно сохранен!", "Успех");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}