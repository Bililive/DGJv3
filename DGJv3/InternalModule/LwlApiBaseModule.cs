﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace DGJv3.InternalModule
{
    internal class LwlApiBaseModule : SearchModule
    {
        private string ServiceName = "undefined";
        protected void SetServiceName(string name) => ServiceName = name;

        private const string API_PROTOCOL = "https://";
        private const string API_HOST = "v1.itooi.cn";
        private const string API_PATH = "/";

        protected const string INFO_PREFIX = "";
        protected const string INFO_AUTHOR = "Genteure & LWL12";
        protected const string INFO_EMAIL = "dgj@genteure.com";
        protected const string INFO_VERSION = "1.1";

        internal static int RoomId = 0;

        internal LwlApiBaseModule()
        {
            IsPlaylistSupported = true;
        }

        protected override DownloadStatus Download(SongItem item)
        {
            throw new NotImplementedException();
        }

        protected override string GetDownloadUrl(SongItem songInfo)
        {
            try
            {
                JObject dlurlobj = JObject.Parse(Fetch(API_PROTOCOL, API_HOST, API_PATH + ServiceName + $"/song?id={songInfo.SongId}"));

                if (dlurlobj["code"].ToString() == "200")
                {
                    return $"{API_PROTOCOL}{API_HOST}{API_PATH}{ServiceName}/url?id={songInfo.SongId}";
                }
                else
                {
                    Log($"歌曲 {songInfo.SongName} 因为版权不能下载");
                    return null;
                }
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
                return Fetch(API_PROTOCOL, API_HOST, API_PATH + ServiceName + $"/lrc?id={Id}");

            }
            catch (Exception ex)
            { Log("歌词获取错误(ex:" + ex.ToString() + ",id:" + Id + ")"); }

            return null;
        }

        protected override List<SongInfo> GetPlaylist(string keyword)
        {
            try
            {
                List<SongInfo> songInfos = new List<SongInfo>();

                JObject playlist = JObject.Parse(Fetch(API_PROTOCOL, API_HOST, API_PATH + ServiceName + $"/search?id={HttpUtility.UrlEncode(keyword)}&type=songList&pageSize=5&page=0"));

                if (playlist["code"]?.ToObject<int>() == 200)
                {
                    List<JToken> result = (playlist["data"]["playlists"] as JArray).ToList();

                    //if (result.Count() > 50)
                    //    result = result.Take(50).ToList();

                    result.ForEach(song =>
                    {
                        try
                        {
                            var songInfo = new SongInfo(this,
                                song["id"].ToString(),
                                song["name"].ToString(),
                                (song["ar"] as JArray).Select(x => x["name"].ToString()).ToArray());

                            songInfo.Lyric = null;//在之后再获取Lyric

                            songInfos.Add(songInfo);
                        }
                        catch (Exception) { }
                    });

                    return songInfos;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Log("获取歌单信息时出错 " + ex.Message);
                return null;
            }
        }

        protected override SongInfo Search(string keyword)
        {
            string result_str;
            try
            {
                result_str = Fetch(API_PROTOCOL, API_HOST, API_PATH + ServiceName + $"/search?keyword={HttpUtility.UrlEncode(keyword)}&type=song&pageSize=5&page=0");
            }
            catch (Exception ex)
            {
                Log("搜索歌曲时网络错误：" + ex.Message);
                return null;
            }

            JObject song = null;
            try
            {
                JObject info = JObject.Parse(result_str);
                if (info["code"].ToString() == "200")
                {
                    song = (info["data"]["songs"] as JArray)?[0] as JObject;
                }
            }
            catch (Exception ex)
            {
                Log("搜索歌曲解析数据错误：" + ex.Message);
                return null;
            }

            SongInfo songInfo;


            try
            {
                songInfo = new SongInfo(
                    this,
                    song["id"].ToString(),
                    song["name"].ToString(),
                    (song["ar"] as JArray).Select(x => x["name"].ToString()).ToArray()
                );
            }
            catch (Exception ex)
            { Log("歌曲信息获取结果错误：" + ex.Message); return null; }

            songInfo.Lyric = GetLyricById(songInfo.Id);

            return songInfo;
        }

        private static string Fetch(string prot, string host, string path, string data = null, string referer = null)
        {
            for (int retryCount = 0; retryCount < 4; retryCount++)
            {
                try
                {
                    return Fetch_exec(prot, host, path, data, referer);
                }
                catch (WebException)
                {
                    if (retryCount >= 3)
                    {
                        throw;
                    }

                    continue;
                }
            }

            return null;
        }

        private static string Fetch_exec(string prot, string host, string path, string data = null, string referer = null)
        {
            string address;
            if (GetDNSResult(host, out string ip))
            {
                address = prot + ip + path;
            }
            else
            {
                address = prot + host + path;
            }

            var request = (HttpWebRequest)WebRequest.Create(address);

            request.Timeout = 4000;
            request.Host = host;
            request.UserAgent = "DMPlugin_DGJ/" + (BuildInfo.Appveyor ? BuildInfo.Version : "local") + " RoomId/" + RoomId.ToString();

            if (referer != null)
            {
                request.Referer = referer;
            }

            if (data != null)
            {
                var postData = Encoding.UTF8.GetBytes(data);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            return responseString;
        }
        private static string Fetch(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 10000;
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            return responseString;
        }
        private static bool GetDNSResult(string domain, out string result)
        {
            if (DNSList.TryGetValue(domain, out DNSResult result_from_d))
            {
                if (result_from_d.TTLTime > DateTime.Now)
                {
                    result = result_from_d.IP;
                    return true;
                }
                else
                {
                    DNSList.Remove(domain);
                    if (RequestDNSResult(domain, out DNSResult? result_from_api, out Exception exception))
                    {
                        DNSList.Add(domain, result_from_api.Value);
                        result = result_from_api.Value.IP;
                        return true;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
            }
            else
            {
                if (RequestDNSResult(domain, out DNSResult? result_from_api, out Exception exception))
                {
                    DNSList.Add(domain, result_from_api.Value);
                    result = result_from_api.Value.IP;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
        }
        private static bool RequestDNSResult(string domain, out DNSResult? dnsResult, out Exception exception)
        {
            dnsResult = null;
            exception = null;

            try
            {
                var http_result = Fetch("http://119.29.29.29/d?ttl=1&dn=" + domain);
                if (http_result == string.Empty)
                {
                    return false;
                }

                var m = regex.Match(http_result);
                if (!m.Success)
                {
                    exception = new Exception("HTTPDNS 返回结果不正确");
                    return false;
                }

                dnsResult = new DNSResult()
                {
                    IP = m.Groups[1].Value,
                    TTLTime = DateTime.Now + TimeSpan.FromSeconds(double.Parse(m.Groups[2].Value))
                };
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        [Obsolete("Use GetLyricById instead", true)]
        protected override string GetLyric(SongItem songInfo)
        {
            throw new NotImplementedException();
        }

        private static readonly Dictionary<string, DNSResult> DNSList = new Dictionary<string, DNSResult>();
        private static readonly Regex regex = new Regex(@"((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))\,(\d+)", RegexOptions.Compiled);
        private struct DNSResult
        {
            internal string IP;
            internal DateTime TTLTime;
        }

    }
}
