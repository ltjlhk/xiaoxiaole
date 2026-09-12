using System.Collections.Generic;

namespace Xio.Game
{
    /// <summary>
    /// 精灵技能系统（M2）：解析 FairySkillConfig，按精灵挂载技能。
    /// 原版数据：SkillType 0=定时触发；TriggerType 1=关卡内计时；
    /// TriggerParam1=触发秒数；SkillUpGrade=升级消耗行，每行以末位为金币数（如 [0,1,1,50] → 50 金币）。
    /// </summary>
    public static class FairySkillSystem
    {
        private static Dictionary<int, FairySkillInfo> _skills;
        private static Dictionary<int, List<FairySkillInfo>> _byFairy;
        private static bool _loaded;

        private static void Ensure()
        {
            if (_loaded) return;
            _loaded = true;
            _skills = new Dictionary<int, FairySkillInfo>();
            _byFairy = new Dictionary<int, List<FairySkillInfo>>();

            var raw = ConfigLoader.LoadRaw("FairySkillConfig");
            if (string.IsNullOrEmpty(raw)) return;
            foreach (var kv in ConfigLoader.SplitTopLevel(raw))
            {
                var s = ConfigLoader.FromJson<FairySkillInfo>(kv.Value);
                if (s == null) continue;
                // JsonUtility 不支持嵌套数组，手动校准升级档位
                s.SkillUpGrade = ConfigLoader.ParseNestedIntLists(ConfigLoader.ExtractField(kv.Value, "SkillUpGrade"));
                _skills[s.id] = s;
                if (!_byFairy.TryGetValue(s.id / 100, out var list))
                {
                    list = new List<FairySkillInfo>();
                    _byFairy[s.id / 100] = list;
                }
                list.Add(s);
            }
        }

        /// <summary>全部技能。</summary>
        public static Dictionary<int, FairySkillInfo> All { get { Ensure(); return _skills; } }

        /// <summary>某精灵所属技能（fairyId 与 skillId 同段，如 fairy1 → 1xx）。</summary>
        public static List<FairySkillInfo> SkillsOf(int fairyId)
        {
            Ensure();
            return _byFairy.TryGetValue(fairyId, out var l) ? l : new List<FairySkillInfo>();
        }

        /// <summary>精灵当前等级。</summary>
        public static int LevelOf(int fairyId) => SaveManager.GetFairySkillLevel(fairyId);

        /// <summary>升级第 lv 级（0 基）消耗的金币；无档位返回 -1（满级）。</summary>
        public static int UpgradeGoldCost(FairySkillInfo skill, int lv)
        {
            if (skill == null || skill.SkillUpGrade == null || lv >= skill.SkillUpGrade.Count)
                return -1;
            var row = skill.SkillUpGrade[lv];
            return row != null && row.Count >= 4 ? row[3] : -1;
        }

        /// <summary>最高可升级次数（SkillUpGrade 行数）。</summary>
        public static int MaxLevel(FairySkillInfo skill)
        {
            return skill == null || skill.SkillUpGrade == null ? 0 : skill.SkillUpGrade.Count;
        }
    }
}