using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameDummy
{
    public class BaseAppNetwork : INetEventListener
    {
        public const int port = 10055;
        public const string address = "localhost";

        NetManager netManager;
        NetPacketProcessor packetProcessor = new();
        readonly bool isServer;
        readonly bool isClient;

        public BaseAppNetwork(bool isServer)
        {
            this.isServer = isServer;
            this.isClient = !isServer;

            netManager = new NetManager(this)
            {
                AutoRecycle = true,
                EnableStatistics = true,
                IPv6Enabled = false,
                SimulateLatency = true,
                SimulationMinLatency = 50,
                SimulationMaxLatency = 60,
                SimulatePacketLoss = false,
                SimulationPacketLossChance = 10
            };

            // registry first!!
            packetProcessor.RegisterNestedType<GeneralObjectPacket>();
            packetProcessor.SubscribeReusable<MessageEventPacket, NetPeer>(HandleOnMessageEventRecive);
        }

        public bool Start()
        {
            Log("app running...");
            if (isServer)
            {
                bool startStatus = netManager.Start(port);
                if (!startStatus) return false;
            }
            else
            {
                bool startStatus = netManager.Start();
                if (!startStatus) return false;

                netManager.Connect(address, port, "StardewValley");
            }

            Log("app started");
            return true;
        }


        Dictionary<string, Delegate> m_onMessageDelegateMap = new();
        public void RegisterEvent(string msgName, Delegate func)
        {
            m_onMessageDelegateMap.TryAdd(msgName, func);
        }
        void HandleOnMessageEventRecive(MessageEventPacket msg, NetPeer peer)
        {
            try
            {
                Console.WriteLine("try deserialize msg: " + msg.name);
                var args = msg.UnpackArgs();

                if (m_onMessageDelegateMap.TryGetValue(msg.name, out var callback))
                {
                    try
                    {
                        Console.WriteLine("trey invoke callback msg: " + msg.name);
                        Console.WriteLine(" - args len: " + args.Length);

                        callback.DynamicInvoke(args);
                    }
                    catch (Exception ex)
                    {
                        Log(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        NetDataWriter m_sendEventWriter = new();
        public void SendEvent(string name, object[] args)
        {
            try
            {
                if (name.Length >= 200)
                {
                    Log("can't send event name length more than 200!");
                    return;
                }

                var msg = new MessageEventPacket();
                msg.name = name;
                msg.PackArgs(args);

                // incorrect data
                if (msg.TotalArgsBytes >= 65000)
                {
                    Log($"error can't send event: {name}, " +
                        $"bytes length it overflow: {msg.TotalArgsBytes}");
                    return;
                }

                m_sendEventWriter.Reset();
                packetProcessor.Write(m_sendEventWriter, msg);
                netManager.SendToAll(m_sendEventWriter, DeliveryMethod.ReliableOrdered);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error try SendEvent(name, args)");
                Console.WriteLine(ex);
                return;
            }
        }

        public void Log(object msg)
        {
            var prefix = isServer ? "SERVER" : "CLIENT";
            Console.WriteLine($"[{prefix}] " + msg);
        }

        public void PerformUpdate()
        {
            netManager.PollEvents();
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            Log("On connect req: " + request);
            request.Accept();
            Log(" accept it!");
        }
        public void OnPeerConnected(NetPeer peer)
        {
            Log("on peer connected: " + peer.Id);
        }
        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
        }
        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Console.WriteLine($"on network error: {endPoint.Address}:{endPoint.Port}");
            Console.WriteLine($" - socketError: {socketError}");
        }
        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            packetProcessor.ReadAllPackets(reader, peer);
        }
        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

    }
}
