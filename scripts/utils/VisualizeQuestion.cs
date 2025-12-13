using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;


public partial class VisualizeQuestion : Control
{
	[Export]
	public QuestionData Question { get; set; }
	[Export]
	public QuestionController Controller { get; set; }
    [Export]
    public Array<BubbleController> Bubbles { get; set; }

	[Export]
	public Vector2 QuestionOQAPosition
	{
		get { return Question == null ? Vector2.Zero : Question.OnQuestionAnswerPosition; }
		set { if (Question != null) Question.OnQuestionAnswerPosition = value; }
	}
	[Export]
	public Vector2 QuestionOQAScale
    {
        get { return Question == null ? Vector2.One : Question.CorrectAnswer.OqaScale; }
        set { if (Question != null) Question.CorrectAnswer.OqaScale = value; }
    }

    protected bool _is_set_question = false;


    public override void _Ready()
    {
        List<Texture2D> answers = new();
        answers.Add(Question.CorrectAnswer.AnswerTexture);
        foreach (var wrong_answer in Question.WrongAnswerOptions)
        {
            answers.Add(wrong_answer.AnswerTexture);
        }
        Controller.Setup(Question.QuestionTexture, answers, Question.OnQuestionAnswerPosition, true);
        QuestionOQAPosition = Question.OnQuestionAnswerPosition;
        QuestionOQAScale = Question.CorrectAnswer.OqaScale;
        Controller.VerticalAnswerLayout = Question.VerticalLayout;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        Controller.SetOQA(Question.CorrectAnswer.OqaTexture, QuestionOQAScale);
        Controller.OnQuestionAnswerAnchor.Position = QuestionOQAPosition;
        foreach (var bubble in Bubbles)
        {
            bubble.Answer.Texture = Question.CorrectAnswer.AnswerTexture;
            bubble.Answer.Scale = Question.CorrectAnswer.AnswerScale;
        }
    }
}
