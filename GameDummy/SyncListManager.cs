using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GuyNetwork
{
    public class SyncListClientSnapshot
    {
        public readonly NetPeer peer;
        BaseSyncList list;
        public SyncListClientSnapshot(BaseSyncList syncList, NetPeer peer)
        {
            this.peer = peer;
            this.list = syncList;
        }
        public bool isSetupFullSync = false;
        internal void Send(NetPacketProcessor processor, NetDataWriter writer)
        {
            if (isSetupFullSync is false)
            {
                list.SendFullSyncToClient(processor, writer, peer);
                isSetupFullSync = true;
                return;
            }

            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    public sealed class SyncListManager
    {
        readonly BaseAppNetwork appNetwork;
        readonly NetPacketProcessor packetProcessor;

        readonly NetDataWriter writer = new();

        readonly Dictionary<string, BaseSyncList> m_syncListMap = new();

        readonly bool isServer;
        readonly bool isClient;

        public SyncListManager(BaseAppNetwork network)
        {
            this.appNetwork = network;
            this.packetProcessor = network.packetProcessor;
            this.isServer = network.isServer;
            this.isClient = network.isClient;

            packetProcessor.SubscribeReusable<SyncListPacket, NetPeer>(OnReciveSyncListPacket);
        }

        void OnReciveSyncListPacket(SyncListPacket packet, NetPeer peer)
        {
            var id = packet.id;

            if (this.isServer)
            {
                return;
            }
            else
            {
                // not found any sync list in registry
                if (m_syncListMap.TryGetValue(id, out BaseSyncList list) is false)
                {
                    return;
                }

                // process actions
                list.ProcessSyncListPacketOnClient(packet);
            }
        }

        internal void PollEvents()
        {
        }

        // key: (SyncList.ID + peer)
        Dictionary<string, SyncListClientSnapshot> m_syncListClientSnapshotMap = new();

        public SyncListClientSnapshot GetClientSnapshot(string syncListID, NetPeer peer)
        {
            string snapshotKey = $"{syncListID}-{peer.Id}";
            var syncList = m_syncListMap[syncListID];
            if (m_syncListClientSnapshotMap.TryGetValue(snapshotKey, out var snapshot) is false)
            {
                snapshot = new(syncList, peer);
                m_syncListClientSnapshotMap[snapshotKey] = snapshot;
            }

            return snapshot;
        }

        public void EndUpdate()
        {
            if (isServer)
                EndUpdateServerSide();
        }

        void EndUpdateServerSide()
        {
            if (!isServer)
                return;

            var peerList = appNetwork.netManager.ConnectedPeerList;

            foreach ((string listID, BaseSyncList syncList) in m_syncListMap)
            {
                var serializePacket = syncList.SerializeSyncListPacket(packetProcessor, writer);

                if (serializePacket is not null)
                {
                    foreach (var peer in peerList)
                    {
                        var snapshot = GetClientSnapshot(listID, peer);
                        snapshot.Send(packetProcessor, writer);
                    }
                }

                syncList.Reset();
            }
        }

        public void RegisterSyncList<T>(SyncList<T> list)
        {
            m_syncListMap.TryAdd(list.ID, list);
            appNetwork.Log("registered syncList id: " + list.ID);
        }
    }
}
