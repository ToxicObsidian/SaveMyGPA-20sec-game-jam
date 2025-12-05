using Godot;
using Godot.Collections;
using System;


/// <summary>
/// This class is the global settings of the game, and it will be created once in the editor.
/// All game resources (to create a level) can be retrieved from here.
/// </summary>
[GlobalClass]
public partial class GlobalSettings : Resource
{
    [Export]
    public float LevelDuration
    {
        get { return _level_duration; }
        set { _level_duration = value; }
    }


    [Export]
    public int Difficulty
    {
        get { return _difficulty; }
        set { _difficulty = value; }
    }
    [Export]
    public Dictionary<int, float> DifficultyMap
    {
        get { return _difficulty_map; }
        set { _difficulty_map = value; }
    }

    [Export]
    public bool Windowed
    {
        get { return _windowed; }
        set { _windowed = value; }
    }


    // Overall settings
    protected float _level_duration = 20.0f;
    protected int _difficulty = 0;
    protected Dictionary<int, float> _difficulty_map = new();

    // Video settings
    protected bool _windowed = false;

    // Audio settings
    protected float _master_volume = 1.0f;
    protected float _sfx_volume = 1.0f;
    protected float _bgm_volume = 1.0f;
}
