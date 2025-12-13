using Godot;
using Godot.Collections;
using System;

[Flags]
public enum SupportedClassroomType
{
    None = 0,
    Classic = 1,
}

[GlobalClass]
public partial class ClassroomStockItem: Resource
{
    [ExportGroup("Meta")]
    [Export]
    public string ClassroomName
    {
        get { return _classroom_name; }
        set { _classroom_name = value; }
    }
    [Export]
    public SupportedClassroomType ClassroomType { get; private set; } = SupportedClassroomType.Classic;
    [Export]
    public Array<ClassroomMatrixSettings> Matrices { get; set; }
    
    

    [ExportGroup("Stock Resources")]
    [Export]
    public ClassmateData Classmates { get; set; }
    [Export]
    public Questions Questions { get; set; }
    [Export]
    public PackedScene ClassroomScene { get; set; }
    [Export]
    public PackedScene SeatSuite { get; set; }
    [Export]
    public PackedScene Teacher { get; set; }
    [Export]
    public PackedScene QuestionLayout { get; set; }



    protected string _classroom_name = "classroom";
    protected Array<Vector2I> _matrix_sizes = new();
    protected Texture2D _background_texture = null;
    protected Texture2D _desk_texture = null;
    protected Texture2D _chair_texture = null;
}
