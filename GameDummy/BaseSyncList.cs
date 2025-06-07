using LiteNetLib.Utils;
using LiteNetLib;
using System.Collections.Generic;

namespace GuyNetwork
{
    public abstract class BaseSyncList
    {
        public struct OnAction
        {
            public SyncListAction action;
            public int index;
            public object item;
        }

        protected List<OnAction> m_onActions = new();
        public List<OnAction> OnActions => m_onActions;

        public bool isInitFullSync { get; protected set; }

        public abstract void Reset();

        public abstract void ProcessSyncListPacketOnClient(SyncListPacket packet);

        public abstract void SendFullSyncToClient(
            NetPacketProcessor processor, NetDataWriter writer, NetPeer clientPeer);

        public abstract SyncListPacket? SerializeSyncListPacket(
            NetPacketProcessor processor, NetDataWriter writer);
    }
}
