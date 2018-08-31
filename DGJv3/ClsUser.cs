using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGJv3
{
    class DMJUser//用户模块
    {
        public int Uid;//用户id
        public int Money = 0;//用户拥有的瓜子 //调整0可以修改为初始赠送多少点歌积分

        //热更新数据:不会被保存,当用户出现的时候更新下数据

        public bool Update = false;//判断用户是否更新了个人数据
        public string Name;
        //public int OrderMax = 0;//用户最大可以使用(点歌)命令条数(默认100/天)//清除方法:重启软件
        public int VipLv = 1;//用户vip等级越高，收益越多(打折播放歌曲等

        public double Discount()
        {
            return 1 - 0.1 * VipLv;
        }
        public DMJUser(string Loadinfo)//加载数据
        {
            string[] tmps = Loadinfo.Split('|');
            try
            {
                Uid = Convert.ToInt32(tmps[0]);
                Money = Convert.ToInt32(tmps[1]);
            }
            catch
            {
                //出bug给空数据，保存的时候不保存即可
                Uid = -1;
                Money = 0;
            }
        }

        public DMJUser(int uid)
        {
            Uid = uid;
        }
        public DMJUser(int uid,int money)
        {
            Uid = uid;
            Money = money;
        }
        public string Data()
        {
            if (Uid == -1)
                return "";
            return Uid + "|" + Money;
        }
    }
}
