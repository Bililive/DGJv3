namespace DGJv3.InternalModule
{
    sealed class LwlApiNetease : LwlApiBaseModule
    {
        internal LwlApiNetease()
        {
            SetServiceName("netease");
            SetInfo("网易云音乐", INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, "搜索网易云音乐的歌曲");
        }
    }
}
