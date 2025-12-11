using Godot;
using Godot.Collections;
using System;



[GlobalClass]
public partial class ClassmateData : Resource
{
    [Export]
    public Array<Texture2D> BoyPostures { get; set; }
    [Export]
    public Array<Texture2D> GirlPostures { get; set; }
    [Export]
    public Array<Texture2D> BoyHairs { get; set; }
    [Export]
    public Array<Texture2D> GirlHairs { get; set; }
    [Export]
    public Array<Texture2D> BoyOutfits { get; set; }
    [Export]
    public Array<Texture2D> GirlOutfits { get; set; }
    [Export]
    public Array<ClassmateCompleteOutfitData> CompleteOutfits { get; set; }
    [Export]
    public int AllowCompleteOutfitsDuplicate { get; set; } = 1;


    [Export]
    public Emotions ClassmateEmotions { get; set; }

    public override void _ValidateProperty(Dictionary property)
    {
        _ClearArray(BoyPostures);
        _ClearArray(GirlPostures);
        _ClearArray(BoyHairs);
        _ClearArray(GirlHairs);
        _ClearArray(BoyOutfits);
        _ClearArray(GirlOutfits);
        _ClearArray(CompleteOutfits);

        base._ValidateProperty(property);
    }

    protected void _ClearArray<[MustBeVariant] T>(Array<T> array)
    {
        for (int i = array.Count - 1; i >= 0; i--) if (array[i] == null) array.RemoveAt(i);
    }
}
