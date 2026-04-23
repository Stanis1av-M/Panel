using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using panel.Models;

namespace Panel.Views
{
    public partial class SupplyAddItemWindow : Window
    {
        private AppDbContext _db = new AppDbContext();
        public SupplyItem NewItem { get; private set; }

        public SupplyAddItemWindow()
        {
            InitializeComponent();
            cmbProduct.ItemsSource = _db.Products.ToList();
        }

        // --- ВАЛИДАЦИЯ ВВОДА ---

        // Только цифры для количества
        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Цифры и запятая для цены
        private void DecimalValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]");
            e.Handled = regex.IsMatch(e.Text);
        }

        // --- АВТОМАТИЧЕСКИЙ РАСЧЕТ ---
        private void CalculateTotal(object sender, TextChangedEventArgs e)
        {
            if (txtQuantity == null || txtUnitPrice == null || txtTotalPrice == null) return;

            if (int.TryParse(txtQuantity.Text, out int qty) &&
                decimal.TryParse(txtUnitPrice.Text, out decimal price))
            {
                txtTotalPrice.Text = (qty * price).ToString("N2");
            }
            else
            {
                txtTotalPrice.Text = "0,00";
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Проверка выбора товара
            if (cmbProduct.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар из списка!", "Внимание");
                return;
            }

            // Валидация количества (> 0)
            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Количество должно быть больше нуля!", "Ошибка валидации");
                return;
            }

            // Валидация цены (> 0)
            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice <= 0)
            {
                MessageBox.Show("Цена за единицу должна быть больше нуля!", "Ошибка валидации");
                return;
            }

            // Итоговая цена (уже посчитана)
            decimal totalPrice = qty * unitPrice;

            NewItem = new SupplyItem
            {
                Product = (Product)cmbProduct.SelectedItem,
                ProductId = ((Product)cmbProduct.SelectedItem).ProductId,
                Quantity = qty,
                Price = totalPrice // В базу записываем общую сумму за позицию
            };

            this.DialogResult = true;
            this.Close();
        }
    }
}