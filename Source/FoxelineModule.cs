using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.Foxeline
{
    public static class FoxelineConst
    {
        public const string Velocity = "foxeline_velocity";
        public const string TailPositions = "foxeline_tail_pos";
        public const string TailOffset = "foxeline_tail_offset";
        public const int Variants = 3;
        public const string GravHelperFlip = "GravityHelper_Inverted";

        public static readonly Dictionary<string,Vector2> customTailPositions = new Dictionary<string, Vector2>(){
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
            { "bubble", 2 },
            { "spin", 1 },
            { "launch", 2 },
            { "launchRecover", 2 },
            { "wakeUp", 1 },
            { "roll", 1 },
            { "sleep", 1 },
            { "sitDown", 1 },
            { "fallPose", 1 },
            { "bagDown", 1 },
            { "asleep", 1 },
            { "halfWakeUp", 1 },
            { "bigFall", 1 },
            { "carryTheoWalk", 1 },
            { "carryTheoCollapse", 1 }
        };
        public static readonly float[] tailSize = { 3, 2, 1, 3, 1, 2, 2, 2 };
        public static readonly int[] tailID = {0, 2, 3, 4, 4, 3, 1, 0};
        public static readonly int tailLen = tailSize.Length;
    }
    /*
    Foxeline Documentation

    The general process is very shrimple:
    1. We create a list of tail positions and velocities for each tail piece
    2. We update the tail positions based on the previous tail position and the direction the tail should go in
    3. We clamp the tail positions to make sure the tail doesn't stretch too far
    4. We turn the tail positions into offsets for rendering
    5. We draw different sphere-esque textures for each tail piece during PlayerHair:on_render
    */
    public static class FoxelineHelpers {
        /// <summary>
        /// Clamps the tail piece into reach of the previous tail piece
        /// </summary>
        /// <param name="tailPositions">List of tail positions to edit</param>
        /// <param name="i">Tail index</param>
        public static void clampTail(List<Vector2> tailPositions, int i) {
            
            if (Vector2.Distance(tailPositions[i - 1], tailPositions[i]) > FoxelineConst.tailSize[i] * FoxelineModule.Settings.TailScale / 100f)
            {
                Vector2 diff = tailPositions[i - 1] - tailPositions[i];
                diff.Normalize();
                tailPositions[i] = tailPositions[i - 1] - diff * FoxelineConst.tailSize[i] * FoxelineModule.Settings.TailScale / 100f;
            }
        }

        public static void drawTail(PlayerHair self, DynamicData selfData) {
            /// <summary>
            /// Draws the tail of the player based on the tail positions defined under selfData and the hair offset
            /// </summary>
            /// <param name="self">The PlayerHair object</param>
            /// <param name="selfData">The DynamicData object for the PlayerHair object</param>
            List<Vector2> tailOffset = selfData.Get<List<Vector2>>(FoxelineConst.TailOffset);
            int currentVariant = (int)FoxelineModule.Settings.Tail - 1 + FoxelineConst.Variants * (FoxelineModule.Settings.TailScale > 100 ? 1 : 0);
            //repeat but fill this time
            for (int i = FoxelineConst.tailLen - 1; i >= 0; i--)
            {
                MTexture tex = FoxelineModule.Instance.tailtex[currentVariant][FoxelineConst.tailID[i]];
                bool fill = (i < FoxelineConst.tailLen * (100 - FoxelineModule.Settings.FoxelineConstants.Softness) / 100f) != FoxelineModule.Settings.PaintBrushTail;
                //fill color is either the hair color or a blend of white and the hair color at the tip of the tail and the base of the tail (sometimes visible)
                Color color = fill 
                        ? getHairColor(i, self, selfData) 
                        : Color.Lerp(
                            Color.White, 
                            getHairColor(i, self, selfData), 
                            FoxelineModule.Settings.TailBrushTint / 100f);
                Vector2 position = self.Nodes[0].Floor() + tailOffset[i].Floor();
                Vector2 center = Vector2.One * (float)Math.Floor(tex.Width / 2f);
                float Scale = FoxelineModule.Settings.TailScale / 100f / (FoxelineModule.Settings.TailScale > 100 ? 2 : 1);
                tex.Draw(position, center, color, Scale);
            }
        }

        public static void drawTailOutline(PlayerHair self, DynamicData selfData) {
            List<Vector2> tailOffset = selfData.Get<List<Vector2>>(FoxelineConst.TailOffset);
            int currentVariant = (int)FoxelineModule.Settings.Tail - 1 + FoxelineConst.Variants * (FoxelineModule.Settings.TailScale > 100 ? 1 : 0);
            for (int i = FoxelineConst.tailLen - 1; i >= 0; i--)
            {
                //we select the current tail piece. tailID is currently baked and chosen to be pretty
                MTexture tex = FoxelineModule.Instance.tailtex[currentVariant][FoxelineConst.tailID[i]];
                //we calculate the position of the texture by offsetting it by half its size
                Vector2 position = self.Nodes[0].Floor() + tailOffset[i].Floor();
                Vector2 center = Vector2.One * (float)Math.Floor(tex.Width / 2f);
                float Scale = FoxelineModule.Settings.TailScale / 100f / (FoxelineModule.Settings.TailScale > 100 ? 2 : 1);
                tex.Draw(position + Vector2.UnitX, center, Color.Black, Scale);
                tex.Draw(position + Vector2.UnitY, center, Color.Black, Scale);
                tex.Draw(position - Vector2.UnitX, center, Color.Black, Scale);
                tex.Draw(position - Vector2.UnitY, center, Color.Black, Scale);
            }
            
        }

        public static void Collective_Render(PlayerHair self, DynamicData selfData) {
            //original celeste draw function
            PlayerSprite sprite = self.Sprite;
            if (!sprite.HasHair)
            {
                return;
            }

            Vector2 origin = new Vector2(5f, 5f);
            Color color = self.Border * self.Alpha;
            if (self.DrawPlayerSpriteOutline)
            {
                Color color2 = sprite.Color;
                Vector2 position = sprite.Position;
                sprite.Color = color;
                sprite.Position = position + new Vector2(0f, -1f);
                sprite.Render();
                sprite.Position = position + new Vector2(0f, 1f);
                sprite.Render();
                sprite.Position = position + new Vector2(-1f, 0f);
                sprite.Render();
                sprite.Position = position + new Vector2(1f, 0f);
                sprite.Render();
                sprite.Color = color2;
                sprite.Position = position;
            }

            self.Nodes[0] = self.Nodes[0].Floor();
            //we collect the Outline draw calls
            drawTailOutline(self, selfData);
            if (color.A > 0)
            {
                for (int num = sprite.HairCount - 1; num >= FoxelineModule.Settings.FoxelineConstants.CollectHairLength; num--)
                {
                    MTexture hairTexture = self.GetHairTexture(num);
                    Vector2 hairScale = self.GetHairScale(num);
                    hairTexture.Draw(self.Nodes[num] + new Vector2(-1f, 0f), origin, color, hairScale);
                    hairTexture.Draw(self.Nodes[num] + new Vector2(1f, 0f), origin, color, hairScale);
                    hairTexture.Draw(self.Nodes[num] + new Vector2(0f, -1f), origin, color, hairScale);
                    hairTexture.Draw(self.Nodes[num] + new Vector2(0f, 1f), origin, color, hairScale);
                }
            }
            drawTail(self, selfData);
            //some of the outline should be drawn after the tail to add slight difference in depth
            if (color.A > 0)
            {
                for (int num = FoxelineModule.Settings.FoxelineConstants.CollectHairLength - 1; num >= 0; num--)
                {
                    MTexture hairTexture = self.GetHairTexture(num);
                    Vector2 hairScale = self.GetHairScale(num);
                    hairTexture.Draw(self.Nodes[num] + new Vector2(-1f, 0f), origin, color, hairScale);
                    hairTexture.Draw(self.Nodes[num] + new Vector2(1f, 0f), origin, color, hairScale);
                    hairTexture.Draw(self.Nodes[num] + new Vector2(0f, -1f), origin, color, hairScale);
                    hairTexture.Draw(self.Nodes[num] + new Vector2(0f, 1f), origin, color, hairScale);
                }
            }

            for (int num = sprite.HairCount - 1; num >= 0; num--)
            {
                self.GetHairTexture(num).Draw(self.Nodes[num], origin, self.GetHairColor(num), self.GetHairScale(num));
            }
        }

        public static Color getHairColor(int hairNodeIndex, PlayerHair self, DynamicData selfData) {
            /// <summary>
            /// Helper function to get the hair color of the player based on the current animation
            /// </summary>
            /// <param name="hairNodeIndex">The index of the hair color</param>
            /// <param name="self">The PlayerHair object</param>
            /// <param name="selfData">The DynamicData object for the PlayerHair object</param>
            /// <returns>The smarter hair color of the player</returns>
            
            if (!(FoxelineModule.Settings.FixCutscenes && self is {
                Entity: Player player,
                Sprite.Mode: not PlayerSpriteMode.MadelineAsBadeline and not PlayerSpriteMode.Badeline
            }))
                return self.GetHairColor(hairNodeIndex);
            
            Dictionary<string, int> CutsceneToDashLookup = self.Sprite.EntityAs<Player>().Inventory.Backpack 
                ? FoxelineConst.backpackCutscenes 
                : FoxelineConst.noBackpackCutscenes;

            //if there's no animation id in the lookup, just do the default
            if (!CutsceneToDashLookup.TryGetValue(self.Sprite.CurrentAnimationID, out var dashes))
                return self.GetHairColor(hairNodeIndex);


            //SMH PLUS
            if(selfData.TryGet("smh_hairConfig", out var hairConfig)) {
                //anything can be null here - use null conditional operator to avoid null reference exceptions
                Dictionary<int, List<Color>> hairColors = (Dictionary<int, List<Color>>)hairConfig.GetType()?.GetField("actualHairColors")?.GetValue(hairConfig);
                if(hairColors is not null) {
                    //we found something that looks like a hairConfig from SMH PLUS
                    if (hairColors?.ContainsKey(hairNodeIndex) ?? false) 
                        //fallback to default hair color if the hairConfig is broken
                        hairNodeIndex = 100;
                    dashes = Math.Max(Math.Min(dashes, hairColors[hairNodeIndex].Count - 1), 0);
                    return hairColors[hairNodeIndex][dashes];
                }
            }

            //VANILLA FALLBACK
            return dashes switch {
                0 => Player.UsedHairColor,
                1 => Player.NormalHairColor,
                2 => Player.TwoDashesHairColor,
                _ => self.GetHairColor(hairNodeIndex)
            };
        }

        public static bool isCrouched(PlayerHair hair)
            => hair is {
                Sprite.CurrentAnimationID: "duck" or "slide"
            };

        public static bool shouldDroopTail(PlayerHair hair)
            => hair is {
                Sprite.CurrentAnimationID: "spin" or "launch"
            }
            || hair.Sprite.EntityAs<Player>().Stamina <= Player.ClimbTiredThreshold;

        public static bool shouldFlipTail(PlayerHair hair)
            => hair is {
                Sprite.CurrentAnimationID: "wakeUp" or "sleep" or "sitDown" or "bagDown" or "asleep" or "halfWakeUp"
            };

        public static bool shouldStretchTail(PlayerHair hair)
            => hair is {
                Sprite.CurrentAnimationID: "edge" or "idleC"
            }
            || isCrouched(hair);

    }
    public static class FoxelineHooks {
        public static void PlayerHair_AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self) {

            orig(self);

            //only handle tail if:
            //- it's enabled
            //- the entity is a player
            //- the player sprite mode is not badeline or other self
            if (FoxelineModule.Settings.Tail == TailVariant.None || self.Sprite.Mode == PlayerSpriteMode.Badeline || !(self.Entity is Player))
            {
                return;
            }

            DynamicData selfData = DynamicData.For(self);
            //List<T> is a reference type - this means that modifying these lists will modify the lists in DynamicData
            //simplifies code and improves performance - a double whammy
            List<Vector2> tailPositions = selfData.Get<List<Vector2>>(FoxelineConst.TailPositions);
            List<Vector2> tailOffsets = selfData.Get<List<Vector2>>(FoxelineConst.TailOffset);
            List<Vector2> tailVelocities = selfData.Get<List<Vector2>>(FoxelineConst.Velocity);

            //special cases
            bool crouched = FoxelineHelpers.isCrouched(self);
            bool droopTail = FoxelineHelpers.shouldDroopTail(self);
            bool flipTail = FoxelineHelpers.shouldFlipTail(self);
            bool stretchTail = FoxelineHelpers.shouldStretchTail(self);

            //Vertical flip
            bool GravHelperFlip = DynamicData.For(self.Sprite.Entity).Data.TryGetValue(FoxelineConst.GravHelperFlip, out var value) && (bool)value;
            float flipped = self.Sprite.Scale.Y * (GravHelperFlip ? -1 : 1);
            
            
            
            //the current direction the player is looking in
            Vector2 faceDirection = new Vector2((float)self.Facing, flipped);

            //the position the tail will grow out of
            //if in animation, use custom tail center
            if (!FoxelineConst.customTailPositions.TryGetValue(self.Sprite.CurrentAnimationID, out Vector2 offset))
            {
                offset = new(droopTail ? 0 : -2, crouched ? 3 : 6);
                offset.X += MathF.Sin(Engine.FrameCounter / 30f) / 2f;
            }

            tailOffsets[0] = offset * faceDirection;
            tailPositions[0] = self.Nodes[0] + tailOffsets[0];

            for (int i = 1; i < FoxelineConst.tailLen; i++)
            {
                if(i >= tailPositions.Count) {
                    tailPositions.Add(tailPositions[i - 1]);
                    tailVelocities.Add(Vector2.Zero);
                    tailOffsets.Add(Vector2.Zero);
                }
                if(self.SimulateMotion) {
                    
                    //this equation moves the points along an S shape one behind the other
                    float x = ((float)i / FoxelineConst.tailLen - 0.5f) * 2;
                    if(x < 0)
                        x = (float)Math.Sqrt(-x);
                    
                    Vector2 tailDir = new Vector2(-0.5f, -1 + x);

                    //if we are thrown by badeline we want to have a droopy tail since we cannot rotate it around madeline
                    //if madeline's out of stamina she doesn't have the strength to keep it up either
                    //make it sway side to side a bit to make it a bit more lively
                    if (droopTail) 
                        tailDir = Vector2.UnitY
                        + Vector2.UnitX 
                            * ((float)i/FoxelineConst.tailLen) 
                            * MathF.Sin(
                                Engine.FrameCounter / (float)FoxelineModule.Settings.FoxelineConstants.droopSwaySpeed
                                    - (ulong)(i 
                                        * FoxelineConst.tailSize[i] 
                                        * (FoxelineModule.Settings.FoxelineConstants.droopSwayFrequency / 100f) 
                                        / FoxelineConst.tailLen))
                            * (FoxelineModule.Settings.FoxelineConstants.droopSwayAmplitude / 100f);
                    
                    //if we are in a cutscene where the tail looks cuter close to the player, modify the tail curve
                    if (flipTail) 
                        tailDir *= new Vector2(-1,-0f);

                    //if we are balancing we want it to look like we use the tail for that
                    if (stretchTail) 
                        tailDir *= new Vector2(2f,0.5f);

                    //tail direction can go zonkers
                    tailDir = tailDir.SafeNormalize();

                    //bugfix
                    if (stretchTail) tailDir *= 0.95f;

                    //we clamp the tail piece into reach for the other tail piece as it has moved
                    FoxelineHelpers.clampTail(tailPositions, i);

                    //the position each part of the tail want to reach
                    Vector2 basePos = tailPositions[i - 1] + tailDir * FoxelineConst.tailSize[i] * (FoxelineModule.Settings.TailScale / 100f) * faceDirection;

                    //the tail tries to get into position and accelerates towards it or breaks towards it
                    tailVelocities[i] = Calc.LerpSnap(
                        tailVelocities[i], 
                        basePos - tailPositions[i], 
                        1f - (float)Math.Pow(
                            1f - FoxelineModule.Settings.FoxelineConstants.Control / 100f,
                            Engine.DeltaTime
                            ),
                        1f);
                    
                    //while flying, keep tail as trail
                    if (self.Sprite.CurrentAnimationID == "starFly")
                        tailVelocities[i] = Vector2.Zero;
                    
                    //the tail then updates its positon based on how much it was accelerated
                    tailPositions[i] += tailVelocities[i] 
                        * Engine.DeltaTime 
                        / (1 - FoxelineModule.Settings.FoxelineConstants.Speed / 100f);
                }

                //we clamp the tail piece into reach for the other tail piece as the current tail has moved
                FoxelineHelpers.clampTail(tailPositions, i);

                //update the tail offset for rendering because celeste will sometimes not use hair_move function
                //this is a bugfix
                tailOffsets[i] = tailPositions[i] - self.Nodes[0];
            }
        }
        public static void PlayerHair_Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            //only handle tail if:
            //- it's enabled
            //- the entity is a player
            //- the player sprite mode is not badeline or other self
            if (!(FoxelineModule.Settings.Tail != TailVariant.None && self is {
                Entity: Player,
                Sprite.Mode: not PlayerSpriteMode.MadelineAsBadeline and not PlayerSpriteMode.Badeline
            }))
            {
                orig(self);
                return;
            }

            DynamicData selfData = DynamicData.For(self);

            //special case for star fly
            if (self.Sprite.CurrentAnimationID == "starFly") {
                //Dont draw the tail if disabled
                if(!FoxelineModule.Settings.FeatherTail) {
                    orig(self);
                    return;
                }

                //else draw the tail instead of the hair
                Vector2 pos = self.Sprite.RenderPosition;
                self.Sprite.Texture.Draw(pos + Vector2.UnitX, self.Sprite.Origin, Color.Black, self.Sprite.Scale);
                self.Sprite.Texture.Draw(pos + Vector2.UnitY, self.Sprite.Origin, Color.Black, self.Sprite.Scale);
                self.Sprite.Texture.Draw(pos - Vector2.UnitX, self.Sprite.Origin, Color.Black, self.Sprite.Scale);
                self.Sprite.Texture.Draw(pos - Vector2.UnitY, self.Sprite.Origin, Color.Black, self.Sprite.Scale);
                FoxelineHelpers.drawTailOutline(self, selfData);
                FoxelineHelpers.drawTail(self, selfData);
                return;
            }

            //draw the tail and the hair after
            if(FoxelineModule.Settings.CollectHair) {
                FoxelineHelpers.Collective_Render(self, selfData);
                return;
            }

            FoxelineHelpers.drawTailOutline(self, selfData);
            FoxelineHelpers.drawTail(self, selfData);
            orig(self);
        }
        public static void PlayerHair_ctor(On.Celeste.PlayerHair.orig_ctor orig, PlayerHair self, PlayerSprite sprite) {
            DynamicData selfData = DynamicData.For(self);
            List<Vector2> tailPositions = [];
            List<Vector2> tailOffsets = [];
            List<Vector2> tailVelocities = [];
            selfData.Set(FoxelineConst.TailPositions, tailPositions);
            selfData.Set(FoxelineConst.Velocity, tailOffsets);
            selfData.Set(FoxelineConst.TailOffset, tailVelocities);
            for (int i = 0; i < FoxelineConst.tailLen; i++)
            {
                tailPositions.Add(Vector2.Zero);
                tailVelocities.Add(Vector2.Zero);
                tailOffsets.Add(Vector2.Zero);
            }
            orig(self, sprite);
        }
        public static void PlayerHair_Start(On.Celeste.PlayerHair.orig_Start orig, PlayerHair self)
        {
            DynamicData selfData = DynamicData.For(self);
            List<Vector2> tailPositions = selfData.Get<List<Vector2>>(FoxelineConst.TailPositions);
            Vector2 value = self.Entity.Position + new Vector2((0 - self.Facing) * 200, 200f);
            for (int i = 0; i < tailPositions.Count; i++)
            {
                tailPositions[i] = value;
            }
            orig(self);
        }
        public static void PlayerHair_MoveHairBy(On.Celeste.PlayerHair.orig_MoveHairBy orig, PlayerHair self, Vector2 amount)
        {
            DynamicData selfData = DynamicData.For(self);
            List<Vector2> tailPositions = selfData.Get<List<Vector2>>(FoxelineConst.TailPositions);
            for (int i = 0; i < tailPositions.Count; i++)
            {
                tailPositions[i] += amount;
            }
            orig(self, amount);
        }

        public static MTexture Hair_GetTexture(On.Celeste.PlayerHair.orig_GetHairTexture orig, PlayerHair self, int index)
        {
            //change bangs texture if:
            //- bangs are enabled
            //- the hair node is madeline's bangs
            //- the entity is a player
            //- the player sprite mode is not badeline or other self
            if (!(FoxelineModule.Settings.EnableBangs && index == 0 && self is {
                Entity: Player,
                Sprite.Mode: not PlayerSpriteMode.MadelineAsBadeline and not PlayerSpriteMode.Badeline
            }))
                return orig(self, index);
            if(self.Sprite.HairFrame >= FoxelineModule.Instance.bangs.Count || self.Sprite.HairFrame < 0)
                return FoxelineModule.Instance.bangs[0];
            return FoxelineModule.Instance.bangs[self.Sprite.HairFrame];
        }


    }
    public class FoxelineModule : EverestModule
    {
        public static FoxelineModule Instance { get; private set; }

        public override Type SettingsType => typeof(FoxelineModuleSettings);
        public static FoxelineModuleSettings Settings => (FoxelineModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(FoxelineModuleSession);
        public static FoxelineModuleSession Session => (FoxelineModuleSession)Instance._Session;


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


        public List<MTexture>[] tailtex;
        public List<MTexture> bangs;

        public override void Load()
        {
            On.Celeste.PlayerHair.Render += FoxelineHooks.PlayerHair_Render;
            On.Celeste.PlayerHair.ctor += FoxelineHooks.PlayerHair_ctor;
            On.Celeste.PlayerHair.Start += FoxelineHooks.PlayerHair_Start;
            On.Celeste.PlayerHair.AfterUpdate += FoxelineHooks.PlayerHair_AfterUpdate;
            On.Celeste.PlayerHair.GetHairTexture += FoxelineHooks.Hair_GetTexture;
            On.Celeste.PlayerHair.MoveHairBy += FoxelineHooks.PlayerHair_MoveHairBy;
        }

        public override void LoadContent(bool inGame)
        {
            tailtex = new List<MTexture>[FoxelineConst.Variants * 2];
            for(int variant = 0; variant < FoxelineConst.Variants; variant++) {
                tailtex[variant] = GFX.Game.GetAtlasSubtextures("Foxeline/tail/variant_" + variant + "/tail");
                tailtex[variant + FoxelineConst.Variants] = GFX.Game.GetAtlasSubtextures("Foxeline/tail/variant_" + variant + "/tail_resize");
            }
            bangs = GFX.Game.GetAtlasSubtextures("Foxeline/bangs/bangs");
        }

        public override void Unload()
        {
            On.Celeste.PlayerHair.Render -= FoxelineHooks.PlayerHair_Render;
            On.Celeste.PlayerHair.ctor -= FoxelineHooks.PlayerHair_ctor;
            On.Celeste.PlayerHair.Start -= FoxelineHooks.PlayerHair_Start;
            On.Celeste.PlayerHair.AfterUpdate -= FoxelineHooks.PlayerHair_AfterUpdate;
            On.Celeste.PlayerHair.GetHairTexture -= FoxelineHooks.Hair_GetTexture;
            On.Celeste.PlayerHair.MoveHairBy -= FoxelineHooks.PlayerHair_MoveHairBy;
        }
    }
}