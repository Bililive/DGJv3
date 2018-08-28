namespace DGJv3.InternalModule
{
    sealed class LwlApiKugou : LwlApiBaseModule
    {
        internal LwlApiKugou()
        {
            SetServiceName("kugou");
            SetInfo("酷狗音乐", INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, "搜索酷狗音乐的歌曲");
        }
    }
}
