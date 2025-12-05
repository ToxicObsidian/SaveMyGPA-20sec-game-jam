using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PaperController : Sprite2D
{
    [Signal]
    public delegate void PaperFirstFlippedEventHandler();
    

	[Export]
	public Panel FrontFlip { get; set; }
    [Export]
    public Panel BackFlip { get; set; }
    [Export]
    public VBoxContainer Front { get; set; }
    [Export]
    public VBoxContainer Back { get; set; }


    public bool Side
    {
        get { return _paper_side; }
        set { _paper_side = value; _UpdatePaperSide(); }
    }


    protected bool _paper_has_flipped = false;
    protected bool _paper_side = true; // true = front, false = back
    protected List<QuestionController> _questions = new();
    protected int _hide_count = 0;

    public override void _Ready()
    {
        FrontFlip.GuiInput += _OnFlipPressed;
        BackFlip.GuiInput += _OnFlipPressed;
        _UpdatePaperSide();
    }

    public async Task SetupQCs(PackedScene qc_scene, int total_count, int hide_count)
    {
        foreach (var fn in Front.GetChildren()) Front.RemoveChild(fn);
        foreach (var bn in Back.GetChildren()) Back.RemoveChild(bn);
        _questions.Clear();

        _hide_count = hide_count;
        for (int i = 0; i < total_count; i++)
        {
            var qc = qc_scene.Instantiate<QuestionController>();
            _questions.Add(qc);
            if (i + hide_count >= total_count) Back.AddChild(qc);
            else Front.AddChild(qc);

            if ((i + 1) % GameManager.Instance.BatchLPF == 0)
                await ToSignal(GameManager.Instance.GameRoot.GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }
    public void SetOnQuestionAnswerTexture(int index, Texture2D oqa_texture, Vector2 oqa_scale)
    {
        if (index >= _questions.Count)
        {
            GD.Print("OQA Index mismatched.");
            return;
        }

        _questions[index].SetOQA(oqa_texture, oqa_scale);
    }


    public void FlipPaper()
    {
        if (!_paper_has_flipped) EmitSignal(SignalName.PaperFirstFlipped);

        // Flip the paper.
        Side = !Side;
    }


    protected void _OnFlipPressed(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Left &&
            mouseButton.Pressed) 
        {
            FlipPaper();
        }
    }

    protected void _UpdatePaperSide()
    {
        Front.Visible = _paper_side;
        FrontFlip.Visible = _paper_side;
        Back.Visible = !_paper_side;
        BackFlip.Visible = !_paper_side;

        FlipH = !_paper_side;
    }
}
