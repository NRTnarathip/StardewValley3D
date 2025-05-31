using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using MessagePack.Resolvers;
using StardewValleyAR;
using System.Net;
using System.Net.Sockets;

public sealed class StardewApp : BaseAppNetwork
{
    public StardewApp() : base(true)
    {

    }
}
