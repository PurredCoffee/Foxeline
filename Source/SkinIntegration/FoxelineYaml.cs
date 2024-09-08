using System;
using System.Collections.Generic;

namespace Celeste.Mod.Foxeline.SkinIntegration;

public class FoxelineYaml
{
    public static Dictionary<string, FoxelineConfig> SkinTailConfigs = new(StringComparer.OrdinalIgnoreCase);

    public static void ReloadYaml()
    {
        foreach (ModContent mod in Everest.Content.Mods)
        {
            if (!mod.Map.TryGetValue("FoxelineConfig", out ModAsset asset)) {
                continue;
            }
            Logger.Info("Foxeline Yaml", $"Loading config from {mod.Name}");

            if (!LoadConfigFile<List<FoxelineConfig>>(asset, out var configs) || configs.Count < 1) {
                Logger.Warn("Foxeline Yaml", $"Failed deserializing config");
                continue;
            }

            foreach (FoxelineConfig config in configs) {
                if (!Enum.TryParse(config.Tail, out TailVariant TailString)) {
                    Logger.Warn("Foxeline Yaml", $"Incorrect tail type \"{config.Tail}\"");
                    continue;
                }
                config.TailEnum = TailString;
                SkinTailConfigs[config.SkinName] = config;
            }
        }
    }

    public static bool LoadConfigFile<T>(ModAsset skinConfigYaml, out T t) {
        return skinConfigYaml.TryDeserialize(out t);
    }
}

