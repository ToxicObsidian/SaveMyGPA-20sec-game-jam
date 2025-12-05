using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

[Tool]
public partial class QuestionController : VBoxContainer
{
    [ExportGroup("Prefab Setting")]
    [Export]
    public TextureRect QuestionRect { get; set; }
    [Export]
    public TextureRect OnQuestionAnswerRect { get; set; }
    [Export]
    protected HBoxContainer AnswerLayoutContainer { get; set; }
    [Export]
    protected Control QASpacing { get; set; }
    [Export]
    protected Control QAOffset { get; set; }
    
    
    [ExportGroup("Question Visualization")]
    [Export]
    public QuestionType Type { get; set; }

    [ExportGroup("Creative/Debug")]
    [Export]
    protected QuestionData VisualizedData { get; set; }

    [Export]
    public bool DisplayAnswers
    {
        get { return _display_answers; }
        set { _display_answers = value; _h_answers.Visible = value; _v_answers.Visible = value; }
    }
    [Export]
    public bool VerticalAnswerLayout
    {
        get { return _vertical_layout; }
        set { _SetVAL(value); }
    }
    [Export]
    public float Offset
    {
        get { return _qa_offset; }
        set { _qa_offset = value; _UpdateSpacing(); }
    }
    [Export]
    public float Spacing
    {
        get { return _qa_spacing; }
        set { _qa_spacing = value; _UpdateSpacing(); }
    }


    protected HBoxContainer _h_answers = new();
    protected VBoxContainer _v_answers = new();
    protected bool _display_answers;
    protected bool _vertical_layout;
    protected Vector2 _oqa_position;
    protected float _qa_offset = 0.0f;
    protected float _qa_spacing = 0.0f;

    public override void _Ready()
    {
        // Change to visible when SetOQA is called.
        OnQuestionAnswerRect.Visible = false;
        AddThemeConstantOverride("seperation", 0);
        AnswerLayoutContainer.AddThemeConstantOverride("seperation", 0);
        _h_answers.AddThemeConstantOverride("seperation", 0);
        _v_answers.AddThemeConstantOverride("seperation", 0);
        _UpdateSpacing();

        _h_answers.GetParent()?.RemoveChild(_h_answers);
        _v_answers.GetParent()?.RemoveChild(_v_answers);
        AnswerLayoutContainer.AddChild(_vertical_layout ? _v_answers : _h_answers);
    }

    public void SetQuestion(
        Texture2D question_texture,
        Vector2 oqa_position
    )
    {
        QuestionRect.Texture = question_texture;
        _oqa_position = oqa_position;
    }
    public void SetAnswers(List<Texture2D> answers)
    {
        foreach (var answer_texture in answers)
        {
            var rect = new TextureRect();
            rect.Texture = answer_texture;
            ((BoxContainer)(_vertical_layout ? _v_answers : _h_answers)).AddChild(rect);
        }
    }
    public void SetOQA(Texture2D oqa_texture, Vector2 oqa_scale)
    {
        Vector2 oqa_size = oqa_texture.GetSize() * oqa_scale;
        Vector2 oqa_position = -oqa_size / 2;
        OnQuestionAnswerRect.Position = oqa_position;
        OnQuestionAnswerRect.Size = oqa_size;
        OnQuestionAnswerRect.Texture = oqa_texture;
        OnQuestionAnswerRect.Visible = true;
    }

    // Set Vertical Answer Layout
    protected void _SetVAL(bool set_to)
    {
        if (_vertical_layout != set_to)
        {
            BoxContainer from = _vertical_layout ? _v_answers : _h_answers;
            BoxContainer to = _vertical_layout ? _h_answers : _v_answers;

            var nodes = from.GetChildren();
            foreach (var node in nodes)
            {
                from.RemoveChild(node);
                to.AddChild(node);
            }

            AnswerLayoutContainer.RemoveChild(from);
            AnswerLayoutContainer.AddChild(to);

            _vertical_layout = set_to;
        }
    }
    protected void _UpdateSpacing()
    {
        if (!IsInsideTree()) return;
        QAOffset.CustomMinimumSize = new Vector2(_qa_offset, 0);
        QASpacing.CustomMinimumSize = new Vector2(0, _qa_spacing);
    }
}
