using MessagePack;
using System.IO.Pipes;

namespace GuyPipeCore;

public abstract class BaseGuyPipe
{
    public readonly PipeStream pipeStream;
    public readonly bool isServer;
    public readonly bool isClient;
    public PipeDirection pipeDirection;

    public BaseGuyPipe(string pipeName, bool isServer)
    {
        this.isServer = isServer;
        this.isClient = !isServer;
        this.pipeDirection = isServer ? PipeDirection.InOut : PipeDirection.In;

        if (isServer)
        {
            var server = new NamedPipeServerStream(pipeName, pipeDirection);
            pipeStream = server;

            Log("waiting server connection..");
            server.WaitForConnection();
            Log("server connected!");
        }
        else
        {
            var client = new NamedPipeClientStream(pipeName);
            pipeStream = client;

            Log("waiting client connection...");
            client.Connect();
            Log("client connected!");
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

