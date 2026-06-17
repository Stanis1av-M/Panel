using System;
using System.Linq;
using System.Windows;
using panel.Models;

namespace Panel.Views
{
    public partial class OrderStatusWindow : Window
    {
        private Order _order;
        private AppDbContext _db = new AppDbContext();

        public OrderStatusWindow(Order order)
        {
            InitializeComponent();
            _order = order;

            txtOrderInfo.Text = $"Заказ №{_order.OrderId} ({_order.User.FullName})";

            // Загружаем все возможные статусы
            cmbNewStatus.ItemsSource = _db.OrderStatuses.ToList();

            // Устанавливаем текущий статус заказа в комбобоксе
            cmbNewStatus.SelectedValue = _order.OrderStatusId;

            // Освобождаем DbContext, когда окно закрывается, чтобы не держать
            // открытое соединение с БД дольше, чем нужно.
            this.Closed += (s, e) => _db.Dispose();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cmbNewStatus.SelectedValue is not int newStatusId)
            {
                MessageBox.Show("Выберите статус заказа.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var orderInDb = _db.Orders.Find(_order.OrderId);
                if (orderInDb == null)
                {
                    MessageBox.Show("Заказ не найден (возможно, был удалён).", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.DialogResult = false;
                    this.Close();
                    return;
                }

                orderInDb.OrderStatusId = newStatusId;
                _db.SaveChanges();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось обновить статус заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}