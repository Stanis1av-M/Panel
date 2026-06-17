using System;
using System.Linq;
using panel.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;

namespace Panel.Services
{
    /// <summary>
    /// Генерирует PDF-чек по уже оформленному заказу. Используется на странице корзины
    /// после успешного оформления заказа ("Скачать чек (PDF)").
    /// </summary>
    public static class ReceiptPdfGenerator
    {
        // Ссылка на вашу Google форму
        private const string FeedbackFormUrl = "https://docs.google.com/forms/d/e/1FAIpQLSdQ3mKzIhxYtdVEpeWBbcwRjAillYsUAdjYOQhWAllSJbyPEQ/viewform?usp=publish-editor";

        /// <summary>
        /// Строит PDF-документ чека и сохраняет его по указанному пути.
        /// Заказ должен быть загружен с Include для OrderItems.Product, DeliveryMethod,
        /// PaymentMethod и User — иначе в чеке будут пустые поля.
        /// </summary>
        public static void Save(Order order, string filePath)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Не указан путь для сохранения файла.", nameof(filePath));

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Apex Shop").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("Чек по заказу").FontSize(13).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Заказ №{order.OrderId}").Bold().FontSize(14);
                                c.Item().Text($"Дата оформления: {order.CreatedAt:dd.MM.yyyy HH:mm}");
                                c.Item().Text($"Статус: {order.OrderStatus?.Name ?? "—"}");
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignRight().Text("Покупатель").Bold();
                                c.Item().AlignRight().Text(order.User?.FullName ?? "—");
                                c.Item().AlignRight().Text(order.User?.Email ?? "—");
                                if (!string.IsNullOrWhiteSpace(order.User?.Phone))
                                    c.Item().AlignRight().Text(order.User.Phone);
                            });
                        });

                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                        // Таблица позиций заказа
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);  // Наименование
                                columns.RelativeColumn(1);  // Кол-во
                                columns.RelativeColumn(2);  // Цена за шт.
                                columns.RelativeColumn(2);  // Сумма
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Товар").Bold();
                                header.Cell().AlignRight().Text("Кол-во").Bold();
                                header.Cell().AlignRight().Text("Цена").Bold();
                                header.Cell().AlignRight().Text("Сумма").Bold();

                                header.Cell().ColumnSpan(4).PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
                            });

                            foreach (var item in order.OrderItems.OrderBy(i => i.OrderItemId))
                            {
                                string productName = item.Product?.Name ?? $"Товар #{item.ProductId}";
                                decimal lineTotal = item.Price * item.Quantity;

                                table.Cell().PaddingVertical(4).Text(productName);
                                table.Cell().PaddingVertical(4).AlignRight().Text(item.Quantity.ToString());
                                table.Cell().PaddingVertical(4).AlignRight().Text($"{item.Price:N2} руб.");
                                table.Cell().PaddingVertical(4).AlignRight().Text($"{lineTotal:N2} руб.");
                            }
                        });

                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                        decimal itemsTotal = order.OrderItems.Sum(i => i.Price * i.Quantity);
                        decimal deliveryPrice = order.DeliveryMethod?.Price ?? 0;

                        col.Item().AlignRight().Column(c =>
                        {
                            c.Item().Text($"Товары: {itemsTotal:N2} руб.");
                            c.Item().Text($"Доставка ({order.DeliveryMethod?.Name ?? "—"}): {deliveryPrice:N2} руб.");
                            c.Item().PaddingTop(4).Text($"Итого: {order.TotalAmount:N2} руб.").Bold().FontSize(14);
                        });

                        col.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Способ оплаты: {order.PaymentMethod?.Name ?? "—"}");
                            row.RelativeItem().AlignRight().Text($"Способ доставки: {order.DeliveryMethod?.Name ?? "—"}");
                        });

                        // ------------------------------------------------------------------
                        // ОБНОВЛЕННЫЙ БЛОК: УВЕЛИЧЕННАЯ ПЛАШКА И QR-КОД
                        // ------------------------------------------------------------------
                        col.Item().PaddingTop(25).Background(Colors.Grey.Lighten4).Padding(20).Row(row =>
                        {
                            row.Spacing(20); // Расстояние между текстом и QR-кодом

                            // Текст слева от QR кода (увеличен шрифт)
                            row.RelativeItem().AlignMiddle().Column(c =>
                            {
                                c.Item().Text("Оцените качество сервиса!").Bold().FontSize(15).FontColor(Colors.Blue.Darken2);
                                c.Item().PaddingTop(5).Text("Отсканируйте QR-код вашей камерой телефона, чтобы перейти к форме отзыва.").FontSize(11).FontColor(Colors.Grey.Darken3);
                            });

                            // Генерация и вставка самого QR кода
                            byte[] qrBytes = GenerateQrCodeBytes(FeedbackFormUrl);

                            // Увеличили размер QR-кода с 60 до 110 единиц
                            row.ConstantItem(110).AlignRight().AlignMiddle().Image(qrBytes);
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Спасибо за покупку в Apex Shop! ");
                        x.Span($"Документ сформирован {DateTime.Now:dd.MM.yyyy HH:mm}").FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            document.GeneratePdf(filePath);
        }

        /// <summary>
        /// Вспомогательный метод для генерации картинки QR-кода (PNG)
        /// </summary>
        private static byte[] GenerateQrCodeBytes(string url)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);

            using var qrCode = new PngByteQRCode(qrCodeData);
            // Увеличено разрешение исходной картинки с 5 до 10 для лучшей четкости при печати
            return qrCode.GetGraphic(10);
        }
    }
}