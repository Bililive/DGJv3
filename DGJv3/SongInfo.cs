namespace DGJv3
{
    public class SongInfo
    {
        public SearchModule Module;

        public string Id { get; set; }
        public string Name { get; set; }
        public string[] Singers { get; set; }
        public string SingersText { get => string.Join(";", Singers); }
        public string Lyric { get; set; }
        public string Note { get; set; }

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
