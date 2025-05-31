using GameDummy;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Drawing;
using System.Numerics;

namespace StardewValley3D;

public class MainEntry : Mod
{
    public static BaseAppNetwork server;
    public static BaseAppNetwork client;

    Harmony harmony = new("StardewValley3D");
    public static IModHelper HelperSingleton;

    public override void Entry(IModHelper helper)
    {
        HelperSingleton = helper;
        harmony.PatchAll();

        //setup game pipe
        server = new(true);
        server.Start();

#if true
        client = new(false);
        client.Start();
        client.RegisterEvent("Furniture:SpriteBatch:Draw()", (
            string furnitureName,
            byte[] rawPixels,
            System.Drawing.Rectangle srcRect,
            System.Numerics.Vector2 pos,
            System.Numerics.Vector2 scale,
            System.Numerics.Vector2 origin
        ) =>
        {
            client.Log($"on furniture draw(), name: {furnitureName}, pixels len: {rawPixels.Length}");
            //GameHookRenderer.SavePixelsToPng(pixels, width, height, name);
        });
#endif

        // setup game events
        helper.Events.Display.RenderedStep += Display_RenderedStep;
        helper.Events.Display.Rendered += Display_Rendered;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
    }

    private void GameLoop_UpdateTicking(object? sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
    {
        server.PerformUpdate();
        client?.PerformUpdate();
    }

    private void GameLoop_UpdateTicked(object? sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
    {
        server.SendEvent("Game1.ticks", [Game1.ticks]);
        // test
        server.SendEvent("Furniture:SpriteBatch:Draw()", [
            "hello_test_sprite",
            new byte[22*22*4],
            new Rectangle(0, 0, 22, 22),
            new Vector2(22, 11),
            new Vector2(4, 4),
            new Vector2(0, 0),
        ]);
    }

    private void Display_Rendered(object? sender, StardewModdingAPI.Events.RenderedEventArgs e)
    {
    }

    void Display_RenderedStep(object? sender, StardewModdingAPI.Events.RenderedStepEventArgs e)
    {
    }
}
