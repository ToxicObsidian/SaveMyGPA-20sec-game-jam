using Godot;
using System;
using System.Collections.Generic;

public class StartLevelInfo
{
    public float Duration { get; set; } = 20.0f;
    public SupportedClassroomType Type { get; set; } = SupportedClassroomType.Classic;
    public List<string> ExcludedTags { get; set; } = new();

    public int Difficulty { get; set; }
    public float WrongRatio { get; set; }
    public float ConfusedRatio { get; set; }
    public float HesitateRatio { get; set; }
    public int MaxDifficulty { get; set; }
}
