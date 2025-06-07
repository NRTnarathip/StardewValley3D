using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace GuyNetwork
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
                    var syncListManager = server.syncListManager;
                    SyncList<string> players = new("players");
                    syncListManager.RegisterSyncList(players);

                    players.Add("Guy");
                    players.Add("Hello");
                    players.Add("World");

                    var msg = new List<string>();

                    var random = new Random();

                    while (true)
                    {
                        try
                        {
                            // begin frame
                            server.PerformUpdate();

                            // game ticking
                            var randValue = random.NextDouble();
                            if (randValue <= 0.55)
                            {
                                players.Add($"new random:{randValue}");
                            }
                            else
                            {
                                if (players.Count >= 3)
                                {
                                    var item = players[2];
                                    players.Remove(item);
                                }
                            }

                            if (server.fpsCounter.isNewFPS)
                            {
                                server.Log("server players: " + players.Count);
                            }

                            // end ticked
                            server.EndUpdate();

                            await Task.Delay(16);
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

                    var syncListManager = client.syncListManager;
                    var players = new SyncList<string>("players");
                    syncListManager.RegisterSyncList(players);

                    while (true)
                    {
                        try
                        {
                            // begin frame
                            client.PerformUpdate();

                            // ticking
                            if (client.fpsCounter.isNewFPS)
                            {
                                client.Log("total players: " + players.Count);
                            }

                            //end update
                            client.EndUpdate();

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