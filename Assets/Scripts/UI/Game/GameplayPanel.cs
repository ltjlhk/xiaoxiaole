using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>
    /// 关卡玩法面板（栈式面板）：顶栏(返回/第X关/分数) + 棋盘 + 7槽 + 四道具。
    /// 渲染复用 GamePanel（MonoBehaviour），赢/输 → 结算浮层；下一关按全局序，返回 → Pop 回主城/选关。
    /// </summary>
    public class GameplayPanel : UIPanel
    {
        public int LevelId = 101;

        private GamePanel _gamePanel;
        private Text _scoreText;
        private Text _titleText;
        private Text _coinText;
        private GameObject _overlay;
        private bool _settled;
        private PuzzleGame _subscribedGame;
        private SkillTriggerController _skillCtl;

        protected override void Build()
        {
            MakeBackground();
            MakeTopBar();
            MakeBoardArea();

            _gamePanel = Root.gameObject.GetComponent<GamePanel>() ?? Root.gameObject.AddComponent<GamePanel>();
            _gamePanel.boardRoot = Root.Find("BoardRoot");
            _gamePanel.slotRoot = Root.Find("SlotRoot");
            _gamePanel.scoreText = _scoreText;
            _gamePanel.titleText = _titleText;

            StartLevel(LevelId);
        }

        private void MakeBackground()
        {
            var bgTex = OriginalAssets.GetBackground("bg10");
            var bg = UIHelper.Image(Root, "BG", bgTex != null
                ? Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f), 100f)
                : null);
            UIHelper.Stretch((RectTransform)bg.transform);
        }

        private void MakeTopBar()
        {
            // 返回
            var back = UIHelper.Button(Root, "Back", () =>
            {
                if (_overlay == null) PanelManager.Instance.Pop();
            });
            UIHelper.Place((RectTransform)back.transform, new Vector2(0, 1f), new Vector2(96, 56), new Vector2(70, -38));
            var backIcon = OriginalAssets.GetUi("btn_fanhui");
            if (backIcon != null)
            {
                var bi = back.gameObject.GetComponent<Image>();
                bi.sprite = backIcon;
                bi.color = Color.white;
                bi.SetNativeSize();
            }
            else
            {
                var l = UIHelper.Text(back.transform, "L", "返回", 26, Color.white, FontStyle.Bold);
                UIHelper.Stretch(l.rectTransform);
            }

            // 标题（关卡号）
            _titleText = UIHelper.Text(Root, "TitleText", "", 38, new Color(1f, 0.98f, 0.85f), FontStyle.Bold);
            UIHelper.Place(_titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(600, 60), new Vector2(0, -40));
            _titleText.gameObject.AddComponent<Outline>().effectColor = new Color(0.12f, 0.22f, 0.45f);
            _titleText.gameObject.GetComponent<Outline>().effectDistance = new Vector2(2, -2);

            // 分数
            _scoreText = UIHelper.Text(Root, "ScoreText", "第 1 关", 28, new Color(1f, 0.95f, 0.7f), FontStyle.Bold);
            UIHelper.Place(_scoreText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(700, 50), new Vector2(0, -380));

            // 金币（右上，原版 TopHud）
            var goldIcon = UIHelper.Image(Root, "GoldIcon", OriginalAssets.GetUi("bigbig_gold"));
            UIHelper.Place((RectTransform)goldIcon.transform, new Vector2(1f, 1f), new Vector2(34, 34), new Vector2(-92, -42));
            _coinText = UIHelper.Text(Root, "CoinText", "", 24, new Color(1f, 0.95f, 0.6f), FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place(_coinText.rectTransform, new Vector2(1f, 1f), new Vector2(90, 36), new Vector2(-56, -42));
            if (_coinText != null) _coinText.text = SaveManager.Data.coins.ToString();
        }

        private void MakeBoardArea()
        {
            // 棋盘：竖屏居中偏上，留出下方槽位+道具栏空间
            var board = UIHelper.NewRect(Root, "BoardRoot");
            UIHelper.Place(board, new Vector2(0.5f, 0.5f), new Vector2(660, 760), new Vector2(0, 220));

            var slot = UIHelper.NewRect(Root, "SlotRoot");
            UIHelper.Place(slot, new Vector2(0.5f, 0.5f), new Vector2(690, 100), new Vector2(0, -310));

            // 槽底衬
            var plate = UIHelper.Image(slot, "Plate");
            UIHelper.Stretch((RectTransform)plate.transform);
            var pimg = plate.GetComponent<Image>();
            pimg.color = new Color(0.04f, 0.09f, 0.14f, 0.40f);

            // 工具条
            var tb = UIHelper.NewRect(Root, "ToolBar");
            UIHelper.Place(tb, new Vector2(0.5f, 0.5f), new Vector2(660, 120), new Vector2(0, -470));
            MakeToolBar(tb);
        }

        private void MakeToolBar(Transform parent)
        {
            string[] labels = { "清牌", "洗牌", "合成", "冰冻" };
            ToolType[] tools = { ToolType.Clear, ToolType.Shuffle, ToolType.Compose, ToolType.Freeze };
            string[] icons = { "output_icon_2", "shufflesmall", "compose", "freezesmall" };
            for (int i = 0; i < labels.Length; i++)
            {
                int ii = i;
                ToolType tool = tools[i];
                var bt = UIHelper.Button(parent, "Btn_" + labels[i], () =>
                {
                    if (_gamePanel != null && _gamePanel.Game != null) _gamePanel.Game.UseTool(tool);
                });
                UIHelper.Place((RectTransform)bt.transform, new Vector2(0.5f, 0.5f),
                    new Vector2(138, 128), new Vector2(-34 + 116 * ii, 0));

                var board = OriginalAssets.GetUi("mp_board");
                var bi = bt.gameObject.GetComponent<Image>();
                if (board != null) { bi.sprite = board; bi.color = Color.white; }
                else bi.color = new Color(0.95f, 0.78f, 0.32f);

                var iconSp = OriginalAssets.GetUi(icons[i]);
                if (iconSp != null)
                {
                    var ic = UIHelper.Image(bt.transform, "Icon", iconSp);
                    ic.preserveAspect = true;
                    UIHelper.Place((RectTransform)ic.transform, new Vector2(0.5f, 0.5f),
                        new Vector2(66, 66), new Vector2(0, 12));
                }
                var t = UIHelper.Text(bt.transform, "Text", labels[i], 22, new Color(0.45f, 0.25f, 0.08f), FontStyle.Bold);
                var trt = t.rectTransform;
                trt.anchorMin = trt.anchorMax = new Vector2(0, 0);
                trt.pivot = new Vector2(0.5f, 0);
                trt.sizeDelta = new Vector2(130, 26);
                trt.anchoredPosition = new Vector2(0, 8);
            }
        }

        /// <summary>启动指定关卡（面板内切换关卡也走这里）。</summary>
        public void StartLevel(int levelId)
        {
            LevelId = levelId;
            if (_scoreText == null || _gamePanel == null) return;

            if (_overlay != null) { Object.Destroy(_overlay); _overlay = null; }
            _settled = false;

            var map = ConfigLoader.SplitTopLevel(ConfigLoader.LoadRaw("PuzzleConfig"));
            if (!map.TryGetValue(levelId.ToString(), out string cfg))
            {
                Debug.LogError($"[玩法面板] 配置缺关卡 {levelId}");
                return;
            }
            var level = ConfigLoader.FromJson<PuzzleLevel>(cfg);
            if (level != null)
                level.PuzzleReward = ConfigLoader.ParseNestedIntLists(ConfigLoader.ExtractField(cfg, "PuzzleReward"));

            var tex = OriginalAssets.GetPuzzleTexture(levelId);
            if (tex == null)
                tex = OriginalAssets.GetPuzzleTexture(GameBootstrap.FirstLevel + (levelId - GameBootstrap.FirstLevel) % 20);
            if (tex == null)
            {
                Debug.LogError($"[玩法面板] 未加载到拼图贴图 {levelId}");
                return;
            }

            var flowers = new List<string>(OriginalAssets.ListAllFlowerIds());
            _gamePanel.Init(level, flowers, tex);
            var g = _gamePanel.Game;
            if (g == null) return;
            if (_subscribedGame != null && _subscribedGame != g)
                _subscribedGame.OnStateChanged -= OnGameStateChanged;
            g.OnStateChanged += OnGameStateChanged;
            _subscribedGame = g;
            BindSkills(g);
        }

        /// <summary>挂载技能自动触发（按当前精灵）。</summary>
        private void BindSkills(PuzzleGame g)
        {
            if (_skillCtl == null)
                _skillCtl = Root.gameObject.AddComponent<SkillTriggerController>();
            _skillCtl.Bind(g, SaveManager.Data.currentFairy);
        }

        private void OnGameStateChanged(GameState s)
        {
            if (_settled) return;
            if (s == GameState.Win)
            {
                _settled = true;
                SaveManager.RewardLevel(LevelId);   // 解锁推进 +1 星（奖励物品由 GamePanel 写）
                SaveManager.AddActivity(1);
                PublishResult(true);
                ShowSettle(true);
                AudioManager.Inst.PlayWin();
            }
            else if (s == GameState.Lose) { _settled = true; PublishResult(false); ShowSettle(false); AudioManager.Inst.PlayLose(); }
        }

        /// <summary>发布本局结果到事件中心（任务追踪/统计）。</summary>
        private void PublishResult(bool win)
        {
            if (_gamePanel == null) return;
            EventCenter.OnLevelResult?.Invoke(new LevelResult
            {
                LevelId = LevelId,
                Win = win,
                Score = _gamePanel.Game != null ? _gamePanel.Game.Score : 0,
                Stars = win ? 1 : 0,
                Clears = _gamePanel.ClearCount,
                Shuffles = _gamePanel.ShuffleCount,
                Composes = _gamePanel.ComposeCount,
                Freezes = _gamePanel.FreezeCount,
                Matches = _gamePanel.MatchCount,
                Clicks = _gamePanel.Game != null ? _gamePanel.Game.ClickCount : 0,
            });
        }

        private void ShowSettle(bool win)
        {
            if (_overlay != null) Object.Destroy(_overlay);
            _overlay = UIHelper.Image(Root, "SettleMask").gameObject;
            UIHelper.Stretch((RectTransform)_overlay.transform);
            _overlay.GetComponent<Image>().color = new Color(0, 0, 0, 0.62f);

            var panel = UIHelper.NewRect(_overlay.transform, "Settle");
            UIHelper.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(640, 360), new Vector2(0, 0));
            var board = OriginalAssets.GetUi("jiesuankuang_1");
            if (board != null)
            {
                var bi = panel.gameObject.AddComponent<Image>();
                bi.sprite = board;
                bi.type = Image.Type.Sliced;
            }
            else
            {
                panel.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.16f, 0.24f, 0.95f);
            }

            var msg = UIHelper.Text(panel, "Msg", win ? "拼图完成！" : "槽位满了…", 44,
                win ? new Color(1f, 0.96f, 0.7f) : new Color(1f, 0.75f, 0.7f), FontStyle.Bold);
            UIHelper.Place(msg.rectTransform, new Vector2(0.5f, 0.8f), new Vector2(600, 70), Vector2.zero);

            // 下一关 / 重试 + 回主城
            MakeSettleButton(panel, win ? "下一关" : "重 试", win ? (System.Action)OnNextLevel : OnRetry, new Vector2(0, -40));
            MakeSettleButton(panel, "返回选关", () => PanelManager.Instance.Pop(), new Vector2(0, -130));
        }

        private void MakeSettleButton(Transform parent, string label, System.Action onClick, Vector2 pos)
        {
            var bt = UIHelper.Button(parent, "Btn_" + label, onClick);
            UIHelper.Place((RectTransform)bt.transform, new Vector2(0.5f, 0.5f), new Vector2(300, 76), pos);
            var board = OriginalAssets.GetUi("mp_board");
            var bi = bt.gameObject.GetComponent<Image>();
            if (board != null) { bi.sprite = board; bi.color = Color.white; }
            else bi.color = new Color(0.95f, 0.78f, 0.32f);

            var t = UIHelper.Text(bt.transform, "L", label, 30, new Color(0.4f, 0.22f, 0.06f), FontStyle.Bold);
            UIHelper.Stretch(t.rectTransform);
        }

        private void OnNextLevel()
        {
            int next = LevelSegmentModel.NextLevelAfter(LevelId);
            if (next < 0) { PanelManager.Instance.Pop(); return; }
            StartLevel(next);
        }

        private void OnRetry()
        {
            StartLevel(LevelId);
        }

        public override void Refresh()
        {
            // 游戏内变化（如返回上一面板）无需重建；金币可能变化（结算领奖）
            if (_coinText != null) _coinText.text = SaveManager.Data.coins.ToString();
        }

        public override void Close()
        {
            if (_subscribedGame != null)
            {
                _subscribedGame.OnStateChanged -= OnGameStateChanged;
                _subscribedGame = null;
            }
            base.Close();
        }
    }
}