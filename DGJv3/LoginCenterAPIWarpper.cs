using BilibiliDM_PluginFramework;
using LoginCenter.API;
using System;
using System.Threading.Tasks;

namespace DGJv3
{
    internal class LoginCenterAPIWarpper
    {
        private static LoginCenterAPIWarpper warpper = null;
        private static bool isLoginCenterChecked = false;

        internal static bool CheckLoginCenter()
        {
            if (isLoginCenterChecked)
            {
                return warpper != null;
            }

            try
            {
                warpper = new LoginCenterAPIWarpper();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                isLoginCenterChecked = true;
            }
        }

        internal static bool CheckAuth(DMPlugin plugin)
        {
            if (!CheckLoginCenter())
            {
                return false;
            }
            return warpper.checkAuthorization(plugin) == true;
        }

        internal static async Task<bool> DoAuth(DMPlugin plugin)
        {
            if (!CheckLoginCenter())
            {
                return false;
            }
            return await warpper.doAuthorization(plugin);
        }

        internal static string Send(int roomid, string msg, int color = 16777215, int mode = 1, int rnd = -1, int fontsize = 25)
        {
            if (!CheckLoginCenter())
            {
                return null;
            }
            return warpper.trySendMessage(roomid, msg, color, mode, rnd, fontsize).Result;
        }

        internal static async Task<string> Send_Async(int roomid, string msg, int color = 16777215, int mode = 1, int rnd = -1, int fontsize = 25)
        {
            if (!CheckLoginCenter())
            {
                return null;
            }
            return await warpper.trySendMessage(roomid, msg, color, mode, rnd, fontsize);
        }



        public LoginCenterAPIWarpper()
        {
            checkAuthorization();
        }
        public bool checkAuthorization()
        {
            return LoginCenterAPI.checkAuthorization();
        }

        public bool checkAuthorization(DMPlugin plugin)
        {
            return LoginCenterAPI.checkAuthorization(plugin) == LoginCenter.API.AuthorizationResult.Success;
        }

        public Task<string> trySendMessage(int roomid, string msg, int color = 16777215, int mode = 1,
            int rnd = -1, int fontsize = 25)
        {
            return LoginCenterAPI.trySendMessage(roomid, msg, color, mode, rnd, fontsize);
        }


        public async Task<bool> doAuthorization(DMPlugin plugin)
        {
            var result = await LoginCenterAPI.doAuthorization(plugin);
            return result == AuthorizationResult.Success;
        }
    }
}
