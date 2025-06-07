using GameDummy;
using HarmonyLib;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System.Drawing;
using System.Numerics;

[MessagePackObject]
public class ObjectDrawState
{
    [Key(0)]
    public int drawCallCounter;
    [Key(1)]
    public string guid;
    [Key(2)]
    public string textureName;
    [Key(3)]
    public Rectangle srcRect;
    [Key(4)]
    public Vector2 drawTilePos;
    [Key(5)]
    public Vector2 scale;
    [Key(6)]
    public Vector2 originPixel;
    [Key(7)]
    public Color color;
    [Key(8)]
    public int effects;
    [Key(9)]
    public float layerDepth;
}


[HarmonyPatch]
internal class Program
{
    private static void Main(string[] args)
    {
        var h = new Harmony("Game Update");
        h.PatchAll();
        var fps = new FPSCounter();
        MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create(
                        new IMessagePackFormatter[] {
                            new RectangleMsgPackFormatter(),
                            new Vector2MsgPackFormatter(),
                            new ColorMsgPackFormatter(),
                        },
                        new IFormatterResolver[] {
                            StandardResolver.Instance
                        }));
        while (true)
        {
            Console.WriteLine(" ");
            Console.WriteLine("new game tick");
            fps.Update();
            GameUpdate();

            Thread.Sleep(1);
        }
    }

    public static void GameUpdate()
    {
        Console.WriteLine("Game Update");
        var drawState = new ObjectDrawState();

        var bytes = MessagePackSerializer.Serialize(drawState);
        Console.WriteLine("pack bytes: " + bytes.Length);
    }
}