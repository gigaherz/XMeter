using System;
using System.Globalization;
using System.Windows.Data;

namespace XMeter
{
    public class USizeConverter : IValueConverter
    {
        public static string FormatUSize(double dbytes)
        {
            if (dbytes < 1024)
                return $"{dbytes:#0.00} B/s";

            dbytes /= 1024.0;

            if (dbytes < 1024)
                return $"{dbytes:#0.00} KB/s";

            dbytes /= 1024.0;

            if (dbytes < 1024)
                return $"{dbytes:#0.00} MB/s";

            dbytes /= 1024.0;

            // Maybe... someday...
            return $"{dbytes:#0.00} GB/s";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return FormatUSize(value as double? ?? 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
