using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using StardewValleyAR;
using System.Net;
using System.Net.Sockets;

public class MonoApp : INetEventListener
{
    readonly NetManager server;
    NetPacketProcessor packetProcessor = new();

    public MonoApp()
    {
        server = new NetManager(this)
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
    }

    void Log(object msg)
    {
        Console.WriteLine("[MonoApp] " + msg);
    }

    public int tickCounter = 0;
    public bool Start()
    {
        Log("Starting server MonoApp...");
        if (server.Start(GameConfig.port) is false)
        {
            Console.WriteLine("failed server, already on port: " + GameConfig.port);
            return false;
        }

        Console.WriteLine("Started server");
        return true;
    }

    public void PerformUpdate()
    {
        tickCounter++;
        server.PollEvents();
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

            var lz4Options = MessagePackSerializerOptions
                .Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bytes = MessagePackSerializer.Serialize(args, lz4Options);

            // incorrect data
            if (bytes.Length >= 65000)
            {
                Log($"error can't send event: {name}, " +
                    $"bytes length it overflow: {bytes.Length}");
                return;
            }

            var msg = new MessageEventPacket();
            msg.name = name;
            msg.bytes = bytes;

            m_sendEventWriter.Reset();
            packetProcessor.Write(m_sendEventWriter, msg);
            server.SendToAll(m_sendEventWriter, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error try SendEvent(name, args)");
            Console.WriteLine(ex);
            return;
        }
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Log("on connection req: " + request.RemoteEndPoint);
        request.Accept();
        Log("accept req");
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
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
    }

}
