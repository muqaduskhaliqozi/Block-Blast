#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using BlockBlast.UI;
using BlockBlast.FX;
using BlockBlast.Board;
using BlockBlast.Blocks;
using Object = UnityEngine.Object;

namespace BlockBlast.EditorTools
{
    /// <summary>
    /// Builds the complete polished UI for the Splash, Main Menu and Gameplay scenes.
    /// Menu: Block Blast > Build Polished UI (All Scenes). Safe to run again at any time:
    /// it removes the UI it built before and rebuilds it (game objects/managers are kept).
    /// </summary>
    public static class BlockBlastUIBuilder
    {
        private const string UIRoot = "Assets/UI";
        private const string SpriteDir = UIRoot + "/Sprites/";
        private const string BgDir = UIRoot + "/Backgrounds/";
        private const string FontDir = UIRoot + "/Fonts/";

        private const string SplashScenePath = "Assets/Scenes/SplashScene.unity";
        private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string GameScenePath = "Assets/Scenes/BlockBlastGameplay.unity";

        private static readonly Color Navy = new Color32(0x0B, 0x10, 0x33, 0xFF);
        private static readonly Color SoftBlue = new Color32(0xA9, 0xBC, 0xFF, 0xFF);
        private static readonly Color Gold = new Color32(0xFF, 0xD1, 0x66, 0xFF);

        private static TMP_FontAsset font;
        private static Material matOutline;
        private static Material matShadow;

        // ------------------------------------------------------------------
        // Menu items
        // ------------------------------------------------------------------

        [MenuItem("Block Blast/Build Polished UI (All Scenes)", priority = 0)]
        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string previous = SceneManager.GetActiveScene().path;

            PrepareAssets();
            BuildSplash();
            BuildMenu();
            BuildGameplay();

            if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
            Debug.Log("[BlockBlastUIBuilder] Polished UI built for Splash, Main Menu and Gameplay scenes.");
        }

        [MenuItem("Block Blast/Rebuild UI/Splash Scene")]
        public static void MenuSplash() { if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return; PrepareAssets(); BuildSplash(); }

        [MenuItem("Block Blast/Rebuild UI/Main Menu Scene")]
        public static void MenuMenu() { if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return; PrepareAssets(); BuildMenu(); }

        [MenuItem("Block Blast/Rebuild UI/Gameplay Scene")]
        public static void MenuGame() { if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return; PrepareAssets(); BuildGameplay(); }

        // ------------------------------------------------------------------
        // Assets: sprite import settings, font asset, text materials
        // ------------------------------------------------------------------

        private static void PrepareAssets()
        {
            AssetDatabase.Refresh();

            // 9-slice borders are (left, bottom, right, top) in pixels
            foreach (string c in new[] { "green", "orange", "blue", "purple", "red" })
                ConfigureSprite(SpriteDir + "btn_" + c + ".png", new Vector4(80, 92, 80, 74));
            foreach (string c in new[] { "green", "orange", "blue", "purple" })
                ConfigureSprite(SpriteDir + "rbtn_" + c + ".png", Vector4.zero);
            ConfigureSprite(SpriteDir + "panel.png", new Vector4(92, 92, 92, 92));
            foreach (string p in new[] { "pill_white", "pill_dark", "pill_flat", "bar_fill" })
                ConfigureSprite(SpriteDir + p + ".png", new Vector4(39, 39, 39, 39));
            ConfigureSprite(SpriteDir + "ribbon.png", new Vector4(160, 0, 160, 0));
            foreach (string i in new[] { "ic_pause", "ic_play", "ic_home", "ic_restart", "ic_sound_on", "ic_sound_off", "ic_next", "ic_crown", "ic_star", "ic_star_empty", "fx_glow", "fx_sparkle", "fx_tile" })
                ConfigureSprite(SpriteDir + i + ".png", Vector4.zero);
            ConfigureSprite(BgDir + "bg_game.png", Vector4.zero, true);
            ConfigureSprite(BgDir + "bg_menu.png", Vector4.zero, true);
            ConfigureSprite(BgDir + "logo.png", Vector4.zero);

            // App icon (only if the project has no default icon yet)
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(BgDir + "app_icon.png");
            Texture2D[] current = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
            if (icon != null && (current == null || current.Length == 0 || current[0] == null))
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { icon });

            EnsureFont();
        }

        private static void ConfigureSprite(string path, Vector4 border, bool big = false)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null)
            {
                Debug.LogWarning("[BlockBlastUIBuilder] Missing texture: " + path);
                return;
            }

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Bilinear;
            ti.spriteBorder = border;
            ti.maxTextureSize = 2048;
            ti.textureCompression = big ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;

            TextureImporterSettings s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect;
            s.spriteAlignment = (int)SpriteAlignment.Center;
            ti.SetTextureSettings(s);
            ti.SaveAndReimport();
        }

        private static Sprite S(string name)
        {
            string path = (name.StartsWith("bg_") || name == "logo" ? BgDir : SpriteDir) + name + ".png";
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp == null) Debug.LogWarning("[BlockBlastUIBuilder] Sprite not found: " + path);
            return sp;
        }

        private static void EnsureFont()
        {
            string path = FontDir + "LilitaOne SDF.asset";
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null)
            {
                Font ttf = AssetDatabase.LoadAssetAtPath<Font>(FontDir + "LilitaOne-Regular.ttf");
                if (ttf == null)
                {
                    Debug.LogError("[BlockBlastUIBuilder] LilitaOne-Regular.ttf missing, falling back to TMP default font.");
                    font = TMP_Settings.defaultFontAsset;
                }
                else
                {
                    font = TMP_FontAsset.CreateFontAsset(ttf, 90, 12, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
                    font.name = "LilitaOne SDF";
                    AssetDatabase.CreateAsset(font, path);
                    font.atlasTextures[0].name = "LilitaOne SDF Atlas";
                    AssetDatabase.AddObjectToAsset(font.atlasTextures[0], font);
                    font.material.name = "LilitaOne SDF Material";
                    AssetDatabase.AddObjectToAsset(font.material, font);
                    font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 !?.,:;%+-/x'\"()&#");
                    EditorUtility.SetDirty(font);
                    AssetDatabase.SaveAssets();
                }
            }

            matOutline = EnsureMaterial("LilitaOne Outline", m =>
            {
                m.EnableKeyword("OUTLINE_ON");
                m.SetFloat("_FaceDilate", 0.15f);
                m.SetFloat("_OutlineWidth", 0.25f);
                m.SetColor("_OutlineColor", Navy);
                m.EnableKeyword("UNDERLAY_ON");
                m.SetColor("_UnderlayColor", new Color(0f, 0f, 0.08f, 0.6f));
                m.SetFloat("_UnderlayOffsetX", 0f);
                m.SetFloat("_UnderlayOffsetY", -0.9f);
                m.SetFloat("_UnderlayDilate", 0.3f);
                m.SetFloat("_UnderlaySoftness", 0.15f);
            });

            matShadow = EnsureMaterial("LilitaOne Shadow", m =>
            {
                m.EnableKeyword("UNDERLAY_ON");
                m.SetColor("_UnderlayColor", new Color(0f, 0f, 0.1f, 0.55f));
                m.SetFloat("_UnderlayOffsetX", 0f);
                m.SetFloat("_UnderlayOffsetY", -0.7f);
                m.SetFloat("_UnderlayDilate", 0.1f);
                m.SetFloat("_UnderlaySoftness", 0.35f);
            });
        }

        private static Material EnsureMaterial(string name, Action<Material> setup)
        {
            string path = FontDir + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(font.material) { name = name };
                AssetDatabase.CreateAsset(m, path);
            }
            else
            {
                m.shader = font.material.shader;
                m.SetTexture("_MainTex", font.material.GetTexture("_MainTex"));
            }
            setup(m);
            EditorUtility.SetDirty(m);
            return m;
        }

        // ------------------------------------------------------------------
        // SPLASH
        // ------------------------------------------------------------------

        private static void BuildSplash()
        {
            Scene scene = EditorSceneManager.OpenScene(SplashScenePath, OpenSceneMode.Single);
            RemoveRoots(scene, go => go.GetComponent<Canvas>() != null || go.GetComponent<SplashLoader>() != null);
            SetupCamera();

            Canvas canvas = MakeCanvas("SplashCanvas", 0);
            Transform root = canvas.transform;

            Background(root, S("bg_menu"));
            Floaters(root, 14, 0.28f);
            Transform safe = SafeArea(root);

            Glow(safe, new Vector2(0.5f, 0.6f), new Vector2(0f, 0f), 1200f, new Color(0.62f, 0.4f, 1f, 0.45f));
            Logo(safe, new Vector2(0.5f, 0.6f), new Vector2(980f, 630f), 0.15f);

            // Loading area
            GameObject loading = NewUI("Loading", safe);
            Anchor(loading, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 330f), new Vector2(820f, 300f));
            Entrance(loading, new Vector2(0f, -120f), 1f, 0f, 0.5f, 0.5f, UIEntrance.Curve.OutCubic);

            TextMeshProUGUI status = Text(loading.transform, "StatusText", "LOADING", 54f, Color.white, matOutline,
                                          new Vector2(0f, 95f), new Vector2(700f, 80f));

            Slider slider = ProgressSlider(loading.transform, new Vector2(760f, 74f), out TextMeshProUGUI percent);
            TextMeshProUGUI tip = Text(loading.transform, "TipText", "Tip", 36f, SoftBlue, matShadow,
                                       new Vector2(0f, -100f), new Vector2(900f, 100f));
            tip.enableWordWrapping = true;

            GameObject ctrl = NewUI("SplashController", root);
            SplashLoader loader = ctrl.AddComponent<SplashLoader>();
            SetRef(loader, "loadingSlider", slider);
            SetRef(loader, "percentText", percent);
            SetRef(loader, "statusText", status);
            SetRef(loader, "tipText", tip);
            SetFloat(loader, "loadDuration", 4f);

            Save(scene);
        }

        // ------------------------------------------------------------------
        // MAIN MENU
        // ------------------------------------------------------------------

        private static void BuildMenu()
        {
            Scene scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
            RemoveRoots(scene, go => go.GetComponent<Canvas>() != null || go.GetComponent<MainMenuUI>() != null || go.name == "bg");
            SetupCamera();

            Canvas canvas = MakeCanvas("MenuCanvas", 0);
            Transform root = canvas.transform;

            Background(root, S("bg_menu"));
            Floaters(root, 16, 0.3f);
            Transform safe = SafeArea(root);

            Glow(safe, new Vector2(0.5f, 0.72f), Vector2.zero, 1250f, new Color(0.62f, 0.4f, 1f, 0.5f));
            Logo(safe, new Vector2(0.5f, 0.72f), new Vector2(960f, 618f), 0.05f);

            // Best score
            GameObject best = NewUI("BestScore", safe);
            Anchor(best, new Vector2(0.5f, 0.485f), new Vector2(0.5f, 0.485f), Vector2.zero, new Vector2(480f, 160f));
            Entrance(best, new Vector2(0f, -60f), 0.6f, 0f, 0.35f, 0.5f, UIEntrance.Curve.OutBack);
            Text(best.transform, "Label", "BEST SCORE", 40f, Gold, matShadow, new Vector2(0f, 52f), new Vector2(480f, 56f));
            GameObject bestPill = Pill(best.transform, "Pill", S("pill_dark"), new Vector2(0f, -20f), new Vector2(420f, 100f), Color.white);
            Icon(bestPill.transform, S("ic_crown"), new Vector2(-150f, 4f), 84f);
            TextMeshProUGUI bestValue = Text(bestPill.transform, "Value", "0", 62f, Color.white, matOutline, new Vector2(30f, 2f), new Vector2(300f, 90f));

            // Play button (entrance on outer group, pulse on the middle, button press on the inside)
            GameObject playGroup = NewUI("PlayGroup", safe);
            Anchor(playGroup, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(620f, 230f));
            Entrance(playGroup, new Vector2(0f, -200f), 0.4f, 0f, 0.5f, 0.6f, UIEntrance.Curve.OutBack);
            GameObject pulse = NewUI("Pulse", playGroup.transform);
            Stretch(pulse);
            pulse.AddComponent<UIIdleAnim>().Setup(UIIdleAnim.Mode.Pulse, 0.045f, 4f);
            Button play = MakeButton(pulse.transform, "PlayButton", S("btn_green"), new Vector2(600f, 220f), Vector2.zero,
                                     "PLAY", S("ic_play"), 100f);

            TextMeshProUGUI levelText = Text(safe, "LevelText", "LEVEL 1", 46f, SoftBlue, matShadow, Vector2.zero, new Vector2(500f, 70f));
            Anchor(levelText.gameObject, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0f, -165f), new Vector2(500f, 70f));
            Entrance(levelText.gameObject, new Vector2(0f, -40f), 1f, 0f, 0.75f, 0.4f, UIEntrance.Curve.OutCubic);

            // Sound toggle (top-right)
            Button sound = RoundButton(safe, "SoundButton", S("rbtn_blue"), S("ic_sound_on"), 140f);
            Anchor(sound.gameObject, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-100f, -100f), new Vector2(140f, 140f));
            Image soundIcon = sound.transform.Find("Icon").GetComponent<Image>();

            TextMeshProUGUI ver = Text(safe, "Version", "v" + Application.version, 30f, new Color(1f, 1f, 1f, 0.35f), null, Vector2.zero, new Vector2(300f, 50f));
            Anchor(ver.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(300f, 50f));

            GameObject ctrl = NewUI("MainMenuController", root);
            MainMenuUI menu = ctrl.AddComponent<MainMenuUI>();
            SetRef(menu, "playButton", play);
            SetRef(menu, "bestScoreText", bestValue);
            SetRef(menu, "levelText", levelText);
            SetRef(menu, "soundButton", sound);
            SetRef(menu, "soundIcon", soundIcon);
            SetRef(menu, "soundOnSprite", S("ic_sound_on"));
            SetRef(menu, "soundOffSprite", S("ic_sound_off"));

            Save(scene);
        }

        // ------------------------------------------------------------------
        // GAMEPLAY
        // ------------------------------------------------------------------

        private static void BuildGameplay()
        {
            Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            RemoveRoots(scene, go => go.name == "UI Canvas" || go.name == "Background Canvas" || go.GetComponent<GameFX>() != null);
            UnityEngine.Camera cam = SetupCamera();

            // ---- World layout: board higher up, tray lower, real board frame + shadow
            BoardManager board = Object.FindObjectOfType<BoardManager>();
            if (board != null)
            {
                board.transform.position = new Vector3(0f, 0.6f, 0f);
                EnsureWorldSprite(board.transform, "BoardShadow", -21);
                EnsureWorldSprite(board.transform, "BoardFrame", -20);
                EditorUtility.SetDirty(board);
            }

            BlockTray tray = Object.FindObjectOfType<BlockTray>();
            if (tray != null)
            {
                tray.transform.position = new Vector3(0f, -5.25f, 0f);
                SetFloat(tray, "slotSpacing", 2.95f);
                SetFloat(tray, "slotWidth", 2.75f);
                SetFloat(tray, "slotHeight", 2.45f);
            }

            // ---- Background canvas (rendered by the camera, behind every sprite)
            GameObject bgGo = new GameObject("Background Canvas", typeof(RectTransform));
            bgGo.layer = 5;
            Canvas bgCanvas = bgGo.AddComponent<Canvas>();
            bgCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            bgCanvas.worldCamera = cam;
            bgCanvas.planeDistance = 50f;
            bgCanvas.sortingOrder = -100;
            CanvasScaler bgScaler = bgGo.AddComponent<CanvasScaler>();
            bgScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            bgScaler.referenceResolution = new Vector2(1080f, 1920f);
            bgGo.AddComponent<AdaptiveCanvasScaler>();
            Background(bgGo.transform, S("bg_game"));
            Floaters(bgGo.transform, 10, 0.14f);

            // ---- Main UI canvas
            Canvas canvas = MakeCanvas("UI Canvas", 10);
            Transform root = canvas.transform;

            // Header (score + goal) – sized at runtime to the space above the board
            GameObject header = NewUI("Header", root);
            RectTransform hrt = (RectTransform)header.transform;
            hrt.anchorMin = new Vector2(0f, 1f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.pivot = new Vector2(0.5f, 1f);
            hrt.anchoredPosition = Vector2.zero;
            hrt.sizeDelta = new Vector2(0f, 400f);
            header.AddComponent<HUDFitter>();

            TextMeshProUGUI scoreText = Text(header.transform, "ScoreText", "0", 140f, Color.white, matOutline, Vector2.zero, new Vector2(800f, 160f));
            Anchor(scoreText.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 175f), new Vector2(800f, 160f));
            scoreText.enableVertexGradient = true;
            scoreText.colorGradient = new VertexGradient(Color.white, Color.white, new Color(0.78f, 0.86f, 1f), new Color(0.78f, 0.86f, 1f));

            // Goal row: level chip + progress bar
            GameObject goal = NewUI("GoalRow", header.transform);
            Anchor(goal, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(940f, 70f));
            GameObject chip = Pill(goal.transform, "LevelChip", S("pill_white"), new Vector2(-355f, 0f), new Vector2(230f, 70f), new Color32(0x8B, 0x4B, 0xF5, 0xFF));
            TextMeshProUGUI levelText = Text(chip.transform, "LevelText", "LEVEL 1", 36f, Color.white, matShadow, new Vector2(0f, 2f), new Vector2(220f, 64f));
            GameObject bar = Pill(goal.transform, "ProgressBar", S("pill_dark"), new Vector2(120f, 0f), new Vector2(690f, 62f), Color.white);
            GameObject fill = NewUI("Fill", bar.transform);
            RectTransform frt = (RectTransform)fill.transform;
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = new Vector2(0.3f, 1f);
            frt.offsetMin = new Vector2(6f, 6f);
            frt.offsetMax = new Vector2(-6f, -6f);
            Img(fill, S("bar_fill"), Color.white, true, 80f / 50f);
            TextMeshProUGUI progressText = Text(bar.transform, "ProgressText", "LINES 0/0", 34f, Color.white, matOutline, new Vector2(0f, 1f), new Vector2(680f, 60f));
            ScoreProgressUI progress = bar.AddComponent<ScoreProgressUI>();
            SetRef(progress, "text", progressText);
            SetRef(progress, "barFill", frt);
            SetRef(progress, "levelText", levelText);

            // Top bar inside the safe area: pause | best | sound
            Transform safe = SafeArea(root);
            GameObject topBar = NewUI("TopBar", safe);
            RectTransform tbr = (RectTransform)topBar.transform;
            tbr.anchorMin = new Vector2(0f, 1f);
            tbr.anchorMax = new Vector2(1f, 1f);
            tbr.pivot = new Vector2(0.5f, 1f);
            tbr.anchoredPosition = new Vector2(0f, -18f);
            tbr.sizeDelta = new Vector2(0f, 130f);
            Entrance(topBar, new Vector2(0f, 180f), 1f, 0f, 0.1f, 0.5f, UIEntrance.Curve.OutBack);

            Button pause = RoundButton(topBar.transform, "PauseButton", S("rbtn_purple"), S("ic_pause"), 124f);
            Anchor(pause.gameObject, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(95f, 0f), new Vector2(124f, 124f));
            Button sound = RoundButton(topBar.transform, "SoundButton", S("rbtn_blue"), S("ic_sound_on"), 124f);
            Anchor(sound.gameObject, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-95f, 0f), new Vector2(124f, 124f));

            GameObject bestPill = Pill(topBar.transform, "BestPill", S("pill_dark"), new Vector2(0f, 4f), new Vector2(330f, 90f), Color.white);
            Icon(bestPill.transform, S("ic_crown"), new Vector2(-115f, 4f), 74f);
            TextMeshProUGUI bestText = Text(bestPill.transform, "BestScoreText", "0", 52f, Gold, matOutline, new Vector2(30f, 2f), new Vector2(230f, 84f));

            ScoreUI scoreUI = header.AddComponent<ScoreUI>();
            SetRef(scoreUI, "scoreText", scoreText);
            SetRef(scoreUI, "bestScoreText", bestText);
            SetRef(scoreUI, "bestBadge", bestPill.transform);

            // Combo text in the middle of the board
            GameObject comboHolder = NewUI("ComboHolder", root);
            Anchor(comboHolder, new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.54f), Vector2.zero, new Vector2(1000f, 240f));
            GameObject comboGlow = NewUI("ComboGlow", comboHolder.transform);
            Anchor(comboGlow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 520f));
            Img(comboGlow, S("fx_glow"), new Color(1f, 0.85f, 0.4f, 0.8f));
            TextMeshProUGUI comboText = Text(comboHolder.transform, "ComboText", "GREAT!", 116f, Color.white, matOutline, Vector2.zero, new Vector2(1000f, 200f));
            ComboUI combo = comboHolder.AddComponent<ComboUI>();
            SetRef(combo, "comboText", comboText);
            SetRef(combo, "glow", comboGlow.transform);

            // Level intro banner
            GameObject intro = NewUI("LevelIntroPanel", root);
            RectTransform irt = (RectTransform)intro.transform;
            irt.anchorMin = new Vector2(0f, 0.54f);
            irt.anchorMax = new Vector2(1f, 0.54f);
            irt.sizeDelta = new Vector2(0f, 300f);
            irt.anchoredPosition = Vector2.zero;
            GameObject strip = NewUI("Strip", intro.transform);
            Stretch(strip);
            Img(strip, null, new Color(0.03f, 0.05f, 0.16f, 0.82f));
            GameObject lineTop = NewUI("LineTop", intro.transform);
            AnchorStretchX(lineTop, 1f, 0f, 8f);
            Img(lineTop, null, new Color(0.35f, 0.85f, 1f, 0.9f));
            GameObject lineBottom = NewUI("LineBottom", intro.transform);
            AnchorStretchX(lineBottom, 0f, 0f, 8f);
            Img(lineBottom, null, new Color(0.65f, 0.35f, 1f, 0.9f));
            TextMeshProUGUI introTitle = Text(intro.transform, "IntroTitle", "LEVEL 1", 130f, Color.white, matOutline, new Vector2(0f, 38f), new Vector2(1000f, 160f));
            introTitle.enableVertexGradient = true;
            introTitle.colorGradient = new VertexGradient(new Color(1f, 0.97f, 0.6f), new Color(1f, 0.97f, 0.6f), new Color(1f, 0.6f, 0.08f), new Color(1f, 0.6f, 0.08f));
            TextMeshProUGUI introDesc = Text(intro.transform, "IntroDescription", "Clear lines to win!", 46f, Color.white, matShadow, new Vector2(0f, -78f), new Vector2(1000f, 70f));

            // Score popups live above the board UI but below the popups
            GameObject popupLayer = NewUI("PopupLayer", root);
            Stretch(popupLayer);

            // ---- Popups
            Button retry, goHome, resume, restart, pauseHome, next, lcHome;
            TextMeshProUGUI finalScore, goBest, lcTitle, lcScore;
            GameObject newBest;

            GameObject gameOver = Popup(root, "GameOverPanel", "GAME OVER", 1040f, out RectTransform goBox, out List<RectTransform> goItems, out _);
            {
                TextMeshProUGUI lbl = Text(goBox, "ScoreLabel", "SCORE", 50f, SoftBlue, matShadow, new Vector2(0f, 300f), new Vector2(600f, 70f));
                finalScore = Text(goBox, "FinalScore", "0", 150f, Color.white, matOutline, new Vector2(0f, 185f), new Vector2(760f, 170f));
                finalScore.enableVertexGradient = true;
                finalScore.colorGradient = new VertexGradient(new Color(1f, 0.97f, 0.6f), new Color(1f, 0.97f, 0.6f), new Color(1f, 0.6f, 0.08f), new Color(1f, 0.6f, 0.08f));

                newBest = Pill(goBox, "NewBestBadge", S("pill_white"), new Vector2(0f, 75f), new Vector2(360f, 76f), new Color32(0xFF, 0x3D, 0x6E, 0xFF));
                Text(newBest.transform, "Text", "NEW BEST!", 44f, Color.white, matShadow, new Vector2(0f, 2f), new Vector2(340f, 70f));
                newBest.AddComponent<UIIdleAnim>().Setup(UIIdleAnim.Mode.Wobble, 4f, 6f);

                GameObject bestRow = Pill(goBox, "BestRow", S("pill_dark"), new Vector2(0f, -35f), new Vector2(440f, 92f), Color.white);
                Icon(bestRow.transform, S("ic_crown"), new Vector2(-160f, 4f), 74f);
                goBest = Text(bestRow.transform, "Best", "0", 54f, Gold, matOutline, new Vector2(30f, 2f), new Vector2(320f, 84f));

                retry = MakeButton(goBox, "RetryButton", S("btn_green"), new Vector2(580f, 180f), new Vector2(0f, -205f), "RETRY", S("ic_restart"), 74f);
                goHome = MakeButton(goBox, "HomeButton", S("btn_blue"), new Vector2(580f, 160f), new Vector2(0f, -385f), "HOME", S("ic_home"), 64f);
                goItems.AddRange(new[] { (RectTransform)lbl.transform, (RectTransform)finalScore.transform, (RectTransform)newBest.transform, (RectTransform)bestRow.transform, (RectTransform)retry.transform, (RectTransform)goHome.transform });
            }
            FinishPopup(gameOver, goBox, goItems);

            GameObject pausePanel = Popup(root, "GamePausePanel", "PAUSED", 800f, out RectTransform pBox, out List<RectTransform> pItems, out _);
            {
                resume = MakeButton(pBox, "ResumeButton", S("btn_green"), new Vector2(580f, 180f), new Vector2(0f, 150f), "RESUME", S("ic_play"), 74f);
                restart = MakeButton(pBox, "RestartButton", S("btn_orange"), new Vector2(580f, 170f), new Vector2(0f, -35f), "RESTART", S("ic_restart"), 68f);
                pauseHome = MakeButton(pBox, "HomeButton", S("btn_blue"), new Vector2(580f, 170f), new Vector2(0f, -215f), "HOME", S("ic_home"), 68f);
                pItems.AddRange(new[] { (RectTransform)resume.transform, (RectTransform)restart.transform, (RectTransform)pauseHome.transform });
            }
            FinishPopup(pausePanel, pBox, pItems);

            GameObject complete = Popup(root, "GameCompletePanel", "LEVEL CLEAR!", 1040f, out RectTransform cBox, out List<RectTransform> cItems, out lcTitle);
            {
                GameObject stars = NewUI("Stars", cBox);
                Anchor(stars, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 270f), new Vector2(700f, 240f));
                GameObject starGlow = NewUI("StarGlow", stars.transform);
                Anchor(starGlow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(760f, 520f));
                Img(starGlow, S("fx_glow"), new Color(1f, 0.82f, 0.35f, 0.55f));
                starGlow.AddComponent<UIIdleAnim>().Setup(UIIdleAnim.Mode.Pulse, 0.08f, 3f);
                RectTransform s1 = Star(stars.transform, new Vector2(-200f, -20f), 170f, -12f);
                RectTransform s2 = Star(stars.transform, new Vector2(0f, 20f), 220f, 0f);
                RectTransform s3 = Star(stars.transform, new Vector2(200f, -20f), 170f, 12f);
                stars.AddComponent<UIStarsReveal>().Setup(new[] { s1, s2, s3 });

                TextMeshProUGUI lbl = Text(cBox, "ScoreLabel", "SCORE", 50f, SoftBlue, matShadow, new Vector2(0f, 95f), new Vector2(600f, 70f));
                lcScore = Text(cBox, "Score", "0", 130f, Color.white, matOutline, new Vector2(0f, -5f), new Vector2(700f, 150f));
                lcScore.enableVertexGradient = true;
                lcScore.colorGradient = new VertexGradient(new Color(1f, 0.97f, 0.6f), new Color(1f, 0.97f, 0.6f), new Color(1f, 0.6f, 0.08f), new Color(1f, 0.6f, 0.08f));

                next = MakeButton(cBox, "NextButton", S("btn_green"), new Vector2(580f, 180f), new Vector2(0f, -205f), "NEXT", S("ic_next"), 74f);
                lcHome = MakeButton(cBox, "HomeButton", S("btn_blue"), new Vector2(580f, 160f), new Vector2(0f, -385f), "HOME", S("ic_home"), 64f);
                cItems.AddRange(new[] { (RectTransform)lbl.transform, (RectTransform)lcScore.transform, (RectTransform)next.transform, (RectTransform)lcHome.transform });

                GameObject confetti = NewUI("Confetti", complete.transform);
                Stretch(confetti);
                confetti.AddComponent<UIConfetti>().Setup(S("fx_tile"));
            }
            FinishPopup(complete, cBox, cItems);

            // ---- Wire the existing UI scripts
            GameUI gameUI = canvas.gameObject.AddComponent<GameUI>();
            SetRef(gameUI, "restartButton", restart);
            SetRef(gameUI, "soundButton", sound);
            SetRef(gameUI, "soundIconImage", sound.transform.Find("Icon").GetComponent<Image>());
            SetRef(gameUI, "soundOnSprite", S("ic_sound_on"));
            SetRef(gameUI, "soundOffSprite", S("ic_sound_off"));

            GameOverUI goUI = canvas.gameObject.AddComponent<GameOverUI>();
            SetRef(goUI, "gameOverPanel", gameOver);
            SetRef(goUI, "finalScoreText", finalScore);
            SetRef(goUI, "bestScoreText", goBest);
            SetRef(goUI, "retryButton", retry);
            SetRef(goUI, "homeButton", goHome);
            SetRef(goUI, "newBestBadge", newBest);

            LevelCompleteUI lcUI = canvas.gameObject.AddComponent<LevelCompleteUI>();
            SetRef(lcUI, "panel", complete);
            SetRef(lcUI, "titleText", lcTitle);
            SetRef(lcUI, "scoreText", lcScore);
            SetRef(lcUI, "nextButton", next);
            SetRef(lcUI, "homeButton", lcHome);

            LevelIntroUI introUI = canvas.gameObject.AddComponent<LevelIntroUI>();
            SetRef(introUI, "panel", intro);
            SetRef(introUI, "titleText", introTitle);
            SetRef(introUI, "descriptionText", introDesc);
            SetFloat(introUI, "showSeconds", 1.8f);
            SetFloat(introUI, "fadeSeconds", 0.4f);

            PauseUI pauseUI = canvas.gameObject.AddComponent<PauseUI>();
            SetRef(pauseUI, "pausePanel", pausePanel);
            SetRef(pauseUI, "pauseButton", pause);
            SetRef(pauseUI, "resumeButton", resume);
            SetRef(pauseUI, "homeButton", pauseHome);

            // ---- FX
            GameObject fxGo = new GameObject("GameFX");
            GameFX fx = fxGo.AddComponent<GameFX>();
            fx.Setup(S("fx_tile"), S("fx_sparkle"), S("fx_glow"), (RectTransform)popupLayer.transform, font, matOutline);

            // Inactive by default (scripts also hide them on Start)
            gameOver.SetActive(false);
            pausePanel.SetActive(false);
            complete.SetActive(false);

            Save(scene);
        }

        // ------------------------------------------------------------------
        // Building blocks
        // ------------------------------------------------------------------

        private static void RemoveRoots(Scene scene, Func<GameObject, bool> match)
        {
            foreach (GameObject go in scene.GetRootGameObjects())
                if (match(go)) Object.DestroyImmediate(go);
        }

        private static UnityEngine.Camera SetupCamera()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main != null ? UnityEngine.Camera.main : Object.FindObjectOfType<UnityEngine.Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.05f, 0.07f, 0.2f, 1f);
                cam.farClipPlane = Mathf.Max(cam.farClipPlane, 200f);
                EditorUtility.SetDirty(cam);
            }
            return cam;
        }

        private static void Save(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject NewUI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Anchor(GameObject go, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
        {
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static void Stretch(GameObject go)
        {
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static void AnchorStretchX(GameObject go, float y, float pos, float height)
        {
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, y);
            rt.anchorMax = new Vector2(1f, y);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, pos);
            rt.sizeDelta = new Vector2(0f, height);
        }

        private static Image Img(GameObject go, Sprite sprite, Color color, bool sliced = false, float ppuMultiplier = 1f, bool raycast = false)
        {
            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            if (sliced)
            {
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = ppuMultiplier;
            }
            else if (sprite != null)
            {
                img.preserveAspect = true;
            }
            return img;
        }

        private static TextMeshProUGUI Text(Transform parent, string name, string text, float size, Color color, Material mat, Vector2 pos, Vector2 box)
        {
            GameObject go = NewUI(name, parent);
            Anchor(go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, box);
            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            t.font = font;
            if (mat != null) t.fontSharedMaterial = mat;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static GameObject Pill(Transform parent, string name, Sprite sprite, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = NewUI(name, parent);
            Anchor(go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            Img(go, sprite, color, true, 80f / size.y);
            return go;
        }

        private static Image Icon(Transform parent, Sprite sprite, Vector2 pos, float size)
        {
            GameObject go = NewUI("Icon", parent);
            Anchor(go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(size, size));
            return Img(go, sprite, Color.white);
        }

        private static RectTransform Star(Transform parent, Vector2 pos, float size, float rot)
        {
            GameObject holder = NewUI("StarSlot", parent);
            Anchor(holder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(size, size));
            holder.transform.localRotation = Quaternion.Euler(0, 0, rot);
            Img(holder, S("ic_star_empty"), Color.white);
            GameObject star = NewUI("Star", holder.transform);
            Stretch(star);
            Img(star, S("ic_star"), Color.white);
            return (RectTransform)star.transform;
        }

        private static Button MakeButton(Transform parent, string name, Sprite bg, Vector2 size, Vector2 pos, string label, Sprite icon, float fontSize)
        {
            GameObject go = NewUI(name, parent);
            Anchor(go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            Image img = Img(go, bg, Color.white, true, 200f / size.y, true);
            Button b = go.AddComponent<Button>();
            b.targetGraphic = img;
            ColorBlock cb = b.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = Color.white;
            cb.selectedColor = Color.white;
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.fadeDuration = 0.05f;
            b.colors = cb;
            Navigation nav = b.navigation;
            nav.mode = Navigation.Mode.None;
            b.navigation = nav;
            go.AddComponent<UIButtonFX>();

            // the glossy body sits above the darker "lip", so centre content slightly higher
            float lift = size.y * 0.045f;
            float iconSize = size.y * 0.42f;
            if (icon != null && !string.IsNullOrEmpty(label))
            {
                float textWidth = fontSize * 0.55f * label.Length;
                float total = iconSize + 18f + textWidth;
                float startX = -total * 0.5f;
                Icon(go.transform, icon, new Vector2(startX + iconSize * 0.5f, lift + 2f), iconSize);
                Text(go.transform, "Label", label, fontSize, Color.white, matOutline,
                     new Vector2(startX + iconSize + 18f + textWidth * 0.5f, lift), new Vector2(textWidth + 40f, size.y));
            }
            else if (icon != null)
            {
                Icon(go.transform, icon, new Vector2(0f, lift), iconSize);
            }
            else
            {
                Text(go.transform, "Label", label, fontSize, Color.white, matOutline, new Vector2(0f, lift), size);
            }
            return b;
        }

        private static Button RoundButton(Transform parent, string name, Sprite bg, Sprite icon, float size)
        {
            GameObject go = NewUI(name, parent);
            Anchor(go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
            Image img = Img(go, bg, Color.white, false, 1f, true);
            Button b = go.AddComponent<Button>();
            b.targetGraphic = img;
            ColorBlock cb = b.colors;
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.selectedColor = Color.white;
            b.colors = cb;
            Navigation nav = b.navigation;
            nav.mode = Navigation.Mode.None;
            b.navigation = nav;
            go.AddComponent<UIButtonFX>();
            Icon(go.transform, icon, new Vector2(0f, size * 0.04f), size * 0.5f);
            return b;
        }

        private static Canvas MakeCanvas(string name, int order)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            Canvas c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = order;
            CanvasScaler cs = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080f, 1920f);
            cs.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            go.AddComponent<AdaptiveCanvasScaler>();
            return c;
        }

        private static void Background(Transform parent, Sprite sprite)
        {
            GameObject bg = NewUI("Background", parent);
            Anchor(bg, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1080f, 1920f));
            Image img = Img(bg, sprite, Color.white);
            img.preserveAspect = false;
            AspectRatioFitter fit = bg.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = 1080f / 1920f;
        }

        private static void Floaters(Transform parent, int count, float alphaMax)
        {
            GameObject f = NewUI("Floaters", parent);
            Stretch(f);
            f.AddComponent<BackgroundFloaters>().Setup(S("fx_tile"), count, alphaMax);
        }

        private static Transform SafeArea(Transform parent)
        {
            GameObject s = NewUI("SafeArea", parent);
            Stretch(s);
            s.AddComponent<SafeAreaFitter>();
            return s.transform;
        }

        private static void Glow(Transform parent, Vector2 anchor, Vector2 pos, float size, Color color)
        {
            GameObject g = NewUI("Glow", parent);
            Anchor(g, anchor, anchor, pos, new Vector2(size, size));
            Img(g, S("fx_glow"), color);
            g.AddComponent<UIIdleAnim>().Setup(UIIdleAnim.Mode.Pulse, 0.07f, 1.6f);
        }

        private static void Logo(Transform parent, Vector2 anchor, Vector2 size, float delay)
        {
            GameObject holder = NewUI("LogoHolder", parent);
            Anchor(holder, anchor, anchor, Vector2.zero, size);
            Entrance(holder, new Vector2(0f, 500f), 0.5f, 0f, delay, 0.9f, UIEntrance.Curve.OutBack);
            GameObject logo = NewUI("Logo", holder.transform);
            Stretch(logo);
            Img(logo, S("logo"), Color.white);
            logo.AddComponent<UIIdleAnim>().Setup(UIIdleAnim.Mode.Bob, 14f, 2f);
        }

        private static void Entrance(GameObject go, Vector2 offset, float scale, float alpha, float delay, float duration, UIEntrance.Curve curve)
        {
            UIEntrance e = go.AddComponent<UIEntrance>();
            e.Setup(offset, scale, alpha, delay, duration, curve);
        }

        private static Slider ProgressSlider(Transform parent, Vector2 size, out TextMeshProUGUI percent)
        {
            GameObject go = NewUI("LoadingBar", parent);
            Anchor(go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            Img(go, S("pill_dark"), Color.white, true, 80f / size.y);

            GameObject area = NewUI("Fill Area", go.transform);
            RectTransform art = (RectTransform)area.transform;
            art.anchorMin = Vector2.zero;
            art.anchorMax = Vector2.one;
            art.offsetMin = new Vector2(7f, 7f);
            art.offsetMax = new Vector2(-7f, -7f);

            GameObject fill = NewUI("Fill", area.transform);
            RectTransform frt = (RectTransform)fill.transform;
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = new Vector2(0f, 1f);
            frt.offsetMin = frt.offsetMax = Vector2.zero;
            Img(fill, S("bar_fill"), Color.white, true, 80f / (size.y - 14f));

            Slider s = go.AddComponent<Slider>();
            s.fillRect = frt;
            s.handleRect = null;
            s.direction = Slider.Direction.LeftToRight;
            s.minValue = 0f;
            s.maxValue = 1f;
            s.value = 0f;
            s.interactable = false;
            s.transition = Selectable.Transition.None;
            Navigation nav = s.navigation;
            nav.mode = Navigation.Mode.None;
            s.navigation = nav;

            percent = Text(go.transform, "PercentText", "0%", 38f, Color.white, matOutline, new Vector2(0f, 1f), size);
            return s;
        }

        private static GameObject Popup(Transform parent, string name, string title, float height,
                                        out RectTransform box, out List<RectTransform> items, out TextMeshProUGUI titleText)
        {
            GameObject panel = NewUI(name, parent);
            Stretch(panel);
            Img(panel, null, new Color(0.01f, 0.02f, 0.08f, 0.78f), false, 1f, true);

            GameObject boxGo = NewUI("ModalBox", panel.transform);
            Anchor(boxGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(880f, height));
            Img(boxGo, S("panel"), Color.white, true, 1f, true);
            box = (RectTransform)boxGo.transform;

            GameObject ribbon = NewUI("Ribbon", boxGo.transform);
            Anchor(ribbon, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 20f), new Vector2(820f, 210f));
            Img(ribbon, S("ribbon"), Color.white, true, 190f / 210f);
            titleText = Text(ribbon.transform, "Title", title, 76f, Color.white, matOutline, new Vector2(0f, 22f), new Vector2(560f, 130f));
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 40f;
            titleText.fontSizeMax = 76f;
            titleText.enableWordWrapping = false;
            titleText.overflowMode = TextOverflowModes.Overflow;

            items = new List<RectTransform> { (RectTransform)ribbon.transform };
            return panel;
        }

        private static void FinishPopup(GameObject panel, RectTransform box, List<RectTransform> items)
        {
            UIPopupPanel p = panel.AddComponent<UIPopupPanel>();
            p.Setup(panel.GetComponent<Image>(), box, items.ToArray());
        }

        private static void EnsureWorldSprite(Transform parent, string name, int order)
        {
            Transform t = parent.Find(name);
            if (t == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(parent, false);
                t = go.transform;
            }
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (sr == null) sr = t.gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
        }

        // ------------------------------------------------------------------
        // Serialized field helpers (works for private [SerializeField] fields)
        // ------------------------------------------------------------------

        private static void SetRef(Object target, string field, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogWarning($"[BlockBlastUIBuilder] Field '{field}' not found on {target.GetType().Name}");
                return;
            }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(Object target, string field, float value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogWarning($"[BlockBlastUIBuilder] Field '{field}' not found on {target.GetType().Name}");
                return;
            }
            p.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
