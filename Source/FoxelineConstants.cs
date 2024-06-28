using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Foxeline
{
    public static class FoxelineConst
    {
        public const string Velocity = "foxeline_velocity";
        public const string TailPositions = "foxeline_tail_pos";
        public const string TailOffset = "foxeline_tail_offset";
        public const int Variants = 3;
        public const string GravHelperFlip = "GravityHelper_Inverted";

        public static readonly Dictionary<string, Vector2> customTailPositions = new Dictionary<string, Vector2>(){
            {"starFly", new Vector2(0,0)},
            {"carryTheoWalk", new Vector2(2,6)},
            {"carryTheoCollapse", new Vector2(8,-2)},
            {"bigFall", new Vector2(7,-2)},
            {"bubble", new Vector2(0,-4)}
        };
        public static readonly Dictionary<string, int> backpackCutscenes = new Dictionary<string, int>() {
            {"bubble", 1},
            {"spin", 1},
            {"launch", 2},
            {"launchRecover", 2},
            {"wakeUp", 1},
            {"sleep", 1},
            {"sitDown", 1},
            {"fallPose", 1},
            {"bagDown", 1},
            {"asleep", 1},
            {"halfWakeUp", 1},
            {"bigFall", 1},
            {"carryTheoWalk", 1},
            {"carryTheoCollapse", 1}
        };
        public static readonly Dictionary<string, int> noBackpackCutscenes = new Dictionary<string, int>()
        {
            {"bubble", 2},
            {"spin", 1},
            {"launch", 2},
            {"launchRecover", 2},
            {"wakeUp", 1},
            {"roll", 1},
            {"sleep", 1},
            {"sitDown", 1},
            {"fallPose", 1},
            {"bagDown", 1},
            {"asleep", 1},
            {"halfWakeUp", 1},
            {"bigFall", 1},
            {"carryTheoWalk", 1},
            {"carryTheoCollapse", 1}
        };
        public static readonly float[] tailSize = { 3, 2, 1, 3, 1, 2, 2, 2 };
        public static readonly int[] tailID = { 0, 2, 3, 4, 4, 3, 1, 0 };
        public static readonly int tailLen = tailSize.Length;
    }
}