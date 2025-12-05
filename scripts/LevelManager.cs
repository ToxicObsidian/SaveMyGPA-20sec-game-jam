using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    [ExportGroup("Loading Scene")]
    [Export]
    public float LoadingSceneEaseDuration { get; set; } = 0.5f;
    [Export]
    protected Tween.EaseType LoadingSceneEaseType { get; set; }
    [Export]
    protected Tween.TransitionType LoadingSceneTransitionType { get; set; }
    

    [ExportGroup("Stock Items")]
    [Export]
    public ClassroomStock StockClassrooms { get; set; }
    
    /// <summary>
    /// LevelManager is busy if it is initiating a new level.
    /// </summary>
    public bool Busy
    {
        get { return _ls_tween != null; }
    }


    protected LoadingScreenManager _loading_screen = null;
    protected Tween _ls_tween = null;
    protected float _ls_original_y = 0.0f;

    protected ClassroomManager _current_classroom = null;
    protected StartLevelInfo _previous_sli = null;

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
    }


    /// <summary>
    /// Instantiate and initiate a new level by the given StartLevelInfo.
    /// </summary>
    /// <param name="info">A struct containing the info to start specific level.</param>
    /// <returns></returns>
    public async Task StartLevel(StartLevelInfo info)
    {
        if (Instance == null || Busy) return;

        // 1. Change to loading screen.
        await _EaseInLoadingScreen();
        GameManager.Instance.NotifyLevelStarted();
        _previous_sli = info;

        // 2. Load classroom.
        var classroom_mgr = await _StartClassroom(info);
        GameManager.Instance.GameRoot.AddChild(classroom_mgr);

        // 3. Remove the loading screen.
        await _EaseOutLoadingScreen();

        // 4. Start game
        await classroom_mgr.StartGame();
    }


    protected async Task<ClassroomManager> _StartClassroom(StartLevelInfo info)
    {
        // 1. Roll level info by start level info.
        ClassroomStockItem rolled_classroom_stock_item = StockClassrooms.Items.PickRandom();
        var matrix_info = rolled_classroom_stock_item.Matrices.PickRandom();
        var clsrm_scene = rolled_classroom_stock_item.SeatSuite;
        var teacher_base = rolled_classroom_stock_item.Teacher.Instantiate<Node2D>();
        var DEFAULT_ANSWER_TEXTURE = ResourceLoader.Load<Texture2D>("res://resources/images/paper/questions/classic/MultiChoices/t1/c_mc_t1_o1.png");
        var DEFAULT_ANSWER_EMOTION = ResourceLoader.Load<Texture2D>("res://resources/images/characters/emotions/我有一计.png");
        var DEFAULT_OQA_TEXTURE = ResourceLoader.Load<Texture2D>("res://resources/images/paper/icons/◇.png");
        var DEFAULT_QC_SCENE = ResourceLoader.Load<PackedScene>("res://scenes/prefabs/classrooms/question.tscn");
        var clsmt_data = rolled_classroom_stock_item.Classmates;
        var clsmt_textures = _RollClassmateTextures(clsmt_data, matrix_info.MatrixSize);
        var clsmt_answers = await _RollClassmateAnswers(
            new List<QuestionData>(),
            GameManager.Instance.Settings.Difficulty,
            GameManager.Instance.Settings.DifficultyMap.ToDictionary(),
            async (x, d) =>
            {
                foreach (var k in d.Keys)
                {
                    d[k].Clear();
                    for (int i = 0; i < 6; i++) d[k].Add(new Tuple<Texture2D, Texture2D, Texture2D, bool>(
                        DEFAULT_ANSWER_TEXTURE, 
                        DEFAULT_ANSWER_EMOTION, 
                        DEFAULT_OQA_TEXTURE,
                        false));
                    await Task.Delay(0);
                }
            }
        );

        // 2. Initialize the level by info.
        // Note: There's no need for adding signal handlers, since this is a singleton.
        //       ClassroomManager should call Loading* methods to change loading screen.
        ClassroomManager classroom_mgr = rolled_classroom_stock_item.ClassroomScene.Instantiate<ClassroomManager>();
        _current_classroom = classroom_mgr;
        await classroom_mgr.LoadClassroom(
            matrix_info.MatrixSize,
            matrix_info.MatrixOffset,
            matrix_info.MatrixInterval,
            clsrm_scene,
            matrix_info.AllowedPlayerCoordinates.PickRandom(),

            // Teacher
            teacher_base:           teacher_base,
            matrix_margin_topdown:  matrix_info.MatrixMarginTopdown,

            // Classmates
            question_controller_scene: DEFAULT_QC_SCENE,
            classmate_textures:     clsmt_textures,
            classmate_answers:      clsmt_answers,
            total_question_counts:  6,
            hide_question_counts:   2,
            question_points:        [0, 0, 0, 0, 0, 0]
        );
        _current_classroom.ClassroomReady += _OnLevelReady;
        _current_classroom.ClassroomFinished += _OnLevelFinished;

        return classroom_mgr;
    }


    public void SetLoadingScreen(LoadingScreenManager loading_screen)
    {
        _loading_screen = loading_screen;

        // Disable the screen.
        _loading_screen.Visible = false;

        // Hide the loading screen.
        _loading_screen.Position = new Vector2(
            _loading_screen.Position.X,
            Mathf.Min(
                _loading_screen.Position.Y,
                -Mathf.Max(
                    _loading_screen.GetChild<Control>(0).Size.Y,
                    GetViewport().GetVisibleRect().Size.Y
                )
        ));

        _ls_original_y = _loading_screen.Position.Y;
    }


    protected void _OnLevelReady()
    {

    }
    protected async void _OnLevelFinished(bool try_again)
    {
        if (try_again) await _RequestTryAgain();
        else await _RequestedBackToMenu();
    }


    protected async Task _RequestTryAgain()
    {
        // 1. Change to loading screen.
        await _EaseInLoadingScreen();

        // 2. Release current classroom.
        _current_classroom.ReleaseClassroom();
        GameManager.Instance.GameRoot.RemoveChild(_current_classroom);
        _current_classroom.QueueFree();
        _current_classroom = null;

        // 3. Load a new classroom using previous info.
        var classroom_mgr = await _StartClassroom(_previous_sli);
        await GameManager.Instance.AsyncForceGC();
        GameManager.Instance.AddChild(classroom_mgr);

        // 4. Remove the loading screen.
        await _EaseOutLoadingScreen();

        // 5. Notify the game start.
        await classroom_mgr.StartGame();
    }


    protected async Task _RequestedBackToMenu()
    {
        // 1. Change to loading screen.
        await _EaseInLoadingScreen();

        InitLoadingProgress(0.0f, 1.0f);
        UpdateLoadingItem("Releasing classroom resources");

        // 2. Release Classroom
        _current_classroom.ReleaseClassroom();
        UpdateLoadingProgress(0.5f);

        // 3. Delete Classroom
        _current_classroom.QueueFree();
        _current_classroom = null;
        UpdateLoadingProgress(1.0f);

        // 4. Notify to bring up start menu
        GameManager.Instance.NotifyLevelEnded();
        await GameManager.Instance.AsyncForceGC();

        // 5. Hide the loading screen.
        await _EaseOutLoadingScreen();
    }


    protected async Task _EaseInLoadingScreen()
    {
        _loading_screen.Visible = true;
        _ls_tween = CreateTween();
        _ls_tween.TweenMethod(
            Callable.From<float>(rel_pos => _ScreenSlide(rel_pos)),
            _ls_original_y,
            0.0f,
            LoadingSceneEaseDuration
        ).SetTrans(LoadingSceneTransitionType).SetEase(LoadingSceneEaseType);
        await ToSignal(_ls_tween, Tween.SignalName.Finished);
    }


    protected async Task _EaseOutLoadingScreen()
    {
        _loading_screen.EndLoadingScreen();
        _ls_tween = CreateTween();
        _ls_tween.TweenMethod(
            Callable.From<float>(rel_pos => _ScreenSlide(rel_pos)),
            0.0f,
            _ls_original_y,
            LoadingSceneEaseDuration
        ).SetTrans(LoadingSceneTransitionType).SetEase(LoadingSceneEaseType);
        await ToSignal(_ls_tween, Tween.SignalName.Finished);
        _loading_screen.Visible = false;
        _ls_tween = null;
    }


    protected void _ScreenSlide(float rel_pos)
    {
        _loading_screen.Position = new Vector2(_loading_screen.Position.X, rel_pos);
    }



    /// <summary>
    /// This function shall be called by ClassroomManager, in LoadClassroom method.
    /// </summary>
    /// <param name="min_value">Min value of the loading progress</param>
    /// <param name="max_value">Max value of the loading progress</param>
    public void InitLoadingProgress(float min_value, float max_value)
    {
        _loading_screen.StartLoadingScreen(min_value, max_value);
    }

    /// <summary>
    /// This function shall be called by ClassroomManager, in LoadClassroom method.
    /// </summary>
    /// <param name="current_value">Current value of the loading progress.</param>
    public void UpdateLoadingProgress(float current_value)
    {
        _loading_screen.LoadingProgressBar.Value = current_value;
    }

    /// <summary>
    /// This function shall be called by ClassroomManager, in LoadClassroom method.
    /// </summary>
    /// <param name="item">The current loading item (or class) name.</param>
    public void UpdateLoadingItem(string item)
    {
        _loading_screen.LoadingItemLabel.Text = item;
    }


    /// <summary>
    /// Roll the classmate outfits.
    /// </summary>
    /// <returns>The classmate textures in tuple: (posture/body, hair, outfit)</returns>
    protected List<List<Tuple<Texture2D, Texture2D, Texture2D>>> _RollClassmateTextures(ClassmateData data, Vector2I matrix_size, float sex_ratio = 0.5f)
    {
        List<List<Tuple<Texture2D, Texture2D, Texture2D>>> result = new();
        var random = new Random((int)Time.GetUnixTimeFromSystem());

        for (int i = 0; i < matrix_size.X; i++)
        {
            List<Tuple<Texture2D, Texture2D, Texture2D>> row = new();
            for (int j = 0; j < matrix_size.Y; j++)
            {
                var sex = random.NextSingle();

                // Boy
                if (sex < sex_ratio)
                {
                    var tp = data.BoyPostures[random.Next(0, data.BoyPostures.Count - 1)];
                    var th = data.BoyHairs[random.Next(0, data.BoyHairs.Count - 1)];
                    var to = data.BoyOutfits[random.Next(0, data.BoyOutfits.Count - 1)];
                    row.Add(new Tuple<Texture2D, Texture2D, Texture2D>(tp, th, to));
                }
                // Girl
                else
                {
                    var gp = data.GirlPostures[random.Next(0, data.GirlPostures.Count - 1)];
                    var gh = data.GirlHairs[random.Next(0, data.GirlHairs.Count - 1)];
                    var go = data.GirlOutfits[random.Next(0, data.GirlOutfits.Count - 1)];
                    row.Add(new Tuple<Texture2D, Texture2D, Texture2D>(gp, gh, go));
                }
            }
            result.Add(row);
        }

        return result;
    }

    // (Answer, Emotion, OQA, Correctness)
    protected async Task<Dictionary<SeatController.SeatType, List<Tuple<Texture2D, Texture2D, Texture2D, bool>>>> _RollClassmateAnswers(
        List<QuestionData> q_data,
        int difficulty,
        Dictionary<int, float> wrong_ratio_map,
        Func<int, Dictionary<SeatController.SeatType, List<Tuple<Texture2D, Texture2D, Texture2D, bool>>>, Task> overrides
    )
    {
        Dictionary<SeatController.SeatType, List<Tuple<Texture2D, Texture2D, Texture2D, bool>>> result = new();
        SeatController.SeatType[] replyables = 
        {
            SeatController.SeatType.Left,
            SeatController.SeatType.Right,
            SeatController.SeatType.Front
        };
        var random = new Random((int)Time.GetUnixTimeFromSystem());
        float wrong_ratio;
        if (!wrong_ratio_map.TryGetValue(difficulty, out wrong_ratio))
        {
            GD.PrintErr("Invalid wrong_ratio_map, quit game");
            GameManager.Instance.QuitGame();
        }

        foreach (var type in replyables)
        {
            List<Tuple<Texture2D, Texture2D, Texture2D, bool>> replies = new();
            for (int i = 0; i < q_data.Count; i++)
            {
                bool wrong = random.NextSingle() < wrong_ratio;
                bool hesitate = false;

                if (wrong)
                {
                    
                }
                else
                {
                    
                }
            }
            result.Add(type, replies);
        }

        await overrides(difficulty, result);
        return result;
    }


    protected int _composed_count = 0;
    protected async Task<Texture2D> _ComposeHesitatedAnswer(List<Texture2D> answer_textures)
    {
        int max_height = 0;
        int width = 0;
        List<Image> answer_images = new();
        for (int i = 0; i < answer_textures.Count; i++)
        {
            var image = answer_textures[i].GetImage();
            answer_images.Add(image);
            var size = image.GetSize();
            if (max_height < size.Y) max_height = size.Y;
            width += size.X;

            _composed_count++;
            if (_composed_count % GameManager.Instance.BatchTPPF == 0)
            {
                await ToSignal(GameManager.Instance.GameRoot.GetTree(), SceneTree.SignalName.ProcessFrame);
            }
        }

        int cum_width = 0;
        Image result = Image.CreateEmpty(width, max_height, false, answer_images[0].GetFormat());
        for (int i = 0; i < answer_images.Count; i++)
        {
            var isize = answer_images[i].GetSize();
            result.BlitRect(
                answer_images[i],
                new Rect2I(0, 0, isize),
                new Vector2I(cum_width, (max_height - isize.Y) / 2)
            );
        }
        return ImageTexture.CreateFromImage(result);
    }
}
