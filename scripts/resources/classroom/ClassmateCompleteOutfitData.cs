using Godot;
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
}
