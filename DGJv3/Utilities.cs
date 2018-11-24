using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DGJv3
{
    internal static class Utilities
    {
        public static IEnumerable<WaveoutEventDeviceInfo> WaveoutEventDevices
        {
            get
            {
                var infos = new List<WaveoutEventDeviceInfo>();
                for (int i = -1; i < WaveOut.DeviceCount; i++)
                {
                    var caps = WaveOut.GetCapabilities(i);
                    infos.Add(new WaveoutEventDeviceInfo(i, caps.ProductName));
                }
                return infos;
            }
        }

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        internal static readonly string DataDirectoryPath = Path.Combine(AssemblyDirectory, "点歌姬v3");

        internal static readonly string SongsCacheDirectoryPath = Path.Combine(DataDirectoryPath, "歌曲缓存");

        internal static readonly string BinDirectoryPath = Path.Combine(DataDirectoryPath, "bin");

        internal static readonly string ConfigFilePath = Path.Combine(DataDirectoryPath, "config.json");

        internal static readonly string ScribanOutputFilePath = Path.Combine(DataDirectoryPath, "信息.txt");

        internal static readonly string LyricOutputFilePath = Path.Combine(DataDirectoryPath, "歌词.txt");

        internal static readonly string SparePlaylistUser = "空闲歌单";

    }
}
