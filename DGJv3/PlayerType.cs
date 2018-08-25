using System.ComponentModel;

namespace DGJv3
{
    public enum PlayerType : int
    {
        [Description("WaveOutEvent")]
        WaveOutEvent = 0,
        [Description("DirectSound")]
        DirectSound = 1,
    }
}