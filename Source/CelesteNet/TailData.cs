using System;
using System.Diagnostics;
using Celeste.Mod.CelesteNet;
using Celeste.Mod.CelesteNet.DataTypes;

namespace Celeste.Mod.Foxeline.CelesteNet;

#region Versioned packet

public class TailData : DataType<TailData>
{
    static TailData()
    {
        //the unversioned packet didn't have this set; which means the packet id was an empty string
        //turns out that that's a happy accident, since now i don't have to worry about backwards compatibility
        DataID = "Foxeline/TailData";
    }

    /// <summary>
    ///   Determines the latest packet version.
    /// </summary>
    public const ushort LatestPacketVersion = 0;

    /// <summary>
    ///   The packet version.<br/>
    ///   This should always be <see cref="LatestPacketVersion"/> when sending.
    /// </summary>
    public ushort PacketVersion { get; private set; } = LatestPacketVersion;

    public DataPlayerInfo Player;
    public FoxelineModuleSettings.TailDefaults TailInformation;

    public TailData() {}

    public TailData(DataPlayerInfo player) {
        Player = player;
        TailInformation = new FoxelineModuleSettings.TailDefaults {
            Tail = FoxelineModule.Settings.Tail,
            TailBrushTint = FoxelineModule.Settings.TailBrushTint,
            TailBrushColor = FoxelineModule.Settings.TailBrushColor,
            TailScale = FoxelineModule.Settings.TailScale,
            FeatherTail = FoxelineModule.Settings.FeatherTail,
            PaintBrushTail = FoxelineModule.Settings.PaintBrushTail,
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

    private static bool HasReceivedUnrecognizedPacket;

    protected override void Read(CelesteNetBinaryReader reader)
    {
        PacketVersion = reader.ReadUInt16();

        if (PacketVersion > LatestPacketVersion)
        {
            if (!HasReceivedUnrecognizedPacket)
            {
                HasReceivedUnrecognizedPacket = true;
                Logger.Log(LogLevel.Warn, "Foxeline",
                    $"Received packet with a higher version than expected ({PacketVersion} > {LatestPacketVersion})." +
                    $"Using defaults instead - please update Foxeline.");
            }

            TailInformation = FoxelineModule.Settings.CelestenetDefaults;
            return;
        }

        //massive switch go
        switch (PacketVersion)
        {
            case 0:
                ReadVersion0(reader);
                break;
            default:
                throw new UnreachableException($"Missing packet handler for packet version {PacketVersion}");
        }
    }

    #region Packet deserializers

    private void ReadVersion0(CelesteNetBinaryReader reader)
        => TailInformation = new FoxelineModuleSettings.TailDefaults {
            Tail = (TailVariant)reader.ReadByte(),
            TailBrushTint = reader.ReadByte(),
            TailBrushColor = reader.ReadColorNoA(),
            TailScale = Math.Min(reader.ReadUInt16(), FoxelineModule.Settings.FoxelineConstants.ClampCelesteNetTailSize),
            FeatherTail = reader.ReadBoolean(),
            PaintBrushTail = reader.ReadBoolean()
        };

    #endregion

    protected override void Write(CelesteNetBinaryWriter writer) {
        writer.Write(LatestPacketVersion);
        writer.Write((byte)TailInformation.Tail);
        writer.Write((byte)TailInformation.TailBrushTint);
        writer.WriteNoA(TailInformation.TailBrushColor);
        writer.Write((ushort)TailInformation.TailScale);
        writer.Write(TailInformation.FeatherTail);
        writer.Write(TailInformation.PaintBrushTail);
    }

}

#endregion
