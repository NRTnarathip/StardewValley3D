using MessagePack.Formatters;
using MessagePack.Resolvers;
using MessagePack;
using System.Numerics;
using System.Drawing;

public class CustomMsgPackFormatter
{
    public static readonly IFormatterResolver resolver = CompositeResolver.Create(
         new IMessagePackFormatter[]
         {
                new RectangleMsgPackFormatter(),
                new Vector2MsgPackFormatter(),
                TypelessFormatter.Instance,
         },
         new IFormatterResolver[]
         {
                StandardResolver.Instance
         }
     );

}
