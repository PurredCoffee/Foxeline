using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Celeste.Mod.CelesteNet.Client.Entities;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.Foxeline;

public static class FoxelineHelpers
{
    #region Settings helpers

    /// <summary>
    /// Gets the tail variant for the player based on the settings
    /// </summary>
    /// <param name="self">PlayerHair object</param>
    /// <returns>TailVariant enum corresponding to the tail</returns>
    public static TailVariant getTailVariant(PlayerHair self)
    {
        if (isPlayerHair(self))
            return FoxelineModule.Settings.Tail;
        if (isBadelineHair(self))
            return FoxelineModule.Settings.BadelineTail.Tail;
        if (CelesteNetHelper.TryGetTailInformation(self, out var tail))
            return tail.Tail;

        return FoxelineModule.Settings.CelestenetDefaults.Tail;
    }

    /// <summary>
    /// Gets the tail scale for the player based on the settings
    /// </summary>
    /// <param name="self">PlayerHair object</param>
    /// <returns>scaling factor of the tail</returns>
    public static float getTailScale(PlayerHair self)
    {
        if (isPlayerHair(self))
            return FoxelineModule.Settings.TailScale / 100f;
        if (isBadelineHair(self))
            return FoxelineModule.Settings.BadelineTail.TailScale / 100f;
        if (CelesteNetHelper.TryGetTailInformation(self, out var tail))
            return tail.TailScale / 100f;

        return FoxelineModule.Settings.CelestenetDefaults.TailScale / 100f;
    }

    /// <summary>
    /// Gets the tail count for the player based on the settings
    /// </summary>
    /// <param name="self">PlayerHair object</param>
    /// <returns>Number of tails on the player</returns>
    public static int getTailCount(PlayerHair self)
    {
        if (isPlayerHair(self))
            return FoxelineModule.Settings.TailCount;
        if (isBadelineHair(self))
            return FoxelineModule.Settings.BadelineTail.TailCount;
        if (CelesteNetHelper.TryGetTailInformation(self, out var tail))
            return tail.TailCount;

        return FoxelineModule.Settings.CelestenetDefaults.TailCount;
    }

    /// <summary>
    /// Gets the tail offset for the player based on the settings
    /// </summary>
    /// <param name="self">PlayerHair object</param>
    /// <returns>Whether tails should be drawn collectively or with separate outlines</returns>
    public static bool getSeparateTails(PlayerHair self)
    {
        if (isPlayerHair(self))
            return FoxelineModule.Settings.SeparateTails;
        if (isBadelineHair(self))
            return FoxelineModule.Settings.BadelineTail.SeparateTails;
        if (CelesteNetHelper.TryGetTailInformation(self, out var tail))
            return tail.SeparateTails;

        return FoxelineModule.Settings.CelestenetDefaults.SeparateTails;
    }

    /// <summary>
    /// Gets the tail spread for the player based on the settings
    /// </summary>
    /// <param name="self">PlayerHair object</param>
    /// <returns>Spread factor of the tail</returns>
    public static float getTailSpread(PlayerHair self)
    {
        if (isPlayerHair(self))
            return FoxelineModule.Settings.TailSpread / 100f;
        if (isBadelineHair(self))
            return FoxelineModule.Settings.BadelineTail.TailSpread / 100f;
        if (CelesteNetHelper.TryGetTailInformation(self, out var tail))
            return tail.TailSpread / 100f;

        return FoxelineModule.Settings.CelestenetDefaults.TailSpread / 100f;
    }

    /// <summary>
    /// Flag to determine if the tail should be painted as a brush or not
    /// </summary>
    /// <param name="self">PlayerHair object</param>
    /// <returns>True if tail should be painted as a brush</returns>
    public static bool getPaintBrushTail(PlayerHair self)
    {
        if (isPlayerHair(self))
            return FoxelineModule.Settings.PaintBrushTail;
        if (isBadelineHair(self))
            return FoxelineModule.Settings.BadelineTail.PaintBrushTail;
        if (CelesteNetHelper.TryGetTailInformation(self, out var tail))
            return tail.PaintBrushTail;

        return FoxelineModule.Settings.CelestenetDefaults.PaintBrushTail;
    }

    /// <summary>
    /// Gets the tail brush tint for the player based on the settings
    /// </summary>
    /// <param name="self">PlayerHair object</param>
    /// <returns>Color multiplier for the tip of the tail</returns>
    public static float getTailBrushTint(PlayerHair self)
    {
        if (isPlayerHair(self))
            return FoxelineModule.Settings.TailBrushTint / 100f;
        if (isBadelineHair(self))
            return FoxelineModule.Settings.BadelineTail.TailBrushTint / 100f;
        if (CelesteNetHelper.TryGetTailInformation(self, out var tail))
            return tail.TailBrushTint / 100f;

        return FoxelineModule.Settings.CelestenetDefaults.TailBrushTint / 100f;
    }

    /// <summary>
    /// Gets the tail brush color for the player based on the settings
    /// </summary>
    /// <param name="self">PlayerHair object</param>
    /// <returns>Color for the tip of the tail</returns>
    public static Color getTailBrushColor(PlayerHair self)
    {
        if (isPlayerHair(self))
            return FoxelineModule.Settings.TailBrushColor;
        if (isBadelineHair(self))
            return FoxelineModule.Settings.BadelineTail.TailBrushColor;
        if (CelesteNetHelper.TryGetTailInformation(self, out var tail))
            return tail.TailBrushColor;

        return FoxelineModule.Settings.CelestenetDefaults.TailBrushColor;
    }

    /// <summary>
    /// Gets the feather tail flag for the player based on the settings
    /// </summary>
    /// <param name="self">PlayerHair object</param>
    /// <returns>True if the tail should be drawn while player is in feather</returns>
    public static bool getFeatherTail(PlayerHair self)
    {
        if (isPlayerHair(self))
            return FoxelineModule.Settings.FeatherTail;
        if (isBadelineHair(self))
            return FoxelineModule.Settings.BadelineTail.FeatherTail;
        if (CelesteNetHelper.TryGetTailInformation(self, out var tail))
            return tail.FeatherTail;

        return FoxelineModule.Settings.CelestenetDefaults.FeatherTail;
    }

    /// <summary>
    /// Whether the tail scale is greater than 1 and the big tail node textures should be used.
    /// </summary>
    /// <param name="self">The PlayerHair object to draw the tail next to</param>
    public static bool isBigTail(PlayerHair self)
        => getTailScale(self) > 1;

    /// <summary>
    /// Determines if the hair should be changed based on the settings
    /// </summary>
    /// <param name="self">The PlayerHair object</param>
    /// <returns>True if the hair should be changed</returns>
    public static bool shouldChangeHair(PlayerHair self)
        => (isPlayerHair(self) && FoxelineModule.Settings.EnableBangs)
            || (isBadelineHair(self) && FoxelineModule.Settings.BadelineTail.EnableBangs);

    #endregion

    #region Animation helpers

    /// <summary>
    /// Determines whether the player hair is crouched.
    /// </summary>
    /// <param name="hair">The player hair.</param>
    /// <returns><c>true</c> if the player hair is crouched; otherwise, <c>false</c>.</returns>
    public static bool isCrouched(PlayerHair hair)
        => hair is { Sprite.LastAnimationID: "duck" or "slide" or "hug" };

    /// <summary>
    /// Determines whether the player's hair should droop the tail.
    /// </summary>
    /// <param name="hair">The player's hair.</param>
    /// <returns><c>true</c> if the hair should droop the tail; otherwise, <c>false</c>.</returns>
    public static bool shouldDroopTail(PlayerHair hair)
        => hair is { Sprite.LastAnimationID: "launch" or "spin" }
            || (hair.Entity is Player && hair.Sprite.EntityAs<Player>().Stamina <= Player.ClimbTiredThreshold);

    /// <summary>
    /// Determines whether the tail should be flipped based on the current animation ID of the player's hair.
    /// </summary>
    /// <param name="hair">The player's hair.</param>
    /// <returns><c>true</c> if the tail should be flipped; otherwise, <c>false</c>.</returns>
    public static bool shouldFlipTail(PlayerHair hair)
        => hair is {
            Sprite.LastAnimationID: "asleep" or "bagDown" or "edgeBack" or "halfWakeUp" or "sitDown" or "sleep"
            or "wakeUp"
        };

    /// <summary>
    /// Determines whether the tail should be laying on the ground based on the current animation ID of the player's hair.
    /// </summary>
    /// <param name="hair">The player's hair.</param>
    /// <returns><c>true</c> if the tail should be laying on the ground; otherwise, <c>false</c>.</returns>
    public static bool shouldRestTail(PlayerHair hair)
        => hair is {
            Sprite.LastAnimationID: "asleep" or "bagDown" or "carryTheoCollapse" or "downed" or "halfWakeUp"
            or "roll" or "rollGetUp" or "sitDown" or "sleep" or "wakeUp"
        };

    /// <summary>
    /// Determines whether the tail of the player's hair should be stretched.
    /// </summary>
    /// <param name="hair">The player's hair.</param>
    /// <returns><c>true</c> if the tail should be stretched; otherwise, <c>false</c>.</returns>
    public static bool shouldStretchTail(PlayerHair hair)
        => hair is {
                Sprite.LastAnimationID: "dangling" or "edge" or "edgeBack" or "idleC" or "runWind" or "shaking"
                or "tired" or "tiredStill"
            }
            || isCrouched(hair);

    #endregion

    #region General helpers

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
    public static bool isGhostHair(PlayerHair hair)
        => isCnetInstalled() && _isGhostHair(hair);

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
        => Everest.Loader.DependencyLoaded(new EverestModuleMetadata {
            Name = "CelesteNet.Client", Version = new Version(2, 3, 1)
        });

    #endregion

    /// <summary>
    /// Computes the smarter hair gradient.
    /// </summary>
    /// <param name="self">
    /// The <see cref="PlayerHair"/> object.
    /// </param>
    /// <param name="selfData">
    /// The <see cref="DynamicData"/> object for the <see cref="PlayerHair"/> object.
    /// </param>
    /// <returns>
    /// The smarter hair color gradient.
    /// </returns>
    public static Color[] getHairGradient(PlayerHair self, DynamicData selfData)
    {
        Color[] hairGradient = new Color[self.Sprite.HairCount];
        for (int i = 0; i < hairGradient.Length; i++)
            hairGradient[i] = getHairColor(i, self, selfData);

        return hairGradient;
    }

    /// <summary>
    /// Gets a smarter hair color based on the player's current animation.
    /// </summary>
    /// <param name="hairNodeIndex">
    /// The index of the hair color
    /// </param>
    /// <param name="self">
    /// The <see cref="PlayerHair"/> object.
    /// </param>
    /// <param name="selfData">
    /// The <see cref="DynamicData"/> object for the <see cref="PlayerHair"/> object.
    /// </param>
    /// <returns>
    /// The smarter hair color.
    /// </returns>
    public static Color getHairColor(int hairNodeIndex, PlayerHair self, DynamicData selfData)
    {
        //only do this if:
        //- we're fixing cutscenes
        //- the entity is a Player
        if (!FoxelineModule.Settings.FixCutscenes || self.Entity is not Player player)
            return self.GetHairColor(hairNodeIndex);

        Dictionary<string, int> cutsceneToDashLookup = self.Sprite.EntityAs<Player>().Inventory.Backpack
            ? FoxelineConst.backpackCutscenes
            : FoxelineConst.noBackpackCutscenes;

        //if there's no animation id in the lookup, just do the default
        if (!cutsceneToDashLookup.TryGetValue(self.Sprite.LastAnimationID, out var dashes))
            return self.GetHairColor(hairNodeIndex);

        //SMH PLUS
        if (selfData.TryGet(FoxelineConst.smh_hairConfig, out var hairConfig))
        {
            //anything can be null here - use null conditional operator to avoid null reference exceptions
            if (hairConfig?.GetType().GetField("ActualHairColors")?.GetValue(hairConfig)
                is Dictionary<int, List<Color>> hairColors)
            {
                const int DefaultHairColorIndex = 100;
                //we found something that looks like a hairConfig from SMH PLUS
                if (!hairColors.ContainsKey(hairNodeIndex))
                    //fallback to default hair color if the hairConfig is broken
                    hairNodeIndex = DefaultHairColorIndex;

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
        return dashes switch {
            0 => Player.UsedHairColor,
            1 => Player.NormalHairColor,
            2 => Player.TwoDashesHairColor,
            _ => self.GetHairColor(hairNodeIndex)
        };
    }
}
