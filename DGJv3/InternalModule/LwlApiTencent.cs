namespace DGJv3.InternalModule
{
    sealed class LwlApiTencent : LwlApiBaseModule
    {
        internal LwlApiTencent()
        {
            SetServiceName("tencent");
            SetInfo("QQ音乐", INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, "搜索QQ音乐的歌曲");
        }
    }
}
