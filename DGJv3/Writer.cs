using Scriban;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;

namespace DGJv3
{
    class Writer : INotifyPropertyChanged
    {
        private ObservableCollection<SongItem> Songs;

        private ObservableCollection<SongInfo> Playlist;

        private Player Player;

        private DanmuHandler DanmuHandler;

        private Timer timer;

        private Template template = null;

        public string ScribanTemplate { get => scribanTemplate; set => SetField(ref scribanTemplate, value); }
        public string Result { get => result; set => SetField(ref result, value); }

        private string scribanTemplate;
        private string result;

        internal Writer(ObservableCollection<SongItem> songs, ObservableCollection<SongInfo> playlist, Player player, DanmuHandler danmuHandler)
        {
            Songs = songs;
            Playlist = playlist;
            Player = player;
            DanmuHandler = danmuHandler;

            PropertyChanged += Writer_PropertyChanged;

            Player.LyricEvent += Player_LyricEvent;

            timer = new Timer(1000)
            {
                AutoReset = true
            };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Player_LyricEvent(object sender, LyricChangedEventArgs e)
        {
            try
            {
                File.WriteAllText(Utilities.LyricOutputFilePath, e.CurrentLyric + Environment.NewLine + e.UpcomingLyric);
            }
            catch (Exception) { }
        }

        private void Writer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ScribanTemplate))
            {
                var localtemplate = Template.Parse(ScribanTemplate);
                if (localtemplate.HasErrors)
                {
                    Result = "模板有语法错误" + Environment.NewLine + string.Join(Environment.NewLine, localtemplate.Messages);
                    try
                    {
                        File.WriteAllText(Utilities.ScribanOutputFilePath, Result);
                    }
                    catch (Exception) { }
                }
                else
                {
                    template = localtemplate;
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var localsongs = Songs.Select(x => new
            {
                歌名 = x.SongName,
                歌手 = x.SingersText,
                歌曲id = x.SongId,
                点歌人 = x.UserName,
                状态 = x.Status.ToStatusString(),
                搜索模块 = x.ModuleName,
            });
            var localplaylist = Playlist.Select(x => new
            {
                歌名 = x.Name,
                歌手 = x.SingersText,
                歌曲id = x.Id,
                搜索模块 = x.Module.ModuleName,
            });
            var localresult = template?.Render(new
            {
                播放列表 = localsongs,
                空闲歌单 = localplaylist,
                歌曲数量 = Songs.Count,
                当前播放时间 = Player.CurrentTimeString,
                当前总时间 = Player.TotalTimeString,
                总共最大点歌数量 = DanmuHandler.MaxTotalSongNum,
                单人最大点歌数量 = DanmuHandler.MaxPersonSongNum,
            }) ?? string.Empty;

            if (localresult != string.Empty)
            {
                Result = localresult;

                try
                {
                    File.WriteAllText(Utilities.ScribanOutputFilePath, Result);
                }
                catch (Exception) { }
            }
            else
            {
                if (template?.HasErrors == true)
                {
                    try
                    {
                        Result = "模板有错误" + Environment.NewLine + string.Join(Environment.NewLine, template.Messages);
                        File.WriteAllText(Utilities.ScribanOutputFilePath, Result);
                    }
                    catch (Exception) { }
                }
            }
        }

        public event LogEvent LogEvent;
        private void Log(string message, Exception exception = null) => LogEvent?.Invoke(this, new LogEventArgs() { Message = message, Exception = exception });

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
