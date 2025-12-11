using Godot;
using Godot.Collections;
using System;



[GlobalClass]
public partial class Emotions : Resource
{
    public enum EmotionType
    {
        Thinking = 0,
        Sad,
        Confidence,
        LaughCry,
        IHaveAnIdea,
    }

    [Export]
    public Dictionary<EmotionType, Texture2D> EmotionTextures { get; set; }
}
