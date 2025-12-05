using Godot;
using Godot.Collections;
using System;



[GlobalClass]
public partial class ClassmateData : Resource
{
    [Export]
    public Array<Texture2D> BoyPostures;
    [Export]
    public Array<Texture2D> GirlPostures;
    [Export]
    public Array<Texture2D> BoyHairs;
    [Export]
    public Array<Texture2D> GirlHairs;
    [Export]
    public Array<Texture2D> BoyOutfits;
    [Export]
    public Array<Texture2D> GirlOutfits;


    public override void _ValidateProperty(Dictionary property)
    {
        _ClearArray(BoyPostures);
        _ClearArray(GirlPostures);
        _ClearArray(BoyHairs);
        _ClearArray(GirlHairs);
        _ClearArray(BoyOutfits);
        _ClearArray(GirlOutfits);

        base._ValidateProperty(property);
    }

    protected void _ClearArray(Array<Texture2D> array)
    {
        for (int i = array.Count - 1; i >= 0; i--) if (array[i] == null) array.RemoveAt(i);
    }
}
