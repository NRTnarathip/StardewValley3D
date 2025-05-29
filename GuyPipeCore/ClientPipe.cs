using MessagePack;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace GuyPipeCore;

public class ClientPipe : BasePipe
{
    NamedPipeClientStream clientPipe;

    public ClientPipe(string pipeName) : base(pipeName, false)
    {
        this.clientPipe = this.pipeStream as NamedPipeClientStream;
    }

    public void StartConnect()
    {
        Log("Waiting for server connection...");
        clientPipe.Connect();
        Log("connected!");
    }

    Dictionary<string, List<Delegate>> m_registryEvent = new();

    public void RegisterEvent(string name, Delegate callback)
    {
        if (m_registryEvent.TryGetValue(name, out var registry) is false)
        {
            registry = new();
            m_registryEvent.Add(name, registry);
        }

        registry.Add(callback);
    }

    public void ProcessMsgFactory()
    {
        var reader = new BinaryReader(this.pipeStream);
        var reciveEventList = new List<SendEventMsgPack>();
        var stream = this.pipeStream;

        while (true)
        {
            int length = reader.ReadInt32();
            if (length == 0)
            {
                break;
            }

            byte[] data = reader.ReadBytes(length);
            var sendEvent = MessagePackSerializer.Deserialize<SendEventMsgPack>(data);
            reciveEventList.Add(sendEvent);

            // recive message with per tick
            if (sendEvent.name == MessageFactory.EndMsg)
            {
                break;
            }
        }

        foreach (var msg in reciveEventList)
        {
            if (m_registryEvent.TryGetValue(msg.name, out var callbackList))
            {
                foreach (var callback in callbackList)
                {
                    try
                    {
                        callback.DynamicInvoke(msg.args);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
    }
}
