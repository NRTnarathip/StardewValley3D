using GameDummy;
using LiteNetLib.Utils;
using MessagePack;
using System.Reflection;

namespace StardewValleyAR
{
    public struct GeneralObjectPacket : INetSerializable
    {
        public string typeFullName;
        public byte[] bytes;

        public void Deserialize(NetDataReader reader)
        {
            typeFullName = reader.GetString();

            int byteLength = reader.GetInt();
            bytes = new byte[byteLength];
            reader.GetBytes(bytes, byteLength);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(typeFullName);
            writer.Put(bytes.Length);
            writer.Put(bytes);
        }
    }

    public sealed class MessageEventPacket
    {
        public string name { get; set; }
        public GeneralObjectPacket[] args { get; set; }
        public object[] UnpackArgs()
        {
            var targetType = typeof(byte);
            object[] resultArgs = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var valType = TypeFinder.GetTypeFromFullName(arg.typeFullName);
                var val = MessagePackSerializer.Deserialize(valType, arg.bytes, ArgsSerializerOption);
            }

            return resultArgs;
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
                    bytes = MessagePackSerializer.Serialize(valType, val, ArgsSerializerOption)
                };

                TotalArgsBytes += args[i].bytes.Length;
            }
        }

        public int TotalArgsBytes { get; private set; }

        public readonly static MessagePackSerializerOptions ArgsSerializerOption
             = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithResolver(CustomMsgPackFormatter.resolver);
    }
}