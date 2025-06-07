using LiteNetLib.Utils;
using MessagePack;

namespace GuyNetwork
{
    // support difference Assembly version
    public struct GeneralObjectPacket : INetSerializable
    {
        public string? typeFullName;
        public byte[]? bytes;

        // don't call this
        public void Deserialize(NetDataReader reader)
        {
            typeFullName = reader.GetString();
            if (typeFullName is "null")
                return;

            int byteLength = reader.GetInt();
            bytes = new byte[byteLength];
            reader.GetBytes(bytes, byteLength);
        }

        // don't call this
        public void Serialize(NetDataWriter writer)
        {
            if (typeFullName is null || typeFullName is "null")
            {
                writer.Put("null");
                return;
            }

            writer.Put(typeFullName);
            writer.Put(bytes.Length);
            writer.Put(bytes);
        }

        // API
        public T? ReadValue<T>()
        {
            if (typeFullName is "null")
                return default;

            var type = TypeFinder.GetTypeFromFullName(typeFullName);
            return (T)MessagePackSerializer.Deserialize(type, bytes, MyMessagePackSerializerOptions.DefaultOption);
        }
        public object? ReadValue()
        {
            return ReadValue<object>();
        }

        public void WriteValue(object? value)
        {
            if (value is null)
                return;

            typeFullName = value.GetType().FullName;
            bytes = MessagePackSerializer.Serialize(value, MyMessagePackSerializerOptions.DefaultOption);
        }

    }
}