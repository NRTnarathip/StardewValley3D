using System.Drawing;

static class Program
{
    private static async Task Main(string[] args)
    {
        Task.Run(async () =>
        {
            var monoApp = new MonoApp();
            if (monoApp.Start() is false)
            {
                return;
            }

            while (true)
            {
                monoApp.PerformUpdate();
                monoApp.SendEvent("Game1.ticks", [monoApp.tickCounter]);
                var screenBytes = new byte[30_000];
                monoApp.SendEvent("Game1.screen_1", [screenBytes]);
                monoApp.SendEvent("Game1.screen_2", [screenBytes]);
                monoApp.SendEvent("Game1.screen_3", [new byte[65_530]]);
                await Task.Delay(500);
            }
        });

        Task.Run(async () =>
        {
            var client = new UnityApp();
            client.Start();

            client.RegisterEvent("Game1.ticks", (int ticks) =>
            {
                client.Log("game1.ticks: " + ticks);
            });

            client.RegisterEvent("Game1.screen_1", (byte[] screenColors) =>
            {
                client.Log(" Game1.screen_1 colors: " + screenColors.Length);
            });
            client.RegisterEvent("Game1.screen_2", (byte[] screenColors) =>
            {
                client.Log(" Game1.screen_2 colors: " + screenColors.Length);
            });
            client.RegisterEvent("Game1.screen_3", (byte[] screenColors) =>
            {
                client.Log(" Game1.screen_3 colors: " + screenColors.Length);
            });

            while (true)
            {
                client.PerformUpdate();
                await Task.Delay(100);
            }
        });

        while (true)
        {
            await Task.Delay(1000);
        }
    }
}