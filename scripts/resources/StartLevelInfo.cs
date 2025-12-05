using Godot;
using Godot.Collections;
using System;

// This struct will be generated and populated by GameManager, no need for setting this to GlobalClass
public partial class StartLevelInfo : Resource
{
    [Export]
    public float Duration { get; set; } = 20.0f;

    [Export]
    public SupportedClassroomType Type { get; set; } = SupportedClassroomType.Classic;

    [Export]
    public Dictionary<string, string> ExcludedTags { get; set; } = new();
}
