using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewValley3D
{
    internal static class Utils
    {
        public static System.Drawing.Color ToColorSystem(this Microsoft.Xna.Framework.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Microsoft.Xna.Framework.Vector2 ToVec2(this xTile.Dimensions.Location rect)
        {
            return new(rect.X, rect.Y);
        }

        public static Vector2 GetViewportTilePos()
        {
            return Game1.viewport.Location.ToVec2() / 64f;
        }

        public static Vector2 ConvertDrawScreenPosToTilePos(Vector2 drawPos)
        {
            var viewportTilePos = GetViewportTilePos();
            return viewportTilePos + (drawPos / 64f);
        }
    }
}
