using GuyPipeCore;
using MessagePack;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace StardewValley3D
{
    public class MainEntry : Mod
    {
        ServerPipe serverPipe;

        public override void Entry(IModHelper helper)
        {
            Console.WriteLine("Starting NamePipe Server...");
            //setup game pipe
            serverPipe = new("StardewValley");

            // setup game events
            helper.Events.Display.RenderedStep += Display_RenderedStep;
            helper.Events.Display.Rendered += Display_Rendered; ;
        }

        private void Display_Rendered(object? sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            serverPipe.EndMessageFactory();
        }

        void Display_RenderedStep(object? sender, StardewModdingAPI.Events.RenderedStepEventArgs e)
        {
            serverPipe.SendEvent("Events.Display.RenderedStep", [e.Step]);
        }
    }
}
