using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class Level : Resource
{
    [ExportGroup("Level Meta")]
    [Export]
    public int LevelId { get; set; }

    [Export]
    public string LevelName { get; set; }

    [Export]
    public string LevelType { get; set; } = "20s";

    [ExportGroup("Level Variations")]
    [Export]
    public Array<Texture2D> Classrooms { get; set; } = [];
}
