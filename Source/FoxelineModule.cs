using System;
using System.Collections.Generic;
using Monocle;
using MonoMod.ModInterop;

namespace Celeste.Mod.Foxeline;

public class FoxelineModule : EverestModule
{

    /*
    Foxeline Documentation

    The general process is very shrimple:
    1. We create a list of tail positions and velocities for each tail node
    2. We update the tail positions based on the previous tail position and the direction the tail should go in
    3. We clamp the tail positions to make sure the tail doesn't stretch too far
    4. We turn the tail positions into offsets for rendering
    5. We draw different sphere-esque textures for each tail node during PlayerHair:on_render
    */

    public static FoxelineModule Instance { get; private set; }

    public override Type SettingsType => typeof(FoxelineModuleSettings);
    public static FoxelineModuleSettings Settings => (FoxelineModuleSettings)Instance._Settings;


    public FoxelineModule()
    {
        Instance = this;
    #if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(FoxelineModule), LogLevel.Verbose);
    #else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(FoxelineModule), LogLevel.Info);
    #endif
    }


    public List<MTexture>[] TailNodeTextures;
    public List<MTexture> BangsTextures;

    public override void Load()
    {
        typeof(GravityHelperInterop).ModInterop();

        On.Celeste.PlayerHair.Render += FoxelineHooks.PlayerHair_Render;
        On.Celeste.PlayerHair.ctor += FoxelineHooks.PlayerHair_ctor;
        On.Celeste.PlayerHair.Start += FoxelineHooks.PlayerHair_Start;
        On.Celeste.PlayerHair.AfterUpdate += FoxelineHooks.PlayerHair_AfterUpdate;
        On.Celeste.PlayerHair.GetHairTexture += FoxelineHooks.Hair_GetTexture;
        On.Celeste.PlayerHair.MoveHairBy += FoxelineHooks.PlayerHair_MoveHairBy;

    #if DEBUG
        On.Monocle.Component.DebugRender += FoxelineHooks.Component_DebugRender;
        On.Celeste.Player.DebugRender += FoxelineHooks.Player_DebugRender;
    #endif
    }

    public override void LoadContent(bool inGame)
    {
        TailNodeTextures = new List<MTexture>[FoxelineConst.Variants * 2];
        for (int variant = 0; variant < FoxelineConst.Variants; variant++)
        {
            TailNodeTextures[variant] = GFX.Game.GetAtlasSubtextures($"Foxeline/tail/variant_{variant}/tail");
            TailNodeTextures[variant + FoxelineConst.Variants] = GFX.Game.GetAtlasSubtextures($"Foxeline/tail/variant_{variant}/tail_resize");
        }
        BangsTextures = GFX.Game.GetAtlasSubtextures("Foxeline/bangs/bangs");
    }

    public override void Unload()
    {
        On.Celeste.PlayerHair.Render -= FoxelineHooks.PlayerHair_Render;
        On.Celeste.PlayerHair.ctor -= FoxelineHooks.PlayerHair_ctor;
        On.Celeste.PlayerHair.Start -= FoxelineHooks.PlayerHair_Start;
        On.Celeste.PlayerHair.AfterUpdate -= FoxelineHooks.PlayerHair_AfterUpdate;
        On.Celeste.PlayerHair.GetHairTexture -= FoxelineHooks.Hair_GetTexture;
        On.Celeste.PlayerHair.MoveHairBy -= FoxelineHooks.PlayerHair_MoveHairBy;

    #if DEBUG
        On.Monocle.Component.DebugRender -= FoxelineHooks.Component_DebugRender;
        On.Celeste.Player.DebugRender -= FoxelineHooks.Player_DebugRender;
    #endif
    }
}
