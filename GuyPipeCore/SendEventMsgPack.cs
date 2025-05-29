using MessagePack;

namespace GuyPipeCore;

[MessagePackObject]
public struct SendEventMsgPack
{
    [Key(0)]
    public readonly string name;

    [Key(1)]
    public readonly object[] args;

    [Key(2)]
    public readonly int argsLength;

    public SendEventMsgPack(string eventName, object[]? args)
    {
        if (args == null)
            args = [];

        this.name = eventName;
        this.args = args;
        this.argsLength = args.Length;
    }
}


