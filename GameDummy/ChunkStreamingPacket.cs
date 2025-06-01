using MessagePack;

namespace GameDummy
{
    [MessagePackObject]
    public sealed class ChunkStreamingPacket
    {
        [Key(0)]
        public string chunkID;
        [Key(1)]
        public int chunkIndex;
        [Key(2)]
        public int totalChunk;
        [Key(3)]
        public byte[] bytes;

        public const int MaxChunkSize = 900;
    }
}
