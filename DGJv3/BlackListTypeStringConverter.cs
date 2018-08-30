using System;
using System.Globalization;
using System.Windows.Data;

namespace DGJv3
{
    class BlackListTypeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BlackListType blackListType)
            {
                switch (blackListType)
                {
                    case BlackListType.Id:
                        return "歌曲ID";
                    case BlackListType.Name:
                        return "歌曲名字";
                    case BlackListType.Singer:
                        return "歌手名字";
                    default:
                        break;
                }
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
