using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGJv3
{
    class Config
    {
        [JsonProperty("ptyp")]
        public PlayerType PlayerType { get; set; } = PlayerType.DirectSound;

        [JsonProperty("pdsd")]
        public Guid DirectSoundDevice { get; set; } = Guid.Empty;

        [JsonProperty("pwed")]
        public int WaveoutEventDevice { get; set; } = -1;

        [JsonProperty("pvol")]
        public float Volume { get; set; } = 0.5f;

        [JsonProperty("pple")]
        public bool IsPlaylistEnabled { get; set; } = true;

        [JsonProperty("mpid")]
        public string PrimaryModuleId { get; set; }

        [JsonProperty("msid")]
        public string SecondaryModuleId { get; set; }

        [JsonProperty("dmts")]
        public uint MaxTotalSongNum { get; set; } = 10;

        [JsonProperty("dmps")]
        public uint MaxPersonSongNum { get; set; } = 2;

        [JsonProperty("up")]
        public bool IsUserPrior { get; set; } = true;

        [JsonProperty("lrd")]
        public bool IsLogRedirectDanmaku { get; set; } = false;

        [JsonProperty("ldll")]
        public int LogDanmakuLengthLimit { get; set; } = 20;

        [JsonProperty("plst")]
        public SongInfo[] Playlist { get; set; } = new SongInfo[0];

        [JsonProperty("blst")]
        public BlackListItem[] Blacklist { get; set; } = new BlackListItem[0];

        [JsonProperty("sbtp")]
        public string ScribanTemplate { get; set; } = "播放进度 {{当前播放时间}}/{{当前总时间}}\n" +
            "当前列表中有 {{ 歌曲数量 }} 首歌\n还可以再点 {{ 总共最大点歌数量 - 歌曲数量 }} 首歌\n" +
            "每个人可以点 {{ 单人最大点歌数量 }} 首歌\n\n歌名 - 点歌人 - 歌手 - 歌曲平台\n" +
            "{{~ for 歌曲 in 播放列表 ~}}\n" +
            "{{ 歌曲.歌名 }} - {{  歌曲.点歌人 }} - {{ 歌曲.歌手 }} - {{ 歌曲.搜索模块 }}\n" +
            "{{~ end ~}}";

        public Config()
        {
        }

#pragma warning disable CS0168 // 声明了变量，但从未使用过
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

                catch (Exception ex)
                {
                }
            }
            return config;
        }

        internal static void Write(Config config)
        {
            try
            {
                File.WriteAllText(Utilities.ConfigFilePath, JsonConvert.SerializeObject(config), Encoding.UTF8);
            }
            catch (Exception ex)
            {
            }
        }
#pragma warning restore CS0168 // 声明了变量，但从未使用过
    }
}
