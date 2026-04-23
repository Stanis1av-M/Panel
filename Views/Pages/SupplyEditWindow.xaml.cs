using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using panel.Models;

namespace Panel.Views
{
    public partial class SupplyEditWindow : Window
    {
        private AppDbContext _db = new AppDbContext();
        // Временный список товаров для этой поставки
        private List<SupplyItem> _items = new List<SupplyItem>();
        private int _userId;

        public SupplyEditWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            cmbSupplier.ItemsSource = _db.Suppliers.ToList();
            GridSupplyItems.ItemsSource = _items;
        }

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            var addWin = new SupplyAddItemWindow();
            addWin.Owner = this;

            // Если пользователь нажал "Добавить" (DialogResult = true)
            if (addWin.ShowDialog() == true)
            {
                // Теперь NewItem будет доступен!
                _items.Add(addWin.NewItem);
                GridSupplyItems.ItemsSource = null; // Сбрасываем привязку
                GridSupplyItems.ItemsSource = _items; // Привязываем заново для обновления
            }
        }

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (GridSupplyItems.SelectedItem is SupplyItem item)
            {
                _items.Remove(item);
                GridSupplyItems.Items.Refresh();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSupplier.SelectedValue == null)
            {
                MessageBox.Show("Выберите поставщика!");
                return;
            }

            if (_items.Count == 0)
            {
                MessageBox.Show("Список товаров пуст!");
                return;
            }

            try
            {
                // 1. Создаем "шапку" поставки
                var newSupply = new Supply
                {
                    SupplierId = (int)cmbSupplier.SelectedValue,
                    UserId = _userId,
                    Status = "Создано",
                    CreatedAt = DateTime.Now
                };

                _db.Supplies.Add(newSupply);
                _db.SaveChanges(); // Сохраняем, чтобы получить SupplyId

                // 2. Привязываем товары к ID этой поставки
                foreach (var item in _items)
                {
                    item.SupplyId = newSupply.SupplyId;
                    item.Product = null; // Очищаем навигационное свойство перед сохранением
                    _db.SupplyItems.Add(item);
                }

                _db.SaveChanges();
                MessageBox.Show("Поставка успешно сформирована!");
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}