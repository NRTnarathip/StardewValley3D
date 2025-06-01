using GameDummy;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;
using StardewModdingAPI;
using StardewValley;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace StardewValley3D;

public class ModEntry : Mod
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
        serverScreenStreaming = new BufferStreaming("Game.Rendered", server);

#if DEBUG && false
        // tester only
        client = new(false);
        client.Start();
        client.EnableAnnotation(this);
        clientScreenStreaming = new BufferStreaming("Game.Rendered", client);
        clientScreenStreaming.onBufferCompleted += ClientScreenStreaming_onBufferCompleted;
#endif

        // setup game events
        helper.Events.Display.RenderedStep += Display_RenderedStep;
        helper.Events.Display.Rendered += Display_Rendered;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;

    }
    byte[] lastDecompressPixels;
    Texture2D lastDecompessTexture;
    void ClientScreenStreaming_onBufferCompleted(BufferStreaming streaming)
    {
        var rawImage = new byte[streaming.totalBufferSize];
        for (int i = 0; i < streaming.chunkStreamingPackets.Count; i++)
        {
            var chunk = streaming.chunkStreamingPackets[i];
            Array.Copy(
                chunk.bytes, 0,
                rawImage, i * ChunkStreamingPacket.MaxChunkSize,
                chunk.bytes.Length);
        }

        int width = 0;
        int height = 0;
        lastDecompressPixels = TextureUtils.DecompressImagePixelsLZ4(rawImage, ref width, ref height);
        lastDecompessTexture = TextureUtils.CreateTextureFromRaw(lastDecompressPixels, width, height);
    }

    BufferStreaming? serverScreenStreaming;
    BufferStreaming? clientScreenStreaming;

    [OnMessage("Game1.ticks")]
    void OnMsgGameTicks(int ticks)
    {
        //Console.WriteLine("on msg Game1.ticks: " + ticks);
    }

    private void GameLoop_UpdateTicking(object? sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
    {
        server.PerformUpdate();
        client?.PerformUpdate();
    }

    private void GameLoop_UpdateTicked(object? sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
    {
        server.SendEvent("Game1.ticks", [Game1.ticks], LiteNetLib.DeliveryMethod.Unreliable);

        var location = Game1.currentLocation;
        if (location is not null)
        {
            //Console.WriteLine("location: " + location.name);
        }
    }

    private void Display_Rendered(object? sender, StardewModdingAPI.Events.RenderedEventArgs e)
    {
    }

    void Display_RenderedStep(object? sender, StardewModdingAPI.Events.RenderedStepEventArgs e)
    {
        if (e.Step is StardewValley.Mods.RenderSteps.FullScene)
        {
            var graphicDevice = e.SpriteBatch.graphicsDevice;
            var render = (RenderTarget2D)graphicDevice.GetRenderTargets().First().RenderTarget;
            byte[] pixels = new byte[render.width * render.height * 4];
            render.GetPixelsFaster(ref pixels);

            byte[] compressPixels = TextureUtils.CompressImagePixelsWithLz4(pixels, render.width, render.height);

            serverScreenStreaming.SendToAll(compressPixels);

            // debug
            //drawing
            //if (lastDecompressPixels is not null)
            //{
            //    var b = e.SpriteBatch;
            //    b.Draw(lastDecompessTexture,
            //        new Microsoft.Xna.Framework.Vector2(0, 0),
            //        new Microsoft.Xna.Framework.Rectangle(0, 0, 300, 300),
            //        Microsoft.Xna.Framework.Color.White
            //    );
            //}
        }
    }
}
