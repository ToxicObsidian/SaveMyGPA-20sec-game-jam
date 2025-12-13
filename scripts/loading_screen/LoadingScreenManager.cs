using Godot;
using Godot.Collections;
using System;

public partial class LoadingScreenManager : Node2D
{
    [Export]
    public Control LoadingOverlay {  get; protected set; }
    [Export]
    public ProgressBar LoadingProgressBar { get; protected set; }
    [Export]
    public Label TipsLabel { get; protected set; }
    [Export]
    public Label LoadingItemLabel { get; protected set; }

    [Export]
    public Array<string> StockTips { get; set; }
    [Export]
    public bool EnableTipChange { get; set; } = true;

    public bool IsLoading
    {
        get { return _is_loading; }
    }

    protected bool _is_loading = false;
    protected int _cur_tips_index = 0;

    public override void _Ready()
    {
        LoadingOverlay.GuiInput += _OnClickOverlay;
        if (StockTips.Count > 0)
        {
            StockTips.Shuffle();
            TipsLabel.Text = StockTips[0];
        }
    }

    public void StartLoadingScreen(float min_value, float max_value)
    {
        _is_loading = true;
        LoadingItemLabel.Visible = true;
        EnableTipChange = true;
        LoadingProgressBar.MinValue = min_value;
        LoadingProgressBar.MaxValue = max_value;
        LoadingProgressBar.Value = min_value;
    }

    public void EndLoadingScreen()
    {
        _is_loading = false;
        LoadingItemLabel.Visible = false;
        EnableTipChange = false;
        LoadingProgressBar.MinValue = 0;
        LoadingProgressBar.MaxValue = 1;
        LoadingProgressBar.Value = 0;
    }


    protected void _OnClickOverlay(InputEvent input_event)
    {
        if (EnableTipChange &&
            input_event is InputEventMouseButton mb_event &&
            mb_event.ButtonIndex == MouseButton.Left &&
            mb_event.Pressed)
        {
            // Change the tips
            _cur_tips_index++;
            if (_cur_tips_index >= StockTips.Count)
            {
                _cur_tips_index = 0;
                StockTips.Shuffle();
            }
            TipsLabel.Text = StockTips[_cur_tips_index];
        }
    }
}
