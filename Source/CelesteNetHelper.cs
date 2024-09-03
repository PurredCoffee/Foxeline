using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using Color = Microsoft.Xna.Framework.Color;

using Celeste.Mod.CelesteNet.Client;
using Celeste.Mod.CelesteNet.Client.Entities;
using System.Runtime.CompilerServices;
using System.Linq;
using Celeste.Mod.CelesteNet.DataTypes;
using Celeste.Mod.CelesteNet;

namespace Celeste.Mod.Foxeline;


public class TailData : DataType<TailData> {
    public DataPlayerInfo Player;
    public FoxelineModuleSettings.TailDefaults TailInformation;

    public TailData() {}
    public TailData(DataPlayerInfo player) {
        Player = player;
        TailInformation = new FoxelineModuleSettings.TailDefaults() {
            Tail = FoxelineModule.Settings.Tail,
            TailBrushTint = FoxelineModule.Settings.TailBrushTint,
            TailScale = FoxelineModule.Settings.TailScale,
            FeatherTail = FoxelineModule.Settings.FeatherTail,
            PaintBrushTail = FoxelineModule.Settings.PaintBrushTail
        };
    }

    public override bool FilterHandle(DataContext ctx)
        => Player != null;

    public override MetaType[] GenerateMeta(DataContext ctx) => [
        new MetaPlayerPrivateState(Player),
        new MetaBoundRef(DataType<DataPlayerInfo>.DataID, Player?.ID ?? uint.MaxValue, true),
    ];

    public override void FixupMeta(DataContext ctx) {
        Player = Get<MetaPlayerPrivateState>(ctx);
        Get<MetaBoundRef>(ctx).ID = Player?.ID ?? uint.MaxValue;
    }

    protected override void Read(CelesteNetBinaryReader reader) {
        TailInformation = new FoxelineModuleSettings.TailDefaults
        {
            Tail = (TailVariant)reader.ReadByte(),
            TailBrushTint = reader.ReadByte(),
            TailScale = reader.ReadByte(),
            FeatherTail = reader.ReadBoolean(),
            PaintBrushTail = reader.ReadBoolean()
        };
    }

    protected override void Write(CelesteNetBinaryWriter writer) {
        writer.Write((byte)TailInformation.Tail);
        writer.Write((byte)TailInformation.TailBrushTint);
        writer.Write((byte)TailInformation.TailScale);
        writer.Write(TailInformation.FeatherTail);
        writer.Write(TailInformation.PaintBrushTail);
    }
}

public class TailComponent : CelesteNetGameComponent {
    private static CelesteNetClientContext ctx;
    public static Dictionary<uint, FoxelineModuleSettings.TailDefaults> TailInformation = new Dictionary<uint, FoxelineModuleSettings.TailDefaults>();

    public TailComponent(CelesteNetClientContext context, Game game) : base(context, game) {
        ctx = context;
    }

    public void Handle(CelesteNetConnection con, DataReady data) {
        if(ctx.Client?.PlayerInfo != null) {
            ctx.Client.Send(new TailData(ctx.Client.PlayerInfo));
        }
    }

    public void Handle(CelesteNetConnection con, TailData data) {
        if(data.Player != null) {
            TailInformation[data.Player.ID] = data.TailInformation;
        }
    }
}
