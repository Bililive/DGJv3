using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace DGJv3
{
    internal class Downloader : INotifyPropertyChanged
    {
        private Dispatcher dispatcher;

        private ObservableCollection<SongItem> Songs;

        private DispatcherTimer newSongTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromSeconds(1),
            IsEnabled = true,
        };

        private Timer downloadTimeoutTimer = new Timer(1000)
        {
            AutoReset = true,
        };

        public bool IsModuleDownloading { get => _isModuleDownloading; set => SetField(ref _isModuleDownloading, value); }
        private bool _isModuleDownloading = false;

        public double DownloadSpeed { get => _downloadSpeed; set => SetField(ref _downloadSpeed, value, nameof(DownloadSpeed)); }
        private double _downloadSpeed = 0;

        public int DownloadPercentage { get => _downloadPercentage; set => SetField(ref _downloadPercentage, value, nameof(DownloadPercentage)); }
        private int _downloadPercentage = 0;

        private WebClient webClient = null;

        private SongItem currentSong = null;

        private DateTime lastUpdateTime;

        private long lastUpdateDownloadedSize;

        private DateTime lastHighspeedTime;

        private TimeSpan timeout = TimeSpan.FromSeconds(5);

        public Downloader(ObservableCollection<SongItem> songs)
        {
            Songs = songs;
            newSongTimer.Tick += NewSongTimer_Tick;
            downloadTimeoutTimer.Elapsed += DownloadTimeoutTimer_Elapsed;
            dispatcher = Dispatcher.CurrentDispatcher;

            PropertyChanged += Downloader_PropertyChanged;
        }

        private void DownloadTimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DownloadPercentage > 0 && (DateTime.Now - lastHighspeedTime > timeout))
            {
                Log("下载速度过慢，防卡下载自动取消");
                CancelDownload();
            }
        }

        private void Downloader_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DownloadSpeed))
            {
                if (DownloadSpeed > 80)
                {
                    lastHighspeedTime = DateTime.Now;
                }
            }
        }

        private void NewSongTimer_Tick(object sender, EventArgs e)
        {
            if (currentSong == null)
            {
                if (IsModuleDownloading)
                {
                    IsModuleDownloading = false;
                }

                foreach (var songItem in Songs)
                {
                    if (songItem.Status == SongStatus.WaitingDownload)
                    {
                        currentSong = songItem;
                        Task.Run(() => Download());
                        break;
                    }
                }
            }
        }

        private void Download()
        {
            currentSong.FilePath = Path.Combine(Utilities.SongsCacheDirectoryPath, CleanFileName($"{currentSong.ModuleName}{currentSong.SongName}{currentSong.SongId}{DateTime.Now.ToBinary().ToString("X")}.mp3.点歌姬缓存"));

            try { Directory.CreateDirectory(Utilities.SongsCacheDirectoryPath); } catch (Exception) { }

            // currentSong = songItem;

            currentSong.Status = SongStatus.Downloading;
            if (currentSong.Module.IsHandleDownlaod)
            {
                IsModuleDownloading = true;
                new System.Threading.Thread(() =>
                {
                    switch (currentSong.Module.SafeDownload(currentSong))
                    {
                        case DownloadStatus.Success:
                            currentSong.Status = SongStatus.WaitingPlay;
                            break;
                        case DownloadStatus.Failed:
                        default:
                            dispatcher.Invoke(() => Songs.Remove(currentSong));
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
                    string url = currentSong.GetDownloadUrl();
                    if (url != null)
                    {
                        webClient = new WebClient();

                        webClient.DownloadProgressChanged += OnDownloadProgressChanged;
                        webClient.DownloadFileCompleted += OnDownloadFileCompleted;
                        webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.450 Safari/537.35");

                        webClient.DownloadFileAsync(new Uri(url), currentSong.FilePath);
                    }
                }
                catch (Exception ex)
                {
                    webClient?.Dispose();
                    webClient = null;

                    dispatcher.Invoke(() => Songs.Remove(currentSong));
                    Log("启动下载错误 " + currentSong.SongName, ex);
                    currentSong = null;
                }
            }
        }

        internal void CancelDownload()
        {
            if (webClient != null)
            {
                webClient.CancelAsync();
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
                dispatcher.Invoke(() => Songs.Remove(currentSong));
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

            int speed_bps = (int)Math.Floor((e.BytesReceived - lastUpdateDownloadedSize) / interval.TotalSeconds);

            lastUpdateDownloadedSize = e.BytesReceived;
            lastUpdateTime = now;

            DownloadSpeed = speed_bps / 1024d;
            DownloadPercentage = e.ProgressPercentage;
        }

        private static string CleanFileName(string fileName) => Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public event LogEvent LogEvent;
        private void Log(string message, Exception exception = null) => LogEvent?.Invoke(this, new LogEventArgs() { Message = message, Exception = exception });
    }
}
