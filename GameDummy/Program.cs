using GuyPipeCore;
using StardewValley.Mods;

internal class Program
{
    private static async Task Main(string[] args)
    {

#if false
        NewThreadClient();
        NewThreadServer();
        while (true)
        {
            await Task.Delay(1000);
            Console.WriteLine("main thread wait 1 sec.");
        }
#else
        await Task.Run(NewThreadClientStardewValley);
#endif
    }

    private static async Task NewThreadClientStardewValley()
    {
        var client = new ClientPipe("StardewValley");
        client.StartConnect();

        client.RegisterEvent("Helper.Events.Display.RenderedStep", (RenderSteps step) =>
        {
            client.Log("on RenderedStep: " + step);
        });

        client.RegisterEvent("Game1.ticks", (int tick) =>
        {
            client.Log("Game1.ticks: " + tick);
        });


        while (true)
        {
            await Task.Delay(1);
            client.ProcessMsgFactory();
        }
    }

    static void NewThreadClient()
    {
        new Thread(() =>
        {
            var client = new ClientPipe("Game");
            client.StartConnect();

            client.RegisterEvent("Game.TickCounter", (object[] args) =>
            {
                int tick = (int)args[0];
                client.Log("on server game ticks: " + tick);
            });

            client.ProcessMsgFactory();

            while (true)
            {
                Thread.Sleep(500);

                client.Log("on game update");
                client.ProcessMsgFactory();
                client.Log("game updated");

                // my code logic
            }
        }).Start();
    }
    static void NewThreadServer()
    {
        new Thread(() =>
        {

            var sv = new ServerPipe("Game");
            sv.StartHost();

            int tickCounter = 0;
            sv.Log("Start Server Pipe");
            while (true)
            {
                Thread.Sleep(1);
                tickCounter++;

                sv.Log("Server Game Tick: " + tickCounter);

                sv.BeginMessageFactory();

                // my code logic
                sv.SendEvent("Game.TickCounter", [tickCounter]);
                for (var i = 0; i < 10; i++)
                {
                    sv.SendEvent("Game.Render", ["player:" + i]);
                }

                sv.EndMessageFactory();
            }

        }).Start();
    }
}