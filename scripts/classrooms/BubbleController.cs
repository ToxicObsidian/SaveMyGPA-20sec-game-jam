using Godot;

public partial class BubbleController : Node2D
{
    [Signal]
    public delegate void OnAnswerDeterminedEventHandler();
    [Signal]
    public delegate void OnAnswerDisposedEventHandler();

    [Export]
    public Node2D Bubble { get; protected set; }
    [Export]
    public Sprite2D Answer { get; protected set; }
    [Export]
    public Sprite2D Emotion { get; protected set; }
    [Export]
    public TextureButton DetermineAnswer { get; protected set; }
    [Export]
    public TextureButton DisposeAnswer { get; protected set; }


    public override void _Ready()
    {
        if (DetermineAnswer != null) DetermineAnswer.Pressed += () => { EmitSignal(SignalName.OnAnswerDetermined); };
        if (DisposeAnswer != null) DisposeAnswer.Pressed += () => { EmitSignal(SignalName.OnAnswerDisposed); };
    }
}
