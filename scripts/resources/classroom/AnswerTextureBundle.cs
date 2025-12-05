using Godot;
using System;


[GlobalClass]
public partial class AnswerTextureBundle : Resource
{
    [Export]
    public Texture2D AnswerTexture { get; set; }
    [Export]
    public Texture2D OqaTexture { get; set; }

    public bool IsValid()
    {
        return AnswerTexture != null && OqaTexture != null;
    }
}
