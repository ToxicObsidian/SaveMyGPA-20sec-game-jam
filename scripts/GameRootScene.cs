using Godot;
using System;

public partial class GameRootScene : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GameManager.Instance.RegisterRoot(this);
	}

}
