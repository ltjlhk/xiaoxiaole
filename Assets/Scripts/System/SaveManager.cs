using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xio.Game
{
    /// <summary>存档：关卡进度/金币/星星/道具/精灵技能等级。PlayerPrefs 主存 + JSON 可读（微信小游戏兼容）。</summary>
    public static class SaveManager
    {
        private const string SaveKey = "xiaoxiaole_save_v1";
        private static PlayerSave _data;
        private static bool _loaded;

        public static PlayerSave Data
        {
            get
            {
                if (!_loaded) Load();
                return _data;
            }
        }

        public static void Load()
        {
            _loaded = true;
            _data = new PlayerSave();
            string json = PlayerPrefs.GetString(SaveKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                try { _data = JsonUtility.FromJson<PlayerSave>(json); } catch { _data = new PlayerSave(); }
                if (_data == null) _data = new PlayerSave();
                DictsFromLists(_data);
            }
        }

        public static void Save()
        {
            _loaded = true;
            if (_data == null) _data = new PlayerSave();
            DictsToLists(_data);
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(_data));
            PlayerPrefs.Save();
        }

        /// <summary>Dictionary → 可序列化 List（JsonUtility 不支持 Dictionary，直接落盘会静默丢档）。</summary>
        private static void DictsToLists(PlayerSave d)
        {
            Sync(d.items, d.s_items);
            Sync(d.fairyLevel, d.s_fairyLevel);
            Sync(d.seasonProgress, d.s_seasonProgress);
        }

        private static void DictsFromLists(PlayerSave d)
        {
            if (d.items == null) d.items = new Dictionary<int, int>();
            if (d.fairyLevel == null) d.fairyLevel = new Dictionary<int, int>();
            if (d.seasonProgress == null) d.seasonProgress = new Dictionary<int, int>();
            Fill(d.s_items, d.items);
            Fill(d.s_fairyLevel, d.fairyLevel);
            Fill(d.s_seasonProgress, d.seasonProgress);
        }

        private static void Sync(Dictionary<int, int> dict, List<IntPair> list)
        {
            if (dict == null || list == null) return;
            list.Clear();
            foreach (var kv in dict) list.Add(new IntPair { k = kv.Key, v = kv.Value });
        }

        private static void Fill(List<IntPair> list, Dictionary<int, int> dict)
        {
            if (list == null) return;
            if (dict == null) dict = new Dictionary<int, int>();
            dict.Clear();
            foreach (var p in list) dict[p.k] = p.v;
        }

        public static void RewardLevel(int levelId)
        {
            var d = Data;
            int idx = LevelSegmentModel.IndexOf(levelId);
            if (idx != int.MaxValue && idx > d.maxPassedLevel)
            {
                d.maxPassedLevel = idx;
                d.stars += 1;   // 首次通关 +1 星
            }
            Save();
        }

        public static void AddItem(int itemId, int count)
        {
            var d = Data;
            if (!d.items.ContainsKey(itemId)) d.items[itemId] = 0;
            d.items[itemId] += count;
            Save();
        }

        public static void AddStars(int n) { Data.stars += n; Save(); }
        public static void AddCoins(int n) { Data.coins += n; Save(); }

        /// <summary>精灵技能等级（默认 0 = 已解锁未升级）。</summary>
        public static int GetFairySkillLevel(int fairyId)
        {
            var d = Data;
            return d.fairyLevel != null && d.fairyLevel.TryGetValue(fairyId, out int lv) ? lv : 0;
        }

        /// <summary>设置精灵技能等级。</summary>
        public static void SetFairySkillLevel(int fairyId, int lv)
        {
            var d = Data;
            if (d.fairyLevel == null) d.fairyLevel = new Dictionary<int, int>();
            d.fairyLevel[fairyId] = lv;
            Save();
        }

        /// <summary>物品数量（无则 0）。</summary>
        public static int GetItemCount(int itemId)
        {
            var d = Data;
            return d.items != null && d.items.TryGetValue(itemId, out int c) ? c : 0;
        }

        /// <summary>赛季活跃度累计（活跃奖励阶梯用）。</summary>
        public static void AddActivity(int n) { Data.activity += n; Save(); }

        /// <summary>领取赛季任务奖励：入账并标记已领。</summary>
        public static bool ClaimSeasonTask(int taskId, int rewardCoins)
        {
            var d = Data;
            if (d.claimedTasks == null) d.claimedTasks = new List<int>();
            if (d.claimedTasks.Contains(taskId)) return false;
            d.claimedTasks.Add(taskId);
            d.coins += rewardCoins;
            Save();
            return true;
        }

        public static bool IsTaskClaimed(int taskId)
        {
            var d = Data;
            return d.claimedTasks != null && d.claimedTasks.Contains(taskId);
        }

        public static bool IsUnlocked(int levelId)
        {
            return LevelSegmentModel.IsUnlocked(levelId);  // 全局序：通关上一关后本关开放
        }
    }
}
