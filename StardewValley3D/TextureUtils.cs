using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using MonoGame.OpenGL;
using System.Runtime.InteropServices;
using System.Collections;
using StardewValley;
using SkiaSharp;

namespace StardewValley3D;

internal static class TextureUtils
{
    public unsafe static void GetPixelsFaster(this RenderTarget2D renderTarget, ref byte[] pixels)
    {
        var width = renderTarget.Width;
        var height = renderTarget.Height;
        const int pixelSize = 4;
        int glTexture = renderTarget.glTexture;
        fixed (byte* p = pixels)
        {
            var pointer = (IntPtr)p;

            GL.BindTexture(TextureTarget.Texture2D, glTexture);
            GL.PixelStore(PixelStoreParameter.PackAlignment, pixelSize);
            GL.GetTexImageInternal(TextureTarget.Texture2D, 0,
                renderTarget.glFormat, renderTarget.glType, pointer);
        }
    }

    public unsafe static void CopyColorsToBytes(Color[] colors, ref byte[] bytes)
    {
        fixed (Color* srcPtr = colors)
        fixed (byte* dstPtr = bytes)
        {
            System.Buffer.MemoryCopy(srcPtr, dstPtr, bytes.Length, colors.Length * 4);
        }
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
        var saveDir = Path.Combine(ModEntry.HelperSingleton.DirectoryPath, "image_saves");
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

    static Dictionary<Texture2D, Color[]> m_textureFullPixelsMap = new();
    public static void GetPixels(Texture2D texture, Rectangle cropRect, Color[] cropPixels)
    {
        if (m_textureFullPixelsMap.TryGetValue(texture, out var fullPixels) is false)
        {
            fullPixels = new Color[texture.width * texture.height];
            texture.GetData(0,
                new Rectangle(0, 0, texture.width, texture.height),
                fullPixels, 0, fullPixels.Length);

            m_textureFullPixelsMap[texture] = fullPixels;
        }

        int localY = -1;
        for (int fullPixelY = cropRect.Top; fullPixelY < cropRect.Bottom; fullPixelY++)
        {
            localY++;
            int localX = -1;

            for (int fullPixelX = cropRect.Left; fullPixelX < cropRect.Right; fullPixelX++)
            {
                localX++;

                int fullPixelIndex = (fullPixelY * texture.width) + fullPixelX;

                int cropIndex = (localY * cropRect.Width) + localX;

                cropPixels[cropIndex] = fullPixels[fullPixelIndex];
            }
        }
    }
}
