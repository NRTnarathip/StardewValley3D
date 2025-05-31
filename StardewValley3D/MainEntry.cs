using HarmonyLib;
using MessagePack;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System.IO;
using System.IO.Pipes;
using System.Text;
using static StardewValley.Minigames.TargetGame;

namespace StardewValley3D;

public class MainEntry : Mod
{
    public static StardewApp server;
    public static UnityApp client;

    Harmony harmony = new("StardewValley3D");
    public static IModHelper HelperSingleton;

    public override void Entry(IModHelper helper)
    {
        HelperSingleton = helper;
        harmony.PatchAll();

        //setup game pipe
        server = new();
        server.Start();

#if true
        client = new();
        client.Start();
        client.RegisterEvent("Furniture.draw()",
            (byte[] pixels, int width, int height, string name) =>
        {
            //client.Log($"on furniture draw(), name: {name}, pixels len: {pixels.Length}");
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
    }

    private void Display_Rendered(object? sender, StardewModdingAPI.Events.RenderedEventArgs e)
    {
    }

    void Display_RenderedStep(object? sender, StardewModdingAPI.Events.RenderedStepEventArgs e)
    {
    }
}
