using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DGJv3.InternalModule
{
    sealed class LwlApiNetease : LwlApiBaseModule
    {
        private const string API_PROTOCOL = "http://";
        // private const string API_HOST = "v1.itooi.cn";
        private const string API_HOST = "music.163.com";
        private const string API_PATH = "/api";

        internal LwlApiNetease()
        {
            SetServiceName("netease");
            SetInfo("网易云音乐", INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, "搜索网易云音乐的歌曲");
        }

        protected override string GetDownloadUrl(SongItem songInfo)
        {
            try
            {
                // api.Request(CloudMusicApiProviders.SongUrl,
                //     new Dictionary<string, string> { { "id", songInfo.SongId } }, out var response);
                return $"https://music.163.com/song/media/outer/url?id={songInfo.SongId}.mp3";
            }
            catch (Exception ex)
            {
                Log($"歌曲 {songInfo.SongName} 疑似版权不能下载(ex:{ex.Message})");
                return null;
            }
        }

        protected override string GetLyricById(string Id)
        {
            try
            {
                var response = Fetch(API_PROTOCOL, API_HOST,
                    API_PATH +
                    $"/song/lyric?id={Id}&lv=1&kv=1&tv=-1");
                var json = JObject.Parse(response);

                return json["lrc"]["lyric"].ToString();
            }
            catch (Exception ex)
            {
                Log($"歌曲 {Id} 歌词下载错误(ex:{ex.Message})");
                return null;
            }
        }
        
        protected override List<SongInfo> GetPlaylist(string keyword)
        {
            var Id = 0;
            try
            {
                var response = Fetch(API_PROTOCOL, API_HOST,
                    API_PATH +
                    $"/search/get/web?csrf_token=hlpretag=&hlposttag=&s={keyword}&type=1000&offset=0&total=true&limit=3");
                var json = JObject.Parse(response);
                var playlist = (json["result"]["playlists"] as JArray)?[0] as JObject;
                Id = playlist.Value<int>("id");
            }
            catch (Exception ex)
            {
                Log($"歌单下载错误(ex:{ex.Message})");
                return null;
            }

            try
            {
                var response = Fetch(API_PROTOCOL, API_HOST,
                    API_PATH +
                    $"/search/get/web?csrf_token=hlpretag=&hlposttag=&s={keyword}&type=1&offset=0&total=true&limit=3");
                var json = JObject.Parse(response);
                return (json["result"]["tracks"] as JArray)?.Select(song =>
                {
                    SongInfo songInfo;

                    try
                    {
                        songInfo = new SongInfo(
                            this,
                            song["id"].ToString(),
                            song["name"].ToString(),
                            (song["artists"] as JArray).Select(x => x["name"].ToString()).ToArray()
                        );
                    }
                    catch (Exception ex)
                    { Log("歌曲信息获取结果错误：" + ex.Message); return null; }

                    songInfo.Lyric = GetLyricById(songInfo.Id);
                    return songInfo;
                }).ToList();

            }
            catch (Exception ex)
            {
                Log($"歌单下载错误(ex:{ex.Message})");
                return null;
            }
        }

        protected override SongInfo Search(string keyword)
        {
            try
            {
                var response = Fetch(API_PROTOCOL, API_HOST,
                    API_PATH +
                    $"/search/get/web?csrf_token=hlpretag=&hlposttag=&s={keyword}&type=1&offset=0&total=true&limit=3");
                var json = JObject.Parse(response);
                var song = (json["result"]["songs"] as JArray)?[0] as JObject;

                SongInfo songInfo;

                try
                {
                    songInfo = new SongInfo(
                        this,
                        song["id"].ToString(),
                        song["name"].ToString(),
                        (song["artists"] as JArray).Select(x => x["name"].ToString()).ToArray()
                    );
                }
                catch (Exception ex)
                { Log("歌曲信息获取结果错误：" + ex.Message); return null; }

                songInfo.Lyric = GetLyricById(songInfo.Id);

                return songInfo;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return null;
            }
        }

    }
}
