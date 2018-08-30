using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGJv3
{
    [JsonArray(allowNullItems: false)]
    class Config
    {
        [JsonProperty("ptyp")]
        public PlayerType PlayerType { get; set; }

        [JsonProperty("pdsd")]
        public Guid DirectSoundDevice { get; set; }

        [JsonProperty("pwed")]
        public int WaveoutEventDevice { get; set; }

        [JsonProperty("pvol")]
        public float Volume { get; set; }

        [JsonProperty("pple")]
        public bool IsPlaylistEnabled { get; set; }

        [JsonProperty("mpid")]
        public string PrimaryModuleId { get; set; }

        [JsonProperty("msid")]
        public string SecondaryModuleId { get; set; }

        [JsonProperty("dmts")]
        public uint MaxTotalSongNum { get; set; }

        [JsonProperty("dmps")]
        public uint MaxPersonSongNum { get; set; }

        [JsonProperty("plst")]
        public SongInfo[] Playlist { get; set; } = new SongInfo[0];

        [JsonProperty("blst")]
        public BlackListItem[] Blacklist { get; set; } = new BlackListItem[0];

        public Config()
        {
        }

        internal static Config Load(bool reset = false)
        {
            Config config = new Config();
            if (!reset)
            {
                try
                {
                    var str = File.ReadAllText(Utilities.ConfigFilePath, Encoding.UTF8);
                    config = JsonConvert.DeserializeObject<Config>(str);
                }
                catch (Exception)
                { }
            }
            return config;
        }

        internal static void Write(Config config)
        {
            try
            {
                File.WriteAllText(Utilities.ConfigFilePath, JsonConvert.SerializeObject(config), Encoding.UTF8);
            }
            catch (Exception)
            {
            }
        }
    }
}
