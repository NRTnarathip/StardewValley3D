using MessagePack;
using MessagePack.Formatters;
using System.Drawing;
using System;

namespace GuyNetwork
{

    public class RectangleMsgPackFormatter : IMessagePackFormatter<Rectangle>
    {
        public Rectangle Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            // Depth step is the first step to parse any file.
            options.Security.DepthStep(ref reader);

            var h = reader.ReadArrayHeader();
            if (h != 4)
                throw new ArgumentException(h.ToString());

            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            return new Rectangle(x, y, width, height);
        }

        public void Serialize(ref MessagePackWriter writer, Rectangle value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Width);
            writer.Write(value.Height);
        }
    }
}
