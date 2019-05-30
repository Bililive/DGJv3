using BilibiliDM_PluginFramework;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DGJv3
{
    public class DGJMain : DMPlugin
    {
        private readonly DGJWindow window;

        private VersionChecker versionChecker;

        public DGJMain()
        {
            try
            {
                var info = Directory.CreateDirectory(Utilities.BinDirectoryPath);
                info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            catch (Exception) { }
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

            PluginName = "点歌姬";
            PluginVer = BuildInfo.Version;
            PluginDesc = "使用弹幕点播歌曲";
            PluginAuth = "Genteure";
            PluginCont = "dgj3@genteure.com";

            try
            {
                Directory.CreateDirectory(Utilities.DataDirectoryPath);
            }
            catch (Exception) { }
            window = new DGJWindow(this);
            versionChecker = new VersionChecker("DGJv3");
            Task.Run(() =>
            {
                if (versionChecker.FetchInfo())
                {
                    Version current = new Version(BuildInfo.Version);

                    if (versionChecker.HasNewVersion(current))
                    {
                        Log("插件有新版本" + Environment.NewLine +
                            $"当前版本：{BuildInfo.Version}" + Environment.NewLine +
                            $"最新版本：{versionChecker.Version.ToString()} 更新时间：{versionChecker.UpdateDateTime.ToShortDateString()}" + Environment.NewLine +
                            versionChecker.UpdateDescription);
                    }
                }
                else
                {
                    Log("版本检查出错：" + versionChecker?.LastException?.Message);
                }
            });
        }

        public override void Admin()
        {
            window.Show();
            window.Activate();
        }

        public override void DeInit() => window.DeInit();

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            var path = assemblyName.Name + ".dll";
            string filepath = Path.Combine(Utilities.BinDirectoryPath, path);

            if (assemblyName.CultureInfo?.Equals(CultureInfo.InvariantCulture) == false)
            { path = string.Format(@"{0}\{1}", assemblyName.CultureInfo, path); }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null) { return null; }

                var assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                try
                {
                    File.WriteAllBytes(filepath, assemblyRawBytes);
                }
                catch (Exception) { }
            }

            return Assembly.LoadFrom(filepath);
        }
    }
}
