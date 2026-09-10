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
            }
        }

        public static void Save()
        {
            _loaded = true;
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(_data ?? new PlayerSave()));
            PlayerPrefs.Save();
        }

        public static void RewardLevel(int levelId)
        {
            var d = Data;
            int idx = IndexOfLevel(levelId);
            if (idx > d.maxPassedLevel) d.maxPassedLevel = idx;
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

        public static bool IsUnlocked(int levelId)
        {
            int idx = IndexOfLevel(levelId);
            return idx <= Data.maxPassedLevel + 1;  // 当前关之后的下一关开放
        }

        /// <summary>关卡序号：同一精灵主线 101..199 递增，首位 = 100。</summary>
        private static int IndexOfLevel(int levelId)
        {
            if (levelId >= 101 && levelId <= 1199 && levelId % 100 <= 99) return levelId % 100;   // 101 → 1
            return levelId;
        }
    }
}
