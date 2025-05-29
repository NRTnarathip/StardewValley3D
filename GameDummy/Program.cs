using GuyPipeCore;
using StardewValley.Mods;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //NewThreadClient();
        //NewThreadServer();
        await Task.Run(NewThreadClientStardewValley);
        while (true)
        {
            Thread.Sleep(1000);
            Console.WriteLine("main thread wait tick.");
        }
    }

    private static async Task NewThreadClientStardewValley()
    {
        var client = new ClientPipe("StardewValley");

        client.RegisterEvent("Events.Display.RenderedStep", (args) =>
        {
            var step = (RenderSteps)args[0];
            client.Log("on rendered step: " + step);
        });

        while (true)
        {
            await Task.Delay(1);
            client.Log("tick");
            client.ProcessMsgFactory();
        }
    }

    static void NewThreadClient()
    {
        new Thread(() =>
        {
            var client = new ClientPipe("Game");

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
            var random = new Random();
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