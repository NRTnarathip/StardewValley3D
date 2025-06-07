using MessagePack;
using MessagePack.Formatters;
using System.Numerics;

namespace GuyNetwork
{
    public class Vector2MsgPackFormatter : IMessagePackFormatter<Vector2>
    {
        public Vector2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            options.Security.DepthStep(ref reader);

            var h = reader.ReadArrayHeader();
            if (h != 2)
                throw new MessagePackSerializationException("Invalid Vector2 format");

            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            return new Vector2(x, y);
        }

        public void Serialize(ref MessagePackWriter writer, Vector2 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(2);
            writer.Write(value.X);
            writer.Write(value.Y);
        }
    }
}
