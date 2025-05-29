using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuyPipeCore;

public class ServerPipe : BasePipe
{
    NamedPipeServerStream serverPipe;
    public ServerPipe(string pipeName) : base(pipeName, isServer: true)
    {
        this.serverPipe = this.pipeStream as NamedPipeServerStream;
    }

    public void StartHost()
    {
        Log("Waiting for client connection..");
        serverPipe.WaitForConnection();
        Log("connected!");
    }

    public void BeginMessageFactory()
    {
        SendEvent(MessageFactory.BeginMsg);
    }
    public void EndMessageFactory()
    {
        SendEvent(MessageFactory.EndMsg);
    }
}
