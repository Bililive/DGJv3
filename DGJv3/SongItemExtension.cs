using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGJv3
{
    static class SongItemExtension
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
    }
}
