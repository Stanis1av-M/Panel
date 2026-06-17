using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using panel.Models;

namespace Panel.Views.Pages
{
    /// <summary>
    /// Обёртка для отображения товара в карточке витрины (готовые строки/видимость для XAML-биндинга).
    /// </summary>
    public class ShopProductCard
    {
        public Product Product { get; set; } = null!;

        public bool InStock => Product.Stock > 0;

        public string PriceText => $"{Product.Price:N2} руб.";

        public string OldPriceText => Product.OldPrice.HasValue ? $"{Product.OldPrice.Value:N2} руб." : "";

        public Visibility DiscountVisibility =>
            (Product.Discount > 0 && Product.OldPrice.HasValue) ? Visibility.Visible : Visibility.Collapsed;

        public string DiscountText => $"-{Product.Discount}%";

        public string StockText => InStock ? $"В наличии: {Product.Stock} шт." : "Нет в наличии";

        public Brush StockColor => InStock
            ? new SolidColorBrush(Color.FromRgb(0x44, 0xBD, 0x32))
            : new SolidColorBrush(Color.FromRgb(0xFF, 0x47, 0x57));

        public Visibility ImageVisibility =>
            string.IsNullOrWhiteSpace(Product.ImageUrl) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility NoImageVisibility =>
            string.IsNullOrWhiteSpace(Product.ImageUrl) ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Превращает относительный путь из БД (например "/images/products/palatka.png")
        /// в pack-URI, который WPF гарантированно резолвит относительно папки с .exe,
        /// независимо от текущей рабочей директории процесса.
        /// </summary>
        public string ImageSourcePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Product.ImageUrl)) return "";
                string relative = Product.ImageUrl.TrimStart('/', '\\');
                return $"pack://siteoforigin:,,,/{relative}";
            }
        }
    }

    public partial class ShopPage : Page
    {
        private readonly User _currentUser;
        private List<Product> _allProducts = new List<Product>();

        public ShopPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadCategories();
            LoadData();
            UpdateCartCount();
            this.Loaded += ShopPage_Loaded;
        }

        private void ShopPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Обновляем витрину и счётчик корзины при каждом возврате на страницу
            // (например после оформления заказа на странице корзины — остатки на складе изменились).
            LoadData();
            UpdateCartCount();
        }

        private void LoadCategories()
        {
            using (var db = new AppDbContext())
            {
                var categories = db.Categories.AsNoTracking().OrderBy(c => c.Name).ToList();
                categories.Insert(0, new Category { CategoryId = 0, Name = "Все категории" });

                cmbCategory.ItemsSource = categories;
                cmbCategory.SelectedIndex = 0;
            }
        }

        private void LoadData()
        {
            using (var db = new AppDbContext())
            {
                _allProducts = db.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.IsVisible)
                    .OrderBy(p => p.Name)
                    .ToList();
            }

            ApplyFilters();
        }

        private bool IsInputMalicious(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string[] bad = { "DROP ", "DELETE ", "TRUNCATE ", "UNION ", "--" };
            return bad.Any(b => input.ToUpper().Contains(b));
        }

        private void ApplyFilters()
        {
            if (ItemsProducts == null) return;

            IEnumerable<Product> filtered = _allProducts;

            string search = (txtSearch?.Text ?? "").Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Article.ToLower().Contains(search));
            }

            if (cmbCategory.SelectedItem is Category selectedCategory && selectedCategory.CategoryId != 0)
            {
                filtered = filtered.Where(p => p.CategoryId == selectedCategory.CategoryId);
            }

            // Рекомендации по скидке: товары, у которых заполнен Discount (> 0) — это уже
            // готовое поле в Products, отдельная таблица/расчёт не нужны.
            if (chkOnlyDiscount?.IsChecked == true)
            {
                filtered = filtered.Where(p => p.Discount > 0);
            }

            filtered = (cmbSort?.SelectedIndex ?? 0) switch
            {
                1 => filtered.OrderByDescending(p => p.Name),
                2 => filtered.OrderBy(p => p.Price),
                3 => filtered.OrderByDescending(p => p.Price),
                _ => filtered.OrderBy(p => p.Name)
            };

            ItemsProducts.ItemsSource = filtered
                .Select(p => new ShopProductCard { Product = p })
                .ToList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = txtSearch.Text ?? "";
            if (IsInputMalicious(search))
            {
                txtSearch.Clear();
                return;
            }
            ApplyFilters();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ChkOnlyDiscount_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void UpdateCartCount()
        {
            using (var db = new AppDbContext())
            {
                int count = db.CartItems
                    .Where(c => c.UserId == _currentUser.UserId)
                    .Sum(c => (int?)c.Quantity) ?? 0;

                txtCartCount.Text = count.ToString();
            }
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product product)
            {
                if (product.Stock <= 0)
                {
                    MessageBox.Show("Товара нет в наличии.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    using (var db = new AppDbContext())
                    {
                        var existing = db.CartItems
                            .FirstOrDefault(c => c.UserId == _currentUser.UserId && c.ProductId == product.ProductId);

                        if (existing != null)
                        {
                            if (existing.Quantity + 1 > product.Stock)
                            {
                                MessageBox.Show("Достигнут лимит доступного количества на складе.", "Внимание",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            existing.Quantity += 1;
                        }
                        else
                        {
                            db.CartItems.Add(new CartItem
                            {
                                UserId = _currentUser.UserId,
                                ProductId = product.ProductId,
                                Quantity = 1
                            });
                        }

                        db.SaveChanges();
                    }

                    UpdateCartCount();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось добавить товар в корзину: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new CartPage(_currentUser));
        }
    }
}
