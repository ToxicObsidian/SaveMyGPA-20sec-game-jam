using Godot;

public partial class StartMenu : Node2D
{
	[ExportGroup("Buttons")]
	[Export]
	public BaseButton StartGameBtn { get; set; }
	[Export]
	public BaseButton QuitGameBtn { get; set; }
	[Export]
	public BaseButton AboutBtn { get; set; }
	[Export]
	public BaseButton SettingsBtn { get; set; }

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

        AudioManager.Instance.PlayBGM("start_menu");
    }


	protected async void OnStartBtnPressed()
	{
        AudioManager.Instance.PlaySFX("start_game");
		AudioManager.Instance.FadeOutCurrentBGM(1);
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
