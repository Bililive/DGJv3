using BilibiliDM_PluginFramework;
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
    class DanmuHandler : INotifyPropertyChanged
    {
        private ObservableCollection<SongItem> Songs;

        private ObservableCollection<BlackListItem> Blacklist;

        private Player Player;

        private Downloader Downloader;

        private SearchModules SearchModules;

        private Dispatcher dispatcher;

        /// <summary>
        /// 最多点歌数量
        /// </summary>
        public uint MaxTotalSongNum { get => _maxTotalSongCount; set => SetField(ref _maxTotalSongCount, value); }
        private uint _maxTotalSongCount;

        /// <summary>
        /// 每个人最多点歌数量
        /// </summary>
        public uint MaxPersonSongNum { get => _maxPersonSongNum; set => SetField(ref _maxPersonSongNum, value); }
        private uint _maxPersonSongNum;

        internal DanmuHandler(ObservableCollection<SongItem> songs, Player player, Downloader downloader, SearchModules searchModules, ObservableCollection<BlackListItem> blacklist)
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            Songs = songs;
            Player = player;
            Downloader = downloader;
            SearchModules = searchModules;
            Blacklist = blacklist;
        }


        /// <summary>
        /// 处理弹幕消息
        /// <para>
        /// 注：调用侧可能会在任意线程
        /// </para>
        /// </summary>
        /// <param name="danmakuModel"></param>
        internal void ProcessDanmu(DanmakuModel danmakuModel)
        {
            if (danmakuModel.MsgType != MsgTypeEnum.Comment || string.IsNullOrWhiteSpace(danmakuModel.CommentText))
                return;

            string[] commands = danmakuModel.CommentText.Split(SPLIT_CHAR, StringSplitOptions.RemoveEmptyEntries);
            string rest = string.Join(" ", commands.Skip(1));

            if (danmakuModel.isAdmin)
            {
                switch (commands[0])
                {
                    case "切歌":
                        {
                            // Player.Next();

                            dispatcher.Invoke(() =>
                            {
                                if (Songs.Count > 0)
                                {
                                    Songs[0].Remove(Songs, Downloader, Player);
                                    Log("切歌成功！");
                                }
                            });

                            /*
                            if (commands.Length >= 2)
                            {
                                // TODO: 切指定序号的歌曲
                            }
                            */
                        }
                        return;
                    case "暂停":
                    case "暫停":
                        {
                            Player.Pause();
                        }
                        return;
                    case "播放":
                        {
                            Player.Play();
                        }
                        return;
                    case "音量":
                        {
                            if (commands.Length > 1
                                && int.TryParse(commands[1], out int volume100)
                                && volume100 >= 0
                                && volume100 <= 100)
                            {
                                Player.Volume = volume100 / 100f;
                            }
                        }
                        return;
                    default:
                        break;
                }
            }

            switch (commands[0])
            {
                case "点歌":
                case "點歌":
                    {
                        DanmuAddSong(danmakuModel, rest);
                    }
                    return;
                case "取消點歌":
                case "取消点歌":
                    {
                        dispatcher.Invoke(() =>
                        {
                            SongItem songItem = Songs.LastOrDefault(x => x.UserName == danmakuModel.UserName && x.Status != SongStatus.Playing);
                            if (songItem != null)
                            {
                                songItem.Remove(Songs, Downloader, Player);
                            }
                        });
                    }
                    return;
                case "投票切歌":
                    {
                        // TODO: 投票切歌
                    }
                    return;
                default:
                    break;
            }
        }

        private void DanmuAddSong(DanmakuModel danmakuModel, string keyword)
        {
            if (dispatcher.Invoke(callback: () => CanAddSong(username: danmakuModel.UserName)))
            {
                SongInfo songInfo = null;

                if (SearchModules.PrimaryModule != SearchModules.NullModule)
                    songInfo = SearchModules.PrimaryModule.SafeSearch(keyword);

                if (songInfo == null)
                    if (SearchModules.SecondaryModule != SearchModules.NullModule)
                        songInfo = SearchModules.SecondaryModule.SafeSearch(keyword);

                if (songInfo == null)
                    return;

                if (songInfo.IsInBlacklist(Blacklist))
                {
                    Log($"歌曲{songInfo.Name}在黑名单中");
                    return;
                }
                Log($"点歌成功:{songInfo.Name}");
                dispatcher.Invoke(callback: () =>
                {
                    if (CanAddSong(danmakuModel.UserName) &&
                        !Songs.Any(x =>
                            x.SongId == songInfo.Id &&
                            x.Module.UniqueId == songInfo.Module.UniqueId)
                    )
                        Songs.Add(new SongItem(songInfo, danmakuModel.UserName));
                });
            }
        }

        /// <summary>
        /// 能否点歌
        /// <para>
        /// 注：调用侧需要在主线程上运行
        /// </para>
        /// </summary>
        /// <param name="username">点歌用户名</param>
        /// <returns></returns>
        private bool CanAddSong(string username)
        {
            return Songs.Count < MaxTotalSongNum ? (Songs.Where(x => x.UserName == username).Count() < MaxPersonSongNum) : false;
        }

        private readonly static char[] SPLIT_CHAR = { ' ' };

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public event LogEvent LogEvent;
        private void Log(string message, Exception exception = null) => LogEvent?.Invoke(this, new LogEventArgs() { Message = message, Exception = exception });
    }
}
