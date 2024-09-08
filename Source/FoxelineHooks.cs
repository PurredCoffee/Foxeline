using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.Foxeline
{
    public static class FoxelineHooks
    {
        public static FoxelineModuleSettings.TailDefaults oldSettings;
        public static void PlayerHair_AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self)
        {
            orig(self);

            DynamicData selfData = DynamicData.For(self);
            //only handle tail if:
            //- the entity is a tail owner
            if (!FoxelineHelpers.correctTailOwner(self) || FoxelineHelpers.getTailVariant(self) == TailVariant.None)
            {
                return;
            }

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

            //cache dynamic data for performance
            float tailScale = FoxelineHelpers.getTailScale(self);

            for (int i = 1; i < FoxelineConst.tailLen; i++)
            {
                if (i >= tailPositions.Count)
                {
                    tailPositions.Add(tailPositions[i - 1]);
                    tailVelocities.Add(Vector2.Zero);
                    tailOffsets.Add(Vector2.Zero);
                }
                if (self.SimulateMotion)
                {

                    //this equation moves the points along an S shape one behind the other
                    float x = ((float)i / FoxelineConst.tailLen - 0.5f) * 2;
                    if (x < 0)
                        x = (float)Math.Sqrt(-x);

                    Vector2 tailDir = new Vector2(-0.5f, -1 + x);

                    //if we are thrown by badeline we want to have a droopy tail since we cannot rotate it around madeline
                    //if madeline's out of stamina she doesn't have the strength to keep it up either
                    //make it sway side to side a bit to make it a bit more lively
                    if (droopTail)
                        tailDir = Vector2.UnitY
                        + Vector2.UnitX
                            * ((float)i / FoxelineConst.tailLen)
                            * (FoxelineModule.Settings.FoxelineConstants.droopSwayAmplitude / 100f)
                            * MathF.Sin(
                                Engine.FrameCounter 
                                    / (float)FoxelineModule.Settings.FoxelineConstants.droopSwaySpeed
                                    - (ulong)(i
                                        * FoxelineConst.tailSize[i]
                                        * (FoxelineModule.Settings.FoxelineConstants.droopSwayFrequency / 100f)
                                        / FoxelineConst.tailLen));

                    //if we are in a cutscene where the tail looks cuter close to the player, modify the tail curve
                    if (flipTail)
                        tailDir *= new Vector2(-1, -0f);

                    //if we are balancing we want it to look like we use the tail for that
                    if (stretchTail)
                        tailDir *= new Vector2(2f, 0.5f);

                    //tail direction can go zonkers
                    tailDir = tailDir.SafeNormalize();

                    //bugfix
                    if (stretchTail) tailDir *= 0.95f;

                    //we clamp the tail piece into reach for the other tail piece as it has moved
                    FoxelineHelpers.clampTail(tailPositions, i, tailScale);

                    //the position each part of the tail want to reach
                    Vector2 basePos = tailPositions[i - 1] + tailDir * FoxelineConst.tailSize[i] * tailScale * faceDirection;

                    //the tail tries to get into position and accelerates towards it or breaks towards it
                    tailVelocities[i] = Calc.LerpSnap(
                        tailVelocities[i],
                        basePos - tailPositions[i],
                        1f - (float)Math.Pow(
                            1f - FoxelineModule.Settings.FoxelineConstants.Control / 100f,
                            Engine.DeltaTime
                            ),
                        1f);

                    //if we just landed from the big temple fall, make the tail also follow through
                    //makes it look more natural

                    //note: fallPose's frame 0 lasts for 6 frames, so this'll be run for 6 frames
                    if (self.Sprite is { CurrentAnimationID: "fallPose", CurrentAnimationFrame: 0 })
                    {
                        //i just made up some formula with some trial and error and it looks good..? i guess?
                        //- Snip
                        tailVelocities[i] = tailVelocities[i] with { Y = MathF.Exp(i/FoxelineConst.tailLen) + FoxelineConst.tailSize[i] };
                    }

                    //while flying, keep tail as trail
                    if (self.Sprite.CurrentAnimationID == "starFly")
                        tailVelocities[i] = Vector2.Zero;

                    //the tail then updates its positon based on how much it was accelerated
                    tailPositions[i] += tailVelocities[i]
                        * Engine.DeltaTime
                        / (1 - FoxelineModule.Settings.FoxelineConstants.Speed / 100f);
                }

                //we clamp the tail piece into reach for the other tail piece as the current tail has moved
                FoxelineHelpers.clampTail(tailPositions, i, tailScale);

                //update the tail offset for rendering because celeste will sometimes not use hair_move function
                //this is a bugfix
                tailOffsets[i] = tailPositions[i] - self.Nodes[0];
            }
        }

        public static void PlayerHair_Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            DynamicData selfData = DynamicData.For(self);
            //only handle tail if:
            //- it's enabled
            //- the entity is a tail owner
            if (FoxelineHelpers.getTailVariant(self) == TailVariant.None || !FoxelineHelpers.correctTailOwner(self))
            {
                orig(self);
                return;
            }

            //special case for star fly
            if (self.Sprite.CurrentAnimationID == "starFly")
            {
                //Dont draw the tail if disabled
                if (!FoxelineHelpers.getFeatherTail(self))
                {
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

            FoxelineHelpers.drawTailOutline(self, selfData);
            FoxelineHelpers.drawTail(self, selfData);
            orig(self);
        }
        public static void PlayerHair_ctor(On.Celeste.PlayerHair.orig_ctor orig, PlayerHair self, PlayerSprite sprite)
        {
            DynamicData selfData = DynamicData.For(self);
            List<Vector2> tailPositions = new();
            List<Vector2> tailOffsets = new();
            List<Vector2> tailVelocities = new();
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
            //- the current hair piece is the first one
            //and
            //- bangs are enabled
            //- the object is the player
            //or
            //- badeline bangs are enabled
            //- the object is badeline
            if(index != 0 || !FoxelineHelpers.shouldChangeHair(self))
                return orig(self, index);

            if (self.Sprite.HairFrame >= FoxelineModule.Instance.bangs.Count || self.Sprite.HairFrame < 0)
                return FoxelineModule.Instance.bangs[0];
            return FoxelineModule.Instance.bangs[self.Sprite.HairFrame];
        }

        public static void GameLoader_Begin(On.Celeste.GameLoader.orig_Begin orig, GameLoader self)
        {
            Foxeline.SkinIntegration.FoxelineYaml.ReloadYaml();
            orig(self);
        }

    }
}