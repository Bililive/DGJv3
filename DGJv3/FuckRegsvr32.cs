using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace DGJv3
{
    internal static class FuckRegsvr32
    {
        private const uint E_FAIL = 0x80004005;
        private const uint E_UNEXPECTED = 0x8000FFFF;

        [DllExport("DllRegisterServer", CallingConvention.StdCall)]
        public static uint DllRegisterServer()
        {
            MessageBox.Show("点歌姬不是双击dll文件安装的\n你看下载页右侧的安装使用说明了吗？\n你看下载页右侧的安装使用说明了吗？\n你看下载页右侧的安装使用说明了吗？", "你看使用说明了吗？", MessageBoxButton.OK, MessageBoxImage.Question);
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "弹幕姬", "Plugins");
            if (!Directory.Exists(path))
            {
                MessageBox.Show("没有在此计算机找到弹幕姬插件文件夹，你安装弹幕姬了么？\n弹幕姬下载：https://www.danmuji.org", "点歌姬不是双击dll文件安装的", MessageBoxButton.OK, MessageBoxImage.Question);
            }
            else
            {
                Process.Start(path);
            }
            Process.Start("https://www.danmuji.org/plugins/DGJ");
            Environment.Exit(1);
            return E_FAIL;
        }
    }
}
