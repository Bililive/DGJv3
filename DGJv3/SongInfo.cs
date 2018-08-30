using Newtonsoft.Json;

namespace DGJv3
{
    public class SongInfo
    {
        [JsonIgnore]
        public SearchModule Module;

        [JsonProperty("smid")]
        public string ModuleId { get; set; }

        [JsonProperty("siid")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("sing")]
        public string[] Singers { get; set; }
        [JsonIgnore]
        public string SingersText { get => string.Join(";", Singers); }
        [JsonProperty("lrc")]
        public string Lyric { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonConstructor]
        private SongInfo() { }

        public SongInfo(SearchModule module) : this(module, string.Empty, string.Empty, null) { }
        public SongInfo(SearchModule module, string id, string name, string[] singers) : this(module, id, name, singers, string.Empty) { }
        public SongInfo(SearchModule module, string id, string name, string[] singers, string lyric) : this(module, id, name, singers, lyric, string.Empty) { }
        public SongInfo(SearchModule module, string id, string name, string[] singers, string lyric, string note)
        {
            Module = module;

            ModuleId = Module.UniqueId;

            Id = id;
            Name = name;
            Singers = singers;
            Lyric = lyric;
            Note = note;
        }
    }
}
