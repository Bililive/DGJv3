using System;
using System.Globalization;
using System.Windows.Data;

namespace DGJv3
{
    class PlayerVolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Round((float)value * 100d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (float)((double)value / 100f);
        }
    }
}
