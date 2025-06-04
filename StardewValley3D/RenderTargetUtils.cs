using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewValley3D
{
    internal static class RenderTargetUtils
    {
        static RenderTarget2D tempTarget;
        static RenderTarget2D gameTarget;

        public static void BeginUseForTemp()
        {
            var currentTarget = Game1.graphics.GraphicsDevice
                .GetRenderTargets()[0].RenderTarget as RenderTarget2D;

            if (gameTarget is null && currentTarget != tempTarget)
            {
                gameTarget = currentTarget;
            }

            if (tempTarget is null)
            {
                tempTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, 1, 1);
            }

            Game1.SetRenderTarget(tempTarget);
        }

        public static void RestoreBack()
        {
            if (gameTarget is null)
                return;

            Game1.SetRenderTarget(gameTarget);
        }
    }
}
