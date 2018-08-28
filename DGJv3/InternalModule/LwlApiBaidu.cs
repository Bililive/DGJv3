namespace DGJv3.InternalModule
{
    sealed class LwlApiBaidu : LwlApiBaseModule
    {
        internal LwlApiBaidu()
        {
            SetServiceName("baidu");
            SetInfo("百度音乐", INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, "搜索百度音乐的歌曲");
        }
    }
}
