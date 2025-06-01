using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace GameDummy
{
    static class Program
    {
        private static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                try
                {

                    var server = new BaseAppNetwork(true);
                    if (server.Start() is false)
                    {
                        return;
                    }

                    var streaming = new BufferStreaming("Game.Renderer", server);

                    var random = new Random();
                    while (true)
                    {
                        try
                        {
                            server.PerformUpdate();
                            var screenBytes = new byte[50_000 + (int)(random.NextSingle() * 100_000)];
                            streaming.SendToAll(screenBytes);
                            await Task.Delay(1);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });

            Task.Run(async () =>
            {
                try
                {
                    var client = new BaseAppNetwork(false);
                    client.Start();

                    var streaming = new BufferStreaming("Game.Renderer", client);
                    var streamingFPSCounter = new FPSCounter();
                    streaming.onBufferCompleted += (streaming) =>
                    {
                        streamingFPSCounter.Update();
                        if (streamingFPSCounter.isNewFPS)
                        {
                            Console.WriteLine("fps: " + streamingFPSCounter.fps);
                            Console.WriteLine(" total bytes: " + streaming.totalBufferSize);
                        }
                    };

                    while (true)
                    {
                        try
                        {
                            client.PerformUpdate();
                            if (client.fpsCounter.isNewFPS)
                            {
                                Console.WriteLine("client fps: " + client.fpsCounter.fps);
                            }

                            await Task.Delay(1);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}