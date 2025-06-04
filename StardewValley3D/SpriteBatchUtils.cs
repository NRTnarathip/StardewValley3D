using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewValley3D
{
    public class SpriteBatchBeginState
    {
        public SpriteSortMode backupSortMode;
        public BlendState backupBlendState;
        public SamplerState backupSamplerState;
        public DepthStencilState backupDepthStencilState;
        public RasterizerState backupRasterizerState;
        public Effect backupEffect;
        public Matrix? backupMatrix;

        public void Backup(SpriteBatch b)
        {
            backupSortMode = b._sortMode;
            backupBlendState = b._blendState;
            backupSamplerState = b._samplerState;
            backupDepthStencilState = b._depthStencilState;
            backupRasterizerState = b._rasterizerState;
            backupEffect = b._effect;
            backupMatrix = b._spriteEffect?.TransformMatrix;
        }
        public void BackupAndEnd(SpriteBatch b)
        {
            Backup(b);
            b.End();
        }

        public void Begin(SpriteBatch b)
        {
            b.Begin(
               backupSortMode,
               backupBlendState,
               backupSamplerState,
               backupDepthStencilState,
               backupRasterizerState,
               backupEffect,
               backupMatrix
           );
        }
    }

    [HarmonyPatch]
    internal static class SpriteBatchUtils
    {
        static Dictionary<string, SpriteBatchBeginState> m_spriteBatchBeginStateMap = new();
        static void BackupBeginState(string id, SpriteBatch b)
        {
            if (m_spriteBatchBeginStateMap.TryGetValue(id, out var state) is false)
            {
                state = new();
                m_spriteBatchBeginStateMap.Add(id, state);
            }

        }
    }
}
