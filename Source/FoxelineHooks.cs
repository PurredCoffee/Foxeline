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
            List<List<Vector2>> tailPositions = selfData.Get<List<List<Vector2>>>(FoxelineConst.TailPositions);
            List<List<Vector2>> tailOffsets = selfData.Get<List<List<Vector2>>>(FoxelineConst.TailOffset);
            List<List<Vector2>> tailVelocities = selfData.Get<List<List<Vector2>>>(FoxelineConst.Velocity);
            float tailScale = FoxelineHelpers.getTailScale(self);

            //special cases
            bool crouched = FoxelineHelpers.isCrouched(self);
            bool droopTail = FoxelineHelpers.shouldDroopTail(self);
            bool flipTail = FoxelineHelpers.shouldFlipTail(self);
            bool restTail = FoxelineHelpers.shouldRestTail(self);
            bool stretchTail = FoxelineHelpers.shouldStretchTail(self);

            //Vertical flip
            bool GravHelperFlip = DynamicData.For(self.Sprite.Entity).Data.TryGetValue(FoxelineConst.GravHelperFlip, out var value) && (bool)value;
            float flipped = self.Sprite.Scale.Y * (GravHelperFlip ? -1 : 1);



            //the current direction the player is looking in
            Vector2 faceDirection = new Vector2((float)self.Facing, flipped);

            //the position the tail will grow out of
            //if in animation, use custom tail center
            if (!FoxelineConst.customTailPositions.TryGetValue(self.Sprite.LastAnimationID, out Vector2 offset))
            {
                offset = new(droopTail ? 0 : -2, crouched ? 3 : 6);
                offset.X += MathF.Sin(Engine.FrameCounter / 30f) / 2f;
            }

            int tailCount = FoxelineHelpers.getTailCount(self);
            for(int o = 0; o < tailCount; o++) {
                if(tailPositions.Count <= o) {
                    tailPositions.Add(new List<Vector2>());
                    tailVelocities.Add(new List<Vector2>());
                    tailOffsets.Add(new List<Vector2>());
                    for(int i = 0; i < FoxelineConst.tailLen; i++) {
                        tailPositions[o].Add(Vector2.Zero);
                        tailVelocities[o].Add(Vector2.Zero);
                        tailOffsets[o].Add(Vector2.Zero);
                    }
                }

                tailOffsets[o][0] = offset * faceDirection;
                
                Vector2 oldPos = tailPositions[o][0];
                Vector2 newPos = self.Nodes[0] + tailOffsets[o][0];
                Vector2 diff = newPos - oldPos;
                //rotate the difference
                float rotation = 
                    (o + (tailCount % 2 == 0?2:1)) / 2
                    * MathHelper.Pi / 
                    (tailCount + (tailCount % 2 == 0?2:1)) * 
                    (o % 2 == 0 ? -1 : 1) 
                    * FoxelineHelpers.getTailSpread(self);
                Vector2 sign = new Vector2(Math.Sign(diff.X), Math.Sign(diff.Y));
                Vector2 rotatedDiff = new Vector2(Math.Abs(diff.X), Math.Abs(diff.Y)).Rotate(rotation) * sign;
                tailPositions[o][0] = rotatedDiff + oldPos;

                for (int i = 1; i < FoxelineConst.tailLen; i++)
                {
                    if (i >= tailPositions[o].Count)
                    {
                        tailPositions[o].Add(tailPositions[o][i - 1]);
                        tailVelocities[o].Add(Vector2.Zero);
                        tailOffsets[o].Add(Vector2.Zero);
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

                        //are we in an animation which should turn the tail the other direction?
                        //example: sleep
                        if (flipTail)
                            tailDir.X *= -1;

                        //are we in an animation which should keep the tail on the ground?
                        //example: sleep, roll
                        if (restTail)
                            tailDir.Y = 0;

                        //are we in an animation which should keep the tail closer to the ground?
                        //example: idleC (sneeze), edge
                        if (stretchTail)
                            tailDir *= new Vector2(2f, 0.5f);

                        //tail direction can go zonkers
                        tailDir = tailDir.SafeNormalize();

                        //bugfix
                        if (stretchTail) tailDir *= 0.95f;

                        tailDir = tailDir.Rotate(rotation);
                        if(tailCount >= 6 && o % 3 == 2) {
                            tailDir.X *= -1;
                        }

                        //we clamp the tail piece into reach for the other tail piece as it has moved
                        FoxelineHelpers.clampTail(tailPositions[o], i, tailScale);

                        //the position each part of the tail want to reach
                        Vector2 basePos = tailPositions[o][i - 1] + tailDir * FoxelineConst.tailSize[i] * tailScale * faceDirection;

                        //the tail tries to get into position and accelerates towards it or breaks towards it
                        tailVelocities[o][i] = Calc.LerpSnap(
                            tailVelocities[o][i],
                            basePos - tailPositions[o][i],
                            1f - (float)Math.Pow(
                                1f - FoxelineModule.Settings.FoxelineConstants.Control / 100f,
                                Engine.DeltaTime
                                ),
                            1f);

                        //if we just landed from the big temple fall, make the tail also follow through
                        //makes it look more natural

                        //note: fallPose's frame 0 lasts for 6 frames, so this'll be run for 6 frames
                        if (self.Sprite is { LastAnimationID: "fallPose", CurrentAnimationFrame: 0 })
                        {
                            //i just made up some formula with some trial and error and it looks good..? i guess?
                            //- Snip
                            tailVelocities[o][i] = tailVelocities[o][i] with { Y = MathF.Exp(i/FoxelineConst.tailLen) + FoxelineConst.tailSize[i] };
                        }

                        //while flying, keep tail as trail
                        if (self.Sprite.LastAnimationID == "starFly")
                            tailVelocities[o][i] = Vector2.Zero;

                        //the tail then updates its positon based on how much it was accelerated
                        tailPositions[o][i] += tailVelocities[o][i]
                            * Engine.DeltaTime
                            / (1 - FoxelineModule.Settings.FoxelineConstants.Speed / 100f);
                    }

                    //we clamp the tail piece into reach for the other tail piece as the current tail has moved
                    FoxelineHelpers.clampTail(tailPositions[o], i, tailScale);

                    //update the tail offset for rendering because celeste will sometimes not use hair_move function
                    //this is a bugfix
                    tailOffsets[o][i] = tailPositions[o][i] - self.Nodes[0];
                }
                
                //unrotate the difference
                for (int i = 0; i < FoxelineConst.tailLen; i++) {
                    //undo the difference
                    tailPositions[o][i] -= rotatedDiff - diff;
                }
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
            if (self.Sprite.LastAnimationID == "starFly")
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
                FoxelineHelpers.drawTails(self, selfData);
                return;
            }
            FoxelineHelpers.drawTails(self, selfData);
            orig(self);
        }
        public static void PlayerHair_ctor(On.Celeste.PlayerHair.orig_ctor orig, PlayerHair self, PlayerSprite sprite)
        {
            DynamicData selfData = DynamicData.For(self);
            List<List<Vector2>> tailPositions = [];
            List<List<Vector2>> tailOffsets = [];
            List<List<Vector2>> tailVelocities = [];
            selfData.Set(FoxelineConst.TailPositions, tailPositions);
            selfData.Set(FoxelineConst.Velocity, tailOffsets);
            selfData.Set(FoxelineConst.TailOffset, tailVelocities);
            int tailCount = FoxelineHelpers.getTailCount(self);
            for(int o = 0; o < tailCount; o++) {
                tailPositions.Add([]);
                tailOffsets.Add([]);
                tailVelocities.Add([]);
                for (int i = 0; i < FoxelineConst.tailLen; i++)
                {
                    tailPositions[o].Add(Vector2.Zero);
                    tailVelocities[o].Add(Vector2.Zero);
                    tailOffsets[o].Add(Vector2.Zero);
                }
            }
            orig(self, sprite);
        }
        public static void PlayerHair_Start(On.Celeste.PlayerHair.orig_Start orig, PlayerHair self)
        {
            DynamicData selfData = DynamicData.For(self);
            List<List<Vector2>> tailPositions = selfData.Get<List<List<Vector2>>>(FoxelineConst.TailPositions);
            Vector2 value = self.Entity.Position + new Vector2(-(int)self.Facing * 200, 200f);
            for(int o = 0; o < tailPositions.Count; o++) {
                for (int i = 0; i < tailPositions[o].Count; i++)
                {
                    tailPositions[o][i] = value;
                }
            }
            orig(self);
        }
        public static void PlayerHair_MoveHairBy(On.Celeste.PlayerHair.orig_MoveHairBy orig, PlayerHair self, Vector2 amount)
        {
            DynamicData selfData = DynamicData.For(self);
            List<List<Vector2>> tailPositions = selfData.Get<List<List<Vector2>>>(FoxelineConst.TailPositions);
            for (int o = 0; o < tailPositions.Count; o++) {
                for (int i = 0; i < tailPositions[o].Count; i++)
                {
                    tailPositions[o][i] += amount;
                }
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

        //render the exact positions the tail(s) will grow out of for debugging help
        public static void Component_DebugRender(On.Monocle.Component.orig_DebugRender orig, Component self, Camera camera)
        {
            orig(self, camera);

            //the player got its own hook because the hurtbox is rendered after all the components have been rendered
            //which hides the tail position
            if (!(self is PlayerHair { Entity: not Player } hair && FoxelineHelpers.correctTailOwner(hair)))
                return;

            if (FoxelineHelpers.getTailVariant(hair) != TailVariant.None)
                FoxelineHelpers.drawTailPositions(hair, DynamicData.For(self));
        }

        public static void Player_DebugRender(On.Celeste.Player.orig_DebugRender orig, Player self, Camera camera)
        {
            orig(self, camera);

            PlayerHair hair = self.Hair;

            if (FoxelineHelpers.getTailVariant(hair) != TailVariant.None)
                FoxelineHelpers.drawTailPositions(hair, DynamicData.For(hair));
        }

    }
}
