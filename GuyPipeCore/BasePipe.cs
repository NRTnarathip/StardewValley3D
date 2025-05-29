using MessagePack;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace GuyPipeCore;

public abstract class BasePipe
{
    public readonly PipeStream pipeStream;
    public readonly bool isServer;
    public readonly bool isClient;

    public BasePipe(string pipeName, bool isServer)
    {
        this.isServer = isServer;
        this.isClient = !isServer;

        if (isServer)
        {
            pipeStream = new NamedPipeServerStream(
                pipeName,
                PipeDirection.Out
            );
        }
        else
        {
            pipeStream = new NamedPipeClientStream(
                ".",
                pipeName,
                PipeDirection.In
            );
        }
    }

    public void SendEvent(string eventName, object[]? args = null)
    {
        if (args == null)
        {
            args = [];
        }

        var sendEventData = new SendEventMsgPack(eventName, args);
        byte[] dataBytes = MessagePackSerializer.Serialize(sendEventData);
        var dataLength = dataBytes.Length;
        byte[] lengthBytes = BitConverter.GetBytes(dataLength);

        pipeStream.Write(lengthBytes, 0, 4);
        pipeStream.Write(dataBytes, 0, dataLength);
    }

    public void Log(object msg)
    {
        var prefix = this.isClient ? "Client" : "Server";
        Console.WriteLine($"[{prefix}]  {msg}");
    }
}

