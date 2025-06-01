using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDummy
{
    public sealed class FPSCounter
    {
        public int tickFPSCounter;
        public int fps;
        readonly Stopwatch sw = new();
        public bool isNewFPS = false;

        public FPSCounter()
        {
            sw.Start();
        }

        public void Update()
        {
            tickFPSCounter++;
            isNewFPS = false;

            var ms = sw.ElapsedMilliseconds;
            if (ms >= 1000)
            {
                //now
                fps = tickFPSCounter;

                // reset it
                tickFPSCounter = 0;
                sw.Restart();
                isNewFPS = true;
            }
        }
    }
}
