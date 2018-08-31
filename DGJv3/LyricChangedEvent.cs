namespace DGJv3
{
    public delegate void LyricChangedEvent(object sender, LyricChangedEventArgs e);

    public class LyricChangedEventArgs
    {
        public string CurrentLyric { get; set; }
        public string UpcomingLyric { get; set; }
    }
}
