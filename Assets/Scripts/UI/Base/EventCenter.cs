using System;

namespace Xio.UI
{
    /// <summary>一局结束的结果。</summary>
    public class LevelResult
    {
        public int LevelId;      // 配置 Id（如 101）
        public bool Win;
        public int Score;
        public int Stars;
        public int Clears;       // 本局清扫次数
        public int Shuffles;     // 本局刷新次数
        public int Composes;     // 本局合成次数
        public int Freezes;      // 本局冻结次数
        public int Matches;      // 本局消除次数
        public int Clicks;       // 本局翻牌次数
    }

    /// <summary>全局事件总线：玩法数据 → 统计/任务/HUD 解耦。
    /// TaskTracker / TopHud 订阅；GamePanel 在每次玩法事件时发布。</summary>
    public static class EventCenter
    {
        /// <summary>关卡结算（胜/负都触发，携带本局统计）。</summary>
        public static Action<LevelResult> OnLevelResult;

        /// <summary>道具使用（toolId: 4=清除 5=刷新 6=合成 7=冻结，原版 ItemConfig Id）。</summary>
        public static Action<int> OnToolUsed;

        /// <summary>消除一次（patternId=图案）。</summary>
        public static Action<int> OnMatch;

        /// <summary>金币/道具数量变化（itemId, count）。</summary>
        public static Action<int, int> OnItemChanged;

        /// <summary>关卡进度变化（已通过最高关Id）。</summary>
        public static Action<int> OnProgressChanged;

        public static void ClearAll()
        {
            OnLevelResult = null;
            OnToolUsed = null;
            OnMatch = null;
            OnItemChanged = null;
            OnProgressChanged = null;
        }
    }
}