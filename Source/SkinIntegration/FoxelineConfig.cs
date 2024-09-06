namespace Celeste.Mod.Foxeline.SkinIntegration;

public class FoxelineConfig
{
    public string SkinName = "";
    public string Tail = "None"; //Yaml can't deserialize enums so we get a string and parse it to the enum.
    public TailVariant TailEnum { get; set; } = TailVariant.None;
    public int TailBrushTint { get; set; } = 15;
    public int TailScale { get; set; } = 100;
    public bool FeatherTail { get; set; } = true;
    public bool PaintBrushTail { get; set; } = false;
}


/* YAML File Example
 
 - SkinName: katz404_ParrotDash
  Tail: Furry
  TailbrushTint: 0
  TailScale: 150
  FeatherTail: true
  PaintBrushTail: false
 
  - SkinName: katz404_ParrotDash_Gradient
  Tail: Furry
  TailbrushTint: 0
  TailScale: 150
  FeatherTail: true
  PaintBrushTail: false
 

 
 */