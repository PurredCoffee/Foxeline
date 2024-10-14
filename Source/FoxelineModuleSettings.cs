namespace Celeste.Mod.Foxeline
{
    public enum TailVariant
    {
        None = 0,
        Furry = 1,
        Flat = 2,
        Unlit = 3
    }

    public class FoxelineModuleSettings : EverestModuleSettings
    {
        public TailVariant Tail { get; set; } = TailVariant.Furry;

        [SettingRange(0, 100, true)]
        public int TailBrushTint { get; set; } = 15;

        [SettingRange(25, 1000, true)]
        public int TailScale { get; set; } = 100;

        public bool FeatherTail { get; set; } = true;

        public bool PaintBrushTail { get; set; } = false;

        [SettingSubHeader("Bangs")]
        public bool EnableBangs { get; set; } = true;

        [SettingSubHeader("Extra")]

        [SettingSubText("(VANILLA/SMH PLUS ONLY)")]
        public bool FixCutscenes
        {
            get => _FixCutscenes;
            set
            {
                if (_FixCutscenes == value)
                    return;

                _FixCutscenes = value;

                //disable the Use Vanilla Hair Color setting if Fix Cutscenes is off
                if (UseVanillaHairColorEntry is not null)
                    UseVanillaHairColorEntry.Disabled = !_FixCutscenes;
            }
        }
        private bool _FixCutscenes = true;

        public bool UseVanillaHairColor { get; set; } = true;

        [SettingSubText("Badeline's tail configuration")]
        public BadelineTailDefaults BadelineTail { get; set; } = new BadelineTailDefaults();

        [SettingSubText("Configuration for CelesteNet Ghosts that do not have Foxeline installed")]
        public TailDefaults CelestenetDefaults { get; set; } = new TailDefaults();

        [SettingSubText("MIGHT LOOK WEIRD, not Synced with other players")]
        public Constants FoxelineConstants { get; set; } = new Constants();

        private TextMenu.OnOff UseVanillaHairColorEntry = default!;
        public void CreateUseVanillaHairColorEntry(TextMenu menu, bool inGame)
        {
            menu.Add(UseVanillaHairColorEntry =
                    new TextMenu.OnOff(nameof(UseVanillaHairColor).SpacedPascalCase(), UseVanillaHairColor));

            //i don't know why these aren't generic
            //AddDescription also expects the item to be in the menu when calling
            UseVanillaHairColorEntry.AddDescription(menu,
                "Set to off if you want cutscene animations to use custom hair colors, like Hyperline's");

            UseVanillaHairColorEntry.Change(newValue => UseVanillaHairColor = newValue);

            UseVanillaHairColorEntry.Disabled = !FixCutscenes;
        }

        [SettingSubMenu]
        public class TailDefaults
        {
            public TailVariant Tail { get; set; } = TailVariant.None;

            [SettingRange(0, 100, true)]
            public int TailBrushTint { get; set; } = 15;

            //limited to 255 because it's cast to byte when sent in cnet
            [SettingRange(25, 255, true)]
            public int TailScale { get; set; } = 100;

            public bool FeatherTail { get; set; } = true;

            public bool PaintBrushTail { get; set; } = false;
        }

        [SettingSubMenu]
        public class BadelineTailDefaults : TailDefaults {
            public new TailVariant Tail { get; set; } = TailVariant.Flat;

            public bool EnableBangs { get; set; } = true;
        }

        [SettingSubMenu]
        public class Constants
        {
            [SettingRange(0, 100, true)]
            public int droopSwayAmplitude { get; set; } = 20;

            [SettingRange(0, 100, true)]
            public int droopSwayFrequency { get; set; } = 20;

            [SettingRange(1, 100, true)]
            public int droopSwaySpeed { get; set; } = 30;

            [SettingRange(0, 100, true)]
            public int Control { get; set; } = 60;

            [SettingRange(0, 99, true)]
            public int Speed { get; set; } = 90;

            [SettingRange(0, 100, true)]
            public int Softness { get; set; } = 30;
        }
    }
}
