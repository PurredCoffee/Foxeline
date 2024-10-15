using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using Color = Microsoft.Xna.Framework.Color;

using Celeste.Mod.CelesteNet.Client;
using Celeste.Mod.CelesteNet.Client.Entities;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Celeste.Mod.Foxeline
{
    
    public static class TailNetHelper {
        public static bool TryGetTailInformation(PlayerHair hair, out FoxelineModuleSettings.TailDefaults tail) {
            if(FoxelineHelpers.isCnetInstalled()) {
                _TryGetTailInformation(hair, out tail);
                if(tail != null) {
                    return true;
                }
            }
            tail = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool _TryGetTailInformation(PlayerHair hair, out FoxelineModuleSettings.TailDefaults tail) {
            if(hair.Entity is Ghost ghost) {
                if(CelesteNet.TailComponent.TailInformation.TryGetValue(ghost.PlayerInfo.ID, out FoxelineModuleSettings.TailDefaults tailInfo)) {
                    tail = tailInfo;
                    return true;
                }
            }
            tail = null;
            return false;
        }
    }

    public static class FoxelineHelpers
    {
        /// <summary>
        /// Gets the tail variant for the player based on the settings
        /// </summary>
        /// <param name="selfData">selfData Object for the PlayerHair</param>
        /// <param name="self">PlayerHair object</param>
        /// <returns>TailVariant enum corresponding to the tail</returns>
        public static TailVariant getTailVariant(PlayerHair self)
        {
            if(isPlayerHair(self))
            {
                return FoxelineModule.Settings.Tail;
            }
            if(isBadelineHair(self))
            {
                return FoxelineModule.Settings.BadelineTail.Tail;
            }
            if(TailNetHelper.TryGetTailInformation(self, out var tail)) {
                return tail.Tail;
            }
            return FoxelineModule.Settings.CelestenetDefaults.Tail;
        }
        /// <summary>
        /// Gets the tail scale for the player based on the settings
        /// </summary>
        /// <param name="self">PlayerHair object</param>
        /// <returns>scaling factor of the tail</returns>
        public static float getTailScale(PlayerHair self)
        {
            if(isPlayerHair(self))
            {
                return FoxelineModule.Settings.TailScale / 100f;
            }
            if(isBadelineHair(self))
            {
                return FoxelineModule.Settings.BadelineTail.TailScale / 100f;
            }
            if(TailNetHelper.TryGetTailInformation(self, out var tail)) {
                return tail.TailScale / 100f;
            }
            return FoxelineModule.Settings.CelestenetDefaults.TailScale / 100f;
        }
        /// <summary>
        /// Gets the tail count for the player based on the settings
        /// </summary>
        /// <param name="self">PlayerHair object</param>
        /// <returns>Number of tails on the player</returns>
        public static int getTailCount(PlayerHair self)
        {
            if(isPlayerHair(self))
            {
                return FoxelineModule.Settings.TailCount;
            }
            if(isBadelineHair(self))
            {
                return FoxelineModule.Settings.BadelineTail.TailCount;
            }
            if(TailNetHelper.TryGetTailInformation(self, out var tail)) {
                return tail.TailCount;
            }
            return FoxelineModule.Settings.CelestenetDefaults.TailCount;
        }
        /// <summary>
        /// Gets the tail offset for the player based on the settings
        /// </summary>
        /// <param name="self">PlayerHair object</param>
        /// <returns>Whether tails should be drawn collectively or with separate outlines</returns>
        public static bool getCollectTails(PlayerHair self)
        {
            if(isPlayerHair(self))
            {
                return FoxelineModule.Settings.CollectTails;
            }
            if(isBadelineHair(self))
            {
                return FoxelineModule.Settings.BadelineTail.CollectTails;
            }
            if(TailNetHelper.TryGetTailInformation(self, out var tail)) {
                return tail.CollectTails;
            }
            return FoxelineModule.Settings.CelestenetDefaults.CollectTails;
        }

        /// <summary>
        /// Gets the tail spread for the player based on the settings
        /// </summary>
        /// <param name="self">PlayerHair object</param>
        /// <returns>Spread factor of the tail</returns>
        public static float getTailSpread(PlayerHair self)
        {
            if(isPlayerHair(self))
            {
                return FoxelineModule.Settings.TailSpread / 100f;
            }
            if(isBadelineHair(self))
            {
                return FoxelineModule.Settings.BadelineTail.TailSpread / 100f;
            }
            if(TailNetHelper.TryGetTailInformation(self, out var tail)) {
                return tail.TailSpread / 100f;
            }
            return FoxelineModule.Settings.CelestenetDefaults.TailSpread / 100f;
        }
        /// <summary>
        /// Flag to determine if the tail should be painted as a brush or not
        /// </summary>
        /// <param name="self">PlayerHair object</param>
        /// <returns>True if tail should be painted as a brush</returns>
        public static bool getPaintBrushTail(PlayerHair self)
        {
            if(isPlayerHair(self))
            {
                return FoxelineModule.Settings.PaintBrushTail;
            }
            if(isBadelineHair(self))
            {
                return FoxelineModule.Settings.BadelineTail.PaintBrushTail;
            }
            if(TailNetHelper.TryGetTailInformation(self, out var tail)) {
                return tail.PaintBrushTail;
            }
            return FoxelineModule.Settings.CelestenetDefaults.PaintBrushTail;
        }
        /// <summary>
        /// Gets the tail brush tint for the player based on the settings
        /// </summary>
        /// <param name="self">PlayerHair object</param>
        /// <returns>Color multiplier for the tip of the tail</returns>
        public static float getTailBrushTint(PlayerHair self)
        {
            if(isPlayerHair(self))
            {
                return FoxelineModule.Settings.TailBrushTint / 100f;
            }
            if(isBadelineHair(self))
            {
                return FoxelineModule.Settings.BadelineTail.TailBrushTint / 100f;
            }
            if(TailNetHelper.TryGetTailInformation(self, out var tail)) {
                return tail.TailBrushTint / 100f;
            }
            return FoxelineModule.Settings.CelestenetDefaults.TailBrushTint / 100f;
        }
        /// <summary>
        /// Gets the tail brush color for the player based on the settings
        /// </summary>
        /// <param name="self">PlayerHair object</param>
        /// <returns>Color for the tip of the tail</returns>
        public static Color getTailBrushColor(PlayerHair self)
        {
            if(isPlayerHair(self))
            {
                return FoxelineModule.Settings.TailBrushColor;
            }
            if(isBadelineHair(self))
            {
                return FoxelineModule.Settings.BadelineTail.TailBrushColor;
            }
            if(TailNetHelper.TryGetTailInformation(self, out var tail)) {
                return tail.TailBrushColor;
            }
            return FoxelineModule.Settings.CelestenetDefaults.TailBrushColor;
        }
        /// <summary>
        /// Gets the feather tail flag for the player based on the settings
        /// </summary>
        /// <param name="self">PlayerHair object</param>
        /// <returns>True if the tail should be drawn while player is in feather</returns>
        public static bool getFeatherTail(PlayerHair self)
        {
            if(isPlayerHair(self))
            {
                return FoxelineModule.Settings.FeatherTail;
            }
            if(isBadelineHair(self))
            {
                return FoxelineModule.Settings.BadelineTail.FeatherTail;
            }
            if(TailNetHelper.TryGetTailInformation(self, out var tail)) {
                return tail.FeatherTail;
            }
            return FoxelineModule.Settings.CelestenetDefaults.FeatherTail;
        }

        /// <summary>
        /// Gets the order in which the tails should be drawn
        /// </summary>
        /// <param name="tailCount">Number of tails</param>
        /// <returns>Array of tail indices in the order they should be drawn</returns>
        public static int[] getTailOrder(int tailCount) {
            switch (tailCount) {
                case 1:
                    return [ 0 ];
                case 2:
                    return [ 1, 0 ];
                case 3:
                    return [ 2, 0, 1 ];
                case 4:
                    return [ 2, 1, 3, 0 ];
                case 5:
                    return [ 4, 0, 3, 1, 2 ];
                case 6:
                    return [ 5, 0, 3, 2, 4, 1 ];
                case 7:
                    return [ 6, 0, 5, 1, 4, 2, 3 ];
                case 8:
                    return [ 7, 0, 6, 1, 4, 3, 5, 2 ];
                case 9:
                    return [ 8, 0, 7, 1, 6, 5, 2, 3, 4 ];
                default:
                    return [];
            }
        }
        /// <summary>
        /// Clamps the tail piece into reach of the previous tail piece
        /// </summary>
        /// <param name="tailPositions">List of tail positions to edit</param>
        /// <param name="i">Tail index</param>
        public static void clampTail(List<Vector2> tailPositions, int i, float tailScale)
        {

            if (Vector2.Distance(tailPositions[i - 1], tailPositions[i]) > FoxelineConst.tailSize[i] * tailScale)
            {
                Vector2 diff = tailPositions[i - 1] - tailPositions[i];
                diff.Normalize();
                tailPositions[i] = tailPositions[i - 1] - diff * FoxelineConst.tailSize[i] * tailScale;
            }
        }

        /// <summary>
        /// Draws the tail of the player based on the tail positions defined under selfData and the hair offset
        /// </summary>
        /// <param name="self">The PlayerHair object</param>
        /// <param name="selfData">The DynamicData object for the PlayerHair object</param>
        public static void drawTailInner(PlayerHair self, DynamicData selfData, int o)
        {
            List<List<Vector2>> tailOffset = selfData.Get<List<List<Vector2>>>(FoxelineConst.TailOffset);
            int currentVariant = (int)getTailVariant(self) - 1 + FoxelineConst.Variants * (getTailScale(self) > 1 ? 1 : 0);
            Color[] gradient = new Color[self.Sprite.HairCount];
            for (int i = 0; i < self.Sprite.HairCount; i++)
            {
                gradient[i] = getHairColor(i, self, selfData);
            }
            for (int i = FoxelineConst.tailLen - 1; i >= 0; i--)
            {
                MTexture tex = FoxelineModule.Instance.tailtex[currentVariant][FoxelineConst.tailID[i]];
                bool fill = (i < FoxelineConst.tailLen * (100 - FoxelineModule.Settings.FoxelineConstants.Softness) / 100f) != getPaintBrushTail(self);
                //fill color is either the hair color or a blend of the hair color at the tip of the tail and the base of the tail (sometimes visible)
                float lerp = Math.Min((float)i / FoxelineConst.tailLen, 1) * self.Sprite.HairCount;
                int hairNodeIndex = (int)lerp;
                int nextHairNodeIndex = Math.Min(hairNodeIndex + 1, self.Sprite.HairCount - 1);
                Color fullColor = Color.Lerp(gradient[hairNodeIndex], gradient[nextHairNodeIndex], lerp % 1);
                Color color = fill
                    ? fullColor
                    : Color.Lerp(
                        getTailBrushColor(self),
                        fullColor,
                        getTailBrushTint(self));
                Vector2 position = self.Nodes[0].Floor() + tailOffset[o][i].Floor();
                Vector2 center = Vector2.One * (float)Math.Floor(tex.Width / 2f);
                float Scale = getTailScale(self) / (getTailScale(self) > 1 ? 2 : 1);
                tex.Draw(position, center, color, Scale);
            }
        }

        /// <summary>
        /// Draws the outline of the tail of the player based on the tail positions defined under selfData and the hair offset
        /// </summary>
        /// <param name="self">The PlayerHair object to draw the tail next to</param>
        /// <param name="selfData">The DynamicData object for the PlayerHair object</param>
        public static void drawTailOutline(PlayerHair self, DynamicData selfData, int o)
        {
            List<List<Vector2>> tailOffset = selfData.Get<List<List<Vector2>>>(FoxelineConst.TailOffset);
            int currentVariant = (int)getTailVariant(self) - 1 + FoxelineConst.Variants * (getTailScale(self) > 1 ? 1 : 0);
            for (int i = FoxelineConst.tailLen - 1; i >= 0; i--)
            {
                //we select the current tail piece. tailID is currently baked and chosen to be pretty
                MTexture tex = FoxelineModule.Instance.tailtex[currentVariant][FoxelineConst.tailID[i]];
                //we calculate the position of the texture by offsetting it by half its size
                Vector2 position = self.Nodes[0].Floor() + tailOffset[o][i].Floor();
                Vector2 center = Vector2.One * (float)Math.Floor(tex.Width / 2f);
                float Scale = getTailScale(self) / (getTailScale(self) > 1 ? 2 : 1);
                tex.Draw(position + Vector2.UnitX, center, Color.Black, Scale);
                tex.Draw(position + Vector2.UnitY, center, Color.Black, Scale);
                tex.Draw(position - Vector2.UnitX, center, Color.Black, Scale);
                tex.Draw(position - Vector2.UnitY, center, Color.Black, Scale);
            }
        }

        /// <summary>
        /// Draws the tail and outline of the player based on the tail positions defined under selfData and the hair offset
        /// </summary>
        /// <param name="self">The PlayerHair object</param>
        /// <param name="selfData">The DynamicData object for the PlayerHair object</param>
        public static void drawTails(PlayerHair self, DynamicData selfData)
        {
            int tailCount = Math.Min(getTailCount(self), selfData.Get<List<List<Vector2>>>(FoxelineConst.TailOffset).Count);
            int[] tailOrder = getTailOrder(tailCount);
            if(getCollectTails(self))
            {
                foreach (int o in tailOrder)
                {
                    drawTailOutline(self, selfData, o);
                }
                foreach (int o in tailOrder)
                {
                    drawTailInner(self, selfData, o);
                }
                return;
            }
            foreach (int o in tailOrder)
            {
                drawTailOutline(self, selfData, o);
                drawTailInner(self, selfData, o);
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
            //only do this if:
            //- we're fixing cutscenes
            //- the entity is a Player
            if (!FoxelineModule.Settings.FixCutscenes || self.Entity is not Player player)
                return self.GetHairColor(hairNodeIndex);

            Dictionary<string, int> CutsceneToDashLookup = self.Sprite.EntityAs<Player>().Inventory.Backpack
                ? FoxelineConst.backpackCutscenes
                : FoxelineConst.noBackpackCutscenes;

            //if there's no animation id in the lookup, just do the default
            if (!CutsceneToDashLookup.TryGetValue(self.Sprite.LastAnimationID, out var dashes))
                return self.GetHairColor(hairNodeIndex);


            //SMH PLUS
            if (selfData.TryGet(FoxelineConst.smh_hairConfig, out var hairConfig))
            {
                //anything can be null here - use null conditional operator to avoid null reference exceptions
                Dictionary<int, List<Color>> hairColors = (Dictionary<int, List<Color>>)hairConfig.GetType()?.GetField("ActualHairColors")?.GetValue(hairConfig);
                if (hairColors is not null)
                {
                    //we found something that looks like a hairConfig from SMH PLUS
                    if (!hairColors.ContainsKey(hairNodeIndex))
                        //fallback to default hair color if the hairConfig is broken
                        hairNodeIndex = 100;
                    dashes = Math.Max(Math.Min(dashes, hairColors[hairNodeIndex].Count - 1), 0);
                    return hairColors[hairNodeIndex][dashes];
                }
            }

            //NON-VANILLA
            if (!FoxelineModule.Settings.UseVanillaHairColor)
            {
                int previousDashes = player.Dashes;

                player.Dashes = dashes;
                Color hairColor = self.GetHairColor(hairNodeIndex);
                player.Dashes = previousDashes;

                return hairColor;
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

        /// <summary>
        /// Determines whether the player hair is crouched.
        /// </summary>
        /// <param name="hair">The player hair.</param>
        /// <returns><c>true</c> if the player hair is crouched; otherwise, <c>false</c>.</returns>
        public static bool isCrouched(PlayerHair hair)
            => hair is
            {
                Sprite.LastAnimationID: "duck" or "slide"
            };

        /// <summary>
        /// Determines whether the player's hair should droop the tail.
        /// </summary>
        /// <param name="hair">The player's hair.</param>
        /// <returns><c>true</c> if the hair should droop the tail; otherwise, <c>false</c>.</returns>
        public static bool shouldDroopTail(PlayerHair hair)
            => hair is
            {
                Sprite.LastAnimationID: "launch" or "spin"
            }
            || (hair.Entity is Player && hair.Sprite.EntityAs<Player>().Stamina <= Player.ClimbTiredThreshold);

        /// <summary>
        /// Determines whether the tail should be flipped based on the current animation ID of the player's hair.
        /// </summary>
        /// <param name="hair">The player's hair.</param>
        /// <returns><c>true</c> if the tail should be flipped; otherwise, <c>false</c>.</returns>
        public static bool shouldFlipTail(PlayerHair hair)
            => hair is
            {
                Sprite.LastAnimationID: "asleep" or "bagDown" or "edgeBack" or "halfWakeUp" or "sitDown" or "sleep"
                or "wakeUp"
            };

        /// <summary>
        /// Determines whether the tail should be laying on the ground based on the current animation ID of the player's hair.
        /// </summary>
        /// <param name="hair">The player's hair.</param>
        /// <returns><c>true</c> if the tail should be laying on the ground; otherwise, <c>false</c>.</returns>
        public static bool shouldRestTail(PlayerHair hair)
            => hair is
            {
                Sprite.LastAnimationID: "asleep" or "bagDown" or "carryTheoCollapse" or "downed" or "edgeBack" or "halfWakeUp"
                or "roll" or "rollGetUp" or "sitDown" or "sleep" or "wakeUp"
            };

        /// <summary>
        /// Determines whether the tail of the player's hair should be stretched.
        /// </summary>
        /// <param name="hair">The player's hair.</param>
        /// <returns><c>true</c> if the tail should be stretched; otherwise, <c>false</c>.</returns>
        public static bool shouldStretchTail(PlayerHair hair)
            => hair is
            {
                Sprite.LastAnimationID: "dangling" or "edge" or "edgeBack" or "idleC" or "runWind" or "shaking"
                or "tired" or "tiredStill"
            }
            || isCrouched(hair);

        /// <summary>
        /// Determines if the specified <paramref name="hair"/> belongs to the correct tail owner.
        /// </summary>
        /// <param name="hair">The hair to check.</param>
        /// <returns><c>true</c> if the hair belongs to the correct tail owner; otherwise, <c>false</c>.</returns>
        public static bool correctTailOwner(PlayerHair hair)
            => isPlayerHair(hair) || isBadelineHair(hair) || isGhostHair(hair);

        /// <summary>
        /// Checks if the specified <see cref="PlayerHair"/> belongs to a player entity.
        /// </summary>
        /// <param name="hair">The <see cref="PlayerHair"/> to check.</param>
        /// <returns><c>true</c> if the hair belongs to a player entity; otherwise, <c>false</c>.</returns>
        public static bool isPlayerHair(PlayerHair hair)
            => hair.Entity is Player;
        
        /// <summary>
        /// Determines whether the specified player hair is Badeline's hair.
        /// </summary>
        /// <param name="hair">The player hair to check.</param>
        /// <returns><c>true</c> if the player hair is Badeline's hair; otherwise, <c>false</c>.</returns>
        public static bool isBadelineHair(PlayerHair hair)
            => hair.Entity is BadelineOldsite or BadelineDummy;


        
        /// <summary>
        /// Determines if the specified PlayerHair is a ghost hair.
        /// </summary>
        /// <param name="hair">The PlayerHair to check.</param>
        /// <returns>True if the PlayerHair is a ghost hair; otherwise, false.</returns>
        public static bool isGhostHair(PlayerHair hair) {
            if(isCnetInstalled())
            {
                return _isGhostHair(hair);
            }
            return false;
        }
        /// <summary>
        /// Determines if the PlayerHair object belongs to a ghost entity
        /// Should not be inlined to avoid crash if CelesteNet binary is not loaded
        /// </summary>
        /// <param name="hair">The PlayerHair object</param>
        /// <returns>True if the PlayerHair object belongs to a ghost entity</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool _isGhostHair(PlayerHair hair)
            => hair.Entity is Ghost;
            
        /// <summary>
        /// Checks if CelesteNet.Client is installed.
        /// </summary>
        /// <returns>True if CelesteNet.Client is installed; otherwise, false.</returns>
        public static bool isCnetInstalled()
            => Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "CelesteNet.Client", Version = new Version(2, 3, 1) });

        /// <summary>
        /// Determines if the hair should be changed based on the settings
        /// </summary>
        /// <param name="self">The PlayerHair object</param>
        /// <returns>True if the hair should be changed</returns>
        public static bool shouldChangeHair(PlayerHair self)
            => (isPlayerHair(self) && FoxelineModule.Settings.EnableBangs) || (isBadelineHair(self) && FoxelineModule.Settings.BadelineTail.EnableBangs);
    }
}
