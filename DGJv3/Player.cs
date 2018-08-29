using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace DGJv3
{
    class Player : INotifyPropertyChanged
    {
        // TODO: 歌词

        private ObservableCollection<SongItem> Songs;

        private Dispatcher dispatcher;

        private DispatcherTimer newSongTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromSeconds(1),
            IsEnabled = true,
        };

        private DispatcherTimer updateTimeTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromMilliseconds(100),
            IsEnabled = true,
        };

        public UniversalCommand PlayPauseCommand { get; private set; }
        public UniversalCommand NextCommand { get; private set; }

        /// <summary>
        /// 播放器类型
        /// </summary>
        public PlayerType PlayerType { get => _playerType; set => SetField(ref _playerType, value); }
        private PlayerType _playerType;

        /// <summary>
        /// DirectSound 设备
        /// </summary>
        public Guid DirectSoundDevice { get => _directSoundDevice; set => SetField(ref _directSoundDevice, value); }
        private Guid _directSoundDevice;

        /// <summary>
        /// WaveoutEvent 设备
        /// </summary>
        public int WaveoutEventDevice { get => _waveoutEventDevice; set => SetField(ref _waveoutEventDevice, value); }
        private int _waveoutEventDevice;

        /// <summary>
        /// 当前播放时间
        /// </summary>
        public TimeSpan CurrentTime
        {
            get => mp3FileReader == null ? TimeSpan.Zero : mp3FileReader.CurrentTime;
            set { if (mp3FileReader != null) mp3FileReader.CurrentTime = value; }
        }

        /// <summary>
        /// 当前播放时间秒数
        /// </summary>
        public double CurrentTimeDouble
        {
            get => CurrentTime.TotalSeconds;
            set => CurrentTime = TimeSpan.FromSeconds(value);
        }

        /// <summary>
        /// 歌曲全长
        /// </summary>
        public TimeSpan TotalTime { get => mp3FileReader == null ? TimeSpan.Zero : mp3FileReader.TotalTime; }

        /// <summary>
        /// 当前是否正在播放歌曲
        /// </summary>
        public bool IsPlaying
        {
            get => Status == PlayerStatus.Playing;
            set { if (value) Play(); else Pause(); }
        }

        /// <summary>
        /// 当前歌曲播放状态
        /// </summary>
        public PlayerStatus Status
        {
            get
            {
                if (wavePlayer != null)
                {
                    switch (wavePlayer.PlaybackState)
                    {
                        case PlaybackState.Stopped:
                            return PlayerStatus.Stopped;
                        case PlaybackState.Playing:
                            return PlayerStatus.Playing;
                        case PlaybackState.Paused:
                            return PlayerStatus.Paused;
                        default:
                            return PlayerStatus.Stopped;
                    }
                }
                else
                {
                    return PlayerStatus.Stopped;
                }
            }
        }

        /// <summary>
        /// 播放器音量
        /// </summary>
        public float Volume
        {
            get => _volume;
            set
            {
                if (sampleChannel != null)
                {
                    sampleChannel.Volume = value;
                }
                SetField(ref _volume, value, nameof(Volume));
            }
        }
        private float _volume = 1f;

        /// <summary>
        /// 当前歌词
        /// </summary>
        public string CurrentLyric { get => currentLyric; set => SetField(ref currentLyric, value); }
        private string currentLyric;

        /// <summary>
        /// 下一句歌词       
        /// </summary>
        public string UpcomingLyric { get => upcomingLyric; set => SetField(ref upcomingLyric, value); }
        private string upcomingLyric;

        private IWavePlayer wavePlayer = null;

        private Mp3FileReader mp3FileReader = null;

        private SampleChannel sampleChannel = null;

        private SongItem currentSong = null;

        private int currentLyricIndex = -1;

        public Player(ObservableCollection<SongItem> songs)
        {
            Songs = songs;
            dispatcher = Dispatcher.CurrentDispatcher;
            newSongTimer.Tick += NewSongTimer_Tick;
            updateTimeTimer.Tick += UpdateTimeTimer_Tick;
            this.PropertyChanged += This_PropertyChanged;
            this.PlayPauseCommand = new UniversalCommand((obj) => { IsPlaying ^= true; });
            this.NextCommand = new UniversalCommand((obj) => { Next(); });
        }

        private void This_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Status))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaying)));
            }
            else if (e.PropertyName == nameof(CurrentTime))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTimeDouble)));
            }
        }

        /// <summary>
        /// 定时器 100ms 调用一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTimeTimer_Tick(object sender, EventArgs e)
        {
            if (mp3FileReader != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));

                if (currentSong != null)
                {
                    var index = currentSong.Lyric.GetLyric(CurrentTimeDouble, out string current, out string upcoming);
                    if (index != currentLyricIndex)
                    {
                        currentLyricIndex = index;
                        CurrentLyric = current;
                        UpcomingLyric = upcoming;
                    }
                }
            }
        }

        /// <summary>
        /// 定时器 1s 调用一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewSongTimer_Tick(object sender, EventArgs e)
        {
            if (Songs.Count > 0 && Songs[0].Status == SongStatus.WaitingPlay)
            {
                LoadSong(Songs[0]);
            }
        }

        /// <summary>
        /// 加载歌曲并开始播放
        /// </summary>
        /// <param name="songItem"></param>
        private void LoadSong(SongItem songItem)
        {
            currentSong = songItem;

            currentSong.Status = SongStatus.Playing;

            wavePlayer = CreateIWavePlayer();
            mp3FileReader = new Mp3FileReader(currentSong.FilePath);
            sampleChannel = new SampleChannel(mp3FileReader)
            {
                Volume = Volume
            };

            wavePlayer.PlaybackStopped += (sender, e) => UnloadSong();

            wavePlayer.Init(sampleChannel);
            wavePlayer.Play();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
        }

        /// <summary>
        /// 卸载歌曲并善后
        /// </summary>
        private void UnloadSong()
        {
            try
            {
                wavePlayer.Stop();
                wavePlayer.Dispose();
                mp3FileReader.Close();
                mp3FileReader.Dispose();
                wavePlayer = null;
                sampleChannel = null;
                mp3FileReader = null;
            }
            catch (Exception)
            {
            }

            try
            {
                File.Delete(currentSong.FilePath);
            }
            catch (Exception)
            {
            }

            dispatcher.Invoke(() => Songs.Remove(currentSong));

            currentSong = null;

            CurrentLyric = string.Empty;
            UpcomingLyric = string.Empty;

            // TODO: PlayerBroadcasterLoop 功能

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
        }

        /// <summary>
        /// 根据当前设置初始化 IWavePlayer
        /// </summary>
        /// <returns></returns>
        private IWavePlayer CreateIWavePlayer()
        {
            switch (PlayerType)
            {
                case PlayerType.WaveOutEvent:
                    return new WaveOutEvent() { DeviceNumber = WaveoutEventDevice };
                case PlayerType.DirectSound:
                    return new DirectSoundOut(DirectSoundDevice);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 对外接口 继续
        /// <para>
        /// 注：此接口可在任意线程同步调用
        /// </para>
        /// </summary>
        public void Play()
        {
            if (wavePlayer != null)
            {
                wavePlayer.Play();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }

        }

        /// <summary>
        /// 对外接口 暂停
        /// <para>
        /// 注：此接口可在任意线程同步调用
        /// </para>
        /// </summary>
        public void Pause()
        {
            if (wavePlayer != null)
            {
                wavePlayer.Pause();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }

        }

        /// <summary>
        /// 对外接口 下一首
        /// <para>
        /// 注：此接口可在任意线程同步调用
        /// </para>
        /// </summary>
        public void Next()
        {
            if (wavePlayer != null)
            {
                UnloadSong();
            }
        }

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
