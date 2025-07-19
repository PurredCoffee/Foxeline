using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.CelesteNet.Client;
using Celeste.Mod.CelesteNet.DataTypes;
using Celeste.Mod.CelesteNet;

namespace Celeste.Mod.Foxeline.CelesteNet;

/// <summary>
///   A <see cref="CelesteNetGameComponent"/> which handles <see cref="CelesteNetTailData"/> packets.<br/>
/// </summary>
public class CelesteNetTailDataExchangeComponent : CelesteNetGameComponent
{
    /*
     * here's how CelesteNet interoperability works
     * - on connect, it looks for all loaded types that extend from CelesteNetGameComponent
     *   - (done in CelesteNetClientContext..ctor)
     * - it creates an instance of them, calling the .ctor(CelesteNetClientContext context, Game game) constructor
     *   and registers it in its list
     * - a bit later it dynamically invokes each registered components' virtual Init() method
     *   - (done in CelesteNetClientContext.Init)
     * - CelesteNetGameComponent.Init() calls DataContext.RegisterHandlersIn(this)
     * - it looks for and registers all methods which match the following signature, where T is a DataType:
     *   - bool Filter(CelesteNetConnection, T)
     *     - returns false if the packet should be ignored, true otherwise
     *   - void Handle(CelesteNetConnection, T)
     *     - handles an incoming packet
     * - when a packet of type T is received, all registered filters for T are run
     *   - if any return false, the packet is skipped
     * - all registered handlers for T are then run
     */

    private static CelesteNetClientContext ctx;
    public static Dictionary<uint, FoxelineModuleSettings.TailDefaults> TailInformation = [];

    //instantiated when flipping the Connected toggle
    public CelesteNetTailDataExchangeComponent(CelesteNetClientContext context, Game game) : base(context, game)
    {
        ctx = context;
    }

    //called when receiving a DataReady, which is sent when the connection finished initialization
    public void Handle(CelesteNetConnection con, DataReady data)
    {
        if (ctx.Client?.PlayerInfo != null)
            ctx.Client.Send(new CelesteNetTailData(ctx.Client.PlayerInfo));
    }

    public void SendTailData()
    {
        if (ctx.Client?.PlayerInfo != null)
            ctx.Client.Send(new CelesteNetTailData(ctx.Client.PlayerInfo));
    }

    //called when receiving a CelesteNetTailData, which is only sent when the player connects right now
    public void Handle(CelesteNetConnection con, CelesteNetTailData data)
    {
        if (data.Player != null)
            TailInformation[data.Player.ID] = data.TailInformation;
    }
}
