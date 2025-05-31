using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewValley3D;

internal static class VectorDotnetUtils
{
    static System.Numerics.Vector2 vector2Cache;
    public static System.Numerics.Vector2 ToVec2(this Microsoft.Xna.Framework.Vector2 vec2)
    {
        vector2Cache.X = vec2.X;
        vector2Cache.Y = vec2.Y;
        return vector2Cache;
    }
}
