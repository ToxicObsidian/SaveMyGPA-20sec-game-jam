using Godot;
using System;

public partial class Difficulty : Control
{
	[Export]
	public HSlider DifficultySlider { get; protected set; }
	[Export]
	public Label DifficultyDesc { get; protected set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var s = GameManager.Instance.Settings;
		DifficultySlider.MaxValue = s.MaxDifficulty;
		DifficultySlider.MinValue = s.MinDifficulty;
		DifficultySlider.Value = s.Difficulty;
		DifficultyDesc.Text = s.DifficultyDesc[s.Difficulty];

		DifficultySlider.ValueChanged += _OnDifficultyChanged;
	}

	protected void _OnDifficultyChanged(double value)
	{
		int to_difficulty = (int)(value + 0.01);
        GameManager.Instance.Settings.Difficulty = to_difficulty;
		DifficultyDesc.Text = GameManager.Instance.Settings.DifficultyDesc[to_difficulty];
	}
}
