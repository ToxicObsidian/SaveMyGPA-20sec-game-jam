using Godot;
using System;
using System.Threading.Tasks;


public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	[Export]
	public string VersionName { get; protected set; } = "0.0.1a";

	/// <summary>
	/// Batch Load Per Frame, decreasing this will extend the batch loading time, 
	/// but also increases the idle time per frame.
	/// </summary>
	[Export(PropertyHint.Range, "0,50,")]
	public int BatchLPF { get; protected set; } = 5;
	/// <summary>
	/// Batch Texture Process Per Frame, decreasing this will extend the batch loading time,
	/// but also increases the idel time per frame.
	/// </summary>
	[Export(PropertyHint.Range, "0,100,")]
	public int BatchTPPF { get; protected set; } = 10;

    [Export]
    public GlobalSettings Settings { get; protected set; }

	[ExportGroup("Scenes")]
	[Export]
	public PackedScene StartScene { get; protected set; }
    [Export]
    public PackedScene LoadingScreenScene { get; set; }

    public Control GameRoot
	{
		get {  return _game_root; }
	}



	protected Control _game_root = null;
	protected Node2D _start_menu = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Instance == null) Instance = this;
	}

	public async Task StartGame()
	{
		StartLevelInfo sl_info = new StartLevelInfo();
		sl_info.Duration = Settings.LevelDuration;

		// Call level manager to start a level.
		await LevelManager.Instance.StartLevel(sl_info);
	}


	public void QuitGame(int code = 0)
	{
		GetTree().Quit(code);
	}

	/// <summary>
	/// Once the root node registered. Load the start screen
	/// </summary>
	/// <param name="root"></param>
	public void RegisterRoot(Control root)
	{
		_game_root = root;

		LoadingScreenManager _loading_screen = null;
        // Load the loading screen scene.
        try
        {
            _loading_screen = LoadingScreenScene.Instantiate<LoadingScreenManager>();
            root.AddChild(_loading_screen);
            _start_menu = StartScene.Instantiate<Node2D>();
            root.AddChild(_start_menu);
        }
        catch (System.InvalidCastException)
        {
            GD.PrintErr("Cannot instantiate the loading screen, since it is not a Control PackedScene.");
            QuitGame();
        }

        // Let level manager take control of the loading screen
        LevelManager.Instance.SetLoadingScreen(_loading_screen);
    }

	public void NotifyLevelStarted()
	{
		_start_menu.Visible = false;
	}
    public void NotifyLevelEnded()
    {
		_start_menu.Visible = true;
    }

	public void ForceGC()
	{
		GC.Collect();
	}
	public async Task AsyncForceGC()
	{
		await ToSignal(GameRoot.GetTree(), SceneTree.SignalName.ProcessFrame);
		GC.Collect();
	}
}
