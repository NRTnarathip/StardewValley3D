using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class FPSCounter
{
    public int tickFPSCounter;
    public int fps;
    readonly Stopwatch sw = new();

    public long frameTimeMs;
    readonly Stopwatch frameTimer = new();

    public FPSCounter()
    {
        sw.Start();
        frameTimer.Start();
    }

    public void Update()
    {
        tickFPSCounter++;
        var ms = sw.ElapsedMilliseconds;
        if (ms >= 1000)
        {
            //now
            fps = tickFPSCounter;

            // reset it
            tickFPSCounter = 0;
            sw.Restart();
        }

        frameTimeMs = frameTimer.ElapsedMilliseconds;
        frameTimer.Restart();
    }
}
