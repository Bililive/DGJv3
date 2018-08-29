using System.ComponentModel;

namespace DGJv3
{
    public enum BlackListType
    {
        [Description("歌曲ID")]
        Id,
        [Description("歌曲名字")]
        Name,
        [Description("歌手名字")]
        Singer
    }
}
