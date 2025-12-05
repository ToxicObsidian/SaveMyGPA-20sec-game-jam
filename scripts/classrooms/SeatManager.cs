using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;




[Tool]
public partial class SeatManager : Node2D
{
    [Signal]
    public delegate void SeatLoadingEventHandler(int current_index, int total_count);
    [Signal]
    public delegate void SeatsLoadedEventHandler();
    [Signal]
    public delegate void OnDeterminedAnswerEventHandler(SeatController.SeatType type, int question_index);

    [Export]
    public Vector2 StartPoint 
    { 
        get { return _start_point; }
        set { _start_point = value; QueueRedraw(); _RepositionSeats(); }
    }

    [Export]
    public int Rows 
    { 
        get {  return _rows; }
        set { _rows = value; QueueRedraw(); }
    }
    [Export]
    public int Columns 
    { 
        get { return _columns; }
        set { _columns = value; QueueRedraw(); }
    }
    [Export]
    public float HorizontalInterval 
    {
        get { return _horizontal_interval; }
        set { _horizontal_interval = value; QueueRedraw(); _RepositionSeats(); }
    }
    [Export]
    public float VerticalInterval 
    { 
        get { return _vertical_interval; }
        set { _vertical_interval = value; QueueRedraw(); _RepositionSeats(); }
    }
    [Export]
    public float MarginTop
    {
        get { return _margin_top; }
        set { _margin_top = value; QueueRedraw(); }
    }
    [Export]
    public float MarginBottom
    {
        get { return _margin_bottom; }
        set { _margin_bottom = value; QueueRedraw(); }
    }

    public SeatController Player
    {
        get { return _player; }
    }

    protected Vector2 _start_point;
    protected int _rows = 5;
    protected int _columns = 5;
    protected float _horizontal_interval = 50;
    protected float _vertical_interval = 50;
    protected float _margin_top;
    protected float _margin_bottom;
    protected SeatController _player;

    protected Node2D _seat_anchor = null;
    protected List<List<SeatController>> _seat_matrix = new();

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) QueueRedraw();
    }


    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) QueueRedraw();
    }

    
    public async Task CreateSeats(PackedScene SeatPrefab)
    {
        if (_seat_anchor != null) return;

        // Create the anchor
        _seat_anchor = new Node2D();
        AddChild(_seat_anchor);
        _seat_anchor.Position = StartPoint;

        // Create seats
        int current_count = 0;
        _seat_matrix.Clear();
        for (int i = 0; i < Rows; i++)
        {
            List<SeatController> column = new();
            for (int j = 0; j < Columns; j++) 
            {
                var node = SeatPrefab.Instantiate<SeatController>();
                // node.ZIndex = (Rows - i - 1); // Do not use this.
                _seat_anchor.AddChild(node);
                node.Position = new Vector2(i * HorizontalInterval, j * VerticalInterval);
                column.Add(node);
                
                current_count++;
                EmitSignal(SignalName.SeatLoading, current_count, Rows * Columns);
                if (current_count % GameManager.Instance.BatchLPF == 0)
                {
                    await ToSignal(GameManager.Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
                }
            }
            _seat_matrix.Add(column);
        }

        // Emit signals if seats are completely loaded.
        EmitSignal(SignalName.SeatsLoaded);
    }


    public void ReleaseSeats()
    {
        _seat_matrix.ForEach(c => { c.Clear(); });
        _seat_matrix.Clear();
        _seat_anchor.QueueFree();
    }


    public async Task SetupSeats(
        PaperController paper,
        Vector2I player_coord,

        List<List<Tuple<Texture2D, Texture2D, Texture2D>>> classmate_textures,
        Dictionary<SeatController.SeatType, List<Tuple<Texture2D, Texture2D, Texture2D, bool>>> classmate_answers,

        int total_question_count,
        int hide_question_count
    )
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                var cur_seat = _seat_matrix[i][j];
                // 1. Set the seat types.
                if (player_coord.X == i && player_coord.Y == j)
                {
                    cur_seat.Type = SeatController.SeatType.Player;
                    _player = cur_seat;
                }
                else if (player_coord.X - 1 == i && player_coord.Y == j) cur_seat.Type = SeatController.SeatType.Left;
                else if (player_coord.X + 1 == i && player_coord.Y == j) cur_seat.Type = SeatController.SeatType.Right;
                else if (player_coord.X == i && player_coord.Y - 1 == j) cur_seat.Type = SeatController.SeatType.Front;

                // 2. Set the characters.
                var ctextures = classmate_textures[i][j];
                cur_seat.SetCharacterTextures(ctextures.Item1, ctextures.Item2, ctextures.Item3);

                // 3. Set the answers
                if (cur_seat.Replyable)
                {
                    await cur_seat.SetupSeat(total_question_count, hide_question_count);
                    cur_seat.OnDeterminedAnswer += _OnClassmateDeterminedAnswer;
                    paper.PaperFirstFlipped += cur_seat.Interaction.SetAllButtonVisible;

                    if (classmate_answers.ContainsKey(cur_seat.Type))
                    {
                        cur_seat.SetAnswers(classmate_answers[cur_seat.Type]);
                    }
                }
            }
        }
    }


    public Vector2 GetCollectPaperAreaCoord()
    {
        if (_seat_matrix == null || _seat_matrix.Count == 0)
        {
            GD.PrintErr("The seat manager has not created seats yet, but queried the CPA coord. Please check the logic.");
        }
        return _seat_matrix[0][0].GetCollectAreaPosition();
    }


    public void SetInteractionButtonDisabled(bool disabled, int index = -1)
    {
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                var seat = _seat_matrix[i][j];
                if (seat.Replyable)
                {
                    if (index == -1) seat.Interaction.SetAllButtonsDisabled(disabled);
                    else seat.Interaction.SetButtonDisabled(index, disabled);
                }
            }
        }
    }


    public override void _Draw()
    {
        if (!Engine.IsEditorHint()) return;

        DrawLine(StartPoint + Vector2.Up * 8.0f, StartPoint + Vector2.Down * 8.0f, Colors.Yellow, 5);
        DrawLine(StartPoint + Vector2.Left * 8.0f, StartPoint + Vector2.Right * 8.0f, Colors.Yellow, 5);

        DrawLine(
            new Vector2(StartPoint.X - 50, StartPoint.Y - MarginTop),
            new Vector2(StartPoint.X + (_columns - 1) * _horizontal_interval + 50, StartPoint.Y - MarginTop),
            Colors.Green,
            5
        );
        DrawLine(
            new Vector2(StartPoint.X - 50, StartPoint.Y + (_rows - 1) * _vertical_interval + MarginBottom),
            new Vector2(StartPoint.X + (_columns - 1) * _horizontal_interval + 50, StartPoint.Y + (_rows - 1) * _vertical_interval + MarginBottom),
            Colors.Green,
            5
        );


        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                if (i == 0 && j == 0) continue;
                Vector2 pos = new Vector2(StartPoint.X + i * _horizontal_interval, StartPoint.Y + j * _vertical_interval);
                //if (j == 3) DrawCircle(pos, 4.0f, Colors.Blue);
                //else DrawCircle(pos, 4.0f, Colors.Red);
                DrawCircle(pos, 4.0f, Colors.Red);
            }
        }
    }

    protected void _RepositionSeats()
    {
        if (Engine.IsEditorHint()) return;

        for (int i = 0; i < _seat_matrix.Count; i++)
        {
            for (int j = 0; j < _seat_matrix[i].Count; j++)
            {
                _seat_matrix[i][j].Position = new Vector2(i * HorizontalInterval, j * VerticalInterval);
            }
        }
    }

    protected void _OnClassmateDeterminedAnswer(SeatController.SeatType type, int question_index)
    {
        EmitSignal(SignalName.OnDeterminedAnswer, (int)type, question_index);
    }
}
