using Godot;
using System;


[GlobalClass]
public partial class AnswerTextureBundle : Resource
{
    [Export]
    public Texture2D AnswerTexture { get; set; }
    [Export]
    public Texture2D OqaTexture { get; set; }
    [Export]
    public Vector2 AnswerScale { get; set; } = Vector2.One;
    [Export]
    public Vector2 OqaScale { get; set; } = Vector2.One;

    public bool IsValid()
    {
        return AnswerTexture != null 
            && OqaTexture != null 
            /*&& HesitateTexture != null*/ 
            && OqaScale != Vector2.Zero;
    }
}
