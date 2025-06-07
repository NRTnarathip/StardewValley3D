namespace GuyNetwork
{
    public sealed class SyncListPacket
    {
        public string id { get; set; }
        public byte[] actions { get; set; }
        public int[] actionIndexs { get; set; }
        public GeneralObjectPacket[] items { get; set; }
    }
}
