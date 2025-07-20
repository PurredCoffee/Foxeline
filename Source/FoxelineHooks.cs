using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

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

        TailCollection tails = TailCollection.GetOrCreate(self, selfData);
        float tailScale = FoxelineHelpers.getTailScale(self);

        bool droopTail = FoxelineHelpers.shouldDroopTail(self);
        bool flipTail = FoxelineHelpers.shouldFlipTail(self);
        bool restTail = FoxelineHelpers.shouldRestTail(self);
        bool stretchTail = FoxelineHelpers.shouldStretchTail(self);
        int tailCount = FoxelineHelpers.getTailCount(self);
        float tailSpread = FoxelineHelpers.getTailSpread(self);

        FoxelineModuleSettings.Constants constants = FoxelineModule.Settings.FoxelineConstants;
        float swayAmplitude = constants.droopSwayAmplitude / 100f;
        float swaySpeed = constants.droopSwaySpeed;
        float swayFrequency = constants.droopSwayFrequency / 100f;
        float tailControl = constants.Control / 100f;
        float tailSpeed = constants.Speed / 100f;

        //Vertical flip
        bool isFlipped = GravityHelperInterop.IsPlayerInverted();
        float flipped = self.Sprite.Scale.Y * (isFlipped ? -1 : 1);

        //the current direction the player is looking in
        Vector2 faceDirection = new((float)self.Facing, flipped);

        //the position the tails will grow out of
        Vector2 basePosition = FoxelineHelpers.getTailBasePosition(self);

        tails.EnsureTailsInitialized(tailCount);
        foreach (Tail tail in tails)
        {
            TailNode baseNode = tail.TailNodes[0];

            baseNode.Offset = basePosition * faceDirection;

            Vector2 oldPos = baseNode.Position;
            Vector2 newPos = self.Nodes[0] + baseNode.Offset;
            Vector2 diff = newPos - oldPos;

            //we want to fan out the tails side-by-side evenly from tailDir (defined later below)
            //see https://www.desmos.com/calculator/5uj1pvifna for a visualization
            //- fanAngleSplit = how many segments should the fan be split into
            //- tailFanMultiplier = which fan segment should the tail follow

            int fanAngleSplit = tailCount + 2 - tailCount % 2;
            int tailFanMultiplier = (tail.TailIndex + 2 - tailCount % 2) / 2;
            if (tail.TailIndex % 2 == 0)
                // spread clockwise instead of counter-clockwise for even tail indices
                tailFanMultiplier *= -1;

            float rotation = Calc.HalfCircle / fanAngleSplit * tailFanMultiplier * tailSpread;
            Vector2 rotatedDiff = diff.Abs().Rotate(rotation) * diff.Sign();

            baseNode.Position = rotatedDiff + oldPos;

            for (int i = 1; i < Tail.TailNodeCount; i++)
            {
                TailNode previousNode = tail.TailNodes[i-1];
                TailNode thisNode = tail.TailNodes[i];

                if (self.Sprite.LastAnimationID == "starFly")
                {
                    //while flying, keep tail as trail
                    thisNode.Velocity = Vector2.Zero;
                }
                else if (self.SimulateMotion)
                {
                    float normalizedIndex = thisNode.NormalizedTailNodeIndex;

                    //this equation moves the points along an S-ish shape one behind the other
                    float x = (normalizedIndex - 0.5f) * 2;
                    if (x < 0)
                        x = (float)Math.Sqrt(-x);

                    Vector2 tailDir = new(-0.5f, -1 + x);

                    //if we are thrown by badeline we want to have a droopy tail since we cannot rotate it around madeline
                    //if madeline's out of stamina she doesn't have the strength to keep it up either
                    //make it sway side to side a bit to make it a bit more lively
                    if (droopTail)
                    {
                        float t = Engine.FrameCounter / swaySpeed;
                        float swayOffsetPerNode = normalizedIndex * thisNode.NodeSize * swayFrequency;

                        float targetSwayX = normalizedIndex * swayAmplitude * MathF.Sin(t - swayOffsetPerNode);

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
                    if(tailCount >= 6 && tail.TailIndex % 3 == 2)
                        tailDir.X *= -1;

                    //we clamp the tail node into reach for the other tail node as it has moved
                    tail.ClampPosition(i, tailScale);

                    //the position each part of the tail want to reach
                    Vector2 targetPosition =
                        previousNode.Position + tailDir * thisNode.NodeSize * tailScale * faceDirection;

                    //the tail tries to get into position and accelerates towards it or brakes towards it
                    thisNode.Velocity = Calc.LerpSnap(
                        thisNode.Velocity,
                        targetPosition - thisNode.Position,
                        1f - (float)Math.Pow(1f - tailControl, Engine.DeltaTime),
                        1f);

                    //if we just landed from the big temple fall, make the tail also follow through
                    //makes it look more natural

                    //note: fallPose's frame 0 lasts for 6 frames, so this will be run for 6 frames
                    if (self.Sprite is { LastAnimationID: "fallPose", CurrentAnimationFrame: 0 })
                        //i just made up some formula with some trial and error, and it looks good...? i guess?
                        //- Snip
                        thisNode.Velocity = thisNode.Velocity with {
                            Y = MathF.Exp(normalizedIndex) + thisNode.NodeSize
                        };

                    //the tail then updates its positon based on how much it was accelerated
                    thisNode.Position += thisNode.Velocity * Engine.DeltaTime / (1 - tailSpeed);
                }

                //we clamp the tail node into reach for the other tail node as the current tail has moved
                tail.ClampPosition(i, tailScale);
            }

            //unrotate the difference
            foreach (TailNode node in tail.TailNodes) {
                //undo the difference
                node.Position -= rotatedDiff - diff;

                //update the tail offset for rendering because celeste will sometimes not use hair_move function
                //this is a bugfix
                node.Offset = node.Position - self.Nodes[0];
            }
        }
    }

    public static void PlayerHair_Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
    {
        if (!self.Visible || !self.Sprite.Visible)
        {
            orig(self);
            return;
        }

        //only handle tail if:
        //- it's enabled
        //- the entity is a tail owner
        if (FoxelineHelpers.getTailVariant(self) == TailVariant.None || !FoxelineHelpers.correctTailOwner(self))
        {
            orig(self);
            return;
        }

        TailCollection tails = TailCollection.GetOrCreate(self);

        if (self.Sprite.LastAnimationID != "starFly")
        {
            tails.DrawAllTails();
            orig(self);
            return;
        }

        //special case for star fly
        if (!FoxelineHelpers.getFeatherTail(self))
        {
            //don't draw the tail if disabled
            orig(self);
            return;
        }

        //else draw the tail instead of the hair
        Vector2 pos = self.Sprite.RenderPosition;
        self.Sprite.Texture.Draw(pos + Vector2.UnitX, self.Sprite.Origin, Color.Black, self.Sprite.Scale);
        self.Sprite.Texture.Draw(pos + Vector2.UnitY, self.Sprite.Origin, Color.Black, self.Sprite.Scale);
        self.Sprite.Texture.Draw(pos - Vector2.UnitX, self.Sprite.Origin, Color.Black, self.Sprite.Scale);
        self.Sprite.Texture.Draw(pos - Vector2.UnitY, self.Sprite.Origin, Color.Black, self.Sprite.Scale);
        tails.DrawAllTails();
    }

    public static void PlayerHair_ctor(On.Celeste.PlayerHair.orig_ctor orig, PlayerHair self, PlayerSprite sprite)
    {
        TailCollection.GetOrCreate(self);

        orig(self, sprite);
    }

    public static void PlayerHair_Start(On.Celeste.PlayerHair.orig_Start orig, PlayerHair self)
    {
        Vector2 startPosition = self.Entity.Position + new Vector2(-(int)self.Facing * 200, 200f);
        TailCollection.GetOrCreate(self).InitializeTailPositions(startPosition);

        orig(self);
    }

    public static void PlayerHair_MoveHairBy(On.Celeste.PlayerHair.orig_MoveHairBy orig, PlayerHair self, Vector2 amount)
    {
        TailCollection.GetOrCreate(self).MoveTailsBy(amount);

        orig(self, amount);
    }

    public static MTexture Hair_GetTexture(On.Celeste.PlayerHair.orig_GetHairTexture orig, PlayerHair self, int index)
    {
        //node index 0 is the bangs node, so we only change that
        if (index != 0 || !FoxelineHelpers.shouldChangeHair(self))
            return orig(self, index);

        int bangsFrame = Calc.Clamp(self.Sprite.HairFrame, 0, FoxelineModule.Instance.BangsTextures.Count - 1);
        return FoxelineModule.Instance.BangsTextures[bangsFrame];
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
            TailCollection.GetOrCreate(hair).DrawTailBasePositions();
    }

    public static void Player_DebugRender(On.Celeste.Player.orig_DebugRender orig, Player self, Camera camera)
    {
        orig(self, camera);

        PlayerHair hair = self.Hair;

        if (FoxelineHelpers.getTailVariant(hair) != TailVariant.None)
            TailCollection.GetOrCreate(hair).DrawTailBasePositions();
    }

}
