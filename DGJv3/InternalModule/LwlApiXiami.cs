namespace DGJv3.InternalModule
{
    sealed class LwlApiXiami : LwlApiBaseModule
    {
        internal LwlApiXiami()
        {
            SetServiceName("xiami");
            SetInfo("虾米音乐", INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, "搜索虾米音乐的歌曲");
        }
    }
}
