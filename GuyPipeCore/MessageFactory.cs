namespace GuyPipeCore;

internal static class MessageFactory
{
    static readonly string prefixEventName = typeof(MessageFactory).FullName;
    public static readonly string BeginMsg = BuildEventName("BeginMessageFactory");
    public static readonly string EndMsg = BuildEventName("EndMessageFactory");
    static string BuildEventName(string evName)
    {
        return $"{prefixEventName}.{evName}";
    }
}

