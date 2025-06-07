using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace GuyNetwork
{
    public class BaseAppNetwork : INetEventListener
    {
        public const int port = 10055;
        public const string address = "localhost";

        public NetManager netManager { get; private set; }
        public readonly NetPacketProcessor packetProcessor = new();
        public readonly bool isServer;
        public readonly bool isClient;
        public readonly SyncListManager syncListManager;

        public BaseAppNetwork(bool isServer)
        {
            this.isServer = isServer;
            this.isClient = !isServer;

            netManager = new NetManager(this)
            {
                AutoRecycle = true,
                EnableStatistics = true,
                IPv6Enabled = false,
                SimulateLatency = false,
                SimulatePacketLoss = false,
            };

            // registry first!!
            packetProcessor.RegisterNestedType<GeneralObjectPacket>();
            packetProcessor.SubscribeReusable<MessageEventPacket, NetPeer>(HandleOnMessageEventRecive);

            // addon
            syncListManager = new(this);
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

        public void SendToAll(NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            netManager.SendToAll(writer, deliveryMethod);
        }

        List<ObjectEnableAnnotation> objectListBindAnnotation = new();
        public void EnableAnnotation(object objectToUse)
        {
            objectListBindAnnotation.Add(new ObjectEnableAnnotation(objectToUse));
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
                var args = msg.UnpackArgs();

                if (m_onMessageDelegateMap.TryGetValue(msg.name, out var callback))
                {
                    try
                    {
                        callback.DynamicInvoke(args);
                    }
                    catch (Exception ex)
                    {
                        Log("Error On Message: " + msg.name);
                        Log(ex);
                    }
                }
                else
                {
                    foreach (var objAnnotation in objectListBindAnnotation)
                    {
                        var methods = objAnnotation.GetMethodsFromAnnotationFullName(OnMessageAttribute.TypeFullName);
                        if (methods is not null)
                        {
                            foreach (var method in methods)
                            {
                                var onMessageAttribute = objAnnotation
                                    .GetBaseAnnotationAttributesFromMethodInfo(method).First()
                                    as OnMessageAttribute;

                                if (onMessageAttribute.MessageName == msg.name)
                                {
                                    try
                                    {
                                        method.Invoke(objAnnotation.obj, args);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log("Error On Message: " + msg.name);
                                        Log(ex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error on message: " + msg.name);
                Log(ex);
            }
        }

        NetDataWriter m_sendEventWriter = new();
        public void SendEvent(string name, object[] args)
        {
            SendEvent(name, args, DeliveryMethod.ReliableOrdered);
        }
        public void SendEvent(string name, object[] args,
            DeliveryMethod deliveryMethod)
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
                netManager.SendToAll(m_sendEventWriter, deliveryMethod);
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
        public readonly FPSCounter fpsCounter = new();
        public int ticks { get; private set; } = 0;
        public void PerformUpdate()
        {
            ticks++;

            netManager.PollEvents();
            syncListManager.PollEvents();

            fpsCounter.Update();
        }

        public void EndUpdate()
        {
            syncListManager.EndUpdate();
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            Log("On connect req: " + request);
            request.Accept();
            Log(" accept it!");
        }

        public NetPeer lastPeerConnected { get; private set; }
        public NetPeer firstPeerConnnected { get; private set; }
        public bool isConncetedToServer { get; private set; }
        public void OnPeerConnected(NetPeer peer)
        {
            if (firstPeerConnnected is null)
            {
                firstPeerConnnected = peer;
            }

            lastPeerConnected = peer;
            if (isClient)
            {
                isConncetedToServer = true;
            }
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
