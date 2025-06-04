using HarmonyLib;

[HarmonyPatch]
internal class Program
{
    private static void Main(string[] args)
    {
        var h = new Harmony("Game Update");
        h.PatchAll();
        var fps = new FPSCounter();
        while (true)
        {
            Console.WriteLine(" ");
            Console.WriteLine("new game tick");
            fps.Update();
            GameUpdate();

            Thread.Sleep(1);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Program), nameof(GameUpdate))]
    public static bool Prefix_Update()
    {
        Console.WriteLine("prefix update");


        Console.WriteLine("try skip game update");
        return false;

        //return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Program), nameof(GameUpdate))]
    public static void Postfix_Update()
    {
        Console.WriteLine("postfix update");
    }

    public static void GameUpdate()
    {
        Console.WriteLine("Game Update");
    }
}