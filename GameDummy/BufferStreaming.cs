using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GameDummy
{
    public class BufferStreaming
    {
        const int MaxChunkSize = ChunkStreamingPacket.MaxChunkSize;

        private BaseAppNetwork net;
        public readonly string streamingID;

        readonly string streamingChunkSendReciveID;

        public BufferStreaming(string streamingID, BaseAppNetwork net)
        {
            this.net = net;
            this.streamingID = streamingID;
            this.streamingChunkSendReciveID = GetChunkStreamingMsgEventID(streamingID);

            net.RegisterEvent(streamingChunkSendReciveID,
                (Action<ChunkStreamingPacket>)((chunk) =>
            {
                OnReciveChunk(chunk);
            }));
        }

        string GetChunkStreamingMsgEventID(string streamingID)
        {
            return $"{typeof(BufferStreaming).FullName}:SendReciveChunk:{streamingID}";
        }

        Dictionary<string, ChunkGroup> chunkGroupMap = new();
        Queue<ChunkGroup> chunkGroupMapQueue = new();

        public void OnReciveChunk(ChunkStreamingPacket chunk)
        {
            string chunkID = chunk.chunkID;
            if (chunkGroupMap.TryGetValue(chunkID, out var chunkGroup) is false)
            {
                chunkGroup = new(chunkID);
                chunkGroup.OnCompleted += ChunkGroup_OnCompleted;

                chunkGroupMap[chunkID] = chunkGroup;
                chunkGroupMapQueue.Enqueue(chunkGroup);

                // clear unuse group
                if (chunkGroupMapQueue.Count >= 2)
                {
                    var removeChunkGroup = chunkGroupMapQueue.Dequeue();
                    chunkGroupMap.Remove(removeChunkGroup.chunkID);
                }
            }
            chunkGroup.AddChunk(chunk);
        }

        public ChunkGroup? lastChunkGroupCompleted { get; private set; }
        void ChunkGroup_OnCompleted(ChunkGroup newChunkGroup)
        {
            // check if you have new chunk completed
            if (newChunkGroup.chunkIDNumber < lastChunkGroupCompleted?.chunkIDNumber)
            {
                return;
            }

            // update new latest completed
            lastChunkGroupCompleted = newChunkGroup;
            // ready fire API
            OnBufferCompleted?.Invoke(this);
        }

        int sendPacketCounter = 0;
        public void SendToAll(byte[] bufferBytes)
        {
            sendPacketCounter++;

            int totalChunks = (int)Math.Ceiling((double)bufferBytes.Length / ChunkStreamingPacket.MaxChunkSize);
            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                int currentOffsetIndex = chunkIndex * MaxChunkSize;
                int currentBytesLength =
                    Math.Min(MaxChunkSize,
                        bufferBytes.Length - currentOffsetIndex);

                byte[] chunkBytes = new byte[currentBytesLength];
                Array.Copy(
                    bufferBytes, currentOffsetIndex,
                    chunkBytes, 0,
                    chunkBytes.Length
                );

                // chunkID you use any ID
                string chunkID = $"{streamingID}:{sendPacketCounter}";
                var chunk = new ChunkStreamingPacket()
                {
                    bytes = chunkBytes,
                    chunkID = chunkID,
                    chunkIndex = chunkIndex,
                    totalChunk = totalChunks,
                };
                net.SendEvent(streamingChunkSendReciveID,
                    new object[] { chunk },
                    LiteNetLib.DeliveryMethod.Unreliable);
            }
        }

        public byte[]? GetLatestCompletedBytes()
        {
            if (lastChunkGroupCompleted is null)
            {
                return null;
            }

            return lastChunkGroupCompleted.fullBytes;
        }

        public event Action<BufferStreaming>? OnBufferCompleted;
    }
}
