using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using panel.Models;
using Panel.Services;

namespace Panel.Views
{
    public partial class OrderSuccessWindow : Window
    {
        private readonly int _orderId;

        public OrderSuccessWindow(int orderId, decimal totalAmount)
        {
            InitializeComponent();
            _orderId = orderId;
            txtOrderInfo.Text = $"Заказ №{orderId} на сумму {totalAmount:N2} руб.";
        }

        private void BtnDownloadPdf_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Сохранить чек",
                Filter = "PDF файл (*.pdf)|*.pdf",
                FileName = $"Чек_заказ_{_orderId}.pdf"
            };

            if (saveDialog.ShowDialog() != true) return;

            try
            {
                using (var db = new AppDbContext())
                {
                    var order = db.Orders
                        .AsNoTracking()
                        .Include(o => o.User)
                        .Include(o => o.DeliveryMethod)
                        .Include(o => o.PaymentMethod)
                        .Include(o => o.OrderStatus)
                        .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                        .FirstOrDefault(o => o.OrderId == _orderId);

                    if (order == null)
                    {
                        MessageBox.Show("Не удалось найти заказ для формирования чека (возможно, он был удалён).",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    ReceiptPdfGenerator.Save(order, saveDialog.FileName);
                }

                MessageBox.Show("Чек успешно сохранён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить чек: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
