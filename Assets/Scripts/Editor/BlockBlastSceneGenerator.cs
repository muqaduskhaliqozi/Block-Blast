using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using BlockBlast.Core;
using BlockBlast.Board;
using BlockBlast.Blocks;
using BlockBlast.Placement;
using BlockBlast.Scoring;
using BlockBlast.Game;
using BlockBlast.UI;
using BlockBlast.Audio;
using BlockBlast.Save;
using BlockBlast.Camera;
using BlockBlast.Visuals;

namespace BlockBlast.EditorTools
{
    public class BlockBlastSceneGenerator
    {
        [MenuItem("Tools/Block Blast/Generate Gameplay Scene")]
        public static void GenerateScene()
        {
            // Ensure PNG sprite assets and Prefabs exist
            SpriteAssetGenerator.GenerateAllSpriteAssets();
            PrefabGeneratorTool.GenerateAllPrefabs();

            string scenePath = "Assets/Scenes/BlockBlastGameplay.unity";
            UnityEngine.SceneManagement.Scene scene;

            if (System.IO.File.Exists(scenePath))
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }

            GameConfig config = GameConfig.Default;
            Sprite roundedSprite = SpriteAssetGenerator.GetSpriteAsset("RoundedSquare");
            Sprite borderSprite = SpriteAssetGenerator.GetSpriteAsset("CellBorder");
            Sprite shadowSprite = SpriteAssetGenerator.GetSpriteAsset("SoftShadow");
            Sprite gradientSprite = SpriteAssetGenerator.GetSpriteAsset("BackgroundGradient");
            Sprite softCircleSprite = SpriteAssetGenerator.GetSpriteAsset("SoftCircle");

            // 1. Camera Setup
            GameObject cameraObj = GetOrCreateGameObject("Main Camera", null);
            UnityEngine.Camera cam = GetOrCreateComponent<UnityEngine.Camera>(cameraObj);
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.backgroundColor = config.bgGradientBottom;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);
            GetOrCreateComponent<CameraController>(cameraObj);

            // 2. System Managers Root
            GameObject managersObj = GetOrCreateGameObject("--- GAME SYSTEMS ---", null);
            var gameMgr = GetOrCreateComponent<GameManager>(managersObj);
            var stateMgr = GetOrCreateComponent<GameStateManager>(managersObj);
            var scoreMgr = GetOrCreateComponent<ScoreManager>(managersObj);
            var comboMgr = GetOrCreateComponent<ComboManager>(managersObj);
            var gameOverChk = GetOrCreateComponent<GameOverChecker>(managersObj);
            var saveMgr = GetOrCreateComponent<SaveManager>(managersObj);
            var audioMgr = GetOrCreateComponent<AudioManager>(managersObj);
            var placementSys = GetOrCreateComponent<BlockPlacementSystem>(managersObj);
            var previewSys = GetOrCreateComponent<BlockPlacementPreview>(managersObj);
            var dragCtrl = GetOrCreateComponent<BlockDragController>(managersObj);
            var spawner = GetOrCreateComponent<BlockSpawner>(managersObj);

            // Assign block prefabs list to spawner
            var blockPrefabs = PrefabGeneratorTool.LoadAllBlockPrefabs();
            spawner.SetBlockPrefabs(blockPrefabs);

            // 3. Board Manager & Hierarchy
            GameObject boardObj = GetOrCreateGameObject("BoardManager", null);
            boardObj.transform.position = new Vector3(0f, 0.6f, 0f);
            var boardMgr = GetOrCreateComponent<BoardManager>(boardObj);

            // Background Gradient
            GameObject gradObj = GetOrCreateGameObject("BackgroundGradient", boardObj.transform);
            gradObj.transform.localPosition = new Vector3(0f, 0f, 10f);
            gradObj.transform.localScale = new Vector3(25f, 35f, 1f);
            SpriteRenderer gradSr = GetOrCreateComponent<SpriteRenderer>(gradObj);
            gradSr.sprite = gradientSprite;
            gradSr.sortingOrder = -100;

            // Background Decorative Circles
            Vector3[] decoPositions = new Vector3[]
            {
                new Vector3(-3.5f, 5.0f, 5f),
                new Vector3(4.0f, 2.5f, 5f),
                new Vector3(-4.0f, -4.5f, 5f),
                new Vector3(3.5f, -6.0f, 5f)
            };
            float[] decoScales = new float[] { 7.5f, 9.0f, 8.0f, 10.0f };

            for (int i = 0; i < decoPositions.Length; i++)
            {
                GameObject decoObj = GetOrCreateGameObject($"DecoCircle_{i}", boardObj.transform);
                decoObj.transform.localPosition = decoPositions[i];
                decoObj.transform.localScale = Vector3.one * decoScales[i];
                SpriteRenderer dSr = GetOrCreateComponent<SpriteRenderer>(decoObj);
                dSr.sprite = softCircleSprite;
                dSr.color = config.bgDecoCircleColor;
                dSr.sortingOrder = -90;
            }

            // Board Frame & Shadow
            float step = config.cellSize + config.cellSpacing;
            float totalWidth = config.boardWidth * step - config.cellSpacing;
            float totalHeight = config.boardHeight * step - config.cellSpacing;
            float bgPadding = config.boardPadding;
            Vector3 panelScale = new Vector3(totalWidth + bgPadding * 2f, totalHeight + bgPadding * 2f, 1f);

            GameObject boardShadowObj = GetOrCreateGameObject("BoardShadow", boardObj.transform);
            boardShadowObj.transform.localPosition = config.shadowOffset;
            boardShadowObj.transform.localScale = panelScale * 1.08f;
            SpriteRenderer shadowSr = GetOrCreateComponent<SpriteRenderer>(boardShadowObj);
            shadowSr.sprite = shadowSprite;
            shadowSr.color = config.shadowColor;
            shadowSr.sortingOrder = -10;

            GameObject boardFrameObj = GetOrCreateGameObject("BoardFrame", boardObj.transform);
            boardFrameObj.transform.localPosition = Vector3.zero;
            boardFrameObj.transform.localScale = panelScale;
            SpriteRenderer frameSr = GetOrCreateComponent<SpriteRenderer>(boardFrameObj);
            frameSr.sprite = roundedSprite;
            frameSr.color = config.boardBackgroundColor;
            frameSr.sortingOrder = -5;

            // Instantiate BoardCell Prefabs under BoardCellsContainer
            GameObject cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BoardCell.prefab");
            GameObject containerObj = GetOrCreateGameObject("BoardCellsContainer", boardObj.transform);

            if (containerObj.transform.childCount != config.boardWidth * config.boardHeight)
            {
                // Clear and build using PrefabUtility.InstantiatePrefab
                for (int i = containerObj.transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(containerObj.transform.GetChild(i).gameObject);
                }

                Vector3 originWorld = boardObj.transform.position - new Vector3(totalWidth * 0.5f - config.cellSize * 0.5f, totalHeight * 0.5f - config.cellSize * 0.5f, 0f);
                float renderSize = config.cellSize * config.cellGapRatio;

                for (int y = 0; y < config.boardHeight; y++)
                {
                    for (int x = 0; x < config.boardWidth; x++)
                    {
                        Vector3 cellPos = originWorld + new Vector3(x * step, y * step, 0f);
                        GameObject cellInst = PrefabUtility.InstantiatePrefab(cellPrefab, containerObj.transform) as GameObject;
                        if (cellInst != null)
                        {
                            cellInst.name = $"Cell_{x}_{y}";
                            cellInst.transform.position = cellPos;
                            cellInst.transform.localScale = Vector3.one * renderSize;

                            BoardCell cellComp = cellInst.GetComponent<BoardCell>();
                            if (cellComp != null)
                            {
                                cellComp.Initialize(new BoardCoordinate(x, y), config);
                            }
                        }
                    }
                }
            }

            // 4. Block Tray & Permanent Slots
            GameObject trayObj = GetOrCreateGameObject("BlockTray", null);
            trayObj.transform.position = new Vector3(0f, -4.2f, 0f);
            var trayComp = GetOrCreateComponent<BlockTray>(trayObj);

            float slotSpacing = 2.45f;
            float startX = -(config.traySlotCount - 1) * 0.5f * slotSpacing;

            List<BlockSlot> slotList = new List<BlockSlot>();

            for (int i = 0; i < config.traySlotCount; i++)
            {
                GameObject slotObj = GetOrCreateGameObject($"BlockSlot_{i}", trayObj.transform);
                slotObj.transform.localPosition = new Vector3(startX + i * slotSpacing, 0f, 0f);

                BlockSlot slotComp = GetOrCreateComponent<BlockSlot>(slotObj);
                slotComp.Initialize(i, config, 2.2f, 2.2f);
                slotList.Add(slotComp);

                // Place a sample block prefab in Edit Mode for scene visibility
                if (slotObj.transform.childCount <= 2 && blockPrefabs.Count > i)
                {
                    GameObject sampleBlockPrefab = blockPrefabs[i % blockPrefabs.Count];
                    GameObject sampleInst = PrefabUtility.InstantiatePrefab(sampleBlockPrefab, slotObj.transform) as GameObject;
                    if (sampleInst != null)
                    {
                        sampleInst.name = $"Sample_{sampleBlockPrefab.name}";
                        BlockController sampleController = sampleInst.GetComponent<BlockController>();
                        if (sampleController != null)
                        {
                            slotComp.SetBlock(sampleController);
                        }
                    }
                }
            }

            // 5. EventSystem
            GameObject eventSystemObj = GetOrCreateGameObject("EventSystem", null);
            GetOrCreateComponent<UnityEngine.EventSystems.EventSystem>(eventSystemObj);
            GetOrCreateComponent<UnityEngine.EventSystems.StandaloneInputModule>(eventSystemObj);

            // 6. UI Canvas & Permanent Hierarchy
            GameObject canvasObj = GetOrCreateGameObject("UI Canvas", null);
            Canvas canvas = GetOrCreateComponent<Canvas>(canvasObj);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = GetOrCreateComponent<CanvasScaler>(canvasObj);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            GetOrCreateComponent<GraphicRaycaster>(canvasObj);
            GameUI gameUI = GetOrCreateComponent<GameUI>(canvasObj);

            // Top Header Panel
            GameObject topPanel = GetOrCreateUIElement("TopHeader", canvasObj.transform);
            RectTransform topRt = topPanel.GetComponent<RectTransform>();
            SetAnchors(topRt, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            topRt.anchoredPosition = new Vector2(0f, -60f);
            topRt.sizeDelta = new Vector2(0f, 220f);

            GameObject titleObj = GetOrCreateTextElement("TitleText", topPanel.transform, "BLOCK BLAST", 52, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            SetAnchors(titleRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            titleRt.anchoredPosition = new Vector2(0f, -25f);
            titleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.75f, 0.82f, 0.95f);

            GameObject scoreObj = GetOrCreateTextElement("ScoreText", topPanel.transform, "0", 96, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform scoreRt = scoreObj.GetComponent<RectTransform>();
            SetAnchors(scoreRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            scoreRt.anchoredPosition = new Vector2(0f, -20f);

            GameObject bestObj = GetOrCreateTextElement("BestScoreText", topPanel.transform, "👑 BEST: 0", 38, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform bestRt = bestObj.GetComponent<RectTransform>();
            SetAnchors(bestRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            bestRt.anchoredPosition = new Vector2(0f, 15f);
            bestObj.GetComponent<TextMeshProUGUI>().color = new Color(1.0f, 0.82f, 0.35f);

            ScoreUI scoreUI = GetOrCreateComponent<ScoreUI>(topPanel);
            SetPrivateField(scoreUI, "scoreText", scoreObj.GetComponent<TextMeshProUGUI>());
            SetPrivateField(scoreUI, "bestScoreText", bestObj.GetComponent<TextMeshProUGUI>());

            GameObject restartBtnObj = GetOrCreateButtonElement("RestartButton", topPanel.transform, "↻", 52);
            RectTransform restartRt = restartBtnObj.GetComponent<RectTransform>();
            SetAnchors(restartRt, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            restartRt.anchoredPosition = new Vector2(-70f, -60f);
            restartRt.sizeDelta = new Vector2(110f, 110f);

            GameObject soundBtnObj = GetOrCreateButtonElement("SoundButton", topPanel.transform, "🔊", 48);
            RectTransform soundRt = soundBtnObj.GetComponent<RectTransform>();
            SetAnchors(soundRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            soundRt.anchoredPosition = new Vector2(70f, -60f);
            soundRt.sizeDelta = new Vector2(110f, 110f);

            // Combo Text
            GameObject comboObj = GetOrCreateTextElement("ComboText", canvasObj.transform, "GREAT!", 68, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform comboRt = comboObj.GetComponent<RectTransform>();
            SetAnchors(comboRt, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f));
            comboRt.anchoredPosition = Vector2.zero;
            comboObj.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);
            ComboUI comboUI = GetOrCreateComponent<ComboUI>(comboObj);
            SetPrivateField(comboUI, "comboText", comboObj.GetComponent<TextMeshProUGUI>());
            comboObj.SetActive(false);

            // Game Over Panel (Inactive in hierarchy)
            GameObject gameOverPanel = GetOrCreateUIElement("GameOverPanel", canvasObj.transform);
            RectTransform goRt = gameOverPanel.GetComponent<RectTransform>();
            SetAnchors(goRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            goRt.offsetMin = Vector2.zero;
            goRt.offsetMax = Vector2.zero;

            Image bgImg = GetOrCreateComponent<Image>(gameOverPanel);
            bgImg.color = new Color(0.05f, 0.06f, 0.12f, 0.94f);

            GameObject modalBox = GetOrCreateUIElement("ModalBox", gameOverPanel.transform);
            RectTransform modalRt = modalBox.GetComponent<RectTransform>();
            SetAnchors(modalRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            modalRt.sizeDelta = new Vector2(840f, 920f);

            Image modalImg = GetOrCreateComponent<Image>(modalBox);
            modalImg.sprite = roundedSprite;
            modalImg.color = new Color(0.12f, 0.14f, 0.26f);

            GameObject goTitle = GetOrCreateTextElement("GOTitle", modalBox.transform, "GAME OVER", 72, FontStyles.Bold, TextAlignmentOptions.Center);
            goTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 330f);
            goTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.95f, 0.35f, 0.45f);

            GameObject goScoreLabel = GetOrCreateTextElement("GOScoreLabel", modalBox.transform, "SCORE", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            goScoreLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 190f);

            GameObject goFinalScore = GetOrCreateTextElement("GOFinalScore", modalBox.transform, "0", 100, FontStyles.Bold, TextAlignmentOptions.Center);
            goFinalScore.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 85f);
            goFinalScore.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.85f, 0.95f);

            GameObject goBestScore = GetOrCreateTextElement("GOBestScore", modalBox.transform, "👑 BEST: 0", 42, FontStyles.Bold, TextAlignmentOptions.Center);
            goBestScore.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);
            goBestScore.GetComponent<TextMeshProUGUI>().color = new Color(1.0f, 0.82f, 0.35f);

            GameObject retryBtn = GetOrCreateButtonElement("RetryButton", modalBox.transform, "RETRY", 48);
            RectTransform retryRt = retryBtn.GetComponent<RectTransform>();
            retryRt.anchoredPosition = new Vector2(0f, -180f);
            retryRt.sizeDelta = new Vector2(420f, 115f);

            GameObject homeBtn = GetOrCreateButtonElement("HomeButton", modalBox.transform, "HOME", 36);
            RectTransform homeRt = homeBtn.GetComponent<RectTransform>();
            homeRt.anchoredPosition = new Vector2(0f, -315f);
            homeRt.sizeDelta = new Vector2(320f, 90f);

            GameOverUI goUI = GetOrCreateComponent<GameOverUI>(gameOverPanel);
            SetPrivateField(goUI, "gameOverPanel", gameOverPanel);
            SetPrivateField(goUI, "finalScoreText", goFinalScore.GetComponent<TextMeshProUGUI>());
            SetPrivateField(goUI, "bestScoreText", goBestScore.GetComponent<TextMeshProUGUI>());
            SetPrivateField(goUI, "retryButton", retryBtn.GetComponent<Button>());
            SetPrivateField(goUI, "homeButton", homeBtn.GetComponent<Button>());

            gameOverPanel.SetActive(false);

            SetPrivateField(gameUI, "restartButton", restartBtnObj.GetComponent<Button>());
            SetPrivateField(gameUI, "soundButton", soundBtnObj.GetComponent<Button>());

            // 7. Save Scene
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var buildScenes = EditorBuildSettings.scenes;
            bool sceneExistsInBuild = false;
            foreach (var s in buildScenes)
            {
                if (s.path == scenePath) { sceneExistsInBuild = true; break; }
            }

            if (!sceneExistsInBuild)
            {
                var newBuildScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
                buildScenes.CopyTo(newBuildScenes, 0);
                newBuildScenes[newBuildScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = newBuildScenes;
            }

            Debug.Log($"[BlockBlastSceneGenerator] Successfully generated edit-time scene hierarchy at '{scenePath}'!");
        }

        private static GameObject GetOrCreateGameObject(string name, Transform parent)
        {
            if (parent != null)
            {
                Transform child = parent.Find(name);
                if (child != null) return child.gameObject;
            }
            else
            {
                GameObject root = GameObject.Find(name);
                if (root != null) return root;
            }

            GameObject obj = new GameObject(name);
            if (parent != null) obj.transform.SetParent(parent, false);
            return obj;
        }

        private static T GetOrCreateComponent<T>(GameObject target) where T : Component
        {
            T comp = target.GetComponent<T>();
            if (comp == null) comp = target.AddComponent<T>();
            return comp;
        }

        private static GameObject GetOrCreateUIElement(string name, Transform parent)
        {
            GameObject obj = GetOrCreateGameObject(name, parent);
            GetOrCreateComponent<RectTransform>(obj);
            return obj;
        }

        private static GameObject GetOrCreateTextElement(string name, Transform parent, string content, int fontSize, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = GetOrCreateUIElement(name, parent);
            TextMeshProUGUI tmp = GetOrCreateComponent<TextMeshProUGUI>(obj);
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return obj;
        }

        private static GameObject GetOrCreateButtonElement(string name, Transform parent, string labelText, int fontSize)
        {
            GameObject obj = GetOrCreateUIElement(name, parent);
            Image img = GetOrCreateComponent<Image>(obj);
            img.sprite = SpriteAssetGenerator.GetSpriteAsset("RoundedSquare");
            img.color = new Color(0.20f, 0.24f, 0.40f);

            Button btn = GetOrCreateComponent<Button>(obj);
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.32f, 0.38f, 0.60f);
            cb.pressedColor = new Color(0.15f, 0.18f, 0.32f);
            btn.colors = cb;

            GameObject labelObj = GetOrCreateTextElement("Label", obj.transform, labelText, fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform labelRt = labelObj.GetComponent<RectTransform>();
            SetAnchors(labelRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            return obj;
        }

        private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
