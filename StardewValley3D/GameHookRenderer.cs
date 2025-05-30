using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StardewValley3D;

[HarmonyPatch]
static class GameHookRenderer
{
    static RenderTarget2D myRenderTarget;
    static RenderTarget2D backupRenderTarget;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Furniture), "draw")]
    static void Prefix_Furniture_draw(
        Furniture __instance,
        SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
    {
        backupRenderTarget = (RenderTarget2D)Game1.graphics
            .GraphicsDevice.GetRenderTargets().First().RenderTarget;


        if (myRenderTarget is null
            || myRenderTarget.Height * myRenderTarget.Width
                != backupRenderTarget.Height * backupRenderTarget.Width)
        {
            myRenderTarget = new RenderTarget2D(
                Game1.graphics.GraphicsDevice,
                backupRenderTarget.Width,
                backupRenderTarget.Height);
        }

        Game1.graphics.GraphicsDevice.SetRenderTarget(myRenderTarget);
        Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

        spriteBatch.End();
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.Default,
            RasterizerState.CullNone);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Furniture), "draw")]
    static void Postfix_Furniture_draw(
      Furniture __instance,
      SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
    {
        spriteBatch.End();

        // restore
        Game1.graphics.GraphicsDevice.SetRenderTarget(backupRenderTarget);
        spriteBatch.Begin();


        var width = myRenderTarget.Width;
        var height = myRenderTarget.Height;
        var bytes = new byte[width * height * 4];
        var st = Stopwatch.StartNew();
        myRenderTarget.GetPixelsFaster(ref bytes);
        st.Stop();
        //Console.WriteLine($"Read Pixels time: {st.Elapsed.TotalMilliseconds}ms");
        //Console.WriteLine($" width: {width}, height: {height}");

        var server = MainEntry.server;
        server.SendEvent("Furniture.draw()", [bytes, width, height, __instance.name]);
    }


    internal static void SavePixelsToPng(byte[] bytePixels, int width, int height, string fileName)
    {
        var texture = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
        Color[] colorPixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                var r = bytePixels[i * 4 + 0];
                var g = bytePixels[i * 4 + 1];
                var b = bytePixels[i * 4 + 2];
                var a = bytePixels[i * 4 + 3];
                colorPixels[i].R = r;
                colorPixels[i].G = g;
                colorPixels[i].B = b;
                colorPixels[i].A = a;
            }
        }
        texture.SetData(colorPixels);
        var saveDir = Path.Combine(MainEntry.HelperSingleton.DirectoryPath, "image_saves");
        fileName += ".png";
        var fullPath = Path.Combine(saveDir, fileName);
        if (Directory.Exists(saveDir) is false)
            Directory.CreateDirectory(saveDir);

        using (var stream = File.Open(fullPath, FileMode.Create))
        {
            texture.SaveAsPng(stream, width, height);
            Console.WriteLine("saved png: " + fileName);
        }
    }
}
