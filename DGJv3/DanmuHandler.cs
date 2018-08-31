using BilibiliDM_PluginFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
            UserLoad();
        }




        /// <summary>
        /// 定位用户
        /// </summary>
        /// <param name="Uid">用户id</param>
        /// <returns></returns>
        public DMJUser FindUser(int Uid)
        {
            if (!HaveUser.Contains(Uid))
                return null;
            foreach (DMJUser usr in users)
            {
                if (usr.Uid == Uid)
                {
                    return usr;
                }
            }
            return null;
        }
        //原本打算写进config里面，看了下感觉不合适，单独立了个存档
        public void UserSave()
        {
            FileInfo SaveFile = new FileInfo(Utilities.ConfigFilePath + @"\userinfo.dgj");
            FileStream fs = SaveFile.Create();
            StringBuilder sb = new StringBuilder();
            foreach (DMJUser usr in users)
            {
                sb.Append(usr.Data() + "\r\n");
            }
            byte[] wwrite = Encoding.UTF8.GetBytes(sb.ToString());
            fs.Write(wwrite, 0, wwrite.Length);
            fs.Close();
        }
        public void UserLoad()
        {
            //清理旧数据//切歌不清理是因为没有必要
            users.Clear();
            HaveUser.Clear();
            FileInfo SaveFile = new FileInfo(Utilities.ConfigFilePath + @"\userinfo.dgj");
            if (!SaveFile.Exists)
            {
                return;
            }
            FileStream fs = SaveFile.OpenRead();
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            string[] readth = Encoding.UTF8.GetString(buffer).Replace("\r", "").Trim('\n').Split('\n');           
            foreach (string rt in readth)
            {
                if (rt == "")
                {
                    continue;
                }
                users.Add(new DMJUser(rt));
                HaveUser.Add(users[users.Count - 1].Uid);
            }
        }

        public List<DMJUser> users = new List<DMJUser>();
        public bool SetIsGiftPlay = false;//用来判断设置是否进行弹幕点歌
        public int SetGiftPlaySpend = 100; //如果通过礼物点歌，点一首个的价格(瓜子
        public int SetTPChangeMax = 20;//当满足这么多切歌人数时，进行切歌/可以自行设置
        public int NowChange = 0;//现在想要切歌的人数//同理这是{切歌当前票数}
        public List<int> HaveUser = new List<int>();//用来快速判断是否有这个用户
        public List<int> QGUser = new List<int>();//用来快速判断这个用户有没有投票切歌

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
                DMJUser usr = FindUser(danmakuModel.UserID);
                if (usr == null)//没有自动创建
                {
                    usr = new DMJUser(danmakuModel.UserID);
                }
                if (!usr.Update)
                {
                    usr.VipLv = 1 + Convert.ToInt32(danmakuModel.isVIP) + (Convert.ToInt32(danmakuModel.isAdmin) + danmakuModel.UserGuardLevel) * 2;
                    usr.Name = danmakuModel.UserName;
                    usr.Update = true;
                }
                //然后添加金钱到用户
                JObject staff = JObject.Parse(danmakuModel.RawData);

                int gifmon = staff["data"]["price"].ToObject<int>();
                usr.Money += gifmon * danmakuModel.GiftCount;//加钱             

                //返回消息
                Log($"感谢{usr.Name}支持的{danmakuModel.GiftName}*{danmakuModel.GiftCount} 获得{gifmon * danmakuModel.GiftCount}点歌币");//这里暂时用log代替输出，后续等@队长改
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
                            DMJUser usr = FindUser(danmakuModel.UserID);
                            if (usr == null)
                            {
                                Log(danmakuModel.UserName + $":请先打赏{SetGiftPlaySpend}金瓜子后开始点歌");
                            }
                            else if (usr.Money > SetGiftPlaySpend)
                            {
                                usr.Money -= (int)(SetGiftPlaySpend * usr.Discount());
                                Log(danmakuModel.UserName + $":点歌成功 花费{(int)(SetGiftPlaySpend * usr.Discount())}点歌币({(int)(usr.Discount() * 10)}折) 剩余{usr.Money}");
                                DanmuAddSong(danmakuModel, rest);
                            }
                            else
                            {
                                Log(danmakuModel.UserName + $":点歌失败 点歌需要{(int)(SetGiftPlaySpend * usr.Discount())}点歌币 而您剩余{usr.Money}点歌币");
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
                        if (QGUser.Contains(danmakuModel.UserID))
                        {
                            Log(danmakuModel.UserName + $":你已经投票过了,请等下一首歌");
                        }
                        if (SetIsGiftPlay)
                        {
                            DMJUser usr = FindUser(danmakuModel.UserID);
                            if (usr == null)
                            {
                                Log(danmakuModel.UserName + $":请先打赏{SetGiftPlaySpend}金瓜子后投票切歌");
                            }
                            else if (usr.Money > SetGiftPlaySpend)
                            {
                                usr.Money -= (int)(SetGiftPlaySpend * usr.Discount() * 0.5);
                                Log(danmakuModel.UserName + $":切歌投票成功 花费{(int)(SetGiftPlaySpend * usr.Discount() * 0.5)}点歌币({(int)(usr.Discount() * 10)}折) 剩余{usr.Money}");
                                NowChange += 1;
                                QGUser.Add(danmakuModel.UserID);
                            }
                            else
                            {
                                Log(danmakuModel.UserName + $":点歌成功 花费{(int)(SetGiftPlaySpend * usr.Discount())}点歌币({(int)(usr.Discount() * 10)}折) 剩余{usr.Money}");
                            }
                        }
                        else
                        {
                            QGUser.Add(danmakuModel.UserID);//记录防止重复投票
                            NowChange += 1;
                        }

                        if (NowChange >= SetTPChangeMax)//如果满足切歌+清空
                        {
                            NowChange = 0;
                            QGUser.Clear();
                            Player.Next();
                        }

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
