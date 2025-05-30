using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using MonoGame.OpenGL;
using System.Runtime.InteropServices;
using System.Collections;

namespace StardewValley3D;

internal static class TextureCopyOptimize
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
}
