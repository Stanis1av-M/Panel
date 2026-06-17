using System;
using System.Windows;

namespace Panel.Views
{
    public partial class ErrorWindow : Window
    {
        private readonly string _details;

        public ErrorWindow(Exception ex)
        {
            InitializeComponent();

            _details = BuildDetailsText(ex);
            txtDetails.Text = _details;

            // Подсказка по-человечески для самых частых случаев, чтобы пользователь
            // понимал, что делать, а не просто видел "ошибка".
            if (IsConnectionProblem(ex))
            {
                txtFriendlyMessage.Text = "Не удалось подключиться к базе данных. Проверьте, что сервер базы данных запущен, и попробуйте снова.";
            }
            else if (ex is NullReferenceException)
            {
                txtFriendlyMessage.Text = "Программа попыталась обратиться к отсутствующим данным. Попробуйте обновить страницу или перезайти.";
            }
        }

        private static bool IsConnectionProblem(Exception ex)
        {
            var current = ex;
            while (current != null)
            {
                string typeName = current.GetType().Name;
                if (typeName.Contains("SqlException") || typeName.Contains("DbUpdateException") ||
                    typeName.Contains("DbException") || current.Message.Contains("Database") ||
                    current.Message.Contains("connection", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                current = current.InnerException;
            }
            return false;
        }

        private static string BuildDetailsText(Exception ex)
        {
            var sb = new System.Text.StringBuilder();
            var current = ex;
            int level = 0;

            while (current != null)
            {
                string prefix = level == 0 ? "" : new string(' ', level * 2) + "→ ";
                sb.AppendLine($"{prefix}{current.GetType().Name}: {current.Message}");
                current = current.InnerException;
                level++;
            }

            sb.AppendLine();
            sb.AppendLine($"Время: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");

            return sb.ToString();
        }

        private void BtnCopyDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_details);
                MessageBox.Show("Подробности скопированы в буфер обмена.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                // Буфер обмена иногда занят другим процессом — не критично, просто молча игнорируем
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
