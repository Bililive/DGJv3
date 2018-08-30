﻿using BilibiliDM_PluginFramework;
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
        /// 定位用户
        /// </summary>
        /// <param name="Uid">用户id</param>
        /// <returns></returns>
        public ClsUser FindUser(int Uid)
        {
            if (!HaveUser.Contains(Uid))
                return null;
            foreach (ClsUser usr in users)
            {
                if (usr.Uid == Uid)
                {
                    return usr;
                }
            }
            return null;
        }


        public List<ClsUser> users = new List<ClsUser>();
        public bool SetIsGiftPlay = false;//用来判断设置是否进行弹幕点歌
        public int SetGiftPlaySpend = 100; //如果通过礼物点歌，点一首个的价格(瓜子
        public List<int> HaveUser = new List<int>();//用来快速判断是否有这个用户

        /// <summary>
        /// 处理弹幕消息
        /// <para>
        /// 注：调用侧可能会在任意线程
        /// </para>
        /// </summary>
        /// <param name="danmakuModel"></param>
        internal void ProcessDanmu(DanmakuModel danmakuModel)
        {
            //判断是不是礼物
            if (danmakuModel.MsgType == MsgTypeEnum.GiftSend)
            {
                //首先找有没有用户，没有自动创建
                ClsUser usr = FindUser(danmakuModel.UserID);
                if (usr == null)//没有自动创建
                {
                    usr = new ClsUser(danmakuModel.UserID);
                }
                if (!usr.Update)
                {
                    usr.VipLv = 1 + Convert.ToInt32(danmakuModel.isVIP) + (Convert.ToInt32(danmakuModel.isAdmin) + danmakuModel.UserGuardLevel) * 2;
                    usr.Name = danmakuModel.UserName;
                    usr.Update = true;
                }
                //然后添加金钱到用户

                string output = $"感谢{usr.Name}支持的";
                switch (danmakuModel.GiftName)
                {//把基础的支持了，活动就算了每个月都要改//干脆就说限定辣条和金币充值(
                    case "辣条":
                        usr.Money += 10 * danmakuModel.GiftCount;
                        output += $"{danmakuModel.GiftName}*{danmakuModel.GiftCount} 获得{10 * danmakuModel.GiftCount}点歌币";
                        break;
                    case "flag":
                        usr.Money += 200 * danmakuModel.GiftCount;
                        output += $"{danmakuModel.GiftName}*{danmakuModel.GiftCount} 获得{200 * danmakuModel.GiftCount}点歌币";
                        break;
                    case "干杯":
                        usr.Money += 626 * danmakuModel.GiftCount;
                        output += $"{danmakuModel.GiftName}*{danmakuModel.GiftCount} 获得{626 * danmakuModel.GiftCount}点歌币";
                        break;
                    case "金币":
                        usr.Money += 1000 * danmakuModel.GiftCount;
                        output += $"{danmakuModel.GiftName}*{danmakuModel.GiftCount} 获得{1000 * danmakuModel.GiftCount}点歌币";
                        break;
                    case "吃瓜":
                        usr.Money += 100 * danmakuModel.GiftCount;
                        output += $"{danmakuModel.GiftName}*{danmakuModel.GiftCount} 获得{100 * danmakuModel.GiftCount}点歌币";
                        break;
                    default:
                        usr.Money += 1000 * danmakuModel.GiftCount;
                        output += $"{danmakuModel.GiftName}*{danmakuModel.GiftCount} 获得{1000 * danmakuModel.GiftCount}点歌币";
                        break;
                }

                //返回消息
                Log(output);//这里暂时用log代替输出，后续等@队长改
            }

            //判断是不是文本消息
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
                            Player.Next();
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
                        //判断是否开启积分点歌
                        if (SetIsGiftPlay)
                        {
                            ClsUser usr = FindUser(danmakuModel.UserID);
                            if (usr == null)
                            {
                                Log(danmakuModel.UserName + $":请先打赏{SetGiftPlaySpend}金瓜子后开始点歌");
                            }
                            else if (usr.Money > SetGiftPlaySpend)
                            {
                                usr.Money -= (int)(SetGiftPlaySpend * usr.Discount());
                                Log(danmakuModel.UserName + $":点歌成功 花费{(int)(SetGiftPlaySpend * usr.Discount())}点歌币({(int)(usr.Discount()*10)}折) 剩余{usr.Money}");
                                DanmuAddSong(danmakuModel, rest);
                            }
                        }
                        else
                        {
                            DanmuAddSong(danmakuModel, rest);
                        }
                    }
                    return;
                case "取消點歌":
                case "取消点歌":
                    {
                        dispatcher.Invoke(() =>
                        {
                            SongItem songItem = Songs.Last(x => x.UserName == danmakuModel.UserName && x.Status != SongStatus.Playing);
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
                    Log($"歌曲 {songInfo.Name} 在黑名单中");
                    return;
                }

                dispatcher.Invoke(callback: () =>
                {
                    if (CanAddSong(danmakuModel.UserName))
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
