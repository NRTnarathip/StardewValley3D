using System.Drawing;
using System.Threading.Tasks;

namespace GameDummy
{
    static class Program
    {
        private static async Task Main(string[] args)
        {
            Task.Run(async () =>
            {
                var gameApp = new BaseAppNetwork(true);
                if (gameApp.Start() is false)
                {
                    return;
                }

                while (true)
                {
                    gameApp.PerformUpdate();
                    var screenBytes = new byte[10_000];
                    var pos = new System.Numerics.Vector2(22, 99);
                    var scale = new System.Numerics.Vector2(22, 99);
                    //gameApp.SendEvent("draw", [
                    //    "hello guy",
                    //    screenBytes,
                    //    new Rectangle(0, 0, 155, 155),
                    //    pos,
                    //    scale
                    //]);
                    await Task.Delay(500);
                }
            });

            Task.Run(async () =>
            {
                var client = new BaseAppNetwork(false);
                client.Start();

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
}