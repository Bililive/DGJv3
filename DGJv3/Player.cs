using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;

namespace DGJv3
{
    class Player : INotifyPropertyChanged
    {
        // TODO: 歌词

        private ObservableCollection<SongItem> Songs;

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

        public PlayerType PlayerType { get; set; }

        public Guid DirectSoundDevice { get; set; }

        public int WaveoutEventDevice { get; set; }

        public TimeSpan CurrentTime
        {
            get => mp3FileReader == null ? TimeSpan.Zero : mp3FileReader.CurrentTime;
            set { if (mp3FileReader != null) mp3FileReader.CurrentTime = value; }
        }

        public TimeSpan TotalTime { get => mp3FileReader == null ? TimeSpan.Zero : mp3FileReader.TotalTime; }

        public bool IsPlaying { get => Status == PlayerStatus.Playing; }

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

        private IWavePlayer wavePlayer = null;

        private Mp3FileReader mp3FileReader = null;

        private SampleChannel sampleChannel = null;

        private SongItem currentSong = null;

        public Player(ObservableCollection<SongItem> songs)
        {
            Songs = songs;
            newSongTimer.Tick += NewSongTimer_Tick;
            updateTimeTimer.Tick += UpdateTimeTimer_Tick;
            this.PropertyChanged += This_PropertyChanged;
        }

        private void This_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Status))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaying)));
            }
        }

        private void UpdateTimeTimer_Tick(object sender, EventArgs e)
        {
            if (mp3FileReader != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
            }
        }

        private void NewSongTimer_Tick(object sender, EventArgs e)
        {
            if (Songs.Count > 0 && Songs[0].Status == SongStatus.WaitingPlay)
            {
                LoadSong(Songs[0]);
            }
        }

        private void LoadSong(SongItem songItem)
        {
            currentSong = songItem;

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

        private void UnloadSong()
        {
            try
            {
                wavePlayer.Dispose();
                wavePlayer = null;
                mp3FileReader.Dispose();
                mp3FileReader = null;
                sampleChannel = null;
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

            Songs.Remove(currentSong);

            currentSong = null;

            // TODO: PlayerBroadcasterLoop 功能

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
        }

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

        public void Play()
        {
            if (wavePlayer != null)
            {
                wavePlayer.Play();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public void Pause()
        {
            if (wavePlayer != null)
            {
                wavePlayer.Play();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public void Next()
        {
            if (wavePlayer != null)
            {
                UnloadSong();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
