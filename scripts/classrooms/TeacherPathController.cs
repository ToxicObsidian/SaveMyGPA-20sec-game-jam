using Godot;
using System.ComponentModel.DataAnnotations;

public partial class TeacherPathController : Node2D
{
	[Export] public Path2D path_line;
	[Export] public PathFollow2D path_follow;

	protected double start_time = -1.0f;
	protected double end_time = -1.0f;
	protected Tween _walk_tween;
	protected Node2D _teacher_base;
	protected bool _right_side = true;

	public float Progress
	{
		get
		{
			return path_follow.ProgressRatio;
		}
	}

	public Tween WalkTween
	{
		get { return _walk_tween; }
	}

	[Signal]
	public delegate void WalkingStartedEventHandler();
	[Signal]
	public delegate void WalkingFinishedEventHandler();

	/// <summary>
	/// Set the walk path, returns the distance to the player's coordinate.
	/// </summary>
	/// <param name="matrix_startpoint"></param>
	/// <param name="matrix_size"></param>
	/// <param name="matrix_interval"></param>
	/// <param name="matrix_margin_topdown"></param>
	/// <param name="collect_paper_area_coord"></param>
	/// <param name="player_coord"></param>
	/// <returns>The distance from start point to the player's coordinate. (not implemented)</returns>
	public float SetWalkPath(
		Vector2 matrix_startpoint,
        Vector2I matrix_size,
        Vector2 matrix_interval,
		Vector2 matrix_margin_topdown,
		Vector2 collect_paper_area_coord, 
		Vector2I player_coord
	)
	{
		path_follow.Rotates = false;
		path_line.Curve = new Curve2D();

		bool right_side = collect_paper_area_coord.X > 0;
		_right_side = right_side;

		int start = right_side ? matrix_size.X - 1 : 0;
		int end = right_side ? player_coord.X - 1 : player_coord.X + 1;
		int step = right_side ? -1 : 1;

		bool flip_dir = false;
		for (int i = start; i != end; i += step)
		{
			var point_x = i * matrix_interval.X + collect_paper_area_coord.X;
			var top_y = -matrix_margin_topdown.X;
			var bottom_y = i == player_coord.X ? 
				matrix_margin_topdown.Y + matrix_interval.Y * (player_coord.Y) + collect_paper_area_coord.Y :
                matrix_margin_topdown.Y + matrix_interval.Y * (matrix_size.Y - 1);

			var top_point = new Vector2(point_x, top_y) + matrix_startpoint;
			var bottom_point = new Vector2(point_x, bottom_y) + matrix_startpoint;
			
			// Vertical line
			if (flip_dir)
			{
				path_line.Curve.AddPoint(bottom_point);
				path_line.Curve.AddPoint(top_point);
			}
			else
			{
                path_line.Curve.AddPoint(top_point);
                path_line.Curve.AddPoint(bottom_point);
            }
            flip_dir = !flip_dir;
        }

		return 0.0f; // Not implemented
	}


	public void SetTeacher(Node2D teacher_base)
	{
		teacher_base.GetParent()?.RemoveChild(teacher_base);
		AddChild(teacher_base);
		teacher_base.Visible = false;
        _teacher_base = teacher_base;
		if (_right_side) _teacher_base.Scale = new Vector2(-_teacher_base.Scale.X, _teacher_base.Scale.Y);
	}
	public Node2D ReleaseTeacher()
	{
		if (_teacher_base != null)
		{
			_teacher_base.GetParent()?.RemoveChild(_teacher_base);
            var t = _teacher_base;
			_teacher_base = null;
			return t;
		}
		return null;
	}


    /// <summary>
    /// This function will perform a teacher sprite walk along the given Path2D.
    /// </summary>
    /// <param name="walk_duration">The duration of teacher walk.</param>
    /// <param name="teacher_sprite">The teacher sprite.</param>
    /// <param name="hide_after_walk">Hide the teacher sprite when the walk completes.</param>
    /// <param name="reset_sprite_parent">
    /// Reset the sprite parent to its original parent when the walk completes.
    /// </param>
    public void StartTeacherWalk(
		float walk_duration, 
		bool hide_after_walk = true
	)
	{
		if (_walk_tween != null)
		{
			GD.PrintErr("Cannot start teacher walk, since the tween is not null (busy)");
			return;
		}
		Tween teacher_move_tween = CreateTween();

		// Startup callback
		// Set the teacher sprite's parent to the path follow node.
        teacher_move_tween.TweenCallback(Callable.From(() => 
			{
				// Prepare the _teacher_base
				RemoveChild(_teacher_base);
				path_follow.AddChild(_teacher_base);
				_teacher_base.Visible = true;
				_walk_tween = teacher_move_tween;

                EmitSignal(SignalName.WalkingStarted);
            }
		));

		// Create Tween walk.
        teacher_move_tween.TweenMethod(
			Callable.From<float>(t => _TeacherMovement(t)),
			0.0f,
			1.0f,
			walk_duration
		)
		.SetTrans(Tween.TransitionType.Linear);

		// Shutdown callback
		// Deal with the sprite according to the settings.
		teacher_move_tween.TweenCallback(Callable.From(() =>
			{
				_walk_tween = null;
                if (hide_after_walk) _teacher_base.Visible = false;

				EmitSignal(SignalName.WalkingFinished);
			}
		));
	}


	protected void _TeacherMovement(float pos_ratio) 
	{
		path_follow.ProgressRatio = pos_ratio;

		if (path_follow.Position.Y - path_line.Curve.GetPointPosition(0).Y > 1) path_follow.ZIndex = 1;
		else path_follow.ZIndex = 0;
	}


}
