internal class Program
{
    private static void Main(string[] args)
    {
        var fps = new FPSCounter();
        while (true)
        {
            fps.Update();
            Console.WriteLine("fps: " + fps.fps);
            Console.WriteLine(" frame time: " + fps.frameTimeMs);

            Thread.Sleep(1);
        }
    }
}