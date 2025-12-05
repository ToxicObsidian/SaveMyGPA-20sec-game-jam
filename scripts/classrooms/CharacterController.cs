using Godot;
using System;

public partial class CharacterController : Node2D
{
	[Export]
	public Sprite2D Body { get; protected set; }
	[Export]
	public Sprite2D Hair { get; protected set; }
	[Export]
	public Sprite2D Outfit { get; protected set; }
}
