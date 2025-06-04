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

        ObjectRenderer.Init(harmony);
        FarmerRenderer.Init();
        Texture2DCacheManager.Init();

        //setup game pipe
        server = new(true);
        server.Start();
        serverScreenStreaming = new BufferStreaming("Game.Rendered", server);

#if DEBUG && true
        // tester only
        client = new(false);
        client.Start();
        client.EnableAnnotation(this);
        clientScreenStreaming = new BufferStreaming("Game.Rendered", client);
        clientScreenStreaming.OnBufferCompleted += ClientScreenStreaming_onBufferCompleted;
#endif

        // setup game events
        helper.Events.Display.RenderedStep += Display_RenderedStep;
        helper.Events.Display.Rendered += Display_Rendered;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;

        // setup Texture2D Cache
    }
    byte[] lastDecompressPixels;
    Texture2D lastDecompessTexture;
    void ClientScreenStreaming_onBufferCompleted(BufferStreaming streaming)
    {
        byte[]? rawImage = streaming.GetLatestCompletedBytes();
        if (rawImage is null)
        {
            return;
        }

        int width = 0;
        int height = 0;
        lastDecompressPixels = TextureUtils.DecompressImagePixelsLZ4(rawImage, ref width, ref height);
        lastDecompessTexture = TextureUtils.CreateTextureFromRaw(lastDecompressPixels, width, height);
    }

    BufferStreaming? serverScreenStreaming;
    BufferStreaming? clientScreenStreaming;

    private void GameLoop_UpdateTicking(object? sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
    {
        server.PerformUpdate();
        client?.PerformUpdate();

        server.SendEvent("Game:Ticking", [Game1.ticks]);
    }

    private void GameLoop_UpdateTicked(object? sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
    {
        FarmerRenderer.IsDisableDrawToGame = true;
        ObjectRenderer.IsDisableDrawToGame = true;


        Vector2 viewportLeftTop = new(
            Game1.viewport.X,
            Game1.viewport.Y
        );

        server.SendEvent("Camera:Update", [
            viewportLeftTop,
            Game1.ticks,
        ]);

        // update farmers
        foreach (var farmer in Game1.getAllFarmers())
        {
            System.Numerics.Vector2 tilePos = farmer.position.Value.ToVec2System() / 64f;
            string name = farmer.Name;
            long uniqueID = farmer.uniqueMultiplayerID.Value;
            server.SendEvent("Farmer:Update", [
                name,
                uniqueID,
                tilePos,
                Game1.ticks,
            ], LiteNetLib.DeliveryMethod.ReliableUnordered);
        }

        var location = Game1.currentLocation;
        if (location is not null)
        {
            var allObjects = new HashSet<StardewValley.Object>();
            // Update All Furniture 
            foreach (var furniture in location.furniture)
            {
                allObjects.Add(furniture);
            }

            // Update General Objects
            foreach (var obj in location.objects.Values)
            {
                if (allObjects.Contains(obj) is false)
                    allObjects.Add(obj);
            }

            foreach (var o in allObjects)
            {
                var tilePos = o.TileLocation.ToVec2System();
                string name = o.name;
                string guid = $"{o.GetType().FullName}:{name}:{tilePos}";
                server.SendEvent("Object:Update", [
                    guid,
                    name,
                    tilePos,
                ], LiteNetLib.DeliveryMethod.ReliableUnordered);

                if (name.Contains("Furnace"))
                {
                    //Console.WriteLine("update: " + guid);
                    //Console.WriteLine(" class: " + o);
                }
            }
        }

        if (location is not null)
        {
            //Console.WriteLine("location: " + location.name);
        }


        // end game loop
        server.SendEvent("Game:Ticked", [Game1.ticks]);
    }

    private void Display_Rendered(object? sender, StardewModdingAPI.Events.RenderedEventArgs e)
    {
    }

    int lastSendScreenBufferHash = 0;
    int lastSendScreenBufferSize = 0;
    void Display_RenderedStep(object? sender, StardewModdingAPI.Events.RenderedStepEventArgs e)
    {
        if (e.Step is StardewValley.Mods.RenderSteps.FullScene)
        {
            var graphicDevice = e.SpriteBatch.graphicsDevice;
            var render = (RenderTarget2D)graphicDevice.GetRenderTargets().First().RenderTarget;
            byte[] pixels = new byte[render.width * render.height * 4];
            render.GetPixelsFaster(ref pixels);

            byte[] compressPixels = TextureUtils.CompressImagePixelsWithLz4(pixels, render.width, render.height);
            if (compressPixels.Length != lastSendScreenBufferSize)
            {
                lastSendScreenBufferSize = compressPixels.Length;
                var chunkIDSender = serverScreenStreaming.SendToAll(compressPixels);
                server.SendEvent("ScreenStreaming:SenderInfo", [
                    chunkIDSender,
                    Game1.ticks,
                ]);
            }

            // debug
            //drawing
            if (lastDecompessTexture is not null)
            {
                //var b = e.SpriteBatch;
                //b.Draw(lastDecompessTexture,
                //    new Microsoft.Xna.Framework.Vector2(0, 0),
                //    new Microsoft.Xna.Framework.Rectangle(0, 0, 300, 300),
                //    Microsoft.Xna.Framework.Color.White
                //);
            }
        }
    }
}
