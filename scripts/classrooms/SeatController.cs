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


	public void SetAnswers(List<Tuple<Texture2D, Texture2D, Texture2D, bool>> answers)
	{
		_answer_count = answers.Count;
		_answer_textures.Clear();
		_answer_emotions.Clear();
		_answer_correctness.Clear();
		for (int i = 0; i < _answer_count; i++)
		{
			var answer = answers[i];
			_answer_textures.Add(answer.Item1);
			_answer_emotions.Add(answer.Item2);
			_answer_correctness.Add(answer.Item4);
		}
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
		_CurrentBubble.Visible = false;

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
		_CurrentBubble.Visible = false;

		// 2. Show the interaction area.
		Interaction.Visible = true;
    }

	protected void _ShowAnswer(int index)
	{
		_current_answer = index;
		_CurrentBubble.Answer.Texture = _answer_textures[index];
		_CurrentBubble.Emotion.Texture = _answer_emotions[index];
		_CurrentBubble.Visible = true;
	}


	protected void _OnPaperCollected(Area2D area)
	{
		CollectPaperArea.GetChild<CollisionShape2D>(0).SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

		// Do something.
		EmitSignal(SignalName.OnPaperCollected);
		if (Replyable) Interaction.SetAllButtonsDisabled(true);
	}
}
