namespace DGJv3
{
    public class WaveoutEventDeviceInfo
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        internal WaveoutEventDeviceInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
