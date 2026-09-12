using System.Collections.Generic;
using UnityEngine;

namespace Xio.Game
{
    /// <summary>
    /// 关卡内技能自动触发（M2）：挂在 GameplayPanel 上。
    /// 按当前精灵技能每 TriggerParam1 秒执行一次：自动翻牌/自动合成/清除槽位/自动提示。
    /// 技能等级 ≥1 才生效（fairyId 同段技能，如 fairy1 → 1xx）。
    /// </summary>
    public class SkillTriggerController : MonoBehaviour
    {
        private PuzzleGame _game;
        private FairySkillInfo _skill;
        private float _timer;
        private int _calls;

        /// <summary>绑定一局游戏（GameplayPanel.StartLevel 调用）。</summary>
        public void Bind(PuzzleGame game, int fairyId)
        {
            _game = game;
            _timer = 0f;
            _calls = 0;

            var skills = Xio.Game.FairySkillSystem.SkillsOf(fairyId);
            int lv = Xio.Game.SaveManager.GetFairySkillLevel(fairyId);
            if (lv >= 1 && skills.Count > 0) _skill = skills[0];
            else _skill = null;

            if (_skill != null)
                Debug.Log($"[技能] 精灵{fairyId} 技能 {_skill.SkillName} Lv{lv} 每 {_skill.TriggerParam1} 秒触发");
        }

        private void Update()
        {
            if (_skill == null || _game == null || _game.State != GameState.Playing) return;

            float interval = Mathf.Max(1f, _skill.TriggerParam1);
            _timer += Time.deltaTime;
            if (_timer < interval) return;
            _timer = 0f;

            _calls++;
            Execute();
        }

        private void Execute()
        {
            string skillName = _skill.SkillName ?? "";
            if (skillName.Contains("翻牌"))
            {
                // 自动翻正：随机点一张堆顶卡
                var top = PickRandomTop();
                if (top != null) _game.ClickCard(top);
            }
            else if (skillName.Contains("合成"))
            {
                // 自动合成：槽内已有对子则合成
                _game.UseTool(ToolType.Compose);
            }
            else if (skillName.Contains("清除"))
            {
                // 清除槽位：清空槽位退回棋盘
                _game.UseTool(ToolType.Clear);
            }
            else if (skillName.Contains("提示"))
            {
                // 自动提示：点亮可合成对子（这里简化为输出提示）
                Debug.Log("[技能] 提示：可合成对子已在槽内");
            }
        }

        /// <summary>随机挑一张可点的堆顶卡。</summary>
        private Card PickRandomTop()
        {
            var candidates = new List<Card>();
            foreach (var kv in _game.Stacks)
            {
                var s = kv.Value;
                if (s != null && s.Count > 0) candidates.Add(s[s.Count - 1]);
            }
            if (candidates.Count == 0) return null;
            return candidates[Random.Range(0, candidates.Count)];
        }

        public int CallCount => _calls;
    }
}