using System;
using System.Collections.Generic;

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

        public List<ChunkStreamingPacket> chunkStreamingPackets { get; private set; } = new();
        public int totalBufferSize { get; private set; } = 0;

        public void OnReciveChunk(ChunkStreamingPacket chunk)
        {
            // init
            bool isFirstChunk = chunk.chunkIndex == 0;
            if (isFirstChunk)
            {
                chunkStreamingPackets.Clear();
                totalBufferSize = 0;
            }

            // update
            chunkStreamingPackets.Add(chunk);
            totalBufferSize += chunk.bytes.Length;

            // done
            bool isLastChunk = chunkStreamingPackets.Count == chunk.totalChunk;
            if (isLastChunk)
            {
                onBufferCompleted?.Invoke(this);
            }
        }

        int senderCounter = 0;
        public void SendToAll(byte[] bufferBytes)
        {
            senderCounter++;

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
                string chunkID = $"{streamingID}:{senderCounter}";
                var chunk = new ChunkStreamingPacket()
                {
                    bytes = chunkBytes,
                    chunkID = chunkID,
                    chunkIndex = chunkIndex,
                    totalChunk = totalChunks,
                };
                net.SendEvent(streamingChunkSendReciveID, new object[] { chunk });
            }
        }

        public event Action<BufferStreaming>? onBufferCompleted;
    }
}
