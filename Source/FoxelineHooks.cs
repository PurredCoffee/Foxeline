using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.Foxeline;

public static class FoxelineHooks
{
    public static void PlayerHair_AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self)
    {
        orig(self);

        DynamicData selfData = DynamicData.For(self);

        //only handle tail if:
        //- the entity is a tail owner
        //- tail variant isn't none
        if (!(FoxelineHelpers.correctTailOwner(self) && FoxelineHelpers.getTailVariant(self) != TailVariant.None))
            return;

        //List<T> is a reference type - this means that modifying these lists will modify the lists in DynamicData
        //which simplifies code and improves performance
        List<List<Vector2>> allTailPositions = FoxelineHelpers.getAllTailPositions(selfData);
        List<List<Vector2>> allTailOffsets = FoxelineHelpers.getAllTailOffsets(selfData);
        List<List<Vector2>> allTailVelocities = FoxelineHelpers.getAllTailVelocities(selfData);
        float tailScale = FoxelineHelpers.getTailScale(self);

        //special cases
        bool crouched = FoxelineHelpers.isCrouched(self);
        bool droopTail = FoxelineHelpers.shouldDroopTail(self);
        bool flipTail = FoxelineHelpers.shouldFlipTail(self);
        bool restTail = FoxelineHelpers.shouldRestTail(self);
        bool stretchTail = FoxelineHelpers.shouldStretchTail(self);

        //Vertical flip
        bool isFlipped = GravityHelperInterop.IsPlayerInverted();
        float flipped = self.Sprite.Scale.Y * (isFlipped ? -1 : 1);

        //the current direction the player is looking in
        Vector2 faceDirection = new((float)self.Facing, flipped);

        //the position the tail will grow out of
        //if in animation, use custom tail center
        if (!FoxelineConst.customTailPositions.TryGetValue(self.Sprite.LastAnimationID, out Vector2 offset))
        {
            offset = new Vector2(droopTail ? 0 : -2, crouched ? 3 : 6);
            offset.X += MathF.Sin(Engine.FrameCounter / 30f) / 2f;
        }

        int tailCount = FoxelineHelpers.getTailCount(self);
        for (int iTail = 0; iTail < tailCount; iTail++)
        {
            if (allTailPositions.Count <= iTail)
                FoxelineHelpers.ensureTailDataInitialized(selfData, tailCount);

            List<Vector2> tailPositions = allTailPositions[iTail];
            List<Vector2> tailOffsets = allTailOffsets[iTail];
            List<Vector2> tailVelocities = allTailVelocities[iTail];

            tailOffsets[0] = offset * faceDirection;

            Vector2 oldPos = tailPositions[0];
            Vector2 newPos = self.Nodes[0] + tailOffsets[0];
            Vector2 diff = newPos - oldPos;

            //we want to fan out the tails side-by-side evenly from tailDir (defined later below)
            //see https://www.desmos.com/calculator/5uj1pvifna for a visualization
            //- fanAngleSplit = how many segments should the fan be split into
            //- tailFanMultiplier = which fan segment should the tail follow

            int fanAngleSplit = tailCount + 2 - tailCount % 2;
            int tailFanMultiplier = (iTail + 2 - tailCount % 2) / 2;
            if (iTail % 2 == 0)
                // spread clockwise instead of counter-clockwise for even tail indices
                tailFanMultiplier *= -1;

            float rotation = Calc.HalfCircle / fanAngleSplit * tailFanMultiplier * FoxelineHelpers.getTailSpread(self);
            Vector2 rotatedDiff = diff.Abs().Rotate(rotation) * diff.Sign();

            tailPositions[0] = rotatedDiff + oldPos;

            for (int iTailNode = 1; iTailNode < FoxelineConst.tailLen; iTailNode++)
            {
                if (iTailNode >= tailPositions.Count)
                {
                    tailPositions.Add(tailPositions[iTailNode - 1]);
                    tailVelocities.Add(Vector2.Zero);
                    tailOffsets.Add(Vector2.Zero);
                }

                if (self.SimulateMotion)
                {
                    float normalizedTailNode = (float)iTailNode / FoxelineConst.tailLen;

                    //this equation moves the points along an S-ish shape one behind the other
                    float x = (normalizedTailNode - 0.5f) * 2;
                    if (x < 0)
                        x = (float)Math.Sqrt(-x);

                    Vector2 tailDir = new(-0.5f, -1 + x);

                    //if we are thrown by badeline we want to have a droopy tail since we cannot rotate it around madeline
                    //if madeline's out of stamina she doesn't have the strength to keep it up either
                    //make it sway side to side a bit to make it a bit more lively
                    if (droopTail)
                    {
                        float swayAmplitude = FoxelineModule.Settings.FoxelineConstants.droopSwayAmplitude / 100f;
                        float swaySpeed = FoxelineModule.Settings.FoxelineConstants.droopSwaySpeed;
                        float swayFrequency = FoxelineModule.Settings.FoxelineConstants.droopSwayFrequency / 100f;

                        float t = Engine.FrameCounter / swaySpeed;
                        float swayOffsetPerNode = normalizedTailNode * FoxelineConst.tailSize[iTailNode] * swayFrequency;

                        float targetSwayX = normalizedTailNode * swayAmplitude * MathF.Sin(t - swayOffsetPerNode);

                        tailDir = new Vector2(targetSwayX, 1);
                    }

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
                    if(tailCount >= 6 && iTail % 3 == 2)
                        tailDir.X *= -1;

                    //we clamp the tail node into reach for the other tail node as it has moved
                    FoxelineHelpers.clampTail(tailPositions, iTailNode, tailScale);

                    //the position each part of the tail want to reach
                    Vector2 basePos = tailPositions[iTailNode - 1]
                        + tailDir * FoxelineConst.tailSize[iTailNode] * tailScale * faceDirection;

                    //the tail tries to get into position and accelerates towards it or brakes towards it
                    tailVelocities[iTailNode] = Calc.LerpSnap(
                        tailVelocities[iTailNode],
                        basePos - tailPositions[iTailNode],
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
                        tailVelocities[iTailNode] = tailVelocities[iTailNode] with {
                            Y = MathF.Exp(normalizedTailNode) + FoxelineConst.tailSize[iTailNode]
                        };
                    }

                    //while flying, keep tail as trail
                    if (self.Sprite.LastAnimationID == "starFly")
                        tailVelocities[iTailNode] = Vector2.Zero;

                    //the tail then updates its positon based on how much it was accelerated
                    tailPositions[iTailNode] += tailVelocities[iTailNode]
                        * Engine.DeltaTime
                        / (1 - FoxelineModule.Settings.FoxelineConstants.Speed / 100f);
                }

                //we clamp the tail node into reach for the other tail node as the current tail has moved
                FoxelineHelpers.clampTail(tailPositions, iTailNode, tailScale);
            }

            //unrotate the difference
            for (int i = 0; i < FoxelineConst.tailLen; i++) {
                //undo the difference
                tailPositions[i] -= rotatedDiff - diff;

                //update the tail offset for rendering because celeste will sometimes not use hair_move function
                //this is a bugfix
                tailOffsets[i] = tailPositions[i] - self.Nodes[0];
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
        FoxelineHelpers.ensureTailDataInitialized(selfData, FoxelineHelpers.getTailCount(self));
        orig(self, sprite);
    }

    public static void PlayerHair_Start(On.Celeste.PlayerHair.orig_Start orig, PlayerHair self)
    {
        DynamicData selfData = DynamicData.For(self);
        Vector2 startOffset = self.Entity.Position + new Vector2(-(int)self.Facing * 200, 200f);

        foreach (List<Vector2> tailPositions in FoxelineHelpers.getAllTailPositions(selfData))
            for (int i = 0; i < tailPositions.Count; i++)
                tailPositions[i] = startOffset;

        orig(self);
    }
    public static void PlayerHair_MoveHairBy(On.Celeste.PlayerHair.orig_MoveHairBy orig, PlayerHair self, Vector2 amount)
    {
        DynamicData selfData = DynamicData.For(self);

        foreach (List<Vector2> tailPositions in FoxelineHelpers.getAllTailPositions(selfData))
            for (int i = 0; i < tailPositions.Count; i++)
                tailPositions[i] += amount;

        orig(self, amount);
    }

    public static MTexture Hair_GetTexture(On.Celeste.PlayerHair.orig_GetHairTexture orig, PlayerHair self, int index)
    {
        //node index 0 is the bangs node, so we only change that
        if (index != 0 || !FoxelineHelpers.shouldChangeHair(self))
            return orig(self, index);

        int bangsFrame = Calc.Clamp(self.Sprite.HairFrame, 0, FoxelineModule.Instance.bangs.Count - 1);
        return FoxelineModule.Instance.bangs[bangsFrame];
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
