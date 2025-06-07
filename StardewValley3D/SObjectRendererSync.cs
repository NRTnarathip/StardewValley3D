using GuyNetwork;
using LiteNetLib.Utils;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StardewValley3D
{
    [MessagePackObject]
    public class ObjectDrawState
    {
        [Key(0)]
        public int drawCallCounter;
        [Key(1)]
        public string guid;
        [Key(2)]
        public string textureName;
        [Key(3)]
        public Rectangle srcRect;
        [Key(4)]
        public Vector2 drawTilePos;
        [Key(5)]
        public Vector2 scale;
        [Key(6)]
        public Vector2 originPixel;
        [Key(7)]
        public Color color;
        [Key(8)]
        public int effects;
        [Key(9)]
        public float layerDepth;
    }

    internal class SObjectRendererSync
    {

        Dictionary<string, ObjectDrawState> m_prevDrawMap = new();
        List<ObjectDrawState> m_prevDrawList = new();

        Dictionary<string, ObjectDrawState> m_currentDrawMap = new();
        List<ObjectDrawState> m_currentDrawList = new();

        internal void OnDraw(
            int spriteBatchDrawCallCounter, string guid, string? textureFileName,
            Rectangle srcRect, Vector2 drawScreenPos, Vector2 drawTilePos,
            Vector2 scale, Vector2 originPixel, Color color, int effects,
            float layerDepth)
        {
            var drawKey = $"{guid},drawCounter:{spriteBatchDrawCallCounter}";
            m_currentDrawMap.TryAdd(drawKey, new()
            {
                color = color,
                drawCallCounter = spriteBatchDrawCallCounter,
                drawTilePos = drawTilePos,
                effects = effects,
                guid = guid,
                layerDepth = layerDepth,
                originPixel = originPixel,
                srcRect = srcRect,
                scale = scale,
                textureName = textureFileName,
            });
        }

        internal void OnTicking()
        {
            //reset
            m_currentDrawMap.Clear();
            m_currentDrawList.Clear();
        }

        internal void OnDisplayRendered()
        {
            // update current value
            m_currentDrawList = new(m_currentDrawMap.Values);
            //Console.WriteLine("total draw object in this frame: " + m_currentDrawList.Count);

            Dictionary<string, ObjectDrawState> newValueMap = new();
            Dictionary<string, ObjectDrawState> removeValueMap = new();
            Dictionary<string, Tuple<ObjectDrawState, ObjectDrawState>> alreadyMap = new();

            foreach (var (k, currentValue) in m_currentDrawMap)
            {
                // already exist
                if (m_prevDrawMap.TryGetValue(k, out var prevValue))
                {
                    //Console.WriteLine("already drawing: " + k);
                    alreadyMap.TryAdd(k, new(prevValue, currentValue));
                }
                else
                {
                    // on new key
                    newValueMap.Add(k, currentValue);
                    Console.WriteLine("on new key: " + k);
                }
            }

            foreach (var (k, prevValue) in m_prevDrawMap)
            {
                if (m_currentDrawMap.TryGetValue(k, out var currentValue))
                {
                    alreadyMap.TryAdd(k, new(prevValue, currentValue));
                }
                else
                {
                    // on delete key
                    removeValueMap.Add(k, prevValue);
                }
            }

            if (newValueMap.Count > 0)
                Console.WriteLine("on new values: " + newValueMap.Count);

            if (removeValueMap.Count > 0)
                Console.WriteLine("on remove values: " + removeValueMap.Count);

            List<Tuple<ObjectDrawState, ObjectDrawState>> drawStateDirtyList = new();
            foreach (var (k, v) in alreadyMap)
            {
                var prev = v.Item1;
                var current = v.Item2;
                var option = MessagePackSerializerOptions.Standard
                    .WithResolver(CompositeResolver.Create(
                        new IMessagePackFormatter[] {
                            new RectangleMsgPackFormatter(),
                            new Vector2MsgPackFormatter(),
                            new ColorMsgPackFormatter(),
                        },
                        new IFormatterResolver[] {
                            StandardResolver.Instance
                        }));
                var prevBytes = MessagePackSerializer.Serialize(prev, option);
                var currentBytes = MessagePackSerializer.Serialize(current, option);
                if (prevBytes.SequenceEqual(currentBytes) is false)
                {
                    Console.WriteLine($"key: {k} value has changed!!");
                    drawStateDirtyList.Add(new(prev, current));

                    var type = prev.GetType();
                    var fields = type.GetFields();
                    foreach(var f in fields)
                    {
                        var prevFieldValue = f.GetValue(prev);
                        var currentFieldValue = f.GetValue(current);
                        if(prevFieldValue.Equals(currentFieldValue) is false) {
                            //Console.WriteLine($" - {f.Name}: 1:{prevFieldValue} <> 2:{currentFieldValue}");
                        }
                    }
                }
            }

            // cache it
            m_prevDrawMap = new(m_currentDrawMap);
            m_prevDrawList = new(m_currentDrawList);
        }
    }
}
