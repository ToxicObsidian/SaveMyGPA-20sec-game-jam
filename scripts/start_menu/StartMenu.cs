using Godot;

public partial class StartMenu : Node2D
{
	[ExportGroup("Buttons")]
	[Export]
	public Button StartGameBtn { get; set; }
	[Export]
	public Button QuitGameBtn { get; set; }
	[Export]
	public Button AboutBtn { get; set; }
	[Export]
	public Button SettingsBtn { get; set; }

	[ExportGroup("Prefabs")]
	[Export]
	public Control AboutPage {  get; set; }
	[Export]
	public Control SettingsPage { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		StartGameBtn.Pressed += OnStartBtnPressed;
		QuitGameBtn.Pressed += OnQuitBtnPressed;
		AboutBtn.Pressed += OnAboutBtnPressed;
		SettingsBtn.Pressed += OnSettingsBtnPressed;
	}


	protected async void OnStartBtnPressed()
	{
		await GameManager.Instance.StartGame();
	}

	protected void OnAboutBtnPressed()
	{
		if(!AboutPage.Visible) AboutPage.Visible = true;
	}

	protected void OnSettingsBtnPressed()
	{
		if(!SettingsPage.Visible) SettingsPage.Visible = true;
	}


	protected void OnQuitBtnPressed()
	{
		GameManager.Instance.QuitGame();
	}
}
