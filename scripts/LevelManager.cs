using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZLinq;


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

    protected Random random;

    public float NextSingle
    {
        get { return random.NextSingle(); }
    }

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            random = new Random((int)Time.GetUnixTimeFromSystem());
        }
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
        _previous_sli = info;

        // 2. Load classroom.
        var classroom_mgr = await _StartClassroom(info);
        GD.Print($"Game Root: {GameManager.Instance.GameRoot.GetPath()}");
        GameManager.Instance.GameRoot.AddChild(classroom_mgr);

        // 3. Remove the loading screen.
        GameManager.Instance.NotifyLevelStarted();
        await _EaseOutLoadingScreen();

        // 4. Start game
        await classroom_mgr.StartGame();
    }


    protected async Task<ClassroomManager> _StartClassroom(StartLevelInfo info)
    {
        // 1. Roll level info by start level info.
        ClassroomStockItem rolled_classroom_stock_item = StockClassrooms.GetRandomOne(info.ExcludedTags);

        // Teacher
        var teacher_base = rolled_classroom_stock_item.Teacher.Instantiate<TeacherController>();

        // Questions
        var qc_scene = rolled_classroom_stock_item.QuestionLayout;
        var q_total_count = 5;
        var q_hide_count = 2;
        var q_rolled_data = _RollQuestions(
            rolled_classroom_stock_item.Questions.Data,
            q_total_count,
            GameManager.Instance.Settings.MultiChoiceWeight,
            GameManager.Instance.Settings.TrueFalseWeight,
            GameManager.Instance.Settings.FillInBlankWeight
        );
        var q_cwa = rolled_classroom_stock_item.Questions.CommonWrongAnswers
            .AsValueEnumerable().ToList();
        var q_strikes = rolled_classroom_stock_item.Questions.Strikes
            .AsValueEnumerable().ToList();
        var q_scores = q_rolled_data.AsValueEnumerable().Select(q => (float)q.Score).ToList();
        var qc_setup_info = q_rolled_data.AsValueEnumerable().Select(
            (d) => new Tuple<Texture2D, List<Texture2D>, Vector2, QuestionType, bool>(
                d.QuestionTexture,
                d.AllTextures().AsValueEnumerable().Select((bundle) => bundle.AnswerTexture).Shuffle().ToList(),
                d.OnQuestionAnswerPosition,
                d.Type,
                d.VerticalLayout
            )
        ).ToList();

        // Classmates
        var matrix_info = rolled_classroom_stock_item.Matrices.PickRandom();
        var player_coord = matrix_info.AllowedPlayerCoordinates.PickRandom();
        var clsrm_scene = rolled_classroom_stock_item.SeatSuite;
        var clsmt_data = rolled_classroom_stock_item.Classmates;
        var clsmt_textures = _RollClassmateTextures(
            clsmt_data, 
            matrix_info.MatrixSize,
            player_coord
        );
        var clsmt_emotions = clsmt_data.ClassmateEmotions.EmotionTextures;
        
        var clsmt_answers = await _RollClassmateAnswers(
            q_rolled_data,
            q_cwa, 
            q_strikes, 
            clsmt_emotions,
            info.Difficulty,
            info.MaxDifficulty, 
            info.WrongRatio,
            info.ConfusedRatio,
            info.HesitateRatio, 
            null
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
            player_coord,

            // Teacher
            teacher_base:           teacher_base,
            matrix_margin_topdown:  matrix_info.MatrixMarginTopdown,

            // Paper
            question_controller_scene: qc_scene,
            qc_setup_info:          qc_setup_info,

            // Classmates
            classmate_textures:     clsmt_textures,
            classmate_answers:      clsmt_answers,
            total_question_counts:  q_total_count,
            hide_question_counts:   q_hide_count,
            question_points:        q_scores
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
        // Nothing todo yet.
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
        GameManager.Instance.GameRoot.AddChild(classroom_mgr);

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


    protected List<QuestionData> _RollQuestions(
        Godot.Collections.Array<QuestionData> data,
        int total_count, 
        int multichoice_weight = 5, 
        int truefalse_weight = 5,
        int fillinblank_weight = 5,
        List<string> excluded_tags = null
    )
    {
        List<QuestionData> result = new();

        int total_weight = multichoice_weight + truefalse_weight + fillinblank_weight;
        float mc_chance = (float)multichoice_weight / (float)total_weight;
        float tf_chance = (float)truefalse_weight / (float)total_weight;

        List<QuestionData> q_mc = new();
        List<QuestionData> q_tf = new();
        List<QuestionData> q_fb = new();

        var d = data.AsValueEnumerable().Where(
            q => 
            excluded_tags == null ? true : 
            q.Tags.AsValueEnumerable().Where(t => excluded_tags.Contains(t)).Count() == 0
        );

        foreach (var q in d)
        {
            if (q != null && q.IsValid())
            {
                if (q.Type == QuestionType.MultiChoice) q_mc.Add(q);
                else if (q.Type == QuestionType.TrueFalse) q_tf.Add(q);
                else if (q.Type == QuestionType.FillInBlank) q_fb.Add(q);
                else
                {
                    GD.PrintErr($"Question data invalid type: {q.Type}");
                }
            }
        }
        GD.Print($"Size: Total: {data.Count}, MC: {q_mc.Count}, TF: {q_tf.Count}, FB: {q_fb.Count}");
        q_mc = q_mc.AsValueEnumerable().Shuffle().ToList();
        q_tf = q_tf.AsValueEnumerable().Shuffle().ToList();
        q_fb = q_fb.AsValueEnumerable().Shuffle().ToList();
        if (q_fb.Count == 0 && fillinblank_weight > 0)
        {
            GD.PrintErr($"Did not configured fill-in-blank questions, but weight > 0.");
            GameManager.Instance.QuitGame();
        }
        
        int mc_count = 0, tf_count = 0, fb_count = 0;
        for (int i = 0; i < total_count; i++)
        {
            var c = NextSingle;

            // MultiChoices
            if (c < mc_chance)
            {
                GD.Print("Selected MC");
                if (mc_count >= q_mc.Count)
                {
                    GD.PrintErr($"Warning: MultiChoice questions will have duplicates");
                    mc_count = 0;
                }
                result.Add(q_mc[mc_count++]);
            }
            // TrueFalse
            else if (c >= mc_chance && c < (tf_chance + mc_chance))
            {
                GD.Print("Selected TF");
                if (tf_count >= q_tf.Count)
                {
                    GD.PrintErr($"Warning: TrueFalse questions will have duplicates");
                    tf_count = 0;
                }
                result.Add(q_tf[tf_count++]);
            }
            // Fill In Blank
            else
            {
                GD.Print("Selected FB");
                if (fb_count >= q_fb.Count)
                {
                    GD.PrintErr($"Warning: TrueFalse questions will have duplicates");
                    fb_count = 0;
                }
                result.Add(q_fb[fb_count++]);
            }
        }

        result.Sort((qd1, qd2) => { return ((int)qd1.Type) < ((int)qd2.Type) ? -1 : (((int)qd1.Type) == ((int)qd2.Type) ? 0 : 1); });

        return result;
    }


    /// <summary>
    /// Roll the classmate outfits.
    /// </summary>
    /// <returns>The classmate textures in tuple: (posture/body, hair, outfit)</returns>
    protected List<List<Tuple<Texture2D, Texture2D, Texture2D>>> 
    _RollClassmateTextures(ClassmateData data, Vector2I matrix_size, Vector2 player_coord, float sex_ratio = 0.5f)
    {
        List<List<Tuple<Texture2D, Texture2D, Texture2D>>> result = new();

        List<ClassmateCompleteOutfitData> complete_outfits = new();
        for (int i = 0; i < data.AllowCompleteOutfitsDuplicate; i++)
        {
            complete_outfits.AddRange(data.CompleteOutfits);
        }
        complete_outfits = complete_outfits.AsValueEnumerable().Shuffle().ToList();

        float co_ratio = (float)(complete_outfits.Count) / (float)(matrix_size.X * matrix_size.Y);


        for (int i = 0; i < matrix_size.X; i++)
        {
            List<Tuple<Texture2D, Texture2D, Texture2D>> row = new();
            for (int j = 0; j < matrix_size.Y; j++)
            {
                // Complete outfit
                if (NextSingle < co_ratio && 
                    complete_outfits.Count > 0 && 
                    player_coord.X != i && 
                    player_coord.Y != j)
                {
                    var outfit_data = complete_outfits[0];
                    row.Add(new Tuple<Texture2D, Texture2D, Texture2D>(outfit_data.Body, outfit_data.Hair, outfit_data.Outfit));
                    complete_outfits.RemoveAt(0);
                    continue;
                }

                // Boy
                if (NextSingle < sex_ratio)
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


    /// <summary>
    /// Roll classmate answers, return in: (Answer, Emotion, OQA, OQA Position, Correctness)
    /// </summary>
    /// <param name="q_data"></param>
    /// <param name="difficulty"></param>
    /// <param name="wrong_ratio_map"></param>
    /// <param name="wrong_confused_ratio_map"></param>
    /// <param name="hesitate_ratio_map"></param>
    /// <param name="overrides"></param>
    /// <returns></returns>
    protected async Task<Dictionary<SeatController.SeatType, List<Tuple<Texture2D, Texture2D, Texture2D, Vector2, bool>>>> 
    _RollClassmateAnswers(
        List<QuestionData> q_data,
        List<AnswerTextureBundle> common_wrong_answers,
        List<Texture2D> strikes, 
        Godot.Collections.Dictionary<Emotions.EmotionType, Texture2D> emotions,
        int difficulty,
        int max_difficulty, 
        float wrong_ratio,
        float confused_ratio, 
        float hesitate_ratio,
        Func<int, Dictionary<SeatController.SeatType, List<Tuple<Texture2D, Texture2D, Texture2D, Vector2, bool>>>, Task> overrides
    )
    {
        Dictionary<SeatController.SeatType, List<Tuple<Texture2D, Texture2D, Texture2D, Vector2, bool>>> result = new();
        SeatController.SeatType[] replyables = 
        {
            SeatController.SeatType.Left,
            SeatController.SeatType.Right,
            SeatController.SeatType.Front
        };


        foreach (var type in replyables)
        {
            List<Tuple<Texture2D, Texture2D, Texture2D, Vector2, bool>> replies = new();
            for (int i = 0; i < q_data.Count; i++)
            {
                bool wrong = NextSingle < wrong_ratio;
                bool confused = NextSingle < confused_ratio;
                bool hesitate = NextSingle < hesitate_ratio;

                Texture2D reply_emotion = _RollEmotionTexture(
                    emotions, 
                    difficulty, 
                    max_difficulty,
                    wrong, 
                    confused, 
                    hesitate,
                    confused_ratio,
                    hesitate_ratio
                );
                AnswerTextureBundle reply_answer;
                if (wrong)
                {
                    reply_answer = _RollWrongTextures(i, q_data, common_wrong_answers, confused);
                }
                else
                {
                    reply_answer = q_data[i].CorrectAnswer;
                }
                var reply_texture = reply_answer.OqaTexture;
                // var reply_scale = reply_answer.AnswerScale;
                var oqa_texture = reply_answer.OqaTexture;
                var oqa_scale = reply_answer.OqaScale;
                if (hesitate)
                {
                    var hesitate_images = await _RollHesitatedAnswerImages(
                        q_data,
                        common_wrong_answers,
                        strikes, 
                        i,
                        confused_ratio,
                        hesitate_ratio,
                        q_data[i].Type == QuestionType.MultiChoice ? 4 : q_data[i].Type == QuestionType.FillInBlank ? 1 : 4
                    );

                    reply_texture = _ComposeHesitatedAnswer(hesitate_images, reply_texture);
                }

                GD.Print($"Correctness of @{type.ToString()}#{i} is {!wrong}");
                replies.Add(
                    new Tuple<Texture2D, Texture2D, Texture2D, Vector2, bool>(
                        reply_texture, 
                        reply_emotion, 
                        oqa_texture,
                        oqa_scale,
                        !wrong
                ));
            }
            result.Add(type, replies);
        }

        // Do not use await overrides?.Invoke, this will cause NullReferenceException.
        if (overrides is not null)
            await overrides.Invoke(difficulty, result);
        return result;
    }


    protected AnswerTextureBundle
    _RollWrongTextures(
        int cur_index,
        List<QuestionData> questions,
        List<AnswerTextureBundle> common_wrong_answers,
        bool confused
    )
    {
        if (confused)
        {
            var all_t = questions.AsValueEnumerable()
                .Where((qd, idx) => idx != cur_index)
                .SelectMany(qd => qd.AllTextures())
                .Where(atb => atb.OqaTexture != questions[cur_index].CorrectAnswer.OqaTexture)
                .Concat(common_wrong_answers)
                .Shuffle()
                .ToList();
            return all_t[random.Next(0, all_t.Count)];
        }
        else
        {
            var wrong_options = questions[cur_index].WrongAnswerOptions;
            return wrong_options[random.Next(0, wrong_options.Count)];
        }
    }


    protected Texture2D
    _RollEmotionTexture(
        Godot.Collections.Dictionary<Emotions.EmotionType, Texture2D> emotions,
        int difficulty,
        int max_difficulty,
        bool wrong,
        bool confused,
        bool hesitate,
        float confuse_ratio,
        float hesitate_ratio
    )
    {
        if (difficulty == max_difficulty) return emotions[Emotions.EmotionType.IHaveAnIdea];

        if (wrong)
        {
            if (hesitate && NextSingle < confuse_ratio)
            {
                return emotions[Emotions.EmotionType.Sad];
            }
            else if (confused)
            {
                return emotions[Emotions.EmotionType.LaughCry];
            }
            else if (hesitate)
            {
                return emotions[Emotions.EmotionType.Thinking];
            }
            else
            {
                return emotions[Emotions.EmotionType.IHaveAnIdea];
            }
        }
        else
        {
            if (hesitate) return emotions[Emotions.EmotionType.Thinking];
            else if (!confused && NextSingle < hesitate_ratio) return emotions[Emotions.EmotionType.IHaveAnIdea];
        }

        return emotions[Emotions.EmotionType.Confidence];
    }

    protected int _img_count = 0;
    protected async Task<List<Image>> 
    _RollHesitatedAnswerImages(
        List<QuestionData> questions, 
        List<AnswerTextureBundle> common_wrong_answers, 
        List<Texture2D> strikes, 
        int cur_index, 
        float confused_ratio, 
        float hesitate_ratio,

        int max_count
    )
    {
        List<Image> result = new();
        List<AnswerTextureBundle> selected_answers = new();

        var total_answers = questions.AsValueEnumerable()
            .SelectMany(qd => qd.AllTextures())
            .Concat(common_wrong_answers)
            .Shuffle()
            .ToList();
        var cur_answers = questions[cur_index].AllTextures();

        do
        {
            if (NextSingle < confused_ratio)
            {

                selected_answers.Add(total_answers[random.Next(0, total_answers.Count)]);
            }
            else
            {
                selected_answers.Add(cur_answers[random.Next(0, cur_answers.Count)]);
            }
        }
        while (NextSingle < hesitate_ratio && result.Count < max_count);

        // 2. Cover with strikes
        for (int i = 0; i < selected_answers.Count; i++)
        {
            var t = selected_answers[i].OqaTexture;
            var timg = t.GetImage();
            var tsize = timg.GetSize();
            var img = Image.CreateEmpty(tsize.X, tsize.Y, false, timg.GetFormat());
            var strikeimg = strikes[random.Next(0, strikes.Count)].GetImage();
            _img_count += 1;

            img.BlitRect(timg, new Rect2I(0, 0, tsize), Vector2I.Zero);
            img.BlendRect(strikeimg, new Rect2I(0, 0, tsize), Vector2I.Zero);
            result.Add(img);

            if (_img_count % GameManager.Instance.BatchTPPF == 0) 
                await ToSignal(GameManager.Instance.GameRoot.GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        return result;
    }


    
    protected Texture2D
    _ComposeHesitatedAnswer(
        List<Image> hesitate_images,
        Texture2D final_texture
    )
    {
        List<Image> images = hesitate_images.AsValueEnumerable().Append(final_texture.GetImage()).ToList();
        GD.Print($"Compose: images length: {images.Count}");
        int max_height = 0;
        int width = 0;
        foreach (var img in images)
        {
            var isize = img.GetSize();
            max_height = Math.Max(max_height, isize.Y);
            width += isize.X;
        }

        int cum_width = 0;
        Image result = Image.CreateEmpty(width, max_height, false, images[0].GetFormat());
        GD.Print($"Compose: Result size: {width}x{max_height}");
        for (int i = 0; i < images.Count; i++)
        {
            var isize = images[i].GetSize();
            result.BlitRect(
                images[i],
                new Rect2I(0, 0, isize),
                new Vector2I(cum_width, random.Next(0, (max_height - isize.Y)))
            );
            cum_width += isize.X;
        }
        return ImageTexture.CreateFromImage(result);
    }
}
