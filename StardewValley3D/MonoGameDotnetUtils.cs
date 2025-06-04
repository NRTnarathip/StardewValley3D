using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewValley3D;

internal static class MonoGameDotnetUtils
{
    static System.Numerics.Vector2 vector2Cache;
    public static System.Numerics.Vector2 ToVec2System(this Microsoft.Xna.Framework.Vector2 vec2)
    {
        vector2Cache.X = vec2.X;
        vector2Cache.Y = vec2.Y;
        return vector2Cache;
    }

    static System.Drawing.Rectangle rectDotnetCache = new System.Drawing.Rectangle();
    public static System.Drawing.Rectangle ToRectSystem(this Microsoft.Xna.Framework.Rectangle srcRect)
    {
        rectDotnetCache.Width = srcRect.Width;
        rectDotnetCache.Height = srcRect.Height;
        rectDotnetCache.X = srcRect.X;
        rectDotnetCache.Y = srcRect.Y;
        return rectDotnetCache;
    }
}

