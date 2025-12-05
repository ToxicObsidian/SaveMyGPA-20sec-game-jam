using Godot;
using System;
using System.Collections.Generic;


public partial class VisualizeQuestion : Control
{
	[Export]
	public QuestionData Question { get; set; }
	[Export]
	public QuestionController Controller { get; set; }

	[Export]
	public Vector2 QuestionOQAPosition
	{
		get { return Question == null ? Vector2.Zero : Question.OnQuestionAnswerPosition; }
		set { if (Question != null) Question.OnQuestionAnswerPosition = value; }
	}
	[Export]
	public float QuestionOQAScale
    {
        get { return Question == null ? 1.0f : Question.OnQuestionAnswerScale; }
        set { if (Question != null) Question.OnQuestionAnswerScale = value; }
    }

    protected bool _is_set_question = false;


    public override void _Ready()
    {
        Controller.SetQuestion(Question.QuestionTexture, Question.OnQuestionAnswerPosition);
        List<Texture2D> answers = new();
        answers.Add(Question.CorrectAnswer.AnswerTexture);
        foreach (var wrong_answer in Question.WrongAnswerOptions)
        {
            answers.Add(wrong_answer.AnswerTexture);
        }
        Controller.SetAnswers(answers);
        Controller.SetOQA(Question.CorrectAnswer.OqaTexture, Question.OnQuestionAnswerScale * Vector2.One);
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        Controller.OnQuestionAnswerRect.Position = QuestionOQAPosition;
        Controller.OnQuestionAnswerRect.Scale = Vector2.One * QuestionOQAScale;
	}
}
