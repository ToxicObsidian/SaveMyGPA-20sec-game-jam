using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public enum LevelState
{
    Created = -1,
    Initializing = 0,
    OK = 1,
    Proceeding = 2,
    Finished = 3,
    Unloaded = 4,
}


public partial class ClassroomManager : Node2D
{
	// Signals will be emitted, but there are propably no handlers.
    [Signal]
    public delegate void BeforeLoadingResourcesEventHandler(int min_value, int max_value);
    [Signal]
    public delegate void AfterResourcesLoadedEventHandler();
	[Signal]
	public delegate void ClassroomReadyEventHandler();
	[Signal]
	public delegate void ClassroomFinishedEventHandler(bool try_again);


    [Export]
	public TeacherPathController PathController { get; set; }
	[Export]
	public SeatManager Seats { get; set; }
	[Export]
	public PaperController Paper { get; set; }
	[Export]
	public EndGameMenu EndGame { get; set; }


	public LevelState State 
	{ 
		get {  return _level_state; }
		set 
		{ 
			_level_state = value; 
			if (_level_state == LevelState.OK) EmitSignal(SignalName.ClassroomReady); 
		}
	}

    protected Node2D _teacher_base;
    protected LevelState _level_state = LevelState.Created;

	// Loading Stages
	protected int _loading_cur_stage = 0;
	protected int _loading_total_stage = 0;
	protected int _loading_cur_index = 0;
	protected int _loading_total_index = 0;

	// Classmate answers
	protected Dictionary<SeatController.SeatType, List<Tuple<Texture2D, bool>>> _classmate_answers = new();
	protected List<SeatController.SeatType> _populated_answers = new();
	protected List<float> _points = new();

    public async Task LoadClassroom(
		// Seats
		Vector2I matrix_size,
		Vector2 matrix_offset,
		Vector2 matrix_interval,
        PackedScene seat_scene,
        Vector2I player_coord,

		// Teacher
        Node2D teacher_base,
		Vector2 matrix_margin_topdown,
		
		// Classmate textures (body, hair, outfit)
		List<List<Tuple<Texture2D, Texture2D, Texture2D>>> classmate_textures,

        // Answers (answer, emotion, OQA, correctness)
		PackedScene question_controller_scene,
        Dictionary<SeatController.SeatType, List<Tuple<Texture2D, Texture2D, Texture2D, bool>>> classmate_answers,
		int total_question_counts,
		int hide_question_counts,
		List<float> question_points
	)
	{
		// The classroom should only be loaded once.
		if (State != LevelState.Created) return;
        Seats.SeatLoading -= _OnSeatLoading;
        Seats.SeatLoading += _OnSeatLoading;
		_InitLoading(3);
		State = LevelState.Initializing;

        // 1. Load Seats
        _NextLoadingStage("Loading seats");
        Seats.Rows = matrix_size.X;
		Seats.Columns = matrix_size.Y;
		Seats.StartPoint = matrix_offset;
		Seats.HorizontalInterval = matrix_interval.X;
		Seats.VerticalInterval = matrix_interval.Y;
		Seats.MarginTop = matrix_margin_topdown.X;
		Seats.MarginBottom = matrix_margin_topdown.Y;
		await Seats.CreateSeats(seat_scene);

        // 2. Load Classmates (Characters)
        _NextLoadingStage("Loading classmates");
		_UpdateLoadingIndex(1, 1);
		await Seats.SetupSeats(
			Paper, 
			player_coord, 
			classmate_textures,
			classmate_answers, 
			total_question_counts, hide_question_counts
		);
		Seats.SetInteractionButtonDisabled(true);
		Seats.OnDeterminedAnswer += _OnClassmateDeterminedAnswer;

		// 3. Load paper.
		_NextLoadingStage("Loading paper");
		// 3.1 Store paper info
		_populated_answers.Clear();
		_classmate_answers.Clear();
		_points.Clear();
        for (int i = 0; i < total_question_counts; i++)
		{
			_populated_answers.Add(SeatController.SeatType.Normal);
			_points.Add(question_points[i]);
			foreach (var type in classmate_answers.Keys)
			{
				if (!_classmate_answers.ContainsKey(type)) _classmate_answers[type] = new();
				_classmate_answers[type].Add(new Tuple<Texture2D, bool>(classmate_answers[type][i].Item3, classmate_answers[type][i].Item4));
			}
        }
		// 3.2 Set the PaperController
		await Paper.SetupQCs(question_controller_scene, total_question_counts, hide_question_counts);

		// 4. Load Teacher
		_NextLoadingStage("Loading teacher");
		_teacher_base = teacher_base;
		PathController.SetWalkPath(
			matrix_offset,
			matrix_size,
			matrix_interval,
			matrix_margin_topdown,
			Seats.GetCollectPaperAreaCoord(),
			player_coord
		);
        PathController.WalkingStarted += _TeacherWalkingStarted;
		PathController.WalkingFinished += _TeacherWalkingFinished;


		// 4. Set stop signal, and change state to playable
		Seats.Player.OnPaperCollected += StopGame;
		EndGame.OnRequestTryAgain += () => { EmitSignal(SignalName.ClassroomFinished, true); };
		EndGame.OnRequestBackToMenu += () => { EmitSignal(SignalName.ClassroomFinished, false); };
        State = LevelState.OK;
	}


    public void ReleaseClassroom()
	{
        Seats.ReleaseSeats();
        State = LevelState.Unloaded;
        QueueFree();
	}


	public async Task StartGame()
	{
        // 1. Play Intro
        // Note: Place teacher to correct position, or let teacher go to the first place.
		await Task.Delay(500);

		// 2. Enable user inputs
		Seats.SetInteractionButtonDisabled(false);

        // 3. Start Teacher movement.
        PathController.SetTeacher(_teacher_base);
        PathController.StartTeacherWalk(20.0f + 1.0f, false);  // Add some robustness...
    }


	public async void StopGame()
	{
		// 1. Play outro
		await Task.Delay(500);

		// 2. Block user inputs.
		Seats.SetInteractionButtonDisabled(true);

		// 3. Calculate marks
		float player_points = _CalculateMarks();

		// 4. Popup end game window.
		EndGame.SetSettlement(_PassedExam(player_points, 0.6f));
	}

	
	protected void _TeacherWalkingStarted()
	{
		
	}
	protected void _TeacherWalkingFinished()
	{

	}


	protected float _CalculateMarks()
	{
		float result = 0.0f;
		for (int i = 0; i < _points.Count; i++)
		{
			var pseat = _populated_answers[i];
            if (pseat != SeatController.SeatType.Normal && pseat != SeatController.SeatType.Player)
			{
				result += _classmate_answers[pseat][i].Item2 ? _points[i] : 0;
			}
		}
		return result;
	}

	protected bool _PassedExam(float cur_points, float ratio)
	{
		float total_points = 0.0f;
		for (int i = 0; i < _points.Count; i++) total_points += _points[i];
		if (total_points == 0.0f)
		{
			GD.PrintErr("Please configure the questions, the total points are 0.");
            return false;
        }
        return (cur_points / total_points) > ratio;
	}

    protected void _InitLoading(int total_stages)
	{
        LevelManager.Instance.InitLoadingProgress(0.0f, 1.0f);
		_loading_cur_stage = 0;
		_loading_total_stage = total_stages;
		_loading_cur_index = 0;
		_loading_total_index = 1;
    }


	protected void _NextLoadingStage(string item)
	{
		_loading_cur_stage++;
		_loading_cur_index = 0;
		_loading_total_index = 1;
		_UpdateLoadingScreen();
		LevelManager.Instance.UpdateLoadingItem(item);
	}


	protected void _UpdateLoadingIndex(int current_index, int total_index)
	{
		_loading_cur_index = current_index;
		_loading_total_index = total_index;
		_UpdateLoadingScreen();
	}


	protected void _UpdateLoadingScreen()
	{
		float cur_progress = (((float)_loading_cur_stage - 1.0f) + ((float)_loading_cur_index / (float)_loading_total_index)) 
			/ (float)_loading_total_stage;
		LevelManager.Instance.UpdateLoadingProgress(cur_progress);
	}



	protected void _OnSeatLoading(int current_index, int total_count)
	{
		_UpdateLoadingIndex(current_index, total_count);
	}

	protected void _OnClassmateDeterminedAnswer(SeatController.SeatType type, int question_index)
	{
		_populated_answers[question_index] = type;

		// Set answer to the paper.
		var _populated_texture = _classmate_answers[type][question_index].Item1;
		Paper.SetOnQuestionAnswerTexture(question_index, _populated_texture, Vector2.One);

		Seats.SetInteractionButtonDisabled(true, question_index);
	}
}
