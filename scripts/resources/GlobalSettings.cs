using Godot;
using Godot.Collections;
using System;
using ZLinq;


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
    public Dictionary<int, float> ConfusedMap
    {
        get { return _confused_map; }
        set { _confused_map = value; }
    }
    [Export]
    public Dictionary<int, float> HesitateMap
    {
        get { return _hesitate_map; }
        set { _hesitate_map = value; }
    }
    [Export]
    public Dictionary<int, string> DifficultyDesc 
    {
        get { return _difficulty_desc; }
        set { _difficulty_desc = value; }
    }
    [Export]
    public int MultiChoiceWeight
    {
        get { return _mc_weight; }
        set { _mc_weight = value; }
    }
    [Export]
    public int TrueFalseWeight
    {
        get { return _tf_weight; }
        set { _tf_weight = value; }
    }
    [Export]
    public int FillInBlankWeight
    {
        get { return _fb_weight; }
        set { _fb_weight = value; }
    }

    [ExportSubgroup("Display")]
    [Export]
    public bool Windowed
    {
        get { return _windowed; }
        set { _windowed = value; }
    }


    public int MaxDifficulty
    {
        get
        {
            return _difficulty_map.AsValueEnumerable()
                .Select(kvp => kvp.Key)
                .Order()
                .Reverse()
                .ToList()[0];
        }
    }
    public int MinDifficulty
    {
        get
        {
            return _difficulty_map.AsValueEnumerable()
                .Select(kvp => kvp.Key)
                .Order()
                .ToList()[0];
        }
    }

    // Overall settings
    protected float _level_duration = 20.0f;
    protected int _difficulty = 0;
    protected Dictionary<int, float> _difficulty_map = new();
    protected Dictionary<int, float> _confused_map = new();
    protected Dictionary<int, float> _hesitate_map = new();
    protected Dictionary<int, string> _difficulty_desc = new();
    protected int _mc_weight = 5;
    protected int _tf_weight = 5;
    protected int _fb_weight = 5;

    // Video settings
    protected bool _windowed = false;


    public bool IsValid()
    {
        if (LevelDuration <= 0) return false;

        if (Difficulty < 0) return false;
        
        if (!_DictionaryKeysEqual(DifficultyMap, ConfusedMap) || 
            !_DictionaryKeysEqual(DifficultyMap, HesitateMap) ||
            !_DictionaryKeysEqual(DifficultyMap, DifficultyDesc)) return false;
        if (!DifficultyMap.Keys.Contains(Difficulty)) return false;

        if (MultiChoiceWeight < 0 ||
            TrueFalseWeight < 0 ||
            FillInBlankWeight < 0) return false;

        return true;
    }

    protected bool 
    _DictionaryKeysEqual<[MustBeVariant] T1, [MustBeVariant] T2>(
        Dictionary<int, T1> d1, 
        Dictionary<int, T2> d2
    )
    {
        if (d1.Count != d2.Count)
            return false;

        foreach (var key in d1.Keys)
        {
            if (!d2.ContainsKey(key))
                return false;
        }

        return true;
    }
}
