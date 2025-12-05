using Godot;
using Godot.Collections;
using System;


/// <summary>
/// The type of the questions.
/// </summary>
public enum QuestionType
{
    MultiChoice,
    TrueFalse,
    FillInBlank
}



[GlobalClass]
public partial class QuestionData : Resource
{
    [Export]
    /// <summary>
    /// Note: The type will determine how the question will be displayed.
    /// </summary>
    public QuestionType Type { get; set; }
    [Export]
    public int Score { get; set; } = 10;

    [ExportSubgroup("Question")]
    [Export]
    public Texture2D QuestionTexture { get; set; }

    [ExportSubgroup("Answers")]
    [Export]
    public Array<AnswerTextureBundle> WrongAnswerOptions { get; set; }
    [Export]
    public Array<Texture2D> HesitateAnswers { get; set; }
    [Export]
    public AnswerTextureBundle CorrectAnswer { get; set; }
    [Export]
    public Vector2 OnQuestionAnswerPosition { get; set; }
    [Export]
    public float OnQuestionAnswerScale { get; set; } = 1.0f;

    [ExportSubgroup("Meta")]
    [Export]
    public string Description { get; set; }
    [Export]
    public Array<string> Tags { get; set; }

    public bool IsValid()
    {
        if (QuestionTexture == null) return false;
        foreach (var wrong in WrongAnswerOptions) if (wrong == null || !wrong.IsValid()) return false;
        if (CorrectAnswer == null || !CorrectAnswer.IsValid()) return false;
        if (HesitateAnswers != null) foreach (var hesitate in HesitateAnswers) if (hesitate == null) return false;


        return true;
    }

}