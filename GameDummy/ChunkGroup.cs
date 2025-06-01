using System;
using System.Collections.Generic;
using System.Linq;

namespace GameDummy
{
    public sealed class ChunkGroup
    {
        public readonly string chunkID;
        public readonly int chunkIDNumber;
        public int expectTotal { get; private set; }
        public bool isCompleted { get; private set; }
        public event Action<ChunkGroup>? OnCompleted;
        public ChunkStreamingPacket[] chunksOrdered { get; private set; } = new ChunkStreamingPacket[0];
        public readonly List<ChunkStreamingPacket> chunksUnordered = new();
        HashSet<int> m_chunksHetSet = new();

        public ChunkStreamingPacket? recivedFirstChunk { get; private set; }
        public ChunkStreamingPacket? recivedLastChunk { get; private set; }

        public byte[]? fullBytes { get; private set; }
        public int currentTotalBytes { get; private set; }

        public ChunkGroup(string chunkID)
        {
            this.chunkID = chunkID;
            this.chunkIDNumber = int.Parse(chunkID.Split(":").Last());
        }

        public void AddChunk(ChunkStreamingPacket chunk)
        {
            // check duplicate
            if (m_chunksHetSet.Contains(chunk.chunkIndex))
                return;

            m_chunksHetSet.Add(chunk.chunkIndex);

            // init
            if (recivedFirstChunk is null)
            {
                recivedFirstChunk = chunk;
                expectTotal = chunk.totalChunk;
                chunksOrdered = new ChunkStreamingPacket[expectTotal];
            }

            //update
            chunksOrdered[chunk.chunkIndex] = chunk;
            chunksUnordered.Add(chunk);
            currentTotalBytes += chunk.bytes.Length;

            //check completed
            isCompleted = chunksUnordered.Count == expectTotal;

            if (isCompleted)
            {
                //setup
                recivedLastChunk = chunk;
                AssemblyBytes();

                // fire API
                OnCompleted?.Invoke(this);
            }
        }
        void AssemblyBytes()
        {
            this.fullBytes = new byte[this.currentTotalBytes];
            var chunks = this.chunksOrdered;
            int currentByteOffset = 0;
            for (int i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var chunkBytes = chunk.bytes;
                Array.Copy(chunkBytes, 0, fullBytes, currentByteOffset, chunkBytes.Length);
                currentByteOffset += chunkBytes.Length;
            }
        }
    }
}
