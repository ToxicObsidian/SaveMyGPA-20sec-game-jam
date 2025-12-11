using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public partial class SeatController : Node2D
{
	[Signal]
	public delegate void OnDeterminedAnswerEventHandler(SeatType type, int question_index);
	[Signal]
	public delegate void OnPaperCollectedEventHandler();

	[Export]
	public CharacterController Character { get; protected set; }
	[Export]
	public InteractionController Interaction { get; protected set; }
	[Export]
	protected Area2D CollectPaperAreaLeft { get; set; }
	[Export]
	protected Area2D CollectPaperAreaRight { get; set; }

	[Export]
	public Sprite2D Desk { get; protected set; }
	[Export]
	public Sprite2D Chair { get; protected set; }

	[Export]
	public BubbleController NormalBubble { get; protected set; }
	[Export]
	public BubbleController LeftBubble { get; protected set; }
	[Export]
	public BubbleController RightBubble { get; protected set; }
	[Export]
	public BubbleController FrontBubble { get; protected set; }

	protected int _answer_count = 0;
	protected List<Texture2D> _answer_textures = new();
	protected List<Texture2D> _answer_emotions = new();
	protected List<Vector2> _answer_scales = new();
	protected List<bool> _answer_correctness = new();
	protected bool _right_collect = true;

	public Area2D CollectPaperArea
	{
		get { return _right_collect ? CollectPaperAreaRight : CollectPaperAreaLeft;  }
	}
	public bool CollectDirection
	{
		get { return _right_collect; }
		set { _right_collect = value; }
	}
	public bool IsSpecialCharacter { get; set; } = false;

	[Export]
	public SeatType Type { get; set; }
	public bool Replyable
	{
		get { return Type != SeatType.Normal && Type != SeatType.Player; }
	}

	protected BubbleController _CurrentBubble
	{
		get 
		{
			if (Type == SeatType.Left) return LeftBubble;
			else if (Type == SeatType.Right) return RightBubble;
			else if (Type == SeatType.Front) return FrontBubble;
			else return NormalBubble;
		}
	}
	protected int _current_answer = -1;

	
	public enum SeatType
	{
		Normal = 0,
		Left = 1,
		Front = 2,
		Right = 3,
		Player = 4,
	}


    public override void _Ready()
    {
		if (!Replyable) Interaction.Visible = false;
		else
		{
			Interaction.OnQueryAnswer += _OnQueryAnswer;
			_CurrentBubble.DetermineAnswer.Pressed += _OnQuerySubmitBubble;
			_CurrentBubble.DisposeAnswer.Pressed += _OnQueryCancelBubble;
		}
		CollectPaperArea.AreaEntered += _OnPaperCollected;

		LeftBubble.Visible = false;
		RightBubble.Visible = false;
		FrontBubble.Visible = false;
		NormalBubble.Visible = false;

		// Prevent from being blocked by other normal bubbles.
		if (Replyable) _CurrentBubble.ZIndex += 1;
    }

	public async Task SetupSeat(
		int total_question_count,
		int hide_question_count
	)
	{
		await Interaction.SetButtons(total_question_count, hide_question_count);

		_answer_count = total_question_count;
	}

	public void SetCharacterTextures(Texture2D body, Texture2D hair, Texture2D outfit)
	{
		Character.Body.Texture = body;
		Character.Hair.Texture = hair;
		Character.Outfit.Texture = outfit;
	}


	public void SetAnswers(List<Tuple<Texture2D, Texture2D, Texture2D, Vector2, bool>> answers)
	{
		_answer_count = answers.Count;
		_answer_textures.Clear();
		_answer_emotions.Clear();
        _answer_scales.Clear();
        _answer_correctness.Clear();
		for (int i = 0; i < _answer_count; i++)
		{
			var answer = answers[i];
			_answer_textures.Add(answer.Item1);
			_answer_emotions.Add(answer.Item2);
			_answer_scales.Add(answer.Item4);
			_answer_correctness.Add(answer.Item5);
		}
	}
	public void NotifyCloseBubble(int question_index)
	{
		if (question_index != _current_answer) return;
        _current_answer = -1;
        _EaseCurrentBubble(1.0f, 0.0f, 0.25f, false);
        Interaction.Visible = true;
    }
	public void SetNormalBubbleEmotion(Texture2D emotion)
	{
		NormalBubble.Emotion.Texture = emotion;
	}


	public Vector2 GetCollectAreaPosition()
	{
		return CollectPaperArea.GetParent<Node2D>().Position;
	}


	protected void _OnQueryAnswer(int question_index)
	{
		if (!Replyable)
		{
			GD.PrintErr($"Controller queried answer, but this seat is not responsible (type={Type}.");
		}

		// 1. Hide the interaction area.
		Interaction.Visible = false;

		// 2. Show the bubble.
		_ShowAnswer(question_index);
        AudioManager.Instance.PlaySFX("show_bubble");
    }

	protected void _OnQuerySubmitBubble()
	{
        if (!Replyable)
        {
            GD.PrintErr($"Controller queried answer, but this seat is not responsible.");
        }
		else if (_current_answer == -1)
		{
			GD.PrintErr("The current answer index is -1, which is invalid.");
		}

		// 1. Emit the signal
		EmitSignal(SignalName.OnDeterminedAnswer, (int)Type, _current_answer);

		// 2. Hide the bubble.
		_current_answer = -1;
        _EaseCurrentBubble(1.0f, 0.0f, 0.25f, false);
		AudioManager.Instance.PlaySFX("hide_bubble");

        // 3. Show the interaction area.
        Interaction.Visible = true;
    }


	protected void _OnQueryCancelBubble()
	{
        if (!Replyable)
        {
            GD.PrintErr($"Controller queried answer, but this seat is not responsible.");
        }

		// 1. Hide the bubble.
		_current_answer = -1;
        _EaseCurrentBubble(1.0f, 0.0f, 0.25f, false);
        AudioManager.Instance.PlaySFX("hide_bubble");

        // 2. Show the interaction area.
        Interaction.Visible = true;
    }

	
	protected void _ShowAnswer(int index)
	{
		_current_answer = index;
		_CurrentBubble.Answer.Texture = _answer_textures[index];
		_CurrentBubble.Answer.Scale = _answer_scales[index];
		_CurrentBubble.Emotion.Texture = _answer_emotions[index];
		_CurrentBubble.Visible = true;

		_EaseCurrentBubble(0, 1, 0.25f);		// Ease in
	}


	protected void _EaseCurrentBubble(float r_in, float r_out, float duration, bool visible_after_ease = true)
	{
        var _tween = CreateTween();
        _tween.TweenMethod(
            Callable.From<float>(
                (ratio) =>
                {
					_CurrentBubble.Scale = Vector2.One * ratio;
                }
            ),
            r_in,
            r_out,
            duration
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		_tween.TweenCallback(
			Callable.From(() => { _CurrentBubble.Visible = visible_after_ease; })
		);
    }


	protected void _OnPaperCollected(Area2D area)
	{
		CollectPaperArea.GetChild<CollisionShape2D>(0).SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

		// Do something.
		EmitSignal(SignalName.OnPaperCollected);
		if (Replyable) Interaction.SetAllButtonsDisabled(true);
		else _ShowNormalBubble(0.3f, 2.5);
	}
	protected void _ShowNormalBubble(float chance, double duration)
	{
		if (new Random().NextSingle() >= chance && !IsSpecialCharacter) return;

		var _tween = CreateTween();
		_tween.TweenCallback(Callable.From(() => { _CurrentBubble.Visible = true; }));
        _tween.TweenMethod(
            Callable.From<float>(
                (ratio) =>
                {
                    _CurrentBubble.Scale = Vector2.One * ratio;
                }
            ),
            0.0f,
            1.0f,
            0.25
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		_tween.TweenInterval(duration);
        _tween.TweenMethod(
            Callable.From<float>(
                (ratio) =>
                {
                    _CurrentBubble.Scale = Vector2.One * ratio;
                }
            ),
            1.0f,
            0.0f,
            0.25
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        _tween.TweenCallback(Callable.From(() => { _CurrentBubble.Visible = false; }));
    }
}
