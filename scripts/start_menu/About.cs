using Godot;
using System;
using System.Collections.Generic;

public partial class About : Control
{
	[Export]
	public ColorRect DetectionBG { get; set; }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetProcess(true);
		DetectionBG.GuiInput += OnDetectionBGPressed;

		Stack<Node> children_stack = new(GetChildren());
		while(children_stack.Count > 0)
		{
			var child = children_stack.Pop();
			if (child is RichTextLabel rtl) rtl.MetaClicked += OnRTLUrlClicked;
			foreach (var node in child.GetChildren()) children_stack.Push(node);
		}
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
			GD.Print($"Open url: {url}");
			OS.ShellOpen(url);
		}
	}
}
