using Godot;
using System.Threading.Tasks;

public partial class EndGameMenu : Node2D
{
	[Signal]
	public delegate void OnRequestTryAgainEventHandler();
	[Signal]
	public delegate void OnRequestBackToMenuEventHandler();

	[Export]
	public Label WinText { get; protected set; }
	[Export]
	public Label LoseText { get; protected set; }
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

	public void SetSettlement(bool win)
	{
		WinText.Visible = win;
		LoseText.Visible = !win;
		Visible = true;
	}

	protected async Task _Win()
	{
		await Task.Delay(500);
	}

	protected async Task _Lose()
	{
        await Task.Delay(500);
    }
}
