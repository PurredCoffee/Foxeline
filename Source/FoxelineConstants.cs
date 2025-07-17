using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Foxeline;

public static class FoxelineConst
{
    public const string Velocity = "foxeline_velocity";
    public const string TailPositions = "foxeline_tail_pos";
    public const string TailOffset = "foxeline_tail_offset";

    public const int Variants = 3;
    public const string GravHelperFlip = "GravityHelper_Inverted";
    public const string smh_hairConfig = "smh_hairConfig";

    public static readonly Dictionary<string, Vector2> customTailPositions = new() {
        {"bigFall", new Vector2(7,-2)},
        {"bubble", new Vector2(0,-4)},
        {"carryTheoCollapse", new Vector2(6,4)},
        {"carryTheoWalk", new Vector2(2,6)},
        {"edgeBack", new Vector2(0,6)},
        {"runWind", new Vector2(-8,4)},
        {"starFly", new Vector2(0,0)},
        {"tentacle_dangling", new Vector2(-2,12)},

        //modded animations
        {"anim_player_elytra_fly", new Vector2(-6,2)} //CommunalHelper - elytra
    };

    public static readonly Dictionary<string, int> backpackCutscenes = new() {
        {"asleep", 1},
        {"bagDown", 1},
        {"bubble", 1},
        {"carryTheoCollapse", 1},
        {"carryTheoWalk", 1},
        {"fallPose", 1},
        {"halfWakeUp", 1},
        {"launch", 2},
        {"launchRecover", 2},
        {"sitDown", 1},
        {"sleep", 1},
        {"spin", 1},
        {"wakeUp", 1},
    };

    public static readonly Dictionary<string, int> noBackpackCutscenes = new() {
        {"asleep", 1},
        {"bagDown", 1},
        {"bubble", 2},
        {"carryTheoCollapse", 1},
        {"carryTheoWalk", 1},
        {"fallPose", 1},
        {"halfWakeUp", 1},
        {"launch", 2},
        {"launchRecover", 2},
        {"sitDown", 1},
        {"sleep", 1},
        {"spin", 1},
        {"wakeUp", 1},

        //extra no backpack animations
        {"downed", 1},
        {"roll", 1},
        {"rollGetUp", 1},
    };

    public static readonly float[] tailSize = [3, 2, 1, 3, 1, 2, 2, 2];
    public static readonly int[] tailID = [0, 2, 3, 4, 4, 3, 1, 0];
    public static readonly int tailLen = tailSize.Length;
}
