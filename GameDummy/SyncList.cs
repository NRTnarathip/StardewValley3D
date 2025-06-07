using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GuyNetwork
{
    public enum SyncListAction : byte
    {
        Add = 0,
        Remove = 1,
        Update = 2,
        Clear = 3,
        FullSync = 4,
    }

    public sealed class SyncList<T> : BaseSyncList, IReadOnlyList<T>
    {
        readonly IList<T> m_items = new List<T>();

        public int Count => m_items.Count;

        public T this[int index]
        {
            get => m_items[index];
            set
            {
                var item = this.m_items[index];
                // if set new value
                if (item?.Equals(value) is false)
                {
                    m_items[index] = value;
                    OnSetValueAtIndex(index, item, value);
                }
            }
        }

        readonly string? m_id;
        internal string? ID => m_id;

        public SyncList(string id)
        {
            this.m_id = id;
        }

        public override void Reset()
        {
            m_onActions.Clear();
        }

        public override void ProcessSyncListPacketOnClient(SyncListPacket packet)
        {
            var totalActions = packet.actions.Length;
            if (totalActions == 0)
                return;

            var firstAction = (SyncListAction)packet.actions[0];

            // full sync packet
            if (firstAction is SyncListAction.FullSync)
            {
                // already full sync
                if (isInitFullSync)
                {
                    Console.WriteLine("skip full sync");
                    return;
                }

                isInitFullSync = true;
                m_items.Clear();
                foreach (var item in packet.items)
                {
                    T value = item.ReadValue<T>();
                    m_items.Add(value);
                }
                Console.WriteLine("initialized full sync");

                return;
            }

            // !! Need to init full sync first
            if (isInitFullSync is false)
                return;

            // other actions
            //Console.WriteLine("Try process actions");
            //Console.WriteLine(" total actions: " + totalActions);
            //Console.WriteLine(" total items: " + packet.items.Length);
            //Console.WriteLine(" total actionIndexs: " + packet.actionIndexs.Length);
            for (int i = 0; i < totalActions; i++)
            {
                var action = (SyncListAction)packet.actions[i];
                var itemPacket = packet.items[i];
                var item = itemPacket.ReadValue<T>();
                var index = packet.actionIndexs[i];
                //Console.WriteLine($" - [{i}] = action:{action} itemIndex:{index}, item:{item}");
                //Console.WriteLine("my current list: " + m_items.Count);
                switch (action)
                {
                    case SyncListAction.Add:
                        m_items.Add(item);
                        break;
                    case SyncListAction.Update:
                        m_items[index] = item;
                        break;
                    case SyncListAction.Remove:
                        m_items.RemoveAt(index);
                        break;
                }
            }
        }

        public override void SendFullSyncToClient(NetPacketProcessor processor, NetDataWriter writer, NetPeer clientPeer)
        {
            // setup packet
            SyncListPacket packet = new();
            packet.id = this.ID;
            packet.actions = new byte[1] { (byte)SyncListAction.FullSync };
            packet.items = new GeneralObjectPacket[Count];
            var totalItem = packet.items.Length;
            for (int i = 0; i < totalItem; i++)
            {
                var obj = new GeneralObjectPacket();
                obj.WriteValue(m_items[i]);
                packet.items[i] = obj;
            }

            // ready to send
            writer.Reset();
            processor.Write(writer, packet);
            clientPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            writer.Reset();
        }

        public override SyncListPacket? SerializeSyncListPacket(
            NetPacketProcessor packetProcessor, NetDataWriter writer)
        {
            var onActions = this.OnActions;
            var totalAction = onActions.Count;
            if (totalAction == 0)
            {
                return null;
            }

            var packet = new SyncListPacket();
            packet.id = this.m_id;
            packet.items = new GeneralObjectPacket[totalAction];
            packet.actions = new byte[totalAction];
            packet.actionIndexs = new int[totalAction];

            for (int i = 0; i < totalAction; i++)
            {
                var actionInfo = onActions[i];
                GeneralObjectPacket objPacket = new();
                objPacket.WriteValue(actionInfo.item);

                packet.items[i] = objPacket;
                packet.actions[i] = (byte)actionInfo.action;
                packet.actionIndexs[i] = actionInfo.index;
            }

            //ready to send all
            writer.Reset();
            packetProcessor.Write(writer, packet);
            return packet;
        }

        void OnSetValueAtIndex(int index, T oldItem, T newItem)
        {
            m_onActions.Add(new()
            {
                index = index,
                action = SyncListAction.Update,
                item = newItem,
            });
        }

        void AddOnAction(SyncListAction action, int index, T? oldItem, T? newItem)
        {
            m_onActions.Add(new()
            {
                action = action,
                index = index,
                item = newItem,
            });
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < m_items.Count; i++)
                if (item.Equals(m_items[i]))
                    return i;

            return -1;
        }

        public void Add(T item)
        {
            m_items.Add(item);
            AddOnAction(SyncListAction.Add, m_items.Count - 1, default, item);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            m_items.RemoveAt(index);
            AddOnAction(SyncListAction.Remove, index, default, default);
        }

        public void Clear()
        {
            m_items.Clear();
            AddOnAction(SyncListAction.Clear, 0, default, default);
        }

        public IEnumerator<T> GetEnumerator() => new SyncListEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new SyncListEnumerator(this);


        public struct SyncListEnumerator : IEnumerator<T>
        {
            readonly SyncList<T> list;
            int index;

            public T Current { get; private set; }

            public SyncListEnumerator(SyncList<T> list)
            {
                this.list = list;
                index = -1;
                Current = default;
            }

            public bool MoveNext()
            {
                if (++index >= list.Count)
                    return false;

                Current = list[index];
                return true;
            }

            public void Reset() => index = -1;
            object IEnumerator.Current => Current;
            public void Dispose() { }
        }
    }
}
