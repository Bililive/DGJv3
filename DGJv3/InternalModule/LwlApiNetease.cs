namespace DGJv3.InternalModule
{
    sealed class LwlApiNetease : LwlApiBaseModule
    {
        internal LwlApiNetease()
        {
            SetServiceName("netease");
            SetInfo("网易音乐", INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, "搜索网易音乐的歌曲");
        }
    }
}
