using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliDM_PluginFramework;
using LoginCenter.API;

namespace DGJv3
{
    
    static class Extensions
    {
        internal static void Remove(this SongItem songItem, ObservableCollection<SongItem> songList, Downloader downloader, Player player)
        {
            switch (songItem.Status)
            {
                case SongStatus.WaitingDownload:
                    songList.Remove(songItem);
                    break;
                case SongStatus.Downloading:
                    downloader.CancelDownload();
                    break;
                case SongStatus.WaitingPlay:
                    songList.Remove(songItem);
                    try { File.Delete(songItem.FilePath); } catch (Exception) { }
                    break;
                case SongStatus.Playing:
                    player.Next();
                    break;
                default:
                    break;
            }
        }

        internal static string GetDownloadUrl(this SongItem songItem) => songItem.Module.SafeGetDownloadUrl(songItem);

        internal static bool IsInBlacklist(this SongInfo songInfo, IEnumerable<BlackListItem> blackList)
        {
            return blackList.ToArray().Any(x =>
            {
                switch (x.BlackType)
                {
                    case BlackListType.Id: return songInfo.Id.Equals(x.Content);
                    case BlackListType.Name: return songInfo.Name.IndexOf(x.Content, StringComparison.CurrentCultureIgnoreCase) > -1;
                    case BlackListType.Singer: return songInfo.SingersText.IndexOf(x.Content, StringComparison.CurrentCultureIgnoreCase) > -1;
                    default: return false;
                }
            });
        }

        internal static string ToStatusString(this SongStatus songStatus)
        {
            switch (songStatus)
            {
                case SongStatus.WaitingDownload:
                    return "等待下载";
                case SongStatus.Downloading:
                    return "正在下载";
                case SongStatus.WaitingPlay:
                    return "等待播放";
                case SongStatus.Playing:
                    return "正在播放";
                default:
                    return "？？？？";
            }
        }

        private static LoginCenterAPI_Warpper warpper = null;
        private static bool isLoginCenterChecked = false;

        internal static bool CheckLoginCenter()
        {
            if (isLoginCenterChecked)
                return warpper != null;
            isLoginCenterChecked = true;
            try
            {
                warpper = new LoginCenterAPI_Warpper();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private static bool isExists()
        {
            if (!isLoginCenterChecked)
                CheckLoginCenter();
            return warpper != null;
        }

        internal static bool CheckAuth(BilibiliDM_PluginFramework.DMPlugin plugin)
        {
            if (!isExists())
            {
                return false;
            }
            return warpper.checkAuthorization(plugin) == true;
        }

        internal static async Task<bool> DoAuth(BilibiliDM_PluginFramework.DMPlugin plugin)
        {
            if (!isExists())
            {
                return false;
            }
            return await warpper.doAuthorization(plugin);
        }

        internal static string Send(int roomid, string msg, int color = 16777215, int mode = 1, int rnd = -1, int fontsize = 25)
        {
            if (!isExists())
            {
                return null;
            }
            return warpper.trySendMessage(roomid, msg, color, mode, rnd, fontsize).Result;
        }

        internal static async Task<string> Send_Async(int roomid, string msg, int color = 16777215, int mode = 1, int rnd = -1, int fontsize = 25)
        {
            if (!isExists())
            {
                return null;
            }
            return await warpper.trySendMessage(roomid, msg, color, mode, rnd, fontsize);
        }
    }

    internal class LoginCenterAPI_Warpper
    {
        public LoginCenterAPI_Warpper()
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
