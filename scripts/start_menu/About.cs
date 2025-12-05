using Godot;
using System;

public partial class About : Control
{
	[Export]
	public ColorRect DetectionBG { get; set; }
	[Export]
	public RichTextLabel ObsidianRTL { get; set; }
	[Export]
	public RichTextLabel Stardust_MFRTL { get; set; }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetProcess(true);
		DetectionBG.GuiInput += OnDetectionBGPressed;
		ObsidianRTL.MetaClicked += OnRTLUrlClicked;
		Stardust_MFRTL.MetaClicked += OnRTLUrlClicked;
	}


    public override void _Process(double delta)
    {
        if (DetectionBG.Visible && Input.IsActionJustPressed("ui_cancel"))
		{
			Visible = false;
		}
    }


    protected void OnDetectionBGPressed(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			Visible = false;
		}
	}


	protected void OnRTLUrlClicked(Variant meta)
	{
		string url = meta.AsString();

		if(url.StartsWith("http"))
		{
			OS.ShellOpen(url);
		}
	}
}
