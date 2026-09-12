using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.UI;

namespace Xio.Game
{
    /// <summary>关卡主面板：棋盘（亮格=拼图碎片，暗格=卡牌叠堆）+ 7 槽 + 分数。
    /// 羊了个羊式叠堆：每暗格一叠卡，只有堆顶可点；3 同图案消除点亮该格。</summary>
    public class GamePanel : MonoBehaviour
    {
        public PuzzleGame Game { get; private set; }

        [Header("引用（Bootstrap 注入）")]
        public Transform boardRoot;   // 棋盘（拼图+卡叠）
        public Transform slotRoot;
        public Text scoreText;
        public Text titleText;

        private readonly Dictionary<int, Image> _cellImages = new Dictionary<int, Image>(); // 格号->底
        private readonly List<Image> _slotImages = new List<Image>();
        private readonly Dictionary<Card, Image> _cardImages = new Dictionary<Card, Image>();
        private readonly Dictionary<int, Text> _stackBadge = new Dictionary<int, Text>();   // 格号->叠数
        private Sprite[] _fragSprites;

        // 本局统计（EventCenter/任务）
        public int MatchCount;
        public int ClearCount;
        public int ShuffleCount;
        public int ComposeCount;
        public int FreezeCount;

        /// <summary>编辑模式用 DestroyImmediate（批处理验证），Play 用 Destroy。</summary>
        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        /// <summary>每帧驱动：倒计时（原版 08:00）+ 冻结状态。时间耗尽由 Game 判负。</summary>
        private void Update()
        {
            if (Game == null) return;
            Game.Tick(Time.deltaTime);
            if (onTimeChanged != null && Game.State == GameState.Playing)
                onTimeChanged(Game.TimeLeft, Game.TimeFrozen);
        }

        /// <summary>时间展示回调（GameplayPanel 顶栏注入）。</summary>
        public System.Action<float, bool> onTimeChanged;

        public void Init(PuzzleLevel level, List<string> flowerPool, Texture2D puzzleTex)
        {
            MatchCount = ClearCount = ShuffleCount = ComposeCount = FreezeCount = 0;
            Game = new PuzzleGame();
            Game.StartGame(level, flowerPool);
            BuildBoard(level);
            BuildSlots();
            Game.OnCardCollected += OnCardCollected;
            Game.OnCellLit += OnCellLit;
            Game.OnSlotChanged += OnSlotChanged;
            Game.OnToolUsed += OnToolUsed;
            Game.OnPatternMatched += OnPatternMatched;
            Game.OnStateChanged += OnStateChanged;
            // 标题由 GameplayPanel 设置（第 X 关），此处不覆盖
            UpdateScoreText();
        }

        private void BuildFragments(PuzzleLevel level, Texture2D tex)
        {
            // 纯叠塔模式不再渲染拼图碎片（保留空实现）
        }

        // ===== 棋盘：纯叠塔（羊了个羊式）=====
        private void BuildBoard(PuzzleLevel level)
        {
            foreach (var kv in _cellImages) if (kv.Value != null) SafeDestroy(kv.Value.gameObject);
            foreach (var kv in _stackBadge) if (kv.Value != null) SafeDestroy(kv.Value.gameObject);
            foreach (var kv in _cardImages) if (kv.Value != null) SafeDestroy(kv.Value.gameObject);
            _cellImages.Clear(); _stackBadge.Clear(); _cardImages.Clear();

            RectTransform rt = boardRoot as RectTransform;
            float w = rt.rect.width; if (w < 10) w = rt.sizeDelta.x; if (w < 10) w = 660;
            float h = rt.rect.height; if (h < 10) h = rt.sizeDelta.y; if (h < 10) h = 700;

            // 叠塔位置：按行列错落排布（近大远小），每叠一沓卡
            int rows = level.Rows, cols = level.Cols;
            float cw = w / cols, ch = h / rows;
            // 卡片比格子大：让相邻卡片重叠（叠塔效果）
            float cardW = Mathf.Min(cw * 1.05f, ch * 0.85f);
            float cardH = cardW * 1.2f;

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    int g = r * cols + c;
                    // 叠位底板（淡白描边，非碎片）
                    var go = new GameObject("Cell_" + g, typeof(RectTransform), typeof(Image));
                    var img = go.GetComponent<Image>();
                    var crt = (RectTransform)go.transform;
                    crt.SetParent(boardRoot, false);
                    crt.sizeDelta = new Vector2(cw - 4, ch - 4);
                    crt.anchoredPosition = new Vector2((c + 0.5f) * cw - w / 2, h / 2 - (r + 0.5f) * ch);
                    img.color = new Color(0.94f, 0.9f, 0.82f, 0.16f);
                    _cellImages[g] = img;

                    // 叠堆：从底层到顶层依次绘制（顶在上，可见最多）
                    if (Game.Stacks.TryGetValue(g, out var st))
                        for (int k = st.Count - 1; k >= 0; k--)
                            MakeStackCard(st[k], g, crt, cardW, cardH, k, st.Count);
                }
        }

        private void MakeStackCard(Card card, int g, RectTransform cellRt, float cardW, float cardH,
            int depth = 0, int totalCount = 1)
        {
            // 叠堆层次：底层下移更多(露上缘)，顶层在 0 且完整可见
            float stackOffset = (totalCount - 1 - depth) * -14f;   // 底层 -14*n px

            var go = new GameObject("Card_" + g + "_" + depth, typeof(RectTransform), typeof(Image), typeof(Button));
            var img = go.GetComponent<Image>();
            var crt = (RectTransform)go.transform;
            crt.SetParent(cellRt, false);
            crt.sizeDelta = new Vector2(cardW, cardH);
            crt.anchoredPosition = new Vector2(0, stackOffset);
            crt.localRotation = Quaternion.Euler(0, 0, (Mathf.PerlinNoise(g * 1.7f + depth, 3f) - 0.5f) * 4f);

            // ===== 3D 厚度效果（原版青色边）=====
            // 底层投影（大范围漫射）
            var shGo = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            var sh = shGo.GetComponent<Image>();
            var shrt = (RectTransform)shGo.transform;
            shrt.SetParent(crt, false);
            shrt.anchorMin = shrt.anchorMax = new Vector2(0.5f, 0.5f);
            shrt.sizeDelta = crt.sizeDelta + new Vector2(8, 8);
            shrt.anchoredPosition = new Vector2(4, -6);
            sh.color = new Color(0, 0, 0, 0.28f);

            // 青色厚度边（右+下，模拟 3D 侧面）
            var edgeGo = new GameObject("Edge3D", typeof(RectTransform), typeof(Image));
            var edge = edgeGo.GetComponent<Image>();
            var ert = (RectTransform)edgeGo.transform;
            ert.SetParent(crt, false);
            ert.anchorMin = ert.anchorMax = new Vector2(0.5f, 0.5f);
            ert.sizeDelta = crt.sizeDelta + new Vector2(6, 6);
            ert.anchoredPosition = new Vector2(3, -3);
            edge.color = new Color(0.2f, 0.72f, 0.78f, 0.9f);  // 青色

            // 卡底（白色圆角，原版白底卡面）
            var frameSp = OriginalAssets.GetUi("tools_frame");
            if (frameSp != null) { img.sprite = frameSp; img.type = Image.Type.Sliced; }
            img.color = new Color(0.98f, 0.97f, 0.94f, 1f);

            // 花牌图案（居中，保留原版贴图）
            var flower = OriginalAssets.Get("flower", card.TexName);
            if (flower != null)
            {
                var iGo = new GameObject("Face", typeof(RectTransform), typeof(Image));
                var iimg = iGo.GetComponent<Image>();
                iimg.sprite = flower;
                iimg.preserveAspect = true;
                var irt = (RectTransform)iGo.transform;
                irt.SetParent(crt, false);
                irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.sizeDelta = new Vector2(cardW * 0.78f, cardW * 0.78f);
                irt.anchoredPosition = new Vector2(0, cardH * 0.04f);
            }

            // 仅顶层可点（下层不响应点击）
            bool top = depth == 0;
            var btn = go.GetComponent<Button>();
            btn.interactable = top;
            if (top)
            {
                var cardRef = card;
                btn.onClick.AddListener(() => OnStackClick(cardRef, g));
            }
            _cardImages[card] = img;
        }

        private void OnStackClick(Card card, int g)
        {
            if (Game.TopCard(g) != card) return; // 只可点堆顶
            if (Game.ClickCard(card))
            {
                if (_cardImages.TryGetValue(card, out var img) && img != null)
                {
                    _cardImages.Remove(card);
                    SafeDestroy(img.gameObject);
                }
                // 刷新角标与下一张顶卡
                RefreshStack(g);
                AudioManager.Inst.PlayPick();
            }
        }

        private void RefreshStack(int g)
        {
            // 整叠重建（叠高变了）：先销毁该格全部卡再按新叠重绘
            if (_cellImages.TryGetValue(g, out var cellImg) && cellImg != null)
            {
                var crtCell = (RectTransform) cellImg.transform;
                for (int c = crtCell.childCount - 1; c >= 0; c--)
                    SafeDestroy(crtCell.GetChild(c).gameObject);
            }
            if (Game.Stacks.TryGetValue(g, out var st) && st.Count > 0)
            {
                if (_cellImages.TryGetValue(g, out var cellImg2) && cellImg2 != null)
                {
                    var crt = (RectTransform)cellImg2.transform;
                    float cardW = crt.sizeDelta.x * 0.92f, cardH = cardW * 1.2f;
                    for (int k = st.Count - 1; k >= 0; k--)
                        MakeStackCard(st[k], g, crt, cardW, cardH, k, st.Count);
                }
            }
        }

        private void OnCellLit(int g)
        {
            // 纯叠塔：消除后该叠清空，底板经络即淡去
            if (_cellImages.TryGetValue(g, out var img) && img != null)
            {
                RefreshStack(g);
                img.color = new Color(1f, 1f, 1f, 0.05f);
            }
            AudioManager.Inst.PlayLight();
        }

        private void OnPatternMatched(int pid, string flowerTex)
        {
            MatchCount++;
            AudioManager.Inst.PlayMatch(flowerTex);
            EventCenter.OnMatch?.Invoke(pid);
        }

        private void OnCardCollected(Card c) { RefreshSlots(); UpdateScoreText(); }
        private void OnSlotChanged(int count, int cap) { RefreshSlots(); }

        private void OnToolUsed(ToolType t)
        {
            AudioManager.Inst.PlayTool(t);
            // 原版任务类型：5清扫 6刷新 7合成 8冻结
            EventCenter.OnToolUsed?.Invoke((int)t + 4);
            switch (t)
            {
                case ToolType.Clear: ClearCount++; break;
                case ToolType.Shuffle: ShuffleCount++; break;
                case ToolType.Compose: ComposeCount++; break;
                case ToolType.Freeze: FreezeCount++; break;
            }
            // 重建棋盘（叠堆变了）
            BuildBoard(Game.Level);
            RefreshSlots();
        }

        // ===== 槽位 =====
        private void BuildSlots()
        {
            for (int i = _slotImages.Count - 1; i >= 0; i--)
                if (_slotImages[i] != null) SafeDestroy(_slotImages[i].gameObject);
            _slotImages.Clear();

            RectTransform rt = slotRoot as RectTransform;
            float w = rt.rect.width; if (w < 10) w = rt.sizeDelta.x; if (w < 10) w = 690;
            float h = rt.rect.height; if (h < 10) h = rt.sizeDelta.y; if (h < 10) h = 110;
            float cw = w / PuzzleGame.SlotCapacity;

            // 原版深绿半透明托盘（已有 Plate 由 GameplayPanel 建）
            for (int i = 0; i < PuzzleGame.SlotCapacity; i++)
            {
                var go = new GameObject("Slot_" + i, typeof(RectTransform), typeof(Image));
                var img = go.GetComponent<Image>();
                var crt = (RectTransform)go.transform;
                crt.SetParent(slotRoot, false);
                float cw2 = Mathf.Min(cw - 8, h - 6);
                crt.sizeDelta = new Vector2(cw2, cw2 * 1.18f);
                crt.anchoredPosition = new Vector2((i + 0.5f) * cw - w / 2, 0);
                var baseSp = OriginalAssets.GetUi("tools_frame");
                if (baseSp != null) { img.sprite = baseSp; img.type = Image.Type.Sliced; }
                img.color = new Color(0.1f, 0.2f, 0.14f, 0.55f);  // 深绿半透明
                _slotImages.Add(img);

                // 竖线分隔
                if (i < PuzzleGame.SlotCapacity - 1)
                {
                    var div = new GameObject("Div", typeof(RectTransform), typeof(Image));
                    var dimg = div.GetComponent<Image>();
                    var drt = (RectTransform)div.transform;
                    drt.SetParent(slotRoot, false);
                    drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.5f);
                    drt.sizeDelta = new Vector2(2, h - 10);
                    drt.anchoredPosition = new Vector2((i + 1) * cw - w / 2, 0);
                    dimg.color = new Color(1f, 1f, 1f, 0.12f);
                }
            }
            RefreshSlots();
        }

        private void RefreshSlots()
        {
            for (int i = 0; i < _slotImages.Count; i++)
            {
                var img = _slotImages[i];
                for (int c = img.transform.childCount - 1; c >= 0; c--)
                    SafeDestroy(img.transform.GetChild(c).gameObject);
                var crt = (RectTransform)img.transform;
                if (i < Game.Slots.Count)
                {
                    var card = Game.Slots[i];
                    img.color = Color.white;
                    var flower = OriginalAssets.Get("flower", card.TexName);
                    if (flower != null)
                    {
                        var iGo = new GameObject("Face", typeof(RectTransform), typeof(Image));
                        var iimg = iGo.GetComponent<Image>();
                        iimg.sprite = flower;
                        iimg.preserveAspect = true;
                        var irt = (RectTransform)iGo.transform;
                        irt.SetParent(crt, false);
                        irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
                        irt.sizeDelta = new Vector2(crt.sizeDelta.x * 0.82f, crt.sizeDelta.x * 0.82f);
                        irt.anchoredPosition = Vector2.zero;
                    }
                }
                else img.color = new Color(0.1f, 0.2f, 0.14f, 0.55f);
            }
        }

        private void UpdateScoreText()
        {
            if (scoreText != null)
                scoreText.text = $"分数 {Game.Score}  |  剩 {Game.PendingCells.Count}";
        }

        private void OnStateChanged(GameState s)
        {
            if (s == GameState.Win)
            {
                if (scoreText != null) scoreText.text = "拼图完成！得分 " + Game.Score;
                if (Game.Level.PuzzleReward != null)
                    foreach (var rw in Game.Level.PuzzleReward)
                        if (rw.Count >= 2)
                        {
                            if (rw[0] == 0) SaveManager.AddCoins(rw[1]);   // itemId 0 = 金币
                            else SaveManager.AddItem(rw[0], rw[1]);          // 1-4 道具 / 101+ 碎片
                        }
            }
            else if (s == GameState.Lose)
            {
                bool timeout = Game.TimeLeft <= 0f;
                if (scoreText != null) scoreText.text = timeout ? "时间到…再试一次" : "槽位满了…用道具或重开";
                AudioManager.Inst.PlayLose();
            }
        }
    }
}
