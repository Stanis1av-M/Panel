using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Panel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Бесплатная лицензия QuestPDF (учебное/некоммерческое использование,
            // либо доход компании ниже порога — подробнее см. questpdf.com/license).
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            // Перехват необработанных исключений на всех уровнях приложения:
            // UI-поток, фоновые задачи, и общий AppDomain как последний рубеж.
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowErrorWindow(e.Exception);
            e.Handled = true; // Не даём приложению аварийно закрыться
        }

        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ShowErrorWindow(ex);
            }
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ShowErrorWindow(e.Exception);
            e.SetObserved();
        }

        private void ShowErrorWindow(Exception ex)
        {
            try
            {
                var window = new Views.ErrorWindow(ex);
                window.ShowDialog();
            }
            catch
            {
                // Если даже окно ошибки не открылось — последний рубеж защиты
                MessageBox.Show(
                    "Произошла непредвиденная ошибка, и показать подробности не удалось.\nПопробуйте перезапустить приложение.",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}
