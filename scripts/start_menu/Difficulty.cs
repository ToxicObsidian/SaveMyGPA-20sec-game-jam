using Godot;
using System;

public partial class Difficulty : Control
{
	[Export]
	public HSlider DifficultySlider { get; protected set; }
	[Export]
	public TextureRect DifficultyExpr { get; protected set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DifficultySlider.Value = GameManager.Instance.Settings.Difficulty;
		DifficultySlider.ValueChanged += _OnDifficultyChanged;
	}

	protected void _OnDifficultyChanged(double value)
	{
		GameManager.Instance.Settings.Difficulty = (int)(value + 0.01);
	}
}
