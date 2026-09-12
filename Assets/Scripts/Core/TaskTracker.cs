using System.Collections.Generic;
using Xio.Platform;
using Xio.UI;

namespace Xio.Game
{
    /// <summary>
    /// 赛季任务追踪（M3）：订阅 EventCenter，把单局行为累计到赛季任务进度。
    /// 原版 SeasonTaskConfig.Type：1登录 2分享 3在线 4通关 5清扫 6刷新 7合成 8冻结 9星星 10消除。
    /// 进度存 seasonProgress[taskId]，达标可领 TaskReward 金币。
    /// </summary>
    public static class TaskTracker
    {
        private static Dictionary<int, SeasonTaskInfo> _tasks;
        private static float _onlineSeconds;
        private static bool _inited;

        public static void Init()
        {
            if (_inited) return;
            _inited = true;

            _tasks = new Dictionary<int, SeasonTaskInfo>();
            var raw = ConfigLoader.LoadRaw("SeasonTaskConfig");
            if (!string.IsNullOrEmpty(raw))
                foreach (var kv in ConfigLoader.SplitTopLevel(raw))
                {
                    var t = ConfigLoader.FromJson<SeasonTaskInfo>(kv.Value);
                    if (t != null) _tasks[t.Id] = t;
                }

            EventCenter.OnLevelResult += OnLevelResult;
            EventCenter.OnToolUsed += OnToolUsed;
            EventCenter.OnMatch += OnMatch;

            // 每日登录（每次启动 +1）
            AddProgress(TypeTasks(1), 1);
        }

        /// <summary>每帧驱动（在线时长累计，由 GameBootstrap 调用）。</summary>
        public static void Tick(float delta)
        {
            if (!_inited) return;
            _onlineSeconds += delta;
            if (_onlineSeconds >= 60f)
            {
                AddProgress(TypeTasks(3), 1);
                _onlineSeconds = 0f;
            }
        }

        /// <summary>分享调用（任务类型 2）。</summary>
        public static void DoShare(string title, string text)
        {
            PlatformService.Current.Share(title, text, () => AddProgress(TypeTasks(2), 1));
        }

        /// <summary>指定任务类型的全部任务（按 TaskLevel 分组留给 UI）。</summary>
        public static List<SeasonTaskInfo> TypeTasks(int type)
        {
            var list = new List<SeasonTaskInfo>();
            if (_tasks == null) return list;
            foreach (var t in _tasks.Values)
                if (t.Type == type) list.Add(t);
            return list;
        }

        /// <summary>某赛季全部任务。</summary>
        public static List<SeasonTaskInfo> SeasonTasks(int seasonId)
        {
            var list = new List<SeasonTaskInfo>();
            if (_tasks == null) return list;
            foreach (var t in _tasks.Values)
                if (t.Season == seasonId) list.Add(t);
            list.Sort((a, b) => (a.TaskLevel * 1000 + a.Id).CompareTo(b.TaskLevel * 1000 + b.Id));
            return list;
        }

        public static int ProgressOf(int taskId)
        {
            var d = SaveManager.Data;
            return d.seasonProgress != null && d.seasonProgress.TryGetValue(taskId, out int v) ? v : 0;
        }

        private static void AddProgress(List<SeasonTaskInfo> tasks, int delta)
        {
            if (tasks == null) return;
            foreach (var t in tasks)
                if (t != null && t.TaskTarget > 0)
                {
                    var d = SaveManager.Data;
                    if (!d.seasonProgress.ContainsKey(t.Id)) d.seasonProgress[t.Id] = 0;
                    int before = d.seasonProgress[t.Id];
                    if (before >= t.TaskTarget) continue;   // 已达标记不重复累
                    int nv = System.Math.Min(before + delta, t.TaskTarget);
                    if (nv != before) { d.seasonProgress[t.Id] = nv; SaveManager.Save(); }
                }
        }

        private static void OnLevelResult(Xio.UI.LevelResult r)
        {
            if (r == null) return;
            if (r.Win) AddProgress(TypeTasks(4), 1);   // 通关
            AddProgress(TypeTasks(9), r.Stars);        // 星星（通关 +1）
        }

        private static void OnToolUsed(int taskType)
        {
            if (taskType >= 5 && taskType <= 8) AddProgress(TypeTasks(taskType), 1);
        }

        private static void OnMatch(int patternId)
        {
            AddProgress(TypeTasks(10), 1);   // 消除蝴蝶牌
        }
    }
}