using Godot;
using System;
using System.Security.Principal;
using System.Threading.Tasks;

public partial class InteractionController : Control
{
    [Signal]
    public delegate void OnQueryAnswerEventHandler(int index);

	[Export]
	public GridContainer InteractContainer { get; set; }
    [Export]
    public PackedScene ButtonPrefab { get; set; }
    [Export]
    public int GridHSeperation 
    {
        get { return _cur_grid_h_seperation; }
        set {  _cur_grid_h_seperation = value; _UpdateGridSeperation(); }
    }
    [Export]
    public int GridVSeperation
    {
        get { return _cur_grid_v_seperation; }
        set { _cur_grid_v_seperation = value; _UpdateGridSeperation(); }
    }


    protected int _cur_grid_h_seperation = 4;
    protected int _cur_grid_v_seperation = 4;

    public override void _Ready()
    {
        _SetSignalHandlers();
        _UpdateGridSeperation();
    }


    /// <summary>
    /// Set buttons, the latter ones will be hided.
    /// </summary>
    /// <param name="total_count"></param>
    /// <param name="hide_count"></param>
    public async Task SetButtons(int total_count, int hide_count)
    {
        _ClearButtons();

        for (int i = 1; i <= total_count; i++)
        {
            if (i % GameManager.Instance.BatchLPF == 0) 
                await ToSignal(GameManager.Instance.GetTree(), SceneTree.SignalName.ProcessFrame);

            
            var button = ButtonPrefab.Instantiate<InteractionButton>();
            if (button is not InteractionButton)
            {
                GD.PrintErr($"The given button prefab is not an interaction button");
                break;
            }
            button.Text = $"{i}";
            button.Index = i;
            if (i + hide_count > total_count) button.Visible = false;
            InteractContainer.AddChild(button);
        }
    }


    public void SetAllButtonVisible()
    {
        var buttons = InteractContainer.GetChildren();
        foreach (var button in buttons)
        {
            if (button is Button b) b.Visible = true;
        }
    }


    public void SetButtonDisabled(int index, bool disabled)
    {
        var buttons = InteractContainer.GetChildren();
        if (index >= 0 && index < buttons.Count && buttons[index] is BaseButton b)
        {
            b.Disabled = disabled;
            b.MouseDefaultCursorShape = disabled ? CursorShape.Arrow : CursorShape.PointingHand;
        }
    }


    public void SetAllButtonsDisabled(bool disabled)
    {
        var buttons = InteractContainer.GetChildren();
        foreach (var button in buttons)
        {
            if (button is BaseButton b)
            {
                b.Disabled = disabled;
                b.MouseDefaultCursorShape = disabled ? CursorShape.Arrow : CursorShape.PointingHand;
            }
        }
    }


    protected void _ClearButtons()
    {
        var buttons = InteractContainer.GetChildren();
        foreach (var button in buttons)
        {
            InteractContainer.RemoveChild(button);
            button.QueueFree();
        }
    }

    protected void _SetSignalHandlers()
    {
        var buttons = InteractContainer.GetChildren();
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] is InteractionButton b)
            {
                b.Pressed += (() => _OnButtonPressed(b.Index));
            }
            else
            {
                GD.PrintErr($"There are non-Button nodes in the interaction container! (#{i}: {buttons[i].Name})");
            }
        }
    }

    protected void _OnButtonPressed(int index)
    {
        EmitSignal(SignalName.OnQueryAnswer, index - 1);
    }

    protected void _UpdateGridSeperation()
    {
        InteractContainer.AddThemeConstantOverride("h_seperation", _cur_grid_h_seperation);
        InteractContainer.AddThemeConstantOverride("v_seperation", _cur_grid_v_seperation);
    }
}
