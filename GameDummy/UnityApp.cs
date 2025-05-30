using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using StardewValleyAR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class UnityApp : INetEventListener
{
    NetManager client;
    NetPacketProcessor packetProcessor = new();

    public UnityApp()
    {
        client = new NetManager(this)
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

        packetProcessor.SubscribeReusable<MessageEventPacket, NetPeer>(HandleOnMessageEventRecive);
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
            //Console.WriteLine("try deserialize msg: " + msg.name);
            //Console.WriteLine("- bytes: " + msg.bytes.Length);

            var lz4Options = MessagePackSerializerOptions
                .Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            object[] args = MessagePackSerializer
                .Deserialize<object[]>(msg.bytes, lz4Options);

            if (m_onMessageDelegateMap.TryGetValue(msg.name, out var callback))
            {
                try
                {
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

    public void Log(object msg)
    {
        Console.WriteLine($"[UnityApp] " + msg);
    }

    public void Start()
    {
        Log("Unity app running...");
        client.Start();
        client.Connect(GameConfig.address, GameConfig.port, "StardewValley");
    }

    public void PerformUpdate()
    {
        client.PollEvents();
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Log("On connect req: " + request);
    }
    public void OnPeerConnected(NetPeer peer)
    {
        Log("peer connected: " + peer.Id);
    }
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
    }
    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
    }
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        //Log("net latency update: " + peer.Id + ", latency: " + latency);
    }
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        packetProcessor.ReadAllPackets(reader, peer);
    }
    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
    }

}
