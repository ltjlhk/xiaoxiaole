using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.Game
{
    /// <summary>
    /// 游戏入口：搭原版风格竖屏 UI（背景 + 标题 + 棋盘(拼图/卡叠) + 7 槽 + 分数 + 四道具），
    /// 原版 780 关全数据驱动（101~4170，缺图自动用原版图轮换）。
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        public int StartLevel = 101;

        /// <summary>原版全部关卡区间（PuzzleConfig 实际数据）。</summary>
        public const int FirstLevel = 101;
        public const int LastLevel = 4170;

        private GamePanel _panel;

        private void Start()
        {
            Xio.Platform.WxApi.Init();
            RunNow();
        }

        public void RunNow()
        {
            BuildUi();
            StartLevelPlay();
        }

        private void StartLevelPlay()
        {
            // 关键：销毁上一关的结算浮层（否则盖死新关卡）
            if (_overlay != null) { Destroy(_overlay); _overlay = null; }

            var map = ConfigLoader.SplitTopLevel(ConfigLoader.LoadRaw("PuzzleConfig"));
            if (!map.TryGetValue(StartLevel.ToString(), out string cfg))
            {
                Debug.LogError($"[引导] 配置缺关卡 {StartLevel}");
                return;
            }
            var level = ConfigLoader.FromJson<PuzzleLevel>(cfg);

            // 贴图：本关优先，缺失则用 20 张原版图轮换（101-120）
            var tex = OriginalAssets.GetPuzzleTexture(StartLevel);
            if (tex == null)
            {
                int fallback = 101 + (StartLevel - FirstLevel) % 20;
                tex = OriginalAssets.GetPuzzleTexture(fallback);
            }
            if (tex == null)
            {
                Debug.LogError($"[引导] 未加载到拼图贴图 {StartLevel}");
                return;
            }
            var flowers = new List<string>(OriginalAssets.ListAllFlowerIds());
            Debug.Log($"[引导] 关卡 {level.Id} {level.Rows}x{level.Cols} 暗格={level.PieceCount - (level.PuzzleShow?.Count ?? 0)} 贴图={tex.width}x{tex.height} 花池={flowers.Count}");

            _panel = gameObject.GetComponent<GamePanel>() ?? gameObject.AddComponent<GamePanel>();
            _panel.boardRoot = GameObject.Find("BoardRoot")?.transform;
            _panel.slotRoot = GameObject.Find("SlotRoot")?.transform;
            _panel.scoreText = GameObject.Find("ScoreText")?.GetComponent<Text>();
            _panel.titleText = GameObject.Find("TitleText")?.GetComponent<Text>();
            _panel.Init(level, flowers, tex);
            _panel.Game.OnStateChanged += OnGameStateChanged;
            AudioManager.Inst.PlayBgm();
        }

        private GameObject _overlay;

        private void OnGameStateChanged(GameState s)
        {
            if (_overlay != null) { Destroy(_overlay); _overlay = null; }
            if (s == GameState.Win) { AudioManager.Inst.PlayWin(); ShowOverlay(true); }
            else if (s == GameState.Lose) ShowOverlay(false);
        }

        private void ShowOverlay(bool win)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;
            _overlay = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
            _overlay.transform.SetParent(canvas.transform, false);
            var ov = _overlay.GetComponent<Image>();
            ov.color = new Color(0, 0, 0, 0.62f);
            var ort = (RectTransform)_overlay.transform;
            ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one;
            ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;

            string msg = win ? "拼图完成！" : "槽位满了…";
            var tGo = new GameObject("Msg", typeof(RectTransform), typeof(Text));
            tGo.transform.SetParent(_overlay.transform, false);
            var t = tGo.GetComponent<Text>();
            t.font = GetFont();
            t.fontSize = 48;
            t.fontStyle = FontStyle.Bold;
            t.color = win ? new Color(1f, 0.96f, 0.7f) : new Color(1f, 0.75f, 0.7f);
            t.alignment = TextAnchor.MiddleCenter;
            t.text = msg;
            var trt = (RectTransform)tGo.transform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(600, 90);
            trt.anchoredPosition = new Vector2(0, 90);

            var bGo = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            bGo.transform.SetParent(_overlay.transform, false);
            var bimg = bGo.GetComponent<Image>();
            var btnSp = OriginalAssets.GetUi("mp_board");
            if (btnSp != null) { bimg.sprite = btnSp; bimg.type = Image.Type.Sliced; }
            else bimg.color = new Color(0.95f, 0.78f, 0.32f);
            var brt = (RectTransform)bGo.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(320, 96);
            brt.anchoredPosition = new Vector2(0, -40);
            bGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (win) { StartLevel = StartLevel >= LastLevel ? FirstLevel : StartLevel + 1; }
                StartLevelPlay();
            });

            var lGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            lGo.transform.SetParent(bGo.transform, false);
            var l = lGo.GetComponent<Text>();
            l.font = GetFont();
            l.fontSize = 34;
            l.fontStyle = FontStyle.Bold;
            l.color = new Color(0.4f, 0.22f, 0.06f);
            l.alignment = TextAnchor.MiddleCenter;
            l.text = win ? "下一关" : "重 试";
            var lrt = (RectTransform)lGo.transform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        }

        // ===== UI 搭建（750x1334 竖屏）=====
        private void BuildUi()
        {
            if (FindObjectOfType<Canvas>() != null) return;

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.16f, 0.22f);

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(750, 1334);
            scaler.matchWidthOrHeight = 0.5f;

            // EventSystem：没有它 UI 点击全部失效
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
                esGo.transform.SetParent(transform, false);
            }

            MakeBackground(canvasGo.transform);
            MakeTitleBar(canvasGo.transform);
            MakeBoard(canvasGo.transform);
            MakeSlot(canvasGo.transform);
            MakeScoreBar(canvasGo.transform);
            MakeToolBar(canvasGo.transform);
        }

        private void MakeBackground(Transform parent)
        {
            var bgTex = OriginalAssets.GetBackground("bg10");
            var bgGo = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(parent, false);
            var img = bgGo.GetComponent<Image>();
            if (bgTex != null)
            {
                img.sprite = Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f), 100f);
                img.color = Color.white;
            }
            else img.color = new Color(0.10f, 0.30f, 0.45f);
            Stretch((RectTransform)bgGo.transform);
        }

        private void MakeTitleBar(Transform parent)
        {
            var tGo = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            tGo.transform.SetParent(parent, false);
            var t = tGo.GetComponent<Text>();
            t.font = GetFont();
            t.fontSize = 40;
            t.fontStyle = FontStyle.Bold;
            t.color = new Color(1f, 0.98f, 0.85f);
            t.alignment = TextAnchor.MiddleCenter;
            t.text = "就你会消除";
            var trt = (RectTransform)tGo.transform;
            trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.sizeDelta = new Vector2(0, 80);
            trt.anchoredPosition = new Vector2(0, -24);
            var outline = tGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.22f, 0.45f);
            outline.effectDistance = new Vector2(2, -2);
        }

        // 棋盘：拼图 + 卡叠一体，占主区域
        private void MakeBoard(Transform parent)
        {
            var boardGo = new GameObject("BoardRoot", typeof(RectTransform));
            boardGo.transform.SetParent(parent, false);
            var rt = (RectTransform)boardGo.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(660, 820);
            rt.anchoredPosition = new Vector2(0, 150);
        }

        private void MakeSlot(Transform parent)
        {
            var slotGo = new GameObject("SlotRoot", typeof(RectTransform));
            slotGo.transform.SetParent(parent, false);
            var rt = (RectTransform)slotGo.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(690, 112);
            rt.anchoredPosition = new Vector2(0, -348);

            var plate = new GameObject("SlotPlate", typeof(RectTransform), typeof(Image));
            plate.transform.SetParent(slotGo.transform, false);
            var pimg = plate.GetComponent<Image>();
            pimg.color = new Color(0.04f, 0.09f, 0.14f, 0.40f);
            var prt = (RectTransform)plate.transform;
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(-10, -8); prt.offsetMax = new Vector2(10, 8);
        }

        private void MakeScoreBar(Transform parent)
        {
            var sGo = new GameObject("ScoreText", typeof(RectTransform), typeof(Text));
            sGo.transform.SetParent(parent, false);
            var s = sGo.GetComponent<Text>();
            s.font = GetFont();
            s.fontSize = 28;
            s.fontStyle = FontStyle.Bold;
            s.color = new Color(1f, 0.95f, 0.7f);
            s.alignment = TextAnchor.MiddleCenter;
            s.text = "第 1 关";
            var rt = (RectTransform)sGo.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(700, 54);
            rt.anchoredPosition = new Vector2(0, -436);
        }

        private void MakeToolBar(Transform parent)
        {
            var tbGo = new GameObject("ToolBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tbGo.transform.SetParent(parent, false);
            var rt = (RectTransform)tbGo.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(660, 130);
            rt.anchoredPosition = new Vector2(0, -556);
            var h = tbGo.GetComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleCenter;
            h.spacing = 26;
            h.childControlWidth = h.childControlHeight = false;
            string[] labels = { "清牌", "洗牌", "合成", "冰冻" };
            ToolType[] tools = { ToolType.Clear, ToolType.Shuffle, ToolType.Compose, ToolType.Freeze };
            string[] icons = { "output_icon_2", "shufflesmall", "compose", "freezesmall" };
            for (int i = 0; i < labels.Length; i++)
                MakeToolButton(tbGo.transform, labels[i], icons[i], tools[i]);
        }

        private void MakeToolButton(Transform parent, string label, string iconName, ToolType tool)
        {
            var bGo = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            bGo.transform.SetParent(parent, false);
            var img = bGo.GetComponent<Image>();
            var boardSp = OriginalAssets.GetUi("mp_board");
            if (boardSp != null) { img.sprite = boardSp; img.color = Color.white; }
            else img.color = new Color(0.95f, 0.78f, 0.32f);
            var brt = (RectTransform)bGo.transform;
            brt.sizeDelta = new Vector2(138, 128);

            var iconSp = OriginalAssets.GetUi(iconName);
            if (iconSp != null)
            {
                var iGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iGo.transform.SetParent(bGo.transform, false);
                var iimg = iGo.GetComponent<Image>();
                iimg.sprite = iconSp;
                iimg.preserveAspect = true;
                var irt = (RectTransform)iGo.transform;
                irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.sizeDelta = new Vector2(74, 74);
                irt.anchoredPosition = new Vector2(0, 18);
            }
            var tGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            tGo.transform.SetParent(bGo.transform, false);
            var t = tGo.GetComponent<Text>();
            t.font = GetFont();
            t.fontSize = 26;
            t.fontStyle = FontStyle.Bold;
            t.color = new Color(0.45f, 0.25f, 0.08f);
            t.alignment = TextAnchor.MiddleCenter;
            t.text = label;
            var trt = (RectTransform)tGo.transform;
            trt.anchorMin = new Vector2(0, 0); trt.anchorMax = new Vector2(1, 0);
            trt.pivot = new Vector2(0.5f, 0);
            trt.sizeDelta = new Vector2(0, 32);
            trt.anchoredPosition = new Vector2(0, 8);

            var theTool = tool;
            bGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (_panel != null && _panel.Game != null) _panel.Game.UseTool(theTool);
            });
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Font _font;
        private static Font GetFont()
        {
            if (_font != null) return _font;
            _font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 32);
            return _font;
        }
    }
}
