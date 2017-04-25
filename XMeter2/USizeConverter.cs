
using System;
using System.Globalization;
using System.Windows.Data;

namespace XMeter2
{
    public class USizeConverter : IValueConverter
    {
        public static string FormatUSize(ulong bytes)
        {
            double dbytes = bytes;

            if (bytes < 1024)
                return $"{bytes} B/s";

            dbytes /= 1024.0;

            if (dbytes < 1024)
                return $"{dbytes:#0.00} KB/s";

            dbytes /= 1024.0;

            if (dbytes < 1024)
                return $"{dbytes:#0.00} MBs/s";

            dbytes /= 1024.0;

            // Maybe... someday...
            return $"{dbytes:#0.00} GBs/s";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return FormatUSize(value as ulong? ?? 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
