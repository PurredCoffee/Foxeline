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

        [SettingRange(25, 175, true)]
        public int TailScale { get; set; } = 100;
        public bool FeatherTail { get; set; } = true;
        public bool PaintBrushTail { get; set; } = false;

        [SettingSubHeader("Bangs")]
        public bool EnableBangs { get; set; } = true;

        [SettingSubHeader("Extra")]

        [SettingSubText("(VANILLA/SMH PLUS ONLY)")]
        public bool FixCutscenes { get; set; } = true;

        [SettingSubText("VERY EXPERIMENTAL, Might look weird")]
        public bool CollectHair { get; set; } = false;

        [SettingSubText("VERY EXPERIMENTAL, USE AT YOUR OWN RISK")]
        public Constants FoxelineConstants { get; set; } = new Constants();

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
            [SettingRange(0, 8, false)]
            public int CollectHairLength { get; set; } = 2;


        }
    }
}
