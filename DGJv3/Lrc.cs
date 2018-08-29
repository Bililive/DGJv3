using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DGJv3
{
    /// <summary>
    /// 歌词
    /// </summary>
    public class Lrc
    {

        public static readonly Lrc NoLyric = new Lrc()
        {
            Album = string.Empty,
            Artist = string.Empty,
            LrcBy = string.Empty,
            Offset = string.Empty,
            Title = string.Empty,
            LrcWord = new Dictionary<double, string>()
            {
                {
                    0d,
                    "无歌词"
                }
            }
        };

        /// <summary>
        /// 歌曲
        /// </summary>
        public string Title
        { get; set; }


        /// <summary>
        /// 艺术家
        /// </summary>
        public string Artist
        { get; set; }


        /// <summary>
        /// 专辑
        /// </summary>
        public string Album
        { get; set; }


        /// <summary>
        /// 歌词作者
        /// </summary>
        public string LrcBy
        { get; set; }


        /// <summary>
        /// 偏移量
        /// </summary>
        public string Offset
        { get; set; }

        /// <summary>
        /// 歌词
        /// </summary>
        public Dictionary<double, string> LrcWord
        { get; set; }

        public int GetLyric(double seconds, out string current, out string upcoming)
        {
            if (LrcWord.Count < 1)
            {
                current = "无歌词";
                upcoming = string.Empty;
                return -1;
            }
            var list = LrcWord.ToList();
            int i;
            if (seconds < list[0].Key)
            {
                i = 0;
                current = string.Empty;
                upcoming = list[0].Value;
            }
            else
            {
                for (i = 1; i < LrcWord.Count; i++)
                    if (seconds < list[i].Key)
                        break;

                current = list[i - 1].Value;
                if (list.Count > i)
                    upcoming = list[i].Value;
                else
                    upcoming = string.Empty;
            }
            return i;
        }

        /// <summary>
        /// 获得歌词信息
        /// </summary>
        /// <param name="LrcText">歌词文本</param>
        /// <returns>返回歌词信息(Lrc实例)</returns>
        public static Lrc InitLrc(string LrcText)
        {
            Lrc lrc = new Lrc();
            Dictionary<double, string> dicword = new Dictionary<double, string>();

            string[] lines = LrcText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (line.StartsWith("[ti:"))
                {
                    lrc.Title = SplitInfo(line);
                }
                else if (line.StartsWith("[ar:"))
                {
                    lrc.Artist = SplitInfo(line);
                }
                else if (line.StartsWith("[al:"))
                {
                    lrc.Album = SplitInfo(line);
                }
                else if (line.StartsWith("[by:"))
                {
                    lrc.LrcBy = SplitInfo(line);
                }
                else if (line.StartsWith("[offset:"))
                {
                    lrc.Offset = SplitInfo(line);
                }
                else
                {
                    try
                    {
                        Regex regexword = new Regex(@".*\](.*)");
                        Match mcw = regexword.Match(line);
                        string word = mcw.Groups[1].Value;
                        if (word.Replace(" ", "") == "")
                            continue; // 如果为空歌词则跳过不处理
                        Regex regextime = new Regex(@"\[([0-9.:]*)\]", RegexOptions.Compiled);
                        MatchCollection mct = regextime.Matches(line);
                        foreach (Match item in mct)
                        {
                            double time = TimeSpan.Parse("00:" + item.Groups[1].Value).TotalSeconds;
                            dicword.Add(time, word);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            lrc.LrcWord = dicword.OrderBy(t => t.Key).ToDictionary(t => t.Key, p => p.Value);
            return lrc;
        }

        /// <summary>
        /// 处理信息(私有方法)
        /// </summary>
        /// <param name="line"></param>
        /// <returns>返回基础信息</returns>
        static string SplitInfo(string line)
        {
            return line.Substring(line.IndexOf(":") + 1).TrimEnd(']');
        }
    }
}
