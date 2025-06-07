using MessagePack.Formatters;
using MessagePack.Resolvers;
using MessagePack;

namespace GuyNetwork
{
    public class CustomMsgPackFormatter
    {
        public static readonly IFormatterResolver resolver
            = CompositeResolver.Create(new IMessagePackFormatter[]{
                new RectangleMsgPackFormatter(),
                new Vector2MsgPackFormatter(),
                new ColorMsgPackFormatter(),
                TypelessFormatter.Instance,
            },
            new IFormatterResolver[] {
                StandardResolver.Instance
            }
         );

    }
}
