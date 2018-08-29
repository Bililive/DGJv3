using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DGJv3
{
    public class SongItem : INotifyPropertyChanged
    {

        internal SongItem(SongInfo songInfo, string userName)
        {
            Status = SongStatus.WaitingDownload;

            UserName = userName;

            Module = songInfo.Module;
            SongId = songInfo.Id;
            SongName = songInfo.Name;
            Singers = songInfo.Singers;
            Lyric = Lrc.InitLrc(songInfo.Lyric);
            Note = songInfo.Note;

        }

        /// <summary>
        /// 搜索模块名称
        /// </summary>
        public string ModuleName
        { get { return Module.ModuleName; } }

        /// <summary>
        /// 搜索模块
        /// </summary>
        internal SearchModule Module
        { get; set; }

        /// <summary>
        /// 歌曲ID
        /// </summary>
        public string SongId
        { get; internal set; }

        /// <summary>
        /// 歌名
        /// </summary>
        public string SongName
        { get; internal set; }

        /// <summary>
        /// string的歌手列表
        /// </summary>
        public string SingersText
        {
            get
            {
                string output = "";
                foreach (string str in Singers)
                    output += str + ";";
                return output;
            }
        }

        /// <summary>
        /// 歌手列表
        /// </summary>
        public string[] Singers
        { get; internal set; }

        /// <summary>
        /// 点歌人
        /// </summary>
        public string UserName
        { get; internal set; }

        // /// <summary>
        /// 下载地址
        /// </summary>
        // public string DownloadURL
        // { get; internal set; }

        /// <summary>
        /// 歌曲文件储存路径
        /// </summary>
        public string FilePath
        { get; internal set; }

        /// <summary>
        /// 文本歌词
        /// </summary>
        public Lrc Lyric
        { get; internal set; }

        /// <summary>
        /// 歌曲备注
        /// </summary>
        public string Note
        { get; internal set; }

        /// <summary>
        /// 歌曲状态
        /// </summary>
        public SongStatus Status
        { get => _status; internal set => SetField(ref _status, value); }

        private SongStatus _status;

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