using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace GuyPipeCore;

public class ClientPipe : BaseGuyPipe
{
    public ClientPipe(string pipeName) : base(pipeName, false)
    {

    }

    public delegate void SendEventCallbackDelegate(object[] args);

    Dictionary<string, SendEventCallbackDelegate> m_registryEvent = new();
    public void RegisterEvent(string name, SendEventCallbackDelegate callback)
    {
        m_registryEvent.TryAdd(name, callback);
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
            if (m_registryEvent.TryGetValue(msg.name, out var callback))
            {
                try
                {
                    callback.Invoke(msg.args);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
