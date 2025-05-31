using MessagePack.Formatters;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GameDummy
{
    public class ColorMsgPackFormatter : IMessagePackFormatter<System.Drawing.Color>
    {
        public Color Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            options.Security.DepthStep(ref reader);
            if (reader.ReadArrayHeader() != 4)
            {
                throw new MessagePackSerializationException($"Invalid {typeof(Color)} format");
            }

            int r = reader.ReadInt32();
            int g = reader.ReadInt32();
            int b = reader.ReadInt32();
            int a = reader.ReadInt32();
            return Color.FromArgb(a, r, g, b);
        }

        public void Serialize(ref MessagePackWriter writer, Color value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);
            writer.Write(value.R);
            writer.Write(value.G);
            writer.Write(value.B);
            writer.Write(value.A);
        }
    }
}
