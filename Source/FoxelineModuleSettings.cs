using System.Globalization;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Monocle;
using YamlDotNet.Serialization;

namespace Celeste.Mod.Foxeline;

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

    public Color TailBrushColor { get; set; } = Color.White;

    [YamlIgnore]
    public string TailBrushColorString => $"{TailBrushColor.R:X2}{TailBrushColor.G:X2}{TailBrushColor.B:X2}";

    [SettingRange(25, 1000, true)]
    public int TailScale { get; set; } = 100;

    [SettingRange(1, TailCollection.MaxTailCount)]
    public int TailCount { get; set; } = 1;

    [SettingSubText("Draw the outline of each tail separately instead of as a whole")]
    public bool SeparateTails { get; set; } = true;
    [SettingRange(0, 100, true)]
    public int TailSpread { get; set; } = 40;

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

    //menu item creation methods

    private bool WasTailBrushColorInvalid;
    private TextMenu.Button TailBrushColorEntry = default!;
    public void CreateTailBrushColorEntry(TextMenu menu, bool inGame)
    {
        menu.Add(TailBrushColorEntry
            = new TextMenu.Button($"{nameof(TailBrushColor).SpacedPascalCase()}: #{TailBrushColorString}"));

        TailBrushColorEntry.Disabled = inGame;

        //can't use text inputs in-game; just exit
        if (inGame)
            return;

        if (WasTailBrushColorInvalid)
        {
            TailBrushColorEntry.AddDescription(menu, "The color you entered is invalid. Try again.");

            //fix the subheader not showing properly when exiting the text input
            TailBrushColorEntry.OnEnter();
        }

        //reset invalid flags on exit
        menu.OnCancel += () =>
        {
            BadelineTail.WasTailBrushColorInvalid = false;
            CelestenetDefaults.WasTailBrushColorInvalid = false;
            WasTailBrushColorInvalid = false;
        };

        TailBrushColorEntry.Pressed(() =>
        {
            string buffer = TailBrushColorString;
            Audio.Play(SFX.ui_main_savefile_rename_start);
            menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
                buffer,
                newValue => buffer = newValue,
                isConfirm =>
                {
                    if (!isConfirm)
                        return;

                    WasTailBrushColorInvalid = !int.TryParse(buffer, NumberStyles.HexNumber, null, out _);
                    if (!WasTailBrushColorInvalid)
                        TailBrushColor = Calc.HexToColor(buffer);
                },
                minValueLength: 6, maxValueLength: 6
            );
        });
    }

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

    //submenus

    [SettingSubMenu]
    public class TailDefaults
    {
        public TailVariant Tail { get; set; } = TailVariant.None;

        [SettingRange(0, 100, true)]
        public int TailBrushTint { get; set; } = 15;

        public Color TailBrushColor { get; set; } = Color.White;

        [YamlIgnore]
        public string TailBrushColorString => $"{TailBrushColor.R:X2}{TailBrushColor.G:X2}{TailBrushColor.B:X2}";

        [SettingRange(25, 1000, true)]
        public int TailScale { get; set; } = 100;

        [SettingRange(1, TailCollection.MaxTailCount)]
        public int TailCount { get; set; } = 1;

        [SettingSubText("Draw the outline of each tail separately instead of as a whole")]
        public bool SeparateTails { get; set; } = true;

        [SettingRange(0, 100, true)]
        public int TailSpread { get; set; } = 40;

        public bool FeatherTail { get; set; } = true;

        public bool PaintBrushTail { get; set; } = false;

        //submenu item creation methods

        internal bool WasTailBrushColorInvalid;
        private TextMenu.Button TailBrushColorEntry = default!;
        public void CreateTailBrushColorEntry(TextMenuExt.SubMenu menu, bool inGame)
        {
            menu.Add(TailBrushColorEntry
                = new TextMenu.Button($"{nameof(TailBrushColor).SpacedPascalCase()}: #{TailBrushColorString}"));

            TailBrushColorEntry.Disabled = inGame;

            //can't use text inputs in-game; just exit
            if (inGame)
                return;

            //grrr this NREs
            //if (WasTailBrushColorInvalid)
            //    TailBrushColorEntry.AddDescription(menu, "The color you entered is invalid. Try again.");

            TailBrushColorEntry.Pressed(() =>
            {
                string buffer = TailBrushColorString;
                Audio.Play(SFX.ui_main_savefile_rename_start);
                menu.Container.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
                    buffer,
                    newValue => buffer = newValue,
                    isConfirm =>
                    {
                        if (!isConfirm)
                            return;

                        WasTailBrushColorInvalid = !int.TryParse(buffer, NumberStyles.HexNumber, null, out _);
                        if (!WasTailBrushColorInvalid)
                            TailBrushColor = Calc.HexToColor(buffer);
                    },
                    minValueLength: 6, maxValueLength: 6
                );
            });
        }
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

        [SettingRange(25, 1000, true)]
        public int ClampCelesteNetTailSize { get; set; } = 175;
    }
}
