using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.Foxeline
{
    public static class FoxelineHelpers
    {
        /// <summary>
        /// Clamps the tail piece into reach of the previous tail piece
        /// </summary>
        /// <param name="tailPositions">List of tail positions to edit</param>
        /// <param name="i">Tail index</param>
        public static void clampTail(List<Vector2> tailPositions, int i)
        {

            if (Vector2.Distance(tailPositions[i - 1], tailPositions[i]) > FoxelineConst.tailSize[i] * FoxelineModule.Settings.TailScale / 100f)
            {
                Vector2 diff = tailPositions[i - 1] - tailPositions[i];
                diff.Normalize();
                tailPositions[i] = tailPositions[i - 1] - diff * FoxelineConst.tailSize[i] * FoxelineModule.Settings.TailScale / 100f;
            }
        }

        /// <summary>
        /// Draws the tail of the player based on the tail positions defined under selfData and the hair offset
        /// </summary>
        /// <param name="self">The PlayerHair object</param>
        /// <param name="selfData">The DynamicData object for the PlayerHair object</param>
        public static void drawTail(PlayerHair self, DynamicData selfData)
        {
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

        /// <summary>
        /// Draws the outline of the tail of the player based on the tail positions defined under selfData and the hair offset
        /// </summary>
        /// <param name="self">The PlayerHair object to draw the tail next to</param>
        /// <param name="selfData">The DynamicData object for the PlayerHair object</param>
        public static void drawTailOutline(PlayerHair self, DynamicData selfData)
        {
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
        /// <summary>
        /// Draws the hair of the player along with the tail if enabled
        /// This function is a modified version of the original PlayerHair.Render function
        /// It changes the Border drawing to draw the tail below parts of the hair
        /// </summary>
        /// <param name="self"></param>
        /// <param name="selfData"></param>
        public static void Collective_Render(PlayerHair self, DynamicData selfData)
        {
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

        /// <summary>
        /// Helper function to get the hair color of the player based on the current animation
        /// </summary>
        /// <param name="hairNodeIndex">The index of the hair color</param>
        /// <param name="self">The PlayerHair object</param>
        /// <param name="selfData">The DynamicData object for the PlayerHair object</param>
        /// <returns>The smarter hair color of the player</returns>
        public static Color getHairColor(int hairNodeIndex, PlayerHair self, DynamicData selfData)
        {
            //only handle tail if:
            //- it's enabled
            //- the entity is a Player
            if (FoxelineModule.Settings.Tail == TailVariant.None || self.Entity is not Player)
                return self.GetHairColor(hairNodeIndex);

            Dictionary<string, int> CutsceneToDashLookup = self.Sprite.EntityAs<Player>().Inventory.Backpack
                ? FoxelineConst.backpackCutscenes
                : FoxelineConst.noBackpackCutscenes;

            //if there's no animation id in the lookup, just do the default
            if (!CutsceneToDashLookup.TryGetValue(self.Sprite.CurrentAnimationID, out var dashes))
                return self.GetHairColor(hairNodeIndex);


            //SMH PLUS
            if (selfData.TryGet("smh_hairConfig", out var hairConfig))
            {
                //anything can be null here - use null conditional operator to avoid null reference exceptions
                Dictionary<int, List<Color>> hairColors = (Dictionary<int, List<Color>>)hairConfig.GetType()?.GetField("actualHairColors")?.GetValue(hairConfig);
                if (hairColors is not null)
                {
                    //we found something that looks like a hairConfig from SMH PLUS
                    if (hairColors?.ContainsKey(hairNodeIndex) ?? false)
                        //fallback to default hair color if the hairConfig is broken
                        hairNodeIndex = 100;
                    dashes = Math.Max(Math.Min(dashes, hairColors[hairNodeIndex].Count - 1), 0);
                    return hairColors[hairNodeIndex][dashes];
                }
            }

            //VANILLA FALLBACK
            return dashes switch
            {
                0 => Player.UsedHairColor,
                1 => Player.NormalHairColor,
                2 => Player.TwoDashesHairColor,
                _ => self.GetHairColor(hairNodeIndex)
            };
        }

        public static bool isCrouched(PlayerHair hair)
            => hair is
            {
                Sprite.CurrentAnimationID: "duck" or "slide"
            };

        public static bool shouldDroopTail(PlayerHair hair)
            => hair is
            {
                Sprite.CurrentAnimationID: "spin" or "launch"
            }
            || (hair.Entity is Player && hair.Sprite.EntityAs<Player>().Stamina <= Player.ClimbTiredThreshold);

        public static bool shouldFlipTail(PlayerHair hair)
            => hair is
            {
                Sprite.CurrentAnimationID: "wakeUp" or "sleep" or "sitDown" or "bagDown" or "asleep" or "halfWakeUp"
            };

        public static bool shouldStretchTail(PlayerHair hair)
            => hair is
            {
                Sprite.CurrentAnimationID: "edge" or "idleC"
            }
            || isCrouched(hair);

        public static bool correctTailOwner(PlayerHair hair)
            => hair.Entity is Player;
    }
}