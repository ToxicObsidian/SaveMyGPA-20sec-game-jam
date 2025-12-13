using Godot;
using System;

public partial class TeacherController : Node2D
{
	[Export]
	public AnimatedSprite2D ASprite { get; set; }

	public void PlayWalk()
	{
		ASprite.Play("walk");
	}
}
