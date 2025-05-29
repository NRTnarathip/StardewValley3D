using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuyPipeCore;

public class ServerPipe : BaseGuyPipe
{
    public ServerPipe(string pipeName) : base(pipeName, isServer: true)
    {
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
