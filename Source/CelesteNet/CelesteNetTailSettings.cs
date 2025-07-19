using System;
using System.Diagnostics;
using Celeste.Mod.CelesteNet;
using Celeste.Mod.CelesteNet.DataTypes;

namespace Celeste.Mod.Foxeline.CelesteNet;

#region Versioned packet

/// <summary>
/// Packet containing players' Foxeline tail settings.
/// </summary>
public class CelesteNetTailSettings : DataType<CelesteNetTailSettings>
{
    static CelesteNetTailSettings()
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

    //this empty constructor is necessary - when receiving a packet, the parameterless .ctor() is dynamically invoked
    //and then its Read() is called
    public CelesteNetTailSettings()
    {
    }

    public CelesteNetTailSettings(DataPlayerInfo player)
    {
        Player = player;
        TailInformation = new FoxelineModuleSettings.TailDefaults {
            SeparateTails = FoxelineModule.Settings.SeparateTails,
            Tail = FoxelineModule.Settings.Tail,
            TailBrushTint = FoxelineModule.Settings.TailBrushTint,
            TailBrushColor = FoxelineModule.Settings.TailBrushColor,
            TailCount = FoxelineModule.Settings.TailCount,
            TailScale = FoxelineModule.Settings.TailScale,
            TailSpread = FoxelineModule.Settings.TailSpread,
            FeatherTail = FoxelineModule.Settings.FeatherTail,
            PaintBrushTail = FoxelineModule.Settings.PaintBrushTail,
        };
    }

    //false if the packet should be ignored, true otherwise
    public override bool FilterHandle(DataContext ctx)
        => Player != null;

    //i. i genuinely am not sure what the meta methods do
    public override MetaType[] GenerateMeta(DataContext ctx) => [
        new MetaPlayerPrivateState(Player),
        new MetaBoundRef(DataType<DataPlayerInfo>.DataID, Player?.ID ?? uint.MaxValue, true),
    ];

    public override void FixupMeta(DataContext ctx)
    {
        Player = Get<MetaPlayerPrivateState>(ctx);
        Get<MetaBoundRef>(ctx).ID = Player?.ID ?? uint.MaxValue;
    }

    private static bool HasReceivedUnrecognizedPacket;

    //called from CelesteNet when this packet is received
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
            SeparateTails = reader.ReadBoolean(),
            Tail = (TailVariant)reader.ReadByte(),
            TailBrushTint = reader.ReadByte(),
            TailBrushColor = reader.ReadColorNoA(),
            TailCount = Math.Max((byte)1, Math.Min(reader.ReadByte(), (byte)9)),
            TailScale = Math.Min(reader.ReadUInt16(), FoxelineModule.Settings.FoxelineConstants.ClampCelesteNetTailSize),
            TailSpread = Math.Min(reader.ReadByte(), (byte)100),
            FeatherTail = reader.ReadBoolean(),
            PaintBrushTail = reader.ReadBoolean()
        };

    #endregion

    //called from CelesteNet when this packet is sent
    protected override void Write(CelesteNetBinaryWriter writer)
    {
        writer.Write(LatestPacketVersion);
        writer.Write(TailInformation.SeparateTails);
        writer.Write((byte)TailInformation.Tail);
        writer.Write((byte)TailInformation.TailBrushTint);
        writer.WriteNoA(TailInformation.TailBrushColor);
        writer.Write((byte)TailInformation.TailCount);
        writer.Write((ushort)TailInformation.TailScale);
        writer.Write((byte)TailInformation.TailSpread);
        writer.Write(TailInformation.FeatherTail);
        writer.Write(TailInformation.PaintBrushTail);
    }
}

#endregion
