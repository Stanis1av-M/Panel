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
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cmbNewStatus.SelectedValue is int newStatusId)
            {
                var orderInDb = _db.Orders.Find(_order.OrderId);
                if (orderInDb != null)
                {
                    orderInDb.OrderStatusId = newStatusId;
                    _db.SaveChanges();

                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}