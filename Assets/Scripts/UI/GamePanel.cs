using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

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

        /// <summary>编辑模式用 DestroyImmediate（批处理验证），Play 用 Destroy。</summary>
        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        public void Init(PuzzleLevel level, List<string> flowerPool, Texture2D puzzleTex)
        {
            Game = new PuzzleGame();
            Game.StartGame(level, flowerPool);
            BuildFragments(level, puzzleTex);
            BuildBoard(level);
            BuildSlots();
            Game.OnCardCollected += OnCardCollected;
            Game.OnCellLit += OnCellLit;
            Game.OnSlotChanged += OnSlotChanged;
            Game.OnToolUsed += OnToolUsed;
            Game.OnPatternMatched += OnPatternMatched;
            Game.OnStateChanged += OnStateChanged;
            UpdateScoreText();
            if (titleText != null) titleText.text = "就你会消除";
        }

        private void BuildFragments(PuzzleLevel level, Texture2D tex)
        {
            int cols = Mathf.Max(1, level.Cols), rows = Mathf.Max(1, level.Rows);
            _fragSprites = new Sprite[cols * rows];
            if (tex == null) return;
            float fw = tex.width / (float)cols, fh = tex.height / (float)rows;
            for (int idx = 0; idx < _fragSprites.Length; idx++)
            {
                int r = idx / cols, c = idx % cols;
                var rect = new Rect(c * fw, tex.height - (r + 1) * fh, fw, fh);
                _fragSprites[idx] = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
            }
        }

        // ===== 棋盘：亮格显示碎片 / 暗格显示卡叠堆顶 =====
        private void BuildBoard(PuzzleLevel level)
        {
            foreach (var kv in _cellImages) if (kv.Value != null) SafeDestroy(kv.Value.gameObject);
            foreach (var kv in _stackBadge) if (kv.Value != null) SafeDestroy(kv.Value.gameObject);
            foreach (var kv in _cardImages) if (kv.Value != null) SafeDestroy(kv.Value.gameObject);
            _cellImages.Clear(); _stackBadge.Clear(); _cardImages.Clear();

            RectTransform rt = boardRoot as RectTransform;
            float w = rt.rect.width; if (w < 10) w = rt.sizeDelta.x; if (w < 10) w = 600;
            float h = rt.rect.height; if (h < 10) h = rt.sizeDelta.y; if (h < 10) h = 740;
            int rows = level.Rows, cols = level.Cols;
            float cw = w / cols, ch = h / rows;
            float cardW = Mathf.Min(cw * 0.92f, ch * 0.82f);
            float cardH = cardW * 1.18f;

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    int g = r * cols + c;
                    // 格底
                    var go = new GameObject("Cell_" + g, typeof(RectTransform), typeof(Image));
                    var img = go.GetComponent<Image>();
                    var crt = (RectTransform)go.transform;
                    crt.SetParent(boardRoot, false);
                    crt.sizeDelta = new Vector2(cw - 3, ch - 3);
                    crt.anchoredPosition = new Vector2((c + 0.5f) * cw - w / 2, h / 2 - (r + 0.5f) * ch);
                    RefreshCell(img, g);
                    _cellImages[g] = img;

                    // 暗格上的卡叠堆顶
                    var top = Game.TopCard(g);
                    if (top != null) MakeStackCard(top, g, crt, cardW, cardH);
                }
        }

        private void RefreshCell(Image img, int g)
        {
            int frag = (Game.Level.PuzzleStart != null && g < Game.Level.PuzzleStart.Count)
                       ? Game.Level.PuzzleStart[g] : g;
            bool lit = Game.LitCells[g];
            if (lit && _fragSprites != null && frag < _fragSprites.Length && _fragSprites[frag] != null)
            {
                img.sprite = _fragSprites[frag];
                img.color = Color.white;
            }
            else
            {
                img.sprite = null;
                img.color = new Color(0.12f, 0.16f, 0.24f, 0.88f); // 暗格深色底
            }
        }

        private void MakeStackCard(Card card, int g, RectTransform cellRt, float cardW, float cardH)
        {
            var go = new GameObject("Card_" + g, typeof(RectTransform), typeof(Image), typeof(Button));
            var img = go.GetComponent<Image>();
            var crt = (RectTransform)go.transform;
            crt.SetParent(cellRt, false);
            crt.sizeDelta = new Vector2(cardW, cardH);
            crt.anchoredPosition = Vector2.zero;
            crt.localRotation = Quaternion.Euler(0, 0, (Mathf.PerlinNoise(g * 1.7f, 3f) - 0.5f) * 8f);

            // 卡底
            var baseSp = OriginalAssets.GetUi("mp_board");
            if (baseSp != null) { img.sprite = baseSp; img.type = Image.Type.Sliced; }
            else img.color = new Color(0.96f, 0.92f, 0.82f);

            // 花牌图案
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
                irt.sizeDelta = new Vector2(cardW * 0.82f, cardW * 0.82f);
                irt.anchoredPosition = new Vector2(0, cardH * 0.06f);
            }

            // 叠数角标
            int cnt = Game.Stacks[g].Count;
            if (cnt > 1)
            {
                var bGo = new GameObject("Badge", typeof(RectTransform), typeof(Text));
                var b = bGo.GetComponent<Text>();
                b.font = Font.CreateDynamicFontFromOSFont(new[] { "Arial" }, 20);
                b.fontSize = 22;
                b.fontStyle = FontStyle.Bold;
                b.color = new Color(1f, 0.85f, 0.2f);
                b.alignment = TextAnchor.UpperRight;
                b.text = "x" + cnt;
                var brt = (RectTransform)bGo.transform;
                brt.SetParent(crt, false);
                brt.anchorMin = new Vector2(1, 1); brt.anchorMax = new Vector2(1, 1);
                brt.pivot = new Vector2(1, 1);
                brt.sizeDelta = new Vector2(44, 26);
                brt.anchoredPosition = new Vector2(-3, -3);
                _stackBadge[g] = b;
            }

            var cardRef = card;
            go.GetComponent<Button>().onClick.AddListener(() => OnStackClick(cardRef, g));
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
            if (_stackBadge.TryGetValue(g, out var b))
            {
                if (b != null && !Game.Stacks.ContainsKey(g)) { SafeDestroy(b.gameObject); _stackBadge.Remove(g); }
                else if (b != null)
                {
                    int cnt = Game.Stacks[g].Count;
                    if (cnt <= 1) { SafeDestroy(b.gameObject); _stackBadge.Remove(g); }
                    else b.text = "x" + cnt;
                }
            }
            // 新堆顶浮现
            var top = Game.TopCard(g);
            if (top != null && !_cardImages.ContainsKey(top))
            {
                if (_cellImages.TryGetValue(g, out var cellImg) && cellImg != null)
                {
                    var crt = (RectTransform)cellImg.transform;
                    float cardW = crt.sizeDelta.x * 0.92f, cardH = cardW * 1.18f;
                    MakeStackCard(top, g, crt, cardW, cardH);
                }
            }
        }

        private void OnCellLit(int g)
        {
            if (_cellImages.TryGetValue(g, out var img) && img != null)
            {
                RefreshCell(img, g);
                // 清掉该格残余卡（理论无）
                var top = Game.TopCard(g);
                if (top == null && _stackBadge.TryGetValue(g, out var b) && b != null)
                { SafeDestroy(b.gameObject); _stackBadge.Remove(g); }
            }
            AudioManager.Inst.PlayLight();
        }

        private void OnPatternMatched(int pid, string flowerTex) => AudioManager.Inst.PlayMatch(flowerTex);

        private void OnCardCollected(Card c) { RefreshSlots(); UpdateScoreText(); }
        private void OnSlotChanged(int count, int cap) { RefreshSlots(); }

        private void OnToolUsed(ToolType t)
        {
            AudioManager.Inst.PlayTool(t);
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
            for (int i = 0; i < PuzzleGame.SlotCapacity; i++)
            {
                var go = new GameObject("Slot_" + i, typeof(RectTransform), typeof(Image));
                var img = go.GetComponent<Image>();
                var crt = (RectTransform)go.transform;
                crt.SetParent(slotRoot, false);
                float cw2 = Mathf.Min(cw - 8, h - 6);
                crt.sizeDelta = new Vector2(cw2, cw2 * 1.18f);
                crt.anchoredPosition = new Vector2((i + 0.5f) * cw - w / 2, 0);
                var baseSp = OriginalAssets.GetUi("mp_board");
                if (baseSp != null) { img.sprite = baseSp; img.type = Image.Type.Sliced; }
                img.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
                _slotImages.Add(img);
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
                else img.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
            }
        }

        private void UpdateScoreText()
        {
            if (scoreText != null)
                scoreText.text = $"第 {Game.Level.LevelNo} 关  |  分数 {Game.Score}  |  剩余 {Game.PendingCells.Count}";
        }

        private void OnStateChanged(GameState s)
        {
            if (s == GameState.Win)
            {
                if (scoreText != null) scoreText.text = "拼图完成！得分 " + Game.Score;
                if (Game.Level.PuzzleReward != null)
                    foreach (var rw in Game.Level.PuzzleReward)
                        if (rw.Count >= 2) SaveManager.AddItem(rw[0], rw[1]);
            }
            else if (s == GameState.Lose)
            {
                if (scoreText != null) scoreText.text = "槽位满了…用道具或重开";
                AudioManager.Inst.PlayLose();
            }
        }
    }
}
