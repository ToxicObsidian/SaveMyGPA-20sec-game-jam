using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class ClassmateCompleteOutfitData : Resource
{
    [Export]
    public string Name { get; set; }
    [Export]
    public Texture2D Body { get; set; }
    [Export]
    public Texture2D Hair { get; set; }
    [Export]
    public Texture2D Outfit { get; set; }

    /// <summary>
    /// These emotions will only be displayed in normal bubbles.
    /// </summary>
    [Export]
    public Array<Texture2D> SpecialEmotions { get; set; }
}
