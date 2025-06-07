using GuyNetwork;
using MessagePack;
using System.Reflection;

namespace GuyNetwork
{
    public static class MyMessagePackSerializerOptions
    {
        public readonly static MessagePackSerializerOptions DefaultOption
             = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithResolver(CustomMsgPackFormatter.resolver);
    }
    public sealed class MessageEventPacket
    {
        public string name { get; set; }
        public GeneralObjectPacket[] args { get; set; }
        public object[] latestUnpackArgs { get; private set; }

        public object[] UnpackArgs()
        {
            var targetType = typeof(byte);
            latestUnpackArgs = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var valType = TypeFinder.GetTypeFromFullName(arg.typeFullName);
                var val = MessagePackSerializer.Deserialize(valType, arg.bytes,
                    MyMessagePackSerializerOptions.DefaultOption);
                latestUnpackArgs[i] = val;
            }

            return latestUnpackArgs;
        }
        public void PackArgs(object[] inputArgs)
        {
            args = new GeneralObjectPacket[inputArgs.Length];
            TotalArgsBytes = 0;

            for (int i = 0; i < args.Length; i++)
            {
                var val = inputArgs[i];
                var valType = val.GetType();
                args[i] = new()
                {
                    typeFullName = valType.FullName,
                    bytes = MessagePackSerializer.Serialize(valType, val, MyMessagePackSerializerOptions.DefaultOption)
                };

                TotalArgsBytes += args[i].bytes.Length;
            }
        }

        public int TotalArgsBytes { get; private set; }


    }
}