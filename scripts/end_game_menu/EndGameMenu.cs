using Godot;
using Godot.Collections;
using System.Threading.Tasks;

public partial class EndGameMenu : Node2D
{
	[Signal]
	public delegate void OnRequestTryAgainEventHandler();
	[Signal]
	public delegate void OnRequestBackToMenuEventHandler();

	[Export]
	public Label Points { get; set; }
	[Export]
	public Control Win { get; protected set; }
	[Export]
	public Label WinTips { get; protected set; }
	[Export]
	public Sprite2D WinEmote { get; protected set; }
	[Export]
	public Control Lose { get; protected set; }
	[Export]
	public Label LoseTips { get; protected set; }
	[Export]
	public Sprite2D LoseEmote { get; protected set; }
	[Export]
	public Array<string> LoseTipsBundle { get; protected set; }
	[Export]
	public BaseButton TryAgainButton { get; protected set; }
	[Export]
	public BaseButton BackToMenuButton { get; protected set; }


	public override void _Ready()
	{
		Visible = false;
		TryAgainButton.Pressed += () => { EmitSignal(SignalName.OnRequestTryAgain); };
		BackToMenuButton.Pressed += () => { EmitSignal(SignalName.OnRequestBackToMenu); };
	}

	public void SetSettlement(bool win, float got_score, float total_score, bool notify_flipping)
	{
		Win.Visible = win;
		WinEmote.Visible = win;
		Lose.Visible = !win;
		LoseEmote.Visible = !win;

		Points.Text = string.Format(
            "Points: {0}/{1}",
            got_score.ToString("F0"),
            total_score.ToString("F0")
        );

        if (win)
		{
			WinTips.Text = "Congratulations.";
		}
        else
        {
			LoseTips.Text = notify_flipping ? LoseTipsBundle[1] : LoseTipsBundle[0];
        }
        Visible = true;
	}
}
