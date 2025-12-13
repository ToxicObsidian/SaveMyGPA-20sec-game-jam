using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ClassroomMatrixSettings : Resource
{
    [Export]
    public Vector2I MatrixSize
    {
        get { return _matrix_sizes; }
        set { _matrix_sizes = value; }
    }
    [Export]
    public Vector2 MatrixOffset
    {
        get { return _matrix_offset; }
        set { _matrix_offset = value; }
    }
    [Export]
    public Vector2 MatrixInterval
    {
        get { return _matrix_interval; }
        set { _matrix_interval = value; }
    }
    [Export]
    public Array<Vector2I> AllowedPlayerCoordinates
    {
        get { return _allowed_player_coordinates; }
        set { _allowed_player_coordinates = value; }
    }
    [Export]
    public Vector2 MatrixMarginTopdown
    {
        get { return _matrix_margin_topdown; }
        set { _matrix_margin_topdown = value; }
    }

    protected Vector2I _matrix_sizes;
    protected Vector2 _matrix_offset;
    protected Vector2 _matrix_interval;
    protected Array<Vector2I> _allowed_player_coordinates;
    protected Vector2 _matrix_margin_topdown;
}