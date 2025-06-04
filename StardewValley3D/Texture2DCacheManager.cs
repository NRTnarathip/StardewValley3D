using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewValley3D
{
    internal static class Texture2DCacheManager
    {
        public static void Init()
        {
            GameHookRenderer.eventOnSpriteBatchDraw1 += GameHookRenderer_eventOnSpriteBatchDraw;
        }

        private static void GameHookRenderer_eventOnSpriteBatchDraw(SpriteBatch spriteBatch, Texture2D texture, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Rectangle? sourceRectangle, Microsoft.Xna.Framework.Color color, float rotation, Microsoft.Xna.Framework.Vector2 origin, Microsoft.Xna.Framework.Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            //string uniqPath = texture.GetUniquePath();
        }

        public static string? GetUniquePath(this Texture2D t)
        {
            return t.Name;
        }
    }
}
