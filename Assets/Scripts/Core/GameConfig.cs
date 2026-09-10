using System;
using System.Collections.Generic;

namespace Xio.Game
{
    // ===== 关卡配置 PuzzleConfig（780 关）=====
    [Serializable]
    public class PuzzleLevel
    {
        public int Id;
        public string Picture;              // UI/Puzzle/{Id}
        public List<List<int>> PuzzleReward; // [[itemId,count],...]
        public List<int> CutDistance;       // [0,0]
        public List<int> PuzzleCut;        // [rows,cols]
        public List<int> PuzzleShow;       // 初始正面碎片索引
        public List<int> PuzzleStart;      // 初始排列
        public string SkillReinforceText;
        public List<int> SkillReinforce;   // [skillId,a,b,c]
        public string PhotoTitle;
        public string PhotoText;
        public int FairyId;
        public string ShareTitle;
        public string ShareText;

        public int Rows => PuzzleCut[0];
        public int Cols => PuzzleCut[1];
        public int PieceCount => Rows * Cols;

        /// <summary>玩家可见关卡号（原版段内连续）：101→1 … 199→99，1100→100 … 1190→190，501→1。</summary>
        public int LevelNo => Id >= 1000 ? Id % 1000 : Id % 100;
    }

    [Serializable]
    public class PuzzleConfigData
    {
        public Dictionary<string, PuzzleLevel> entries = new Dictionary<string, PuzzleLevel>();
    }

    // ===== 物品配置 ItemConfig =====
    [Serializable]
    public class ItemInfo
    {
        public int Id;
        public string Name;
        public string Icon;
        public string Type;   // Gold / Prop / Fragment / Fragment2 / Fragment3
    }

    // ===== 精灵配置 FairyConfig =====
    [Serializable]
    public class FairyInfo
    {
        public int id;
        public string FairyName;
        public string FairyText;
        public int FairyUnlock;          // 解锁所需通关数
        public string FairyIcon;
        public string FairyLockedIcon;
        public string FairySpineSkeleton;
        public string FairySpine;
        public int FairyFragment;        // 专属碎片 itemId
        public int FairyQuality;
        public List<int> PuzzleId;       // 所属关卡 Id 列表
        public List<int> SkillId;
        public string FairyBG;
    }

    // ===== 精灵技能 FairySkillConfig =====
    [Serializable]
    public class FairySkillInfo
    {
        public int id;
        public string SkillName;
        public string SkillDesc;         // 含 {0} 格式化
        public string SkillIcon;
        public int SkillType;
        public int TriggerType;
        public int TriggerParam1;        // 触发的秒数
        public int TriggerParam2;
        public List<List<int>> SkillUpGrade;
    }

    // ===== 赛季 SeasonConfig =====
    [Serializable]
    public class SeasonInfo
    {
        public int Id;
        public int Front;
        public int Next;
        public string Season;
        public string SeasonPic;
        public List<int> PicUnlock;      // [pieceItemId, count]
        public string StarTime;
        public string EndTime;
        public List<int> Cost;
        public List<int> LimitReward;    // [pieceItemId, count]
        public List<int> LimitRewardPieceId;
    }

    // ===== 赛季任务 SeasonTaskConfig =====
    [Serializable]
    public class SeasonTaskInfo
    {
        public int Id;
        public int Season;
        public int Type;                 // 1登录 2xx 4通关 5清扫 6刷新 7合成 8冻结 9星星 10消除
        public int TaskTarget;
        public int TaskReward;
        public int TaskLevel;
        public string TaskTxt;
    }

    // ===== 队伍存档 =====
    [Serializable]
    public class PlayerSave
    {
        public int maxPassedLevel = 0;                  // 已通关最高关（按序解锁）
        public int stars = 0;
        public int coins = 0;
        public Dictionary<int, int> items = new Dictionary<int, int>();      // itemId -> count
        public Dictionary<int, int> fairyLevel = new Dictionary<int, int>();  // fairyId -> skill level
        public Dictionary<int, int> seasonProgress = new Dictionary<int, int>();
        public int currentFairy = 1;
    }
}
