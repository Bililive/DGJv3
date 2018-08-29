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

        public Player Player { get; set; }

        public Downloader Downloader { get; set; }

        public Writer Writer { get; set; }

        public SearchModules SearchModules { get; set; }

        public DanmuHandler DanmuHandler { get; set; }

        public UniversalCommand RemoveSongCommmand { get; set; }

        public UniversalCommand RemoveAndBlacklistSongCommand { get; set; }

        public DGJWindow(DGJMain dGJMain)
        {
            DataContext = this;
            PluginMain = dGJMain;
            Songs = new ObservableCollection<SongItem>();
            Playlist = new ObservableCollection<SongInfo>();

            Player = new Player(Songs);
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

            InitializeComponent();

            ApplyConfig(Config.Load());

            PluginMain.ReceivedDanmaku += (sender, e) => { DanmuHandler.ProcessDanmu(e.Danmaku); };

            #region PackIcon 问题 workaround

            PackIconPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            PackIconPlay.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            PackIconVolumeHigh.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
            PackIconSkipNext.Kind = MaterialDesignThemes.Wpf.PackIconKind.SkipNext;
            PackIconSettings.Kind = MaterialDesignThemes.Wpf.PackIconKind.Settings;

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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
