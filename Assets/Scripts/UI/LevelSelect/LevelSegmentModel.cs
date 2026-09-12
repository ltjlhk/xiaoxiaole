using System.Collections.Generic;

namespace Xio.Game
{
    /// <summary>
    /// 关卡分段模型：由 PuzzleConfig 数据驱动，勿硬编码段号。
    /// 780 关按 FairyId 分组（首次出现序），组内按 Id 升序：
    ///   精灵1：101..199 + 1100..1190（段内显示号 第1关..第190关）
    /// 全局序号（顺序解锁）＝分段拼接后的下标；通关判定只认全局最大通关序号。
    /// </summary>
    public static class LevelSegmentModel
    {
        private static List<PuzzleLevel> _order;
        private static List<List<PuzzleLevel>> _segments;
        private static List<FairyInfo> _fairyList;
        private static Dictionary<int, int> _levelIndex;

        private static void Ensure()
        {
            if (_order != null) return;

            var map = ConfigLoader.SplitTopLevel(ConfigLoader.LoadRaw("PuzzleConfig"));
            var all = new List<PuzzleLevel>();
            foreach (var kv in map)
            {
                try
                {
                    var lv = ConfigLoader.FromJson<PuzzleLevel>(kv.Value);
                    if (lv != null)
                    {
                        // JsonUtility 不支持嵌套数组，手动校准过关奖励
                        lv.PuzzleReward = ConfigLoader.ParseNestedIntLists(ConfigLoader.ExtractField(kv.Value, "PuzzleReward"));
                        all.Add(lv);
                    }
                }
                catch { /* 跳过损坏条目 */ }
            }

            // 按 FairyId 首次出现序分组
            var byFairy = new Dictionary<int, List<PuzzleLevel>>();
            var fairyOrder = new List<int>();
            foreach (var lv in all)
            {
                if (!byFairy.TryGetValue(lv.FairyId, out var list))
                {
                    list = new List<PuzzleLevel>();
                    byFairy[lv.FairyId] = list;
                    fairyOrder.Add(lv.FairyId);
                }
                list.Add(lv);
            }

            _order = new List<PuzzleLevel>();
            _segments = new List<List<PuzzleLevel>>();
            _fairyList = new List<FairyInfo>();
            _levelIndex = new Dictionary<int, int>();

            var fmap = LoadFairyMap();
            foreach (var fid in fairyOrder)
            {
                var list = byFairy[fid];
                list.Sort((a, b) => a.Id.CompareTo(b.Id));
                _segments.Add(list);
                if (fmap.TryGetValue(fid, out var fi)) _fairyList.Add(fi);
                foreach (var lv in list)
                {
                    _levelIndex[lv.Id] = _order.Count;
                    _order.Add(lv);
                }
            }
        }

        private static Dictionary<int, FairyInfo> LoadFairyMap()
        {
            var map = new Dictionary<int, FairyInfo>();
            var raw = ConfigLoader.LoadRaw("FairyConfig");
            if (string.IsNullOrEmpty(raw)) return map;
            var entries = ConfigLoader.SplitTopLevel(raw);
            foreach (var kv in entries)
            {
                try
                {
                    var fi = ConfigLoader.FromJson<FairyInfo>(kv.Value);
                    if (fi != null) map[fi.id] = fi;
                }
                catch { }
            }
            return map;
        }

        /// <summary>全部关卡（按解锁顺序）。</summary>
        public static List<PuzzleLevel> All { get { Ensure(); return _order; } }

        /// <summary>分段（每精灵一段，段内升序）。</summary>
        public static List<List<PuzzleLevel>> Segments { get { Ensure(); return _segments; } }

        /// <summary>精灵列表（与分段同序，配置缺失的精灵用占位信息）。</summary>
        public static List<FairyInfo> FairyList
        {
            get
            {
                Ensure();
                if (_fairyList.Count == _segments.Count) return _fairyList;
                var merged = new List<FairyInfo>();
                for (int i = 0; i < _segments.Count; i++)
                {
                    int fid = _segments[i][0].FairyId;
                    bool found = false;
                    foreach (var f in _fairyList) if (f.id == fid) { merged.Add(f); found = true; break; }
                    if (!found)
                        merged.Add(new FairyInfo { id = fid, FairyName = "精灵" + (i + 1), FairySpineSkeleton = "jingling_1" });
                }
                _fairyList = merged;
                return _fairyList;
            }
        }

        /// <summary>全局序号（顺序解锁用）。未找到返回 int.MaxValue。</summary>
        public static int IndexOf(int levelId)
        {
            Ensure();
            return _levelIndex.TryGetValue(levelId, out var i) ? i : int.MaxValue;
        }

        /// <summary>是否已通关该关。</summary>
        public static bool IsPassed(int levelId) => IndexOf(levelId) <= SaveManager.Data.maxPassedLevel;

        /// <summary>是否解锁可进（通关上一关后本关开放）。</summary>
        public static bool IsUnlocked(int levelId) => IndexOf(levelId) <= SaveManager.Data.maxPassedLevel + 1;

        /// <summary>下一关 Id（按全局序找 id 的下一个给定序号的关卡，用于"下一关"）。</summary>
        public static int NextLevelAfter(int levelId)
        {
            Ensure();
            int idx = IndexOf(levelId);
            if (idx == int.MaxValue || idx + 1 >= _order.Count) return -1;
            return _order[idx + 1].Id;
        }
    }
}