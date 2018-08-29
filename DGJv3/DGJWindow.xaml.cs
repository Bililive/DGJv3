using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DGJv3
{
    /// <summary>
    /// DGJWindow.xaml 的交互逻辑
    /// </summary>
    internal partial class DGJWindow : Window
    {
        public DGJMain PluginMain { get; set; }

        public ObservableCollection<SongItem> Songs { get; set; }

        public ObservableCollection<SongInfo> Playlist { get; set; }

        public ObservableCollection<BlackListItem> Blacklist { get; set; }

        public Player Player { get; set; }

        public Downloader Downloader { get; set; }

        public Writer Writer { get; set; }

        public SearchModules SearchModules { get; set; }

        public DanmuHandler DanmuHandler { get; set; }

        public UniversalCommand RemoveSongCommmand { get; set; }

        public UniversalCommand RemoveAndBlacklistSongCommand { get; set; }

        public UniversalCommand RemovePlaylistInfoCommmand { get; set; }

        public UniversalCommand ClearPlaylistCommand { get; set; }

        public UniversalCommand RemoveBlacklistInfoCommmand { get; set; }

        public UniversalCommand ClearBlacklistCommand { get; set; }

        public DGJWindow(DGJMain dGJMain)
        {
            DataContext = this;
            PluginMain = dGJMain;
            Songs = new ObservableCollection<SongItem>();
            Playlist = new ObservableCollection<SongInfo>();
            Blacklist = new ObservableCollection<BlackListItem>();

            Player = new Player(Songs, Playlist);
            Downloader = new Downloader(Songs);
            SearchModules = new SearchModules();
            DanmuHandler = new DanmuHandler(Songs, Player, Downloader, SearchModules);

            Player.LogEvent += (sender, e) => { PluginMain.Log("播放 " + e.Message + (e.Exception == null ? string.Empty : e.Exception.Message)); };
            Downloader.LogEvent += (sender, e) => { PluginMain.Log("下载 " + e.Message + (e.Exception == null ? string.Empty : e.Exception.Message)); };
            //Writer.Logevent += (sender, e) => { PluginMain.Log("文本输出 " + e.Message + (e.Exception == null ? string.Empty : e.Exception.Message)); };
            SearchModules.LogEvent += (sender, e) => { PluginMain.Log("搜索模块 " + e.Message + (e.Exception == null ? string.Empty : e.Exception.Message)); };
            DanmuHandler.LogEvent += (sender, e) => { PluginMain.Log("弹幕 " + e.Message + (e.Exception == null ? string.Empty : e.Exception.Message)); };

            RemoveSongCommmand = new UniversalCommand((songobj) =>
            {
                if (songobj != null && songobj is SongItem songItem)
                {
                    songItem.Remove(Songs, Downloader, Player);
                }
            });

            RemoveAndBlacklistSongCommand = new UniversalCommand((songobj) =>
            {
                if (songobj != null && songobj is SongItem songItem)
                {
                    songItem.Remove(Songs, Downloader, Player);
                    // TODO: 添加黑名单
                }
            });

            RemovePlaylistInfoCommmand = new UniversalCommand((songobj) =>
            {
                if (songobj != null && songobj is SongInfo songInfo)
                {
                    Playlist.Remove(songInfo);
                }
            });

            ClearPlaylistCommand = new UniversalCommand((e) =>
            {
                Playlist.Clear();
            });

            RemoveBlacklistInfoCommmand = new UniversalCommand((blackobj) =>
            {
                if (blackobj != null && blackobj is BlackListItem blackListItem)
                {
                    Blacklist.Remove(blackListItem);
                }
            });

            ClearBlacklistCommand = new UniversalCommand((x) =>
            {
                Blacklist.Clear();
            });

            InitializeComponent();

            ApplyConfig(Config.Load());

            PluginMain.ReceivedDanmaku += (sender, e) => { DanmuHandler.ProcessDanmu(e.Danmaku); };

            #region PackIcon 问题 workaround

            PackIconPause.Kind = PackIconKind.Pause;
            PackIconPlay.Kind = PackIconKind.Play;
            PackIconVolumeHigh.Kind = PackIconKind.VolumeHigh;
            PackIconSkipNext.Kind = PackIconKind.SkipNext;
            PackIconSettings.Kind = PackIconKind.Settings;
            PackIconFilterRemove.Kind = PackIconKind.FilterRemove;

            #endregion

        }

        private void ApplyConfig(Config config)
        {
            Player.PlayerType = config.PlayerType;
            Player.DirectSoundDevice = config.DirectSoundDevice;
            Player.WaveoutEventDevice = config.WaveoutEventDevice;
            Player.Volume = config.Volume;
            SearchModules.PrimaryModule = SearchModules.Modules.FirstOrDefault(x => x.UniqueId == config.PrimaryModuleId) ?? SearchModules.NullModule;
            SearchModules.SecondaryModule = SearchModules.Modules.FirstOrDefault(x => x.UniqueId == config.SecondaryModuleId) ?? SearchModules.NullModule;
            DanmuHandler.MaxTotalSongNum = config.MaxTotalSongNum;
            DanmuHandler.MaxPersonSongNum = config.MaxPersonSongNum;
        }

        private Config GatherConfig() => new Config()
        {
            PlayerType = Player.PlayerType,
            DirectSoundDevice = Player.DirectSoundDevice,
            WaveoutEventDevice = Player.WaveoutEventDevice,
            Volume = Player.Volume,
            PrimaryModuleId = SearchModules.PrimaryModule.UniqueId,
            SecondaryModuleId = SearchModules.SecondaryModule.UniqueId,
            MaxPersonSongNum = DanmuHandler.MaxPersonSongNum,
            MaxTotalSongNum = DanmuHandler.MaxTotalSongNum,
        };

        internal void DeInit()
        {
            Config.Write(GatherConfig());
            try
            {
                Directory.Delete(Utilities.SongsCacheDirectoryPath, true);
            }
            catch (Exception)
            {
            }
        }

        private void DialogAddSongs(object sender, DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter.Equals(true) && !string.IsNullOrWhiteSpace(AddSongsTextBox.Text))
            {
                var keyword = AddSongsTextBox.Text;
                SongInfo songInfo = null;

                if (SearchModules.PrimaryModule != SearchModules.NullModule)
                    songInfo = SearchModules.PrimaryModule.SafeSearch(keyword);

                if (songInfo == null)
                    if (SearchModules.SecondaryModule != SearchModules.NullModule)
                        songInfo = SearchModules.SecondaryModule.SafeSearch(keyword);

                if (songInfo == null)
                    return;

                Songs.Add(new SongItem(songInfo, "主播"));
            }
            AddSongsTextBox.Text = string.Empty;
        }

        private void DialogAddSongsToPlaylist(object sender, DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter.Equals(true) && !string.IsNullOrWhiteSpace(AddSongPlaylistTextBox.Text))
            {
                var keyword = AddSongPlaylistTextBox.Text;
                SongInfo songInfo = null;

                if (SearchModules.PrimaryModule != SearchModules.NullModule)
                    songInfo = SearchModules.PrimaryModule.SafeSearch(keyword);

                if (songInfo == null)
                    if (SearchModules.SecondaryModule != SearchModules.NullModule)
                        songInfo = SearchModules.SecondaryModule.SafeSearch(keyword);

                if (songInfo == null)
                    return;

                Playlist.Add(songInfo);
            }
            AddSongPlaylistTextBox.Text = string.Empty;
        }

        private void DialogAddPlaylist(object sender, DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter.Equals(true) && !string.IsNullOrWhiteSpace(AddPlaylistTextBox.Text))
            {
                var keyword = AddPlaylistTextBox.Text;
                List<SongInfo> songInfoList = null;

                if (SearchModules.PrimaryModule != SearchModules.NullModule && SearchModules.PrimaryModule.IsPlaylistSupported)
                    songInfoList = SearchModules.PrimaryModule.SafeGetPlaylist(keyword);

                if (songInfoList == null)
                    return;

                foreach (var item in songInfoList)
                    Playlist.Add(item);
            }
            AddPlaylistTextBox.Text = string.Empty;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
