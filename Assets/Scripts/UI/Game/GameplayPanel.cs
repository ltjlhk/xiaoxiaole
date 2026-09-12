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
        private Text _timeText;
        private GameObject _overlay;
        private bool _settled;
        private PuzzleGame _subscribedGame;
        private SkillTriggerController _skillCtl;
        private readonly List<Text> _badges = new List<Text>();   // 技能角标

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
            _gamePanel.onTimeChanged = OnTimeChanged;

            StartLevel(LevelId);
        }

        private void MakeBackground()
        {
            // 原版：绿色自然渐变（上浅下深），模拟花园阳光
            var bg = UIHelper.Image(Root, "BG", null);
            UIHelper.Stretch((RectTransform)bg.transform);
            var bgImg = bg.GetComponent<Image>();
            // 尝试原版贴图，否则用纯色渐变
            var bgTex = OriginalAssets.GetBackground("bg12");
            if (bgTex != null)
            {
                bgImg.sprite = Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f), 100f);
                bgImg.color = Color.white;
            }
            else
            {
                // 纯绿渐变（上浅下深）
                bgImg.color = new Color(0.45f, 0.68f, 0.38f);
            }
        }

        private void MakeTopBar()
        {
            // ===== 左上：暂停按钮（原版 resetting_icon）=====
            var pause = UIHelper.Button(Root, "Pause", () => ShowPauseMenu());
            UIHelper.Place((RectTransform) pause.transform, new Vector2(0, 1f), new Vector2(72, 72), new Vector2(86, -96));
            var pi = pause.gameObject.GetComponent<Image>();
            var pIcon = OriginalAssets.GetUi("resetting_icon");
            if (pIcon != null) { pi.sprite = pIcon; pi.color = Color.white; }
            else pi.color = new Color(0.2f, 0.3f, 0.45f, 0.9f);

            // ===== 左上第二：倒计时（原版 time_show + mm:ss）=====
            var timeBox = UIHelper.Image(Root, "TimeBox", OriginalAssets.GetUi("time_show"));
            UIHelper.Place((RectTransform) timeBox.transform, new Vector2(0, 1f), new Vector2(190, 64), new Vector2(190, -98));
            var tImg = timeBox.GetComponent<Image>();
            tImg.type = Image.Type.Sliced;
            tImg.color = Color.white;
            var tIcon = UIHelper.Image(timeBox.transform, "TIcon", OriginalAssets.GetUi("timebig"));
            UIHelper.Place((RectTransform) tIcon.transform, new Vector2(0, 0.5f), new Vector2(40, 40), new Vector2(38, 0));
            _timeText = UIHelper.Text(timeBox.transform, "T", "08:00", 34, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UIHelper.Place(_timeText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(120, 48), new Vector2(24, 0));
            _timeText.gameObject.AddComponent<Outline>().effectColor = new Color(0.1f, 0.18f, 0.35f);
            _timeText.gameObject.GetComponent<Outline>().effectDistance = new Vector2(2, -2);

            // ===== 中上：关卡花环「第X关」+ ★星评 =====
            var halo = UIHelper.Image(Root, "Halo", OriginalAssets.GetUi("tools_frame"));
            UIHelper.Place((RectTransform) halo.transform, new Vector2(0.5f, 1f), new Vector2(240, 72), new Vector2(0, -96));
            var hImg = halo.GetComponent<Image>();
            hImg.type = Image.Type.Sliced;
            hImg.color = new Color(0.96f, 0.85f, 0.5f, 0.95f);
            _titleText = UIHelper.Text(halo.transform, "TitleText", "第 1 关", 32, new Color(0.45f, 0.22f, 0.05f), FontStyle.Bold);
            UIHelper.Place(_titleText.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(240, 44), Vector2.zero);
            _titleText.gameObject.AddComponent<Outline>().effectColor = new Color(1f, 0.98f, 0.85f);
            _titleText.gameObject.GetComponent<Outline>().effectDistance = new Vector2(1, -1);
            // ★ 星评（本关已得星，原版 Star01）
            var starSp = OriginalAssets.GetUi("Star01");
            for (int i = 0; i < 3; i++)
            {
                var st = UIHelper.Image(halo.transform, "Star_" + i, starSp);
                UIHelper.Place((RectTransform) st.transform, new Vector2(0.5f, 0.5f), new Vector2(30, 30),
                    new Vector2(-36 + i * 36, -26));
                st.color = i == 0 ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.45f);
            }

            // ===== 右上：设置（原版 wx_shezhi）+ 金币 =====
            var stBtn = UIHelper.Button(Root, "Settings", () => ShowPauseMenu());
            UIHelper.Place((RectTransform) stBtn.transform, new Vector2(1f, 1f), new Vector2(72, 72), new Vector2(-86, -96));
            var si = stBtn.gameObject.GetComponent<Image>();
            var sIcon = OriginalAssets.GetUi("wx_shezhi");
            if (sIcon != null) { si.sprite = sIcon; si.color = Color.white; }
            else si.color = new Color(0.2f, 0.3f, 0.45f, 0.9f);

            var goldIcon = UIHelper.Image(Root, "GoldIcon", OriginalAssets.GetUi("bigbig_gold"));
            UIHelper.Place((RectTransform) goldIcon.transform, new Vector2(1f, 1f), new Vector2(40, 40), new Vector2(-146, -98));
            _coinText = UIHelper.Text(Root, "CoinText", "", 30, new Color(1f, 0.92f, 0.55f), FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place(_coinText.rectTransform, new Vector2(1f, 1f), new Vector2(110, 44), new Vector2(-104, -98));
            _coinText.gameObject.AddComponent<Outline>().effectColor = new Color(0.3f, 0.18f, 0.05f);
            _coinText.gameObject.GetComponent<Outline>().effectDistance = new Vector2(1.5f, -1.5f);
            if (_coinText != null) _coinText.text = SaveManager.Data.coins.ToString();

            // ===== 底部分数（槽上方，进度提示：消除剩余组数）=====
            _scoreText = UIHelper.Text(Root, "ScoreText", "--", 24, new Color(1f, 0.98f, 0.9f), FontStyle.Bold, TextAnchor.MiddleCenter);
            UIHelper.Place(_scoreText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(620, 36), new Vector2(0, -270));
            _scoreText.gameObject.AddComponent<Outline>().effectColor = new Color(0.25f, 0.15f, 0.05f);
            _scoreText.gameObject.GetComponent<Outline>().effectDistance = new Vector2(1.5f, -1.5f);
        }

        private void MakeBoardArea()
        {
            // 棋盘居中，顶栏下方；槽位+道具栏在底部
            var board = UIHelper.NewRect(Root, "BoardRoot");
            UIHelper.Place(board, new Vector2(0.5f, 0.5f), new Vector2(700, 720), new Vector2(0, 140));

            var slot = UIHelper.NewRect(Root, "SlotRoot");
            UIHelper.Place(slot, new Vector2(0.5f, 0.5f), new Vector2(700, 96), new Vector2(0, -320));

            // 槽底衬（原版槽位板）
            var plate = UIHelper.Image(slot, "Plate");
            UIHelper.Stretch((RectTransform)plate.transform);
            var pimg = plate.GetComponent<Image>();
            var plateSp = OriginalAssets.GetUi("dibuheisebanyuandi");
            if (plateSp != null) { pimg.sprite = plateSp; pimg.type = Image.Type.Sliced; }
            pimg.color = new Color(0.06f, 0.1f, 0.16f, 0.45f);

            // 工具条
            var tb = UIHelper.NewRect(Root, "ToolBar");
            UIHelper.Place(tb, new Vector2(0.5f, 0.5f), new Vector2(700, 140), new Vector2(0, -450));
            MakeToolBar(tb);
        }

        private void MakeToolBar(Transform parent)
        {
            string[] labels = { "合成", "刷新", "清扫", "冻结" };
            ToolType[] tools = { ToolType.Compose, ToolType.Shuffle, ToolType.Clear, ToolType.Freeze };
            string[] icons = { "skill_autoCombine", "skill_autoTurn", "skill_clear", "dongjie" };
            for (int i = 0; i < labels.Length; i++)
            {
                int ii = i;
                ToolType tool = tools[i];
                var bt = UIHelper.Button(parent, "Btn_" + labels[i], () =>
                {
                    if (_gamePanel != null && _gamePanel.Game != null) _gamePanel.Game.UseTool(tool);
                });
                // 4 按钮居中：总宽 3*120=360，起点 -180
                UIHelper.Place((RectTransform) bt.transform, new Vector2(0.5f, 0.5f),
                    new Vector2(128, 128), new Vector2(-180 + 120 * ii, 0));

                // 圆形蓝底（原版技能圆钮 Circle01 青蓝渐变）
                var bi = bt.gameObject.GetComponent<Image>();
                var bgSp = OriginalAssets.GetUi("Circle01");
                if (bgSp != null) { bi.sprite = bgSp; bi.color = new Color(0.2f, 0.5f, 0.85f, 0.92f); }
                else bi.color = new Color(0.22f, 0.5f, 0.82f);

                var iconSp = OriginalAssets.GetUi(icons[i]);
                if (iconSp != null)
                {
                    var ic = UIHelper.Image(bt.transform, "Icon", iconSp);
                    ic.preserveAspect = true;
                    UIHelper.Place((RectTransform) ic.transform, new Vector2(0.5f, 0.5f),
                        new Vector2(72, 72), new Vector2(0, 0));
                }
                else
                {
                    var t = UIHelper.Text(bt.transform, "Text", labels[i], 24, Color.white, FontStyle.Bold);
                    UIHelper.Stretch(t.rectTransform);
                }

                // 红角标（次数/不限）
                var badge = UIHelper.Text(bt.transform, "Badge", "+", 24,
                    Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
                var brt = badge.rectTransform;
                brt.anchorMin = brt.anchorMax = new Vector2(1f, 1f);
                brt.pivot = new Vector2(1f, 1f);
                brt.sizeDelta = new Vector2(56, 34);
                brt.anchoredPosition = new Vector2(2, -2);
                badge.gameObject.AddComponent<Outline>().effectColor = new Color(0.4f, 0.05f, 0.05f);
                badge.gameObject.GetComponent<Outline>().effectDistance = new Vector2(1.5f, -1.5f);
                _badges.Add(badge);

                // 技能冷却次数（原版：合成2 刷新3 清扫+ 冻结1；这里用存档道具库存）
                int cnt = StockOf(tool);
                badge.text = cnt < 0 ? "+" : cnt.ToString();
            }
        }

        /// <summary>道具库存：原版道具 1清 2刷 3合 4冻；返回 -1 表示不限（清扫原版为 +）。</summary>
        private int StockOf(ToolType tool)
        {
            // 原版清扫为免费无限（+）；其余按库存显示，这里默认给足演示次数
            if (tool == ToolType.Clear) return -1;
            int[] counts = { 0, 0, 3, 2, 1 };   // [1]=Clear(+) [2]=Shuffle(3) [3]=Compose(2) [4]=Freeze(1) 原版
            return counts[(int) tool];
        }

        /// <summary>倒计时刷新（mm:ss，冻结置蓝、将尽变红）。</summary>
        private void OnTimeChanged(float left, bool frozen)
        {
            if (_timeText == null) return;
            int s = Mathf.CeilToInt(left);
            if (s < 0) s = 0;
            _timeText.text = string.Format("{0:00}:{1:00}", s / 60, s % 60);
            _timeText.color = frozen
                ? new Color(0.5f, 0.85f, 1f)                          // 冻结=蓝
                : (s <= 30 ? new Color(1f, 0.45f, 0.35f) : Color.white); // 30s 内=红
        }

        /// <summary>暂停菜单（暂停/设置共用）：继续 / 重开 / 返回选关。</summary>
        private void ShowPauseMenu()
        {
            if (_overlay != null) { Object.Destroy(_overlay); _overlay = null; return; }
            if (_gamePanel == null || _gamePanel.Game == null) return;
            var g = _gamePanel.Game;
            bool wasPlaying = g.State == GameState.Playing;
            if (wasPlaying) g.TimeFrozen = true;   // 暂停=冻结倒计时

            _overlay = UIHelper.Image(Root, "PauseMask").gameObject;
            UIHelper.Stretch((RectTransform) _overlay.transform);
            _overlay.GetComponent<Image>().color = new Color(0, 0, 0, 0.62f);

            var panel = UIHelper.NewRect(_overlay.transform, "Panel");
            UIHelper.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(560, 420), new Vector2(0, 0));
            var board = OriginalAssets.GetUi("jiesuankuang_1");
            if (board != null) { var bi = panel.gameObject.AddComponent<Image>(); bi.sprite = board; bi.type = Image.Type.Sliced; }
            else panel.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.16f, 0.24f, 0.95f);

            var title = UIHelper.Text(panel, "Title", "暂停", 40, new Color(1f, 0.96f, 0.75f), FontStyle.Bold);
            UIHelper.Place(title.rectTransform, new Vector2(0.5f, 0.85f), new Vector2(400, 60), Vector2.zero);

            MakePauseButton(panel, "继续", () =>
            {
                Object.Destroy(_overlay); _overlay = null;
                if (g.State == GameState.Playing) g.TimeFrozen = false;
            }, new Vector2(0, -30));
            MakePauseButton(panel, "重开本关", () =>
            {
                Object.Destroy(_overlay); _overlay = null;
                StartLevel(LevelId);
            }, new Vector2(0, -120));
            MakePauseButton(panel, "返回选关", () =>
            {
                Object.Destroy(_overlay); _overlay = null;
                PanelManager.Instance.Pop();
            }, new Vector2(0, -210));
        }

        private void MakePauseButton(Transform parent, string label, System.Action onClick, Vector2 pos)
        {
            var bt = UIHelper.Button(parent, "Btn_" + label, onClick);
            UIHelper.Place((RectTransform) bt.transform, new Vector2(0.5f, 0.55f), new Vector2(340, 74), pos);
            var board = OriginalAssets.GetUi("button_active");
            var bi = bt.gameObject.GetComponent<Image>();
            if (board != null) { bi.sprite = board; bi.type = Image.Type.Sliced; bi.color = Color.white; }
            else bi.color = new Color(0.95f, 0.78f, 0.32f);
            var t = UIHelper.Text(bt.transform, "L", label, 30, new Color(0.35f, 0.2f, 0.05f), FontStyle.Bold);
            UIHelper.Stretch(t.rectTransform);
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
            if (_titleText != null) _titleText.text = $"第 {level.LevelNo} 关";
            if (_timeText != null)
                _timeText.text = string.Format("{0:00}:{1:00}",
                    (int) g.TimeLeft / 60, (int) g.TimeLeft % 60);
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

            var msg = UIHelper.Text(panel, "Msg", win ? "关卡完成！" : "失败啦…", 44,
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