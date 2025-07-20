using System;
using MonoMod.ModInterop;

namespace Celeste.Mod.Foxeline;

public class GravityHelperInterop
{
    [ModImportName("GravityHelper.IsPlayerInverted")]
    public static Func<bool> _IsPlayerInverted;

    /// <summary>
    /// Checks if the <see cref="Player"/> is upside-down due to GravityHelper.
    /// If GravityHelper is not present, returns <c>false</c>.
    /// </summary>
    public static bool IsPlayerInverted() => _IsPlayerInverted is not null && _IsPlayerInverted();

    [ModImportName("GravityHelper.IsActorInverted")]
    public static Func<Actor, bool> _IsActorInverted;

    /// <summary>
    /// Checks if an <see cref="Actor"/> is upside-down due to GravityHelper.
    /// If GravityHelper is not present, returns <c>false</c>.
    /// </summary>
    public static bool IsActorInverted(Actor actor) => _IsActorInverted is not null && _IsActorInverted(actor);
}
