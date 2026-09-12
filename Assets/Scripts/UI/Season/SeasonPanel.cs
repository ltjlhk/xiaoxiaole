using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>
    /// 赛季页（M3）：3 赛季 Tab，每赛季展示 活跃奖励阶梯（SeasonActivityRewardConfig）
    /// + 赛季任务列表（SeasonTaskConfig，进度/达标领取金币）。
    /// </summary>
    public class SeasonPanel : UIPanel
    {
        private int _tabIndex;
        private readonly List<Button> _tabButtons = new List<Button>();
        private Transform _listRoot;
        private readonly List<GameObject> _cells = new List<GameObject>();
        private List<SeasonInfo> _seasons = new List<SeasonInfo>();
        private static readonly List<SeasonActivityRewardInfo> _actCache = new List<SeasonActivityRewardInfo>();

        protected override void Build()
        {
            var bgTex = OriginalAssets.GetBackground("bg10");
            var bg = UIHelper.Image(Root, "BG", bgTex != null
                ? Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f), 100f)
                : null);
            UIHelper.Stretch((RectTransform)bg.transform);

            MakeTopBar();
            _seasons = LoadSeasons();
            MakeTabs();
            MakeList();
            SelectTab(0);
        }

        private void MakeTopBar()
        {
            var title = UIHelper.Text(Root, "Title", "赛季", 40, new Color(1f, 0.98f, 0.85f), FontStyle.Bold);
            UIHelper.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(600, 60), new Vector2(0, -36));
            title.gameObject.AddComponent<Outline>().effectColor = new Color(0.12f, 0.22f, 0.45f);
            title.gameObject.GetComponent<Outline>().effectDistance = new Vector2(2, -2);

            var back = UIHelper.Button(Root, "Back", () => PanelManager.Instance.Pop());
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
        }

        private static List<SeasonInfo> LoadSeasons()
        {
            var list = new List<SeasonInfo>();
            var raw = ConfigLoader.LoadRaw("SeasonConfig");
            if (!string.IsNullOrEmpty(raw))
                foreach (var kv in ConfigLoader.SplitTopLevel(raw))
                {
                    var s = ConfigLoader.FromJson<SeasonInfo>(kv.Value);
                    if (s != null) list.Add(s);
                }
            list.Sort((a, b) => a.Id.CompareTo(b.Id));
            return list;
        }

        private void MakeTabs()
        {
            var tabRoot = UIHelper.NewRect(Root, "Tabs");
            UIHelper.Place(tabRoot, new Vector2(0.5f, 1f), new Vector2(720, 108), new Vector2(0, -104));

            for (int i = 0; i < _seasons.Count; i++)
            {
                int idx = i;
                var bt = UIHelper.Button(tabRoot, "Tab_" + i, () => SelectTab(idx));
                UIHelper.Place((RectTransform)bt.transform, new Vector2(0.5f, 0.5f),
                    new Vector2(150, 90), new Vector2(-(Mathf.Max(1, _seasons.Count - 1) * 80f) + i * 160, 0));
                var board = OriginalAssets.GetUi("mp_board");
                if (board != null)
                {
                    var bi = bt.gameObject.GetComponent<Image>();
                    bi.sprite = board;
                    bi.type = Image.Type.Sliced;
                }
                var name = UIHelper.Text(bt.transform, "N",
                    (_seasons[i].Season ?? "赛季" + _seasons[i].Id).Length > 4
                        ? (_seasons[i].Season ?? "赛季").Substring(0, 4)
                        : (_seasons[i].Season ?? "赛季"),
                    22, new Color(0.45f, 0.25f, 0.08f), FontStyle.Bold);
                UIHelper.Stretch((RectTransform)name.transform);
                _tabButtons.Add(bt);
            }
        }

        private void MakeList()
        {
            var scGo = UIHelper.NewRect(Root, "Scroll");
            var sc = scGo.gameObject.AddComponent<ScrollRect>();
            UIHelper.Place(scGo, new Vector2(0.5f, 0.5f), new Vector2(720, 1080), new Vector2(0, -86));

            var vp = UIHelper.NewRect(scGo, "Viewport");
            var vrt = UIHelper.Stretch(vp);
            vp.gameObject.AddComponent<RectMask2D>();

            var content = UIHelper.NewRect(vp, "Content");
            var crt = (RectTransform)content;
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.anchoredPosition = Vector2.zero;

            sc.viewport = vrt;
            sc.content = crt;
            sc.horizontal = false;
            sc.vertical = true;
            sc.movementType = ScrollRect.MovementType.Clamped;
            sc.scrollSensitivity = 24f;
            sc.verticalNormalizedPosition = 1f;

            _listRoot = crt;
        }

        private void SelectTab(int idx)
        {
            if (idx < 0 || idx >= _seasons.Count) return;
            _tabIndex = idx;
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                if (_tabButtons[i] == null) continue;
                var img = _tabButtons[i].gameObject.GetComponent<Image>();
                if (img != null)
                    img.color = i == idx ? new Color(1f, 0.92f, 0.55f) : new Color(0.75f, 0.72f, 0.68f);
            }
            Rebuild(_seasons[idx]);
        }

        private void Rebuild(SeasonInfo s)
        {
            for (int i = _cells.Count - 1; i >= 0; i--)
                if (_cells[i] != null) SafeDestroy(_cells[i]);
            _cells.Clear();

            float y = 0;
            y = MakeActivitySection(s, y);
            y = MakeTaskSection(s, y);

            var crt = (RectTransform)_listRoot;
            crt.sizeDelta = new Vector2(720, y + 20);
            crt.anchoredPosition = Vector2.zero;
        }

        /// <summary>活跃奖励阶梯（Top 到目标活跃度可领取）。</summary>
        private float MakeActivitySection(SeasonInfo s, float y)
        {
            var rewards = ActRewardsOf(s.Id);
            if (rewards.Count == 0) return y;

            var head = UIHelper.Text(_listRoot, "ActHead", "活跃奖励", 30,
                new Color(1f, 0.96f, 0.7f), FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place(head.rectTransform, new Vector2(0.5f, 1f), new Vector2(680, 50), new Vector2(0, -y - 26));
            y += 60;

            foreach (var r in rewards)
            {
                int act = SaveManager.Data.activity;
                bool reached = act >= r.ActiveTarget;
                var row = UIHelper.NewRect(_listRoot, "Act_" + r.Id);
                UIHelper.Place(row, new Vector2(0.5f, 1f), new Vector2(680, 66), new Vector2(0, -y - 32));
                var board = OriginalAssets.GetUi("mp_board");
                var img = row.gameObject.AddComponent<Image>();
                if (board != null) { img.sprite = board; img.type = Image.Type.Sliced; }
                img.color = reached ? new Color(1f, 0.95f, 0.7f) : new Color(0.85f, 0.83f, 0.8f);

                // 奖励内容：ActiveReward [[itemId,count]]
                string rewardTxt = RewardText(r.ActiveReward) ?? "";
                var t = UIHelper.Text(row, "T", $"活跃 {r.ActiveTarget}  ·  {rewardTxt}", 22,
                    new Color(0.3f, 0.25f, 0.2f), FontStyle.Normal, TextAnchor.MiddleLeft);
                UIHelper.Place(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(470, 40), new Vector2(-15, 0));

                float total = r.ActiveTarget;
                float cur = Mathf.Min(total, act);
                var bar = UIHelper.NewRect(row, "Bar");
                UIHelper.Place(bar, new Vector2(0.5f, 0.5f), new Vector2(150, 12), new Vector2(230, 0));
                var bimg = bar.gameObject.AddComponent<Image>();
                bimg.color = new Color(0.15f, 0.2f, 0.3f, 0.8f);
                var fill = UIHelper.NewRect(bar, "Fill");
                ((RectTransform)fill).anchorMin = new Vector2(0, 0);
                ((RectTransform)fill).anchorMax = new Vector2(total > 0 ? cur / total : 0, 1);
                ((RectTransform)fill).offsetMin = ((RectTransform)fill).offsetMax = Vector2.zero;
                fill.gameObject.AddComponent<Image>().color = new Color(0.95f, 0.8f, 0.35f);

                _cells.Add(row.gameObject);
                y += 76;
            }
            return y;
        }

        /// <summary>赛季任务列表（进度/领取）。</summary>
        private float MakeTaskSection(SeasonInfo s, float y)
        {
            var tasks = TaskTracker.SeasonTasks(s.Id);
            if (tasks.Count == 0) return y;

            var head = UIHelper.Text(_listRoot, "TaskHead", "赛季任务", 30,
                new Color(1f, 0.96f, 0.7f), FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place(head.rectTransform, new Vector2(0.5f, 1f), new Vector2(680, 50), new Vector2(0, -y - 26));
            y += 60;

            foreach (var tk in tasks)
            {
                int prog = TaskTracker.ProgressOf(tk.Id);
                bool done = prog >= tk.TaskTarget;
                bool claimed = SaveManager.IsTaskClaimed(tk.Id);

                var row = UIHelper.NewRect(_listRoot, "Task_" + tk.Id);
                UIHelper.Place(row, new Vector2(0.5f, 1f), new Vector2(680, 70), new Vector2(0, -y - 34));
                var board = OriginalAssets.GetUi("mp_board");
                var img = row.gameObject.AddComponent<Image>();
                if (board != null) { img.sprite = board; img.type = Image.Type.Sliced; }
                img.color = claimed ? new Color(0.75f, 0.8f, 0.78f) : (done ? new Color(1f, 0.95f, 0.7f) : new Color(0.9f, 0.88f, 0.85f));

                var t = UIHelper.Text(row, "T",
                    (tk.TaskTxt ?? "任务") + $"  {prog}/{tk.TaskTarget}", 22,
                    new Color(0.3f, 0.25f, 0.2f), FontStyle.Normal, TextAnchor.MiddleLeft);
                UIHelper.Place(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(560, 40), new Vector2(-10, 0));

                if (claimed)
                {
                    var ok = UIHelper.Text(row, "OK", "已领取", 20,
                        new Color(0.3f, 0.45f, 0.3f), FontStyle.Bold);
                    UIHelper.Place(ok.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(90, 40), new Vector2(270, 0));
                }
                else if (done)
                {
                    var bt = UIHelper.Button(row, "Claim", () =>
                    {
                        if (SaveManager.ClaimSeasonTask(tk.Id, tk.TaskReward))
                            Rebuild(_seasons[_tabIndex]);
                    });
                    UIHelper.Place((RectTransform)bt.transform, new Vector2(0.5f, 0.5f), new Vector2(130, 44), new Vector2(270, 0));
                    bt.gameObject.GetComponent<Image>().color = new Color(0.95f, 0.78f, 0.32f);
                    var cl = UIHelper.Text(bt.transform, "L", $"领 {tk.TaskReward}", 20,
                        new Color(0.3f, 0.18f, 0.05f), FontStyle.Bold);
                    UIHelper.Stretch(cl.rectTransform);
                }
                else
                {
                    var lb = UIHelper.Text(row, "L", "未达成", 20,
                        new Color(0.5f, 0.45f, 0.4f), FontStyle.Bold);
                    UIHelper.Place(lb.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(90, 40), new Vector2(270, 0));
                }

                _cells.Add(row.gameObject);
                y += 80;
            }
            return y;
        }

        // ===== 活跃奖励配置 =====
        private static List<SeasonActivityRewardInfo> ActRewardsOf(int seasonId)
        {
            if (_actCache.Count == 0)
            {
                var raw = ConfigLoader.LoadRaw("SeasonActivityRewardConfig");
                if (!string.IsNullOrEmpty(raw))
                    foreach (var kv in ConfigLoader.SplitTopLevel(raw))
                    {
                        var r = ConfigLoader.FromJson<SeasonActivityRewardInfo>(kv.Value);
                        if (r != null)
                        {
                            // JsonUtility 不支持嵌套数组，手动校准活跃奖励
                            r.ActiveReward = ConfigLoader.ParseNestedIntLists(ConfigLoader.ExtractField(kv.Value, "ActiveReward"));
                            _actCache.Add(r);
                        }
                    }
                _actCache.Sort((a, b) => a.ActiveTarget.CompareTo(b.ActiveTarget));
            }
            var list = new List<SeasonActivityRewardInfo>();
            foreach (var r in _actCache)
                if (r.Season == seasonId) list.Add(r);
            return list;
        }

        private static string RewardText(List<List<int>> rewards)
        {
            if (rewards == null || rewards.Count == 0) return "";
            var parts = new List<string>();
            foreach (var rw in rewards)
                if (rw != null && rw.Count >= 2)
                    parts.Add(ItemName(rw[0]) + "×" + rw[1]);
            return string.Join(" + ", parts);
        }

        private static string ItemName(int itemId)
        {
            switch (itemId)
            {
                case 0: return "金币";
                case 1: return "清扫";
                case 2: return "刷新";
                case 3: return "合成";
                case 4: return "冻结";
                default: return "碎片" + (itemId % 100);
            }
        }

        public override void Refresh()
        {
            if (_seasons.Count == 0) return;
            Rebuild(_seasons[Mathf.Clamp(_tabIndex, 0, _seasons.Count - 1)]);
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}