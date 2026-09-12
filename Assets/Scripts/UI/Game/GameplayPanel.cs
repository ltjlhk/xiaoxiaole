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
        private Text _starText;
        private int _levelNo = 1;
        private bool _settled;
        private PuzzleGame _subscribedGame;
        private SkillTriggerController _skillCtl;
        private readonly List<Text> _badges = new List<Text>();   // 技能角标

        protected override void Build()
        {
            // 原版 level2：3D 场景（正交俯视相机+围墙地板+7槽）+ UI overlay，无 UI 背景图
            Scene3D.Ensure();
            MakeTopBar();
            MakeBoardArea();

            _gamePanel = Root.gameObject.GetComponent<GamePanel>() ?? Root.gameObject.AddComponent<GamePanel>();
            _gamePanel.scoreText = _scoreText;
            _gamePanel.titleText = _titleText;
            _gamePanel.onTimeChanged = OnTimeChanged;

            StartLevel(LevelId);
        }

        private void MakeTopBar()
        {
            // ===== 顶栏（原版 res_Game 坐标：锚=顶中心，y 负向下）=====
            // 左上：暂停 imgPause (-334,-111) 66.5×67.9 sp=suspend
            var pause = UIHelper.Button(Root, "Pause", () => ShowPauseMenu());
            UIHelper.Place((RectTransform) pause.transform, new Vector2(0.5f, 1f), new Vector2(67, 68), new Vector2(-334, -111));
            var pi = pause.gameObject.GetComponent<Image>();
            var pIcon = OriginalAssets.GetUi("suspend") ?? OriginalAssets.GetUi("resetting_icon");
            if (pIcon != null) { pi.sprite = pIcon; pi.color = Color.white; }
            else pi.color = new Color(0.2f, 0.3f, 0.45f, 0.9f);

            // 左侧：倒计时文字 txtOverTime (-235,-109) 160×80 "00:00"（白字深描边，无底图）
            _timeText = UIHelper.Text(Root, "TimeText", "08:00", 34, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UIHelper.Place(_timeText.rectTransform, new Vector2(0.5f, 1f), new Vector2(160, 80), new Vector2(-235, -109));
            var tOut = _timeText.gameObject.AddComponent<Outline>();
            tOut.effectColor = new Color(0.12f, 0.2f, 0.1f);
            tOut.effectDistance = new Vector2(2, -2);

            // 中央：倒计时牌 countdownbase (0,-100) 284×132（原版 316×147×0.9）+ 进度条 + 关卡字 + 星
            var cd = UIHelper.Image(Root, "CountdownBase", OriginalAssets.GetUi("countdownbase"));
            UIHelper.Place((RectTransform) cd.transform, new Vector2(0.5f, 1f), new Vector2(284, 132), new Vector2(0, -100));
            cd.color = Color.white;

            // imgLevelPro (0,-90) 220×34 sp=blueprogressbar（Filled 横向，段内进度）
            var pro = UIHelper.Image(Root, "LevelPro", OriginalAssets.GetUi("blueprogressbar"));
            UIHelper.Place((RectTransform) pro.transform, new Vector2(0.5f, 1f), new Vector2(220, 34), new Vector2(0, -90));
            pro.type = Image.Type.Filled;
            pro.fillMethod = Image.FillMethod.Horizontal;
            pro.fillAmount = Mathf.Clamp01(((LevelId - 1) % 20) / 20f);
            pro.color = Color.white;

            // txtLevel (0,-90) 140×50 "第X关"
            _titleText = UIHelper.Text(Root, "TitleText", "第 1 关", 30, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UIHelper.Place(_titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(140, 50), new Vector2(0, -90));
            var o1 = _titleText.gameObject.AddComponent<Outline>();
            o1.effectColor = new Color(0.25f, 0.12f, 0.02f);
            o1.effectDistance = new Vector2(1.5f, -1.5f);

            // imgStar (-34,-129) 35×36 + txtStar (20,-128) 100×55（累计星数）
            var starSp = OriginalAssets.GetUi("star") ?? OriginalAssets.GetUi("Star01");
            var stImg = UIHelper.Image(Root, "StarIcon", starSp);
            UIHelper.Place((RectTransform) stImg.transform, new Vector2(0.5f, 1f), new Vector2(35, 36), new Vector2(-34, -129));
            stImg.preserveAspect = true;
            _starText = UIHelper.Text(Root, "StarText", "0", 28, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place(_starText.rectTransform, new Vector2(0.5f, 1f), new Vector2(100, 55), new Vector2(20, -128));
            var o2 = _starText.gameObject.AddComponent<Outline>();
            o2.effectColor = new Color(0.25f, 0.12f, 0.02f);
            o2.effectDistance = new Vector2(1.5f, -1.5f);

            // ===== 右上：设置 + 金币（原版右上留空，为复刻附加功能保留）=====
            var stBtn = UIHelper.Button(Root, "Settings", () => ShowPauseMenu());
            UIHelper.Place((RectTransform) stBtn.transform, new Vector2(0.5f, 1f), new Vector2(67, 68), new Vector2(-100, -111));
            var si = stBtn.gameObject.GetComponent<Image>();
            var sIcon = OriginalAssets.GetUi("wx_shezhi");
            if (sIcon != null) { si.sprite = sIcon; si.color = Color.white; }
            else si.color = new Color(0.2f, 0.3f, 0.45f, 0.9f);

            var goldIcon = UIHelper.Image(Root, "GoldIcon", OriginalAssets.GetUi("bigbig_gold"));
            UIHelper.Place((RectTransform) goldIcon.transform, new Vector2(0.5f, 1f), new Vector2(40, 40), new Vector2(-185, -111));
            _coinText = UIHelper.Text(Root, "CoinText", "", 30, new Color(1f, 0.92f, 0.55f), FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place(_coinText.rectTransform, new Vector2(0.5f, 1f), new Vector2(120, 44), new Vector2(-155, -111));
            var o3 = _coinText.gameObject.AddComponent<Outline>();
            o3.effectColor = new Color(0.3f, 0.18f, 0.05f);
            o3.effectDistance = new Vector2(1.5f, -1.5f);
            if (_coinText != null) _coinText.text = SaveManager.Data.coins.ToString();

            // ===== combo 区（原版 y=-457）：剩余组数提示 =====
            _scoreText = UIHelper.Text(Root, "ScoreText", "--", 24, new Color(1f, 0.98f, 0.9f), FontStyle.Bold, TextAnchor.MiddleCenter);
            UIHelper.Place(_scoreText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(620, 36), new Vector2(0, -457));
            var o4 = _scoreText.gameObject.AddComponent<Outline>();
            o4.effectColor = new Color(0.25f, 0.15f, 0.05f);
            o4.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private void MakeBoardArea()
        {
            // ===== 原版 level2 3D 牌区（Scene3D 已建：相机/围墙/地板/Droplocation 7 槽/BlockParent）=====
            // 顶栏底(+534) ↔ 后墙屏幕 y=+513；槽区 y=-454 ↔ 道具栏顶(-507)，严丝合缝。

            // 底部衬条 imgBtnDi：stretch-bottom h=100 y=50，sp=Home_panel_01
            var btnDi = UIHelper.Image(Root, "BtnDi", OriginalAssets.GetUi("Home_panel_01"));
            var brt = (RectTransform)btnDi.transform;
            brt.anchorMin = new Vector2(0, 0);
            brt.anchorMax = new Vector2(1, 0);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(0, 100);
            brt.anchoredPosition = new Vector2(0, 50);

            // 道具栏 prop：(0,80,底锚) 750×160 → 屏幕 y=-587
            var tb = UIHelper.NewRect(Root, "ToolBar");
            UIHelper.Place(tb, new Vector2(0.5f, 0f), new Vector2(750, 160), new Vector2(0, 80));
            MakeToolBar(tb);
        }

        private void MakeToolBar(Transform parent)
        {
            string[] labels = { "合成", "刷新", "清扫", "冻结" };
            ToolType[] tools = { ToolType.Compose, ToolType.Shuffle, ToolType.Clear, ToolType.Freeze };
            string[] icons = { "compose", "shuffle", "clear", "freeze" };          // 原版 sp 名
            string[] iconsAlt = { "skill_autoCombine", "skill_autoTurn", "skill_clear", "dongjie" };
            // 原版 prop1-4：x=±258.8/±86.2，y=3，145×145
            float[] xs = { -258.8f, -86.2f, 86.2f, 258.8f };
            for (int i = 0; i < labels.Length; i++)
            {
                int ii = i;
                ToolType tool = tools[i];
                var bt = UIHelper.Button(parent, "Btn_" + labels[i], () =>
                {
                    if (_gamePanel != null && _gamePanel.Game != null) _gamePanel.Game.UseTool(tool);
                });
                UIHelper.Place((RectTransform) bt.transform, new Vector2(0.5f, 0.5f),
                    new Vector2(145, 145), new Vector2(xs[ii], 3));

                // 圆底：原版 imgBg=dibuheisebanyuandi 161×157×0.9
                var bi = bt.gameObject.GetComponent<Image>();
                var bgSp = OriginalAssets.GetUi("dibuheisebanyuandi");
                if (bgSp != null) { bi.sprite = bgSp; bi.color = Color.white; }
                else { bi.sprite = OriginalAssets.GetUi("Circle01"); bi.color = new Color(0.2f, 0.5f, 0.85f, 0.92f); }

                var iconSp = OriginalAssets.GetUi(icons[i]) ?? OriginalAssets.GetUi(iconsAlt[i]);
                if (iconSp != null)
                {
                    var ic = UIHelper.Image(bt.transform, "Icon", iconSp);
                    ic.preserveAspect = true;
                    // 原版 imgIcon (0,13)，约 80×95
                    UIHelper.Place((RectTransform) ic.transform, new Vector2(0.5f, 0.5f),
                        new Vector2(78, 90), new Vector2(0, 13));
                }

                // 名称：原版 txtName (0,-38) 147×50
                var nameT = UIHelper.Text(bt.transform, "Name", labels[i], 24, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
                UIHelper.Place(nameT.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(147, 50), new Vector2(0, -38));
                var no = nameT.gameObject.AddComponent<Outline>();
                no.effectColor = new Color(0.25f, 0.12f, 0.02f);
                no.effectDistance = new Vector2(1.5f, -1.5f);

                // 红角标：原版 txtNum (50.5,42) 65×55 "+"
                var badge = UIHelper.Text(bt.transform, "Badge", "+", 26,
                    Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
                UIHelper.Place(badge.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(65, 55), new Vector2(50.5f, 42));
                var bo = badge.gameObject.AddComponent<Outline>();
                bo.effectColor = new Color(0.4f, 0.05f, 0.05f);
                bo.effectDistance = new Vector2(1.5f, -1.5f);
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

        /// <summary>暂停/设置共用：压栈复刻暂停面板 res_PauseView（暂停=冻结倒计时）。</summary>
        private void ShowPauseMenu()
        {
            if (_gamePanel == null || _gamePanel.Game == null) return;
            var g = _gamePanel.Game;
            if (g.State == GameState.Playing) g.TimeFrozen = true;

            PanelManager.Instance.Push<PauseViewPanel>(p =>
            {
                p.OnResume = () => { if (g.State == GameState.Playing) g.TimeFrozen = false; };
                p.OnRestart = () =>
                {
                    if (g.State == GameState.Playing) g.TimeFrozen = false;
                    StartLevel(LevelId);
                };
                // 回调先于面板自身 Pop 执行：此处 Pop 弹掉 PauseView，按钮再 Pop 弹掉本面板 → 回主城
                p.OnHome = () => PanelManager.Instance.Pop();
            });
        }

        /// <summary>启动指定关卡（面板内切换关卡也走这里）。</summary>
        public void StartLevel(int levelId)
        {
            LevelId = levelId;
            if (_scoreText == null || _gamePanel == null) return;

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
            if (level != null) _levelNo = level.LevelNo;
            if (_titleText != null) _titleText.text = $"第 {level.LevelNo} 关";
            if (_starText != null) _starText.text = SaveManager.Data.stars.ToString();
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

        /// <summary>结算：压栈复刻结算面板 res_Over（winPanel/failPanel 按 Win 切换）。</summary>
        private void ShowSettle(bool win)
        {
            PanelManager.Instance.Push<OverPanel>(p =>
            {
                p.Win = win;
                p.LevelNo = _levelNo;
                p.Stars = SaveManager.Data.stars;
                p.OnNext = OnNextLevel;
                p.OnRetry = OnRetry;
                // 回调先于面板自身 Pop 执行：此处 Pop 弹掉 OverPanel，按钮再 Pop 弹掉本面板 → 回主城
                p.OnHome = () => PanelManager.Instance.Pop();
            });
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
            // 游戏内变化（如返回上一面板）无需重建；金币/星数可能变化（结算领奖）
            if (_coinText != null) _coinText.text = SaveManager.Data.coins.ToString();
            if (_starText != null) _starText.text = SaveManager.Data.stars.ToString();
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