using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;

namespace DGJv3
{
    class Downloader : INotifyPropertyChanged
    {
        private ObservableCollection<SongItem> Songs;

        private DispatcherTimer newSongTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromSeconds(1),
            IsEnabled = true,
        };

        public double DownloadSpeed { get => _downloadSpeed; set => SetField(ref _downloadSpeed, value, nameof(DownloadSpeed)); }
        private double _downloadSpeed = 0;

        public int DownloadPercentage { get => _downloadPercentage; set => SetField(ref _downloadPercentage, value, nameof(DownloadPercentage)); }
        private int _downloadPercentage = 0;

        private WebClient webClient = null;

        private SongItem currentSong = null;

        private DateTime lastUpdateTime;
        private long lastUpdateDownloadedSize;

        public Downloader(ObservableCollection<SongItem> songs)
        {
            Songs = songs;
            newSongTimer.Tick += NewSongTimer_Tick;
        }

        private void NewSongTimer_Tick(object sender, EventArgs e)
        {
            if (currentSong == null)
            {
                foreach (var songItem in Songs)
                {
                    if (songItem.Status == SongStatus.WaitingDownload)
                    {
                        Download(songItem);
                        break;
                    }
                }
            }
        }

        private void Download(SongItem songItem)
        {
            songItem.FilePath = Path.Combine(Utilities.SongsCachePath, CleanFileName($"{songItem.ModuleName}{songItem.SongName}{songItem.SongId}{DateTime.Now.ToBinary().ToString("X")}.mp3.点歌姬缓存"));

            try { Directory.CreateDirectory(Utilities.SongsCachePath); } catch (Exception) { }

            currentSong = songItem;

            currentSong.Status = SongStatus.Downloading;
            if (currentSong.Module.IsHandleDownlaod)
            {
                new Thread(() =>
                {
                    switch (currentSong.Module.SafeDownload(songItem))
                    {
                        case DownloadStatus.Success:
                            currentSong.Status = SongStatus.WaitingPlay;
                            break;
                        case DownloadStatus.Failed:
                        default:
                            Songs.Remove(currentSong);
                            break;
                    }
                    currentSong = null;
                })
                {
                    Name = "SongModuleDownload",
                    IsBackground = true,
                }
                .Start();
            }
            else
            {
                try
                {
                    webClient = new WebClient();
                    webClient.DownloadProgressChanged += OnDownloadProgressChanged;
                    webClient.DownloadFileCompleted += OnDownloadFileCompleted;
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.450 Safari/537.35");

                    webClient.DownloadFileAsync(new Uri(songItem.GetDownloadUrl()), songItem.FilePath);
                }
                catch (Exception ex)
                {
                    try
                    {
                        webClient.Dispose();
                        webClient = null;
                    }
                    catch (Exception)
                    { }

                    Songs.Remove(currentSong);
                    currentSong = null;
                    Log("启动下载错误 " + currentSong.SongName, ex);
                }
            }
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            bool success = true;
            if (e.Cancelled)
            {
                success = false;
                try
                { File.Delete(currentSong.FilePath); }
                catch (Exception) { }
            }
            else if (e.Error != null)
            {
                success = false;
            }

            if (success)
            {
                currentSong.Status = SongStatus.WaitingPlay;
            }
            else
            {
                Songs.Remove(currentSong);
                Log("下载错误 " + currentSong.SongName, e.Error);
            }

            DownloadSpeed = 0;
            DownloadPercentage = 0;

            currentSong = null;
            webClient.Dispose();
            webClient = null;
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan interval = now - lastUpdateTime;
            if (interval.TotalSeconds < 0.5)
            { return; }

            lastUpdateDownloadedSize = e.BytesReceived;
            lastUpdateTime = now;
            int speed_bps = (int)Math.Floor((e.BytesReceived - lastUpdateDownloadedSize) / interval.TotalSeconds);

            DownloadSpeed = speed_bps / 1024d;
            DownloadPercentage = e.ProgressPercentage;
        }

        private static string CleanFileName(string fileName) => Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public event LogEvent LogEvent;
        private void Log(string message, Exception exception) => LogEvent?.Invoke(this, new LogEventArgs() { Message = message, Exception = exception });
    }
}
