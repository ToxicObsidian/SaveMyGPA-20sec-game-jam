using Godot;
using System;

public partial class SettingsManager : Control
{
    [Export]
    public ColorRect DetectionBG { get; protected set; }
    [Export]
    public Control WindowedArea { get; protected set; }
    [Export]
    public HSlider VolumeMaster { get; protected set; }
    [Export]
    public HSlider VolumeSFX { get; protected set; }
    [Export]
    public HSlider VolumeBGM { get; protected set; }


    protected TextureRect _full_window_icon;
    protected TextureRect _windowed_icon;

    public override void _Ready()
    {
        WindowedArea.GuiInput += _OnClickChangeWindowedState;
        DetectionBG.GuiInput += _OnClickBG;
        VolumeMaster.ValueChanged += _OnMasterVolumeSliderChanged;
        VolumeSFX.ValueChanged += _OnSFXVolumeSliderChanged;
        VolumeBGM.ValueChanged += _OnBGMVolumeSliderChanged;

        // Less robustness, but enough
        var nodes = WindowedArea.GetChildren();
        foreach (var child in nodes)
        {
            if (child.Name.ToString().ToLower().Contains("full")) _full_window_icon = child as TextureRect;
            else _windowed_icon = child as TextureRect;
        }

        var window_mode = DisplayServer.WindowGetMode();
        if (window_mode != DisplayServer.WindowMode.Fullscreen && window_mode != DisplayServer.WindowMode.ExclusiveFullscreen)
            Windowed = true;
        else
            Windowed = false;

        _SetVolumeSliders();
    }


    public bool Windowed
    {
        get { return _windowed; }
        set { _windowed = value; _ChangeWindowedState(); }
    }

    protected bool _windowed = false;

    protected void _ChangeWindowedState()
    {
        if (_windowed)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            _windowed_icon.Visible = true;
            _full_window_icon.Visible = false;
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            _windowed_icon.Visible = false;
            _full_window_icon.Visible = true;
        }
    }

    protected void _SetVolumeSliders()
    {
        VolumeMaster.Value = AudioManager.Instance.GetMasterVolumeLinear();
        VolumeSFX.Value = AudioManager.Instance.GetSFXVolumeLinear();
        VolumeBGM.Value = AudioManager.Instance.GetBGMVolumeLinear();
    }


    protected void _OnClickChangeWindowedState(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseEvent &&
            mouseEvent.ButtonIndex == MouseButton.Left &&
            mouseEvent.Pressed)
        {
            Windowed = !Windowed;
        }
    }

    protected void _OnClickBG(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseEvent &&
            mouseEvent.ButtonIndex == MouseButton.Left &&
            mouseEvent.Pressed)
        {
            Visible = false;
        }
    }


    protected void _OnMasterVolumeSliderChanged(double value)
    {
        AudioManager.Instance.SetMasterVolume((float)value);
    }
    protected void _OnSFXVolumeSliderChanged(double value)
    {
        AudioManager.Instance.SetSFXVolume((float)value);
    }
    protected void _OnBGMVolumeSliderChanged(double value)
    {
        AudioManager.Instance.SetBGMVolume((float)value);
    }
}
