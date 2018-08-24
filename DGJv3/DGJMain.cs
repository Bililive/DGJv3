using BilibiliDM_PluginFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGJv3
{
    public class DGJMain : DMPlugin
    {
        private DGJWindow window;

        public DGJMain()
        {
            this.PluginName = "点歌姬";
            this.PluginVer = "3.0-pre-alpha";
            this.PluginDesc = "使用弹幕点播歌曲";
            this.PluginAuth = "Genteure";
            this.PluginCont = "dgj3@genteure.com";

            window = new DGJWindow(this);
        }

        public override void Admin() => window.Activate();

        public override void DeInit()
        {

        }
    }
}
