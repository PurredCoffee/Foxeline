using System.Runtime.CompilerServices;
using Celeste.Mod.CelesteNet.Client.Entities;

namespace Celeste.Mod.Foxeline;

public static class CelesteNetHelper
{
    public static bool TryGetTailInformation(PlayerHair hair, out FoxelineModuleSettings.TailDefaults tail)
    {
        if (FoxelineHelpers.isCnetInstalled() && _TryGetTailInformation(hair, out tail))
            return true;

        tail = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool _TryGetTailInformation(PlayerHair hair, out FoxelineModuleSettings.TailDefaults tail)
    {
        if (hair.Entity is Ghost ghost && CelesteNet.CelesteNetTailSettingsExchangeComponent.TailInformation.TryGetValue(
            ghost.PlayerInfo.ID, out FoxelineModuleSettings.TailDefaults tailInfo))
        {
            tail = tailInfo;
            return true;
        }

        tail = null;
        return false;
    }
}
