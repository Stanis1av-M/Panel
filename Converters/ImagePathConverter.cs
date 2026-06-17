using System;
using System.Globalization;
using System.Windows.Data;

namespace Panel.Converters
{
    /// <summary>
    /// Превращает относительный путь к файлу из БД (например "/images/products/palatka.png")
    /// в pack-URI, который WPF гарантированно резолвит относительно папки с .exe,
    /// независимо от текущей рабочей директории процесса. Если путь пуст — возвращает
    /// заглушку NotFound.jpg.
    /// </summary>
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? path = value as string;

            if (string.IsNullOrWhiteSpace(path))
            {
                path = "/images/products/NotFound.jpg";
            }

            string relative = path.TrimStart('/', '\\');
            return $"pack://siteoforigin:,,,/{relative}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
