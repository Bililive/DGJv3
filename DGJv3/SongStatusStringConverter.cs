using System;
using System.Globalization;
using System.Windows.Data;

namespace DGJv3
{
    class SongStatusStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SongStatus songStatus)
            {
                switch (songStatus)
                {
                    case SongStatus.WaitingDownload:
                        return "等待下载";
                    case SongStatus.Downloading:
                        return "正在下载";
                    case SongStatus.WaitingPlay:
                        return "等待播放";
                    case SongStatus.Playing:
                        return "正在播放";
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
