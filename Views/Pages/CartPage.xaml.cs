using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using panel.Models;

namespace Panel.Views.Pages
{
    /// <summary>
    /// Обёртка над CartItem для отображения в корзине (готовые строки для биндинга).
    /// </summary>
    public class CartLine
    {
        public CartItem Item { get; set; } = null!;

        public decimal Subtotal => Item.Product.Price * Item.Quantity;

        public string PriceText => $"{Item.Product.Price:N2} руб. / шт.";

        public string SubtotalText => $"{Subtotal:N2} руб.";
    }

    public partial class CartPage : Page
    {
        private readonly User _currentUser;
        private List<CartLine> _lines = new List<CartLine>();

        public CartPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadOrderOptions();
            LoadCart();
        }

        private void LoadOrderOptions()
        {
            using (var db = new AppDbContext())
            {
                var deliveryMethods = db.DeliveryMethods.AsNoTracking().Where(d => d.IsActive).ToList();
                var paymentMethods = db.PaymentMethods.AsNoTracking().Where(p => p.IsActive).ToList();

                cmbDelivery.ItemsSource = deliveryMethods;
                if (deliveryMethods.Count > 0) cmbDelivery.SelectedIndex = 0;

                cmbPayment.ItemsSource = paymentMethods;
                if (paymentMethods.Count > 0) cmbPayment.SelectedIndex = 0;
            }
        }

        private void LoadCart()
        {
            using (var db = new AppDbContext())
            {
                var items = db.CartItems
                    .AsNoTracking()
                    .Include(c => c.Product)
                    .Where(c => c.UserId == _currentUser.UserId)
                    .ToList();

                _lines = items.Select(i => new CartLine { Item = i }).ToList();
            }

            RefreshView();
        }

        private void RefreshView()
        {
            ItemsCart.ItemsSource = null;
            ItemsCart.ItemsSource = _lines;

            bool isEmpty = _lines.Count == 0;
            txtEmptyCart.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
            ItemsCart.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;

            decimal deliveryPrice = (cmbDelivery.SelectedItem as DeliveryMethod)?.Price ?? 0;
            decimal total = _lines.Sum(l => l.Subtotal) + deliveryPrice;
            txtTotal.Text = $"{total:N2} руб.";

            btnCheckout.IsEnabled = !isEmpty && cmbDelivery.SelectedItem != null && cmbPayment.SelectedItem != null;
        }

        private void OnOrderOptionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshView();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
            else
            {
                this.NavigationService?.Navigate(new ShopPage(_currentUser));
            }
        }

        private void BtnIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem cartItem)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var entity = db.CartItems.Include(c => c.Product)
                            .FirstOrDefault(c => c.CartItemId == cartItem.CartItemId);

                        if (entity == null) return;

                        if (entity.Quantity + 1 > entity.Product.Stock)
                        {
                            MessageBox.Show("На складе нет столько товара.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        entity.Quantity += 1;
                        db.SaveChanges();
                    }
                    LoadCart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem cartItem)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var entity = db.CartItems.FirstOrDefault(c => c.CartItemId == cartItem.CartItemId);
                        if (entity == null) return;

                        if (entity.Quantity <= 1)
                        {
                            db.CartItems.Remove(entity);
                        }
                        else
                        {
                            entity.Quantity -= 1;
                        }
                        db.SaveChanges();
                    }
                    LoadCart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem cartItem)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var entity = db.CartItems.FirstOrDefault(c => c.CartItemId == cartItem.CartItemId);
                        if (entity != null)
                        {
                            db.CartItems.Remove(entity);
                            db.SaveChanges();
                        }
                    }
                    LoadCart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_lines.Count == 0)
            {
                MessageBox.Show("Корзина пуста.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbDelivery.SelectedItem is not DeliveryMethod delivery || cmbPayment.SelectedItem is not PaymentMethod payment)
            {
                MessageBox.Show("Выберите способ доставки и оплаты.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Оформить заказ на сумму {txtTotal.Text}?",
                "Подтверждение заказа",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            int completedOrderId = 0;
            decimal completedOrderTotal = 0;

            try
            {
                using (var db = new AppDbContext())
                using (var transaction = db.Database.BeginTransaction())
                {
                    var cartItems = db.CartItems
                        .Include(c => c.Product)
                        .Where(c => c.UserId == _currentUser.UserId)
                        .ToList();

                    if (cartItems.Count == 0)
                    {
                        MessageBox.Show("Корзина пуста.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadCart();
                        return;
                    }

                    // Проверка остатков перед оформлением
                    foreach (var ci in cartItems)
                    {
                        if (ci.Quantity > ci.Product.Stock)
                        {
                            MessageBox.Show(
                                $"Недостаточно товара \"{ci.Product.Name}\" на складе (доступно: {ci.Product.Stock}).",
                                "Невозможно оформить заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                            LoadCart();
                            return;
                        }
                    }

                    // Берём статус заказа "Новый" (по аналогии с дефолтным статусом из БД)
                    var newStatus = db.OrderStatuses.FirstOrDefault(s => s.Name == "Новый")
                                    ?? db.OrderStatuses.OrderBy(s => s.OrderStatusId).FirstOrDefault();

                    if (newStatus == null)
                    {
                        MessageBox.Show("В системе не настроены статусы заказов. Обратитесь к администратору.",
                            "Невозможно оформить заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    decimal itemsTotal = cartItems.Sum(c => c.Product.Price * c.Quantity);
                    decimal total = itemsTotal + delivery.Price;

                    var order = new Order
                    {
                        UserId = _currentUser.UserId,
                        OrderStatusId = newStatus.OrderStatusId,
                        DeliveryMethodId = delivery.DeliveryMethodId,
                        PaymentMethodId = payment.PaymentMethodId,
                        CreatedAt = DateTime.Now,
                        CompletedAt = null,
                        TotalAmount = total
                    };

                    db.Orders.Add(order);
                    db.SaveChanges(); // нужно сохранить, чтобы получить OrderId

                    foreach (var ci in cartItems)
                    {
                        db.OrderItems.Add(new OrderItem
                        {
                            OrderId = order.OrderId,
                            ProductId = ci.ProductId,
                            Quantity = ci.Quantity,
                            Price = ci.Product.Price
                        });

                        // списываем со склада, перепроверяя актуальный остаток внутри транзакции
                        // (защита от ситуации, когда другой пользователь купил товар параллельно)
                        var product = db.Products.First(p => p.ProductId == ci.ProductId);
                        if (ci.Quantity > product.Stock)
                        {
                            transaction.Rollback();
                            MessageBox.Show(
                                $"Товар \"{product.Name}\" уже разобрали — на складе осталось {product.Stock} шт. Заказ не оформлен, обновите корзину.",
                                "Невозможно оформить заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                            LoadCart();
                            return;
                        }
                        product.Stock -= ci.Quantity;
                    }

                    db.CartItems.RemoveRange(cartItems);

                    db.SaveChanges();
                    transaction.Commit();

                    // Запоминаем данные оформленного заказа, чтобы показать окно с чеком
                    completedOrderId = order.OrderId;
                    completedOrderTotal = order.TotalAmount;
                }

                // Запоминаем владельца окна заранее: после навигации эта страница
                // уже не будет частью визуального дерева Frame.
                var ownerWindow = Window.GetWindow(this);

                // После оформления возвращаемся в магазин (ShopPage сама обновит остатки при Loaded)
                this.NavigationService?.Navigate(new ShopPage(_currentUser));

                var successWindow = new OrderSuccessWindow(completedOrderId, completedOrderTotal);
                successWindow.Owner = ownerWindow;
                successWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось оформить заказ: {ex.Message}\n\n{ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
