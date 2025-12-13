using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class Questions : Resource
{
    [Export]
    public Array<QuestionData> Data { get; set; }
    [Export]
    public Array<AnswerTextureBundle> CommonWrongAnswers { get; set; }
    [Export]
    public Array<Texture2D> Strikes { get; set; }
}
