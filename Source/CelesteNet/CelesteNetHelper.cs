using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Celeste.Mod.CelesteNet.Client;
using Celeste.Mod.CelesteNet.DataTypes;
using Celeste.Mod.CelesteNet;

namespace Celeste.Mod.Foxeline.CelesteNet;

public class TailComponent : CelesteNetGameComponent {
    private static CelesteNetClientContext ctx;
    public static Dictionary<uint, FoxelineModuleSettings.TailDefaults> TailInformation = [];

    public TailComponent(CelesteNetClientContext context, Game game) : base(context, game) {
        ctx = context;
    }

    public void Handle(CelesteNetConnection con, DataReady data) {
        if(ctx.Client?.PlayerInfo != null)
            ctx.Client.Send(new TailData(ctx.Client.PlayerInfo));
    }

    public void SendTailData() {
        if(ctx.Client?.PlayerInfo != null)
            ctx.Client.Send(new TailData(ctx.Client.PlayerInfo));
    }

    public void Handle(CelesteNetConnection con, TailData data) {
        if(data.Player != null)
            TailInformation[data.Player.ID] = data.TailInformation;
    }
}
