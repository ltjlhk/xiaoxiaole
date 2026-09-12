using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.UI;

namespace Xio.Game
{
    /// <summary>关卡 3D 渲染层：牌堆（Scene3D.CellWorld 网格塔）+ 7 槽（Droplocation 世界坐标）。
    /// 一比一复刻原版 level2：正交俯视相机 + 3D Block + 围墙地板；逻辑层 PuzzleGame 不变。</summary>
    public class GamePanel : MonoBehaviour
    {
        public PuzzleGame Game { get; private set; }

        [Header("引用（Bootstrap 注入，3D 模式由 Init 覆盖）")]
        public Transform boardRoot;   // 牌堆容器（BlockParent）
        public Transform slotRoot;    // 槽定位（Droplocation）
        public Text titleText;

        /// <summary>Card → 3D 视图（牌堆牌 + 槽牌共用）。</summary>
        private readonly Dictionary<Card, Block3D> _viewByCard = new Dictionary<Card, Block3D>();
        /// <summary>当前槽区视觉（与 Game.Slots 同序）。</summary>
        private readonly List<Block3D> _slotBlocks = new List<Block3D>();

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

        /// <summary>每帧驱动：倒计时（原版 08:00）+ 冻结状态 + 3D 主动射线点击。时间耗尽由 Game 判负。</summary>
        private void Update()
        {
            if (Game == null) return;
            Game.Tick(Time.deltaTime);
            if (onTimeChanged != null && Game.State == GameState.Playing)
                onTimeChanged(Game.TimeLeft, Game.TimeFrozen);
            HandleClick();
        }

        /// <summary>3D 点击：UI 命中优先跳过，否则相机射线取最近 Block（OnMouseDown 在 UI 混合 3D 场景不可靠）。</summary>
        private void HandleClick()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            if (Game.State != GameState.Playing) return;
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es != null && es.IsPointerOverGameObject()) return;   // UI（道具栏/顶栏/弹窗）优先
            var cam = Scene3D.Inst != null ? Scene3D.Inst.Cam : Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(ray, 100f);
            if (hits.Length == 0) return;
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var h in hits)
            {
                var blk = h.collider.GetComponentInParent<Block3D>();
                if (blk == null) continue;               // 墙/地板/托盘：继续找牌
                // 第一个命中的牌：可点则收，不可点（非堆顶）则停——模拟实体遮挡不穿透
                if (blk.Clickable) OnBlockClick(blk);
                return;
            }
        }

        /// <summary>时间展示回调（GameplayPanel 顶栏注入）。</summary>
        public System.Action<float, bool> onTimeChanged;

        public void Init(PuzzleLevel level, List<string> flowerPool, Texture2D puzzleTex)
        {
            MatchCount = ClearCount = ShuffleCount = ComposeCount = FreezeCount = 0;
            _combo = 0;
            Game = new PuzzleGame();
            Game.StartGame(level, flowerPool);
            var s3 = Scene3D.Ensure();
            boardRoot = s3.BlockParent;
            slotRoot = s3.Droplocation;
            ClearAllViews();
            BuildBoard(level);
            Game.OnCellLit += OnCellLit;
            Game.OnSlotChanged += OnSlotChanged;
            Game.OnToolUsed += OnToolUsed;
            Game.OnPatternMatched += OnPatternMatched;
            Game.OnStateChanged += OnStateChanged;
            // 标题由 GameplayPanel 设置（第 X 关），此处不覆盖
        }

        private void ClearAllViews()
        {
            foreach (var kv in _viewByCard) if (kv.Value != null) SafeDestroy(kv.Value.gameObject);
            _viewByCard.Clear();
            _slotBlocks.Clear();
        }

        // ===== 牌堆：3D 网格塔（一暗格一叠，层间错位遮挡，仅堆顶可点）=====
        private void BuildBoard(PuzzleLevel level)
        {
            // 销毁牌堆旧视觉（槽区牌 InSlot=true 保留）
            var dead = new List<Card>();
            foreach (var kv in _viewByCard)
                if (kv.Value == null || !kv.Value.InSlot) dead.Add(kv.Key);
            foreach (var c in dead)
            {
                if (_viewByCard[c] != null) SafeDestroy(_viewByCard[c].gameObject);
                _viewByCard.Remove(c);
            }

            int rows = level.Rows, cols = level.Cols;
            foreach (var kv in Game.Stacks)
            {
                int g = kv.Key;
                var st = kv.Value;
                for (int k = 0; k < st.Count; k++)   // k=0 底层；顶=Count-1
                {
                    var card = st[k];
                    // 原版 GameMgr 固定 49 格牌位表（slot=0..48），关卡只占用部分格
                    var pos = Scene3D.BoardSlotWorld(g, k);
                    var blk = Scene3D.CreateBlock(boardRoot, card.TexName, pos);
                    blk.CellId = g;
                    blk.Clickable = k == st.Count - 1;   // 仅堆顶可点
                    blk.OnClick = OnBlockClick;
                    _viewByCard[card] = blk;
                }
            }
        }

        private void OnBlockClick(Block3D blk)
        {
            if (Game == null || Game.State != GameState.Playing) return;
            var top = Game.TopCard(blk.CellId);
            if (top == null) return;
            if (!_viewByCard.TryGetValue(top, out var v) || v != blk) return;   // 只可点堆顶

            blk.Clickable = false;
            if (Game.ClickCard(top))   // 内部触发 OnSlotChanged → SyncSlots（blk 已在 _viewByCard 中）
            {
                AudioManager.Inst.PlayPick();
                RefreshStackClickable(blk.CellId);
            }
        }

        /// <summary>堆顶出栈后，新堆顶恢复可点。</summary>
        private void RefreshStackClickable(int g)
        {
            var top = Game.TopCard(g);
            if (top == null) return;
            if (_viewByCard.TryGetValue(top, out var v) && v != null) v.Clickable = true;
        }

        // ===== 槽区：与 Game.Slots 全量同步（新牌飞入 / 消除消失 / 补位重排）=====
        private void SyncSlots()
        {
            // 目标序列
            var desired = new List<Block3D>();
            foreach (var card in Game.Slots)
                if (_viewByCard.TryGetValue(card, out var b) && b != null) desired.Add(b);

            // 旧槽牌不再在槽中 → 三消/合成移除 → 消失
            for (int i = _slotBlocks.Count - 1; i >= 0; i--)
            {
                var b = _slotBlocks[i];
                if (b == null) { _slotBlocks.RemoveAt(i); continue; }
                if (!desired.Contains(b))
                {
                    _slotBlocks.RemoveAt(i);
                    RemoveViewOf(b);
                    b.Vanish();
                }
            }

            // 飞入 / 补位
            for (int i = 0; i < desired.Count; i++)
            {
                var b = desired[i];
                var target = Scene3D.SlotWorld(i, 0);
                bool isNew = !b.InSlot;
                b.InSlot = true;
                if (!_slotBlocks.Contains(b)) _slotBlocks.Add(b);
                b.FlyTo(target, isNew ? 0.28f : 0.2f);
            }
        }

        private void RemoveViewOf(Block3D b)
        {
            var dead = new List<Card>();
            foreach (var kv in _viewByCard) if (kv.Value == b) dead.Add(kv.Key);
            foreach (var c in dead) _viewByCard.Remove(c);
        }

        private void OnCellLit(int g)
        {
            AudioManager.Inst.PlayLight();
        }

        private int _combo;

        private void OnPatternMatched(int pid, string flowerTex)
        {
            MatchCount++;
            _combo++;
            AudioManager.Inst.PlayMatch(flowerTex);
            EventCenter.OnMatch?.Invoke(pid);
            // 原版特效：baozha 爆炸粒子 + 连击飘字（combo 区 y=-457，Screen 中心系）
            Vector2 slotPos = SlotCenterAnchored();
            GameFX.PlaySpineFx("baozha", slotPos, 1.6f);
            GameFX.PlaySpineFx("kapailizi", slotPos, 1.2f);
            string comboTxt = _combo >= 2 ? $"x{_combo} 连击!" : "+1";
            GameFX.PopText(new Vector2(0, -457 + 60), comboTxt,
                _combo >= 3 ? new Color(1f, 0.55f, 0.2f) : new Color(1f, 0.95f, 0.6f), _combo >= 3 ? 46 : 36);
        }

        /// <summary>槽位中心（根 Canvas 坐标，供特效定位）：原版槽区世界 (0,~,-20.5) → 屏幕 (0,-454)。</summary>
        internal Vector2 SlotCenterAnchored()
        {
            return Scene3D.WorldToScreen(new Vector3(0f, 0f, Scene3D.SlotZ));
        }

        private void OnSlotChanged(int count, int cap) { SyncSlots(); }

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
            // 原版特效：冻结蓝闪 + dongjie 图标 + effect_6 骨骼
            if (t == ToolType.Freeze) GameFX.FreezeFlash();
            else if (t == ToolType.Compose) GameFX.PlaySpineFx("effect_6", SlotCenterAnchored(), 1.4f);
            else if (t == ToolType.Clear) GameFX.PlaySpineFx("effect_3", SlotCenterAnchored(), 1.4f);
            else if (t == ToolType.Shuffle) GameFX.PlaySpineFx("effect_5", SlotCenterAnchored(), 1.4f);

            if (t == ToolType.Clear)
            {
                // 槽牌退回棋盘：槽牌视觉标记回棋盘 → 重建（旧视图销毁，退回牌重新生成）
                foreach (var b in _slotBlocks) if (b != null) b.InSlot = false;
                _slotBlocks.Clear();
                BuildBoard(Game.Level);
            }
            else if (t == ToolType.Shuffle)
            {
                BuildBoard(Game.Level);          // 棋盘重洗（槽牌保留）
            }
            else if (t == ToolType.Compose)
            {
                SyncSlots();                     // 槽内消除 → 补位
            }
        }

        private void OnStateChanged(GameState s)
        {
            if (s == GameState.Win)
            {
                // 原版特效：星从牌区飞向顶栏星标（星数+1 的视觉化）；顶锚(-34,-129) → 中心系 y=+538
                GameFX.FlyStar(SlotCenterAnchored() + new Vector2(0, 260), new Vector2(-34, 538));
                GameFX.PlaySpineFx("effect_5", Vector2.zero, 2f);
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
                AudioManager.Inst.PlayLose();
            }
        }
    }
}
