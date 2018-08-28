using System.Collections.Generic;

namespace DGJv3
{
    /// <summary>
    /// 空搜索模块
    /// </summary>
    sealed class NullSearchModule : SearchModule
    {
        public NullSearchModule() => SetInfo("不使用", string.Empty, string.Empty, string.Empty, string.Empty);

        protected override DownloadStatus Download(SongItem item)
        {
            return DownloadStatus.Failed;
        }

        protected override string GetDownloadUrl(SongItem songInfo)
        {
            return null;
        }

        protected override List<SongInfo> GetPlaylist(string keyword)
        {
            return null;
        }

        protected override SongInfo Search(string keyword)
        {
            return null;
        }
    }
}
