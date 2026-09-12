using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xio.Game
{
    /// <summary>槽位收集卡牌：图案 = 花牌（flower 池 147 选），叠放在拼图暗格上。</summary>
    public class Card
    {
        public int PatternId;    // 图案编号（配对用）
        public string TexName;   // 花牌贴图短名
        public int GridIndex;    // 叠放的棋盘暗格
    }

    public enum GameState { Idle, Playing, Win, Lose }
    public enum ToolType { Clear = 1, Shuffle = 2, Compose = 3, Freeze = 4 }

    /// <summary>
    /// 原版玩法：翻牌三合一 + 拼图点亮。
    /// 1. 拼图 n×n：PuzzleShow 碎片初始点亮，其余暗格待点亮。
    /// 2. 每个暗格对应一种花牌图案，该图案 3 张卡随机叠放在各暗格上（羊了个羊式叠堆）。
    /// 3. 点叠堆顶卡 → 进 7 格槽位（同图案相邻）。
    /// 4. 3 张同图案 → 消除 → 点亮对应暗格。
    /// 5. 全部点亮 = 通关；槽满 7 张无消除 = 失败。
    /// </summary>
    public class PuzzleGame
    {
        public PuzzleLevel Level;
        public GameState State = GameState.Idle;

        public const int SlotCapacity = 7;
        public const int MatchScore = 30;
        public const int ClickScore = 2;

        public int Score { get; private set; }
        public int ClickCount { get; private set; }

        // ===== 限时（原版 08:00）=====
        public const float DefaultTimeLimit = 480f;      // 默认 8 分钟
        public const float FreezeDuration = 8f;          // 冻结=暂停计时 8 秒
        public float TimeLimit = DefaultTimeLimit;
        public float TimeLeft = DefaultTimeLimit;
        public bool TimeFrozen;
        public float FreezeRemain;

        public event Action<Card> OnCardCollected;
        public event Action<int, int> OnSlotChanged;
        public event Action<int> OnCellLit;               // 点亮的格索引
        public event Action<int, string> OnPatternMatched; // 消除的图案编号 + 花牌贴图名
        public event Action<GameState> OnStateChanged;
        public event Action<ToolType> OnToolUsed;

        public bool[] LitCells;
        public readonly List<int> PendingCells = new List<int>();
        public readonly List<Card> Cards = new List<Card>();   // 尚在棋盘上的卡
        public readonly List<Card> Slots = new List<Card>();

        /// <summary>暗格 g 上的叠堆（顶在最后）。只有堆顶可点。</summary>
        public readonly Dictionary<int, List<Card>> Stacks = new Dictionary<int, List<Card>>();
        public int PatternByCell(int g) => g; // 图案编号 = 暗格序号（PendingCells 序）

        private System.Random _rng = new System.Random();
        private readonly Dictionary<int, int> _patternByGrid = new Dictionary<int, int>(); // 暗格 -> 图案

        public void StartGame(PuzzleLevel level, List<string> flowerPool)
        {
            Level = level;
            int n = level.PieceCount;
            Score = 0;
            ClickCount = 0;
            Cards.Clear();
            Slots.Clear();
            PendingCells.Clear();
            Stacks.Clear();
            _patternByGrid.Clear();
            LitCells = new bool[n];
            // 限时：配置 TimeLimit>0 用之，否则默认 8 分钟
            float tl = level != null ? level.TimeLimit : 0f;
            TimeLimit = tl > 0f ? tl : DefaultTimeLimit;
            TimeLeft = TimeLimit;
            TimeFrozen = false;
            FreezeRemain = 0f;

            // 初始点亮：格 g 放 PuzzleStart[g] 碎片，碎片在 PuzzleShow 中 → 点亮
            for (int g = 0; g < n; g++)
            {
                int frag = (level.PuzzleStart != null && g < level.PuzzleStart.Count) ? level.PuzzleStart[g] : g;
                bool lit = level.PuzzleShow != null && level.PuzzleShow.Contains(frag);
                LitCells[g] = lit;
                if (!lit) PendingCells.Add(g);
            }

            // 卡牌：每个暗格一种图案 × 3 张，随机散到各暗格叠堆
            int need = PendingCells.Count;
            if (need > 0 && flowerPool != null && flowerPool.Count > 0)
            {
                var pool = new List<string>(flowerPool);
                var allCards = new List<Card>();
                for (int i = 0; i < need; i++)
                {
                    int g = PendingCells[i];
                    _patternByGrid[g] = i;
                    string tex = pool[_rng.Next(pool.Count)];
                    pool.Remove(tex);
                    if (pool.Count == 0) pool.AddRange(flowerPool);
                    for (int k = 0; k < 3; k++)
                        allCards.Add(new Card { PatternId = i, TexName = tex, GridIndex = -1 });
                }
                // 随机分配叠放位置：均分到各暗格（每格 3 张），乱序
                for (int i = allCards.Count - 1; i > 0; i--)
                {
                    int j = _rng.Next(i + 1);
                    (allCards[i], allCards[j]) = (allCards[j], allCards[i]);
                }
                for (int i = 0; i < allCards.Count; i++)
                {
                    var c = allCards[i];
                    c.GridIndex = PendingCells[i % need];
                    Cards.Add(c);
                }
                // 建叠堆
                foreach (var g in PendingCells) Stacks[g] = new List<Card>();
                foreach (var c in Cards) Stacks[c.GridIndex].Add(c);
            }

            SetState(GameState.Playing);
        }

        /// <summary>堆顶卡（可点）。无卡返回 null。</summary>
        public Card TopCard(int gridIndex)
        {
            return Stacks.TryGetValue(gridIndex, out var s) && s.Count > 0 ? s[s.Count - 1] : null;
        }

        /// <summary>点击堆顶卡收集进槽位。</summary>
        public bool ClickCard(Card card)
        {
            if (State != GameState.Playing || card == null) return false;
            if (!Cards.Remove(card)) return false;
            Stacks[card.GridIndex].Remove(card);

            ClickCount++;
            Score += ClickScore;
            Slots.Add(card);
            Slots.Sort((a, b) => a.PatternId.CompareTo(b.PatternId));
            OnCardCollected?.Invoke(card);

            bool matched = TryMatch();
            OnSlotChanged?.Invoke(Slots.Count, SlotCapacity);

            if (!matched && Slots.Count >= SlotCapacity)
            {
                SetState(GameState.Lose);
                return true;
            }
            CheckWin();
            return true;
        }

        /// <summary>槽内 3 张同图案 → 消除 → 点亮对应暗格。</summary>
        private bool TryMatch()
        {
            bool any = false;
            for (int i = 0; i + 2 < Slots.Count; i++)
            {
                if (Slots[i].PatternId == Slots[i + 1].PatternId &&
                    Slots[i + 1].PatternId == Slots[i + 2].PatternId)
                {
                    int pid = Slots[i].PatternId;
                    string tex = Slots[i].TexName;
                    Slots.RemoveRange(i, 3);
                    Score += MatchScore;
                    OnPatternMatched?.Invoke(pid, tex);
                    LightCellForPattern(pid);
                    any = true;
                    i = -1; // 连锁重扫
                }
            }
            return any;
        }

        /// <summary>消除图案 pid 后点亮其对应暗格。</summary>
        public void LightCellForPattern(int pid)
        {
            foreach (var kv in _patternByGrid)
            {
                if (kv.Value != pid) continue;
                int g = kv.Key;
                if (LitCells[g]) continue;
                LitCells[g] = true;
                PendingCells.Remove(g);
                OnCellLit?.Invoke(g);
                return;
            }
        }

        /// <summary>使用道具。</summary>
        public bool UseTool(ToolType tool)
        {
            if (State != GameState.Playing) return false;
            switch (tool)
            {
                case ToolType.Clear:   // 清扫：槽位全部卡退回棋盘
                    if (Slots.Count == 0) return false;
                    foreach (var c in Slots) { Cards.Add(c); Stacks[c.GridIndex].Add(c); }
                    Slots.Clear();
                    break;

                case ToolType.Shuffle: // 刷新：棋盘叠堆重洗（卡随机换格）
                {
                    var all = new List<Card>(Cards);
                    if (all.Count == 0) return false;
                    foreach (var g in PendingCells) Stacks[g].Clear();
                    for (int i = all.Count - 1; i > 0; i--)
                    {
                        int j = _rng.Next(i + 1);
                        (all[i], all[j]) = (all[j], all[i]);
                    }
                    for (int i = 0; i < all.Count; i++)
                    {
                        var c = all[i];
                        c.GridIndex = PendingCells[i % PendingCells.Count];
                        Cards[i] = c;
                        Stacks[c.GridIndex].Add(c);
                    }
                    break;
                }

                case ToolType.Compose: // 合成：槽内 2 张同图案 → 直接消除点亮
                {
                    int pid = -1;
                    for (int i = 0; i + 1 < Slots.Count; i++)
                        if (Slots[i].PatternId == Slots[i + 1].PatternId) { pid = Slots[i].PatternId; break; }
                    if (pid < 0) return false;
                    string tex = Slots.Find(s => s.PatternId == pid)?.TexName ?? "";
                    for (int i = Slots.Count - 1; i >= 0; i--)
                        if (Slots[i].PatternId == pid) Slots.RemoveAt(i);
                    Score += MatchScore;
                    OnPatternMatched?.Invoke(pid, tex);
                    LightCellForPattern(pid);
                    break;
                }

                case ToolType.Freeze:  // 冻结：暂停倒计时 8 秒（原版）
                    if (TimeFrozen) return false;
                    TimeFrozen = true;
                    FreezeRemain = FreezeDuration;
                    break;
            }
            OnToolUsed?.Invoke(tool);
            OnSlotChanged?.Invoke(Slots.Count, SlotCapacity);
            CheckWin();
            return true;
        }

        /// <summary>每帧驱动：限时倒计时（冻结时暂停）。归零 → Lose。</summary>
        public void Tick(float dt)
        {
            if (State != GameState.Playing) return;
            if (TimeFrozen)
            {
                FreezeRemain -= dt;
                if (FreezeRemain <= 0f) TimeFrozen = false;
                return;
            }
            TimeLeft -= dt;
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                SetState(GameState.Lose);   // 超时判负（原版）
            }
        }

        private void CheckWin()
        {
            if (PendingCells.Count == 0)
                SetState(GameState.Win);
        }

        private void SetState(GameState s)
        {
            State = s;
            OnStateChanged?.Invoke(s);
        }
    }
}
