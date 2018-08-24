namespace DGJv3
{
    public class SongInfo
    {
        public SearchModule Module;

        public string Id;
        public string Name;
        public string[] Singers;
        public string Lyric;
        public string Note;

        public SongInfo(SearchModule module) : this(module, string.Empty, string.Empty, null) { }
        public SongInfo(SearchModule module, string id, string name, string[] singers) : this(module, id, name, singers, string.Empty) { }
        public SongInfo(SearchModule module, string id, string name, string[] singers, string lyric) : this(module, id, name, singers, lyric, string.Empty) { }
        public SongInfo(SearchModule module, string id, string name, string[] singers, string lyric, string note)
        {
            Module = module;

            Id = id;
            Name = name;
            Singers = singers;
            Lyric = lyric;
            Note = note;
        }
    }
}
