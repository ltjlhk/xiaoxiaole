using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>精灵技能升级弹窗（M2）：展示技能描述/当前等级/升级消耗（金币），升级写存档。</summary>
    public class FairyUpgradePopup : UIPanel
    {
        public int FairyId = 1;     // PanelManager.Push 时注入

        private FairyInfo _fairy;
        private FairySkillInfo _skill;
        private Text _infoText;
        private Text _costText;
        private Button _upBtn;

        protected override void Build()
        {
            _fairy = FairySpineMap.Info(FairyId);
            var skills = FairySkillSystem.SkillsOf(FairyId);
            _skill = skills.Count > 0 ? skills[0] : null;

            // 半透明遮罩（拦截下层点击）
            var mask = UIHelper.Image(Root, "Mask");
            UIHelper.Stretch((RectTransform)mask.transform);
            mask.color = new Color(0, 0, 0, 0.55f);

            // 弹窗底板
            var panel = UIHelper.NewRect(Root, "Upgrade");
            UIHelper.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(640, 420), Vector2.zero);
            var board = OriginalAssets.GetUi("jiesuankuang_1");
            var pimg = panel.gameObject.AddComponent<Image>();
            if (board != null) { pimg.sprite = board; pimg.type = Image.Type.Sliced; }
            else pimg.color = new Color(0.13f, 0.17f, 0.25f, 0.96f);

            // 标题
            string fairyName = (_fairy != null && !string.IsNullOrEmpty(_fairy.FairyName))
                ? _fairy.FairyName : "精灵" + FairyId;
            var title = UIHelper.Text(panel, "Title", fairyName + " · 技能升级", 34,
                new Color(1f, 0.96f, 0.7f), FontStyle.Bold);
            UIHelper.Place(title.rectTransform, new Vector2(0.5f, 0.9f), new Vector2(560, 60), Vector2.zero);

            RefreshInfo();

            // 关闭
            MakeButton(panel, "Close", "关 闭", () => PanelManager.Instance.Pop(),
                new Vector2(0, -168), new Color(0.55f, 0.6f, 0.68f));
        }

        private void RefreshInfo()
        {
            if (_infoText != null) { Object.Destroy(_infoText.gameObject); _infoText = null; }
            if (_costText != null) { Object.Destroy(_costText.gameObject); _costText = null; }

            var panel = Root.Find("Upgrade");
            int lv = FairySkillSystem.LevelOf(FairyId);

            string desc = _skill != null
                ? string.Format(_skill.SkillDesc ?? "{0}", _skill.TriggerParam1)
                : "（无技能）";
            int cost = _skill != null ? FairySkillSystem.UpgradeGoldCost(_skill, lv) : -1;
            int maxLv = _skill != null ? FairySkillSystem.MaxLevel(_skill) : 0;

            _infoText = UIHelper.Text(panel, "Info", desc, 26,
                new Color(0.9f, 0.92f, 1f), FontStyle.Normal, TextAnchor.MiddleCenter);
            UIHelper.Place(_infoText.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(560, 90), Vector2.zero);

            string costTxt;
            if (_skill == null) costTxt = "无技能可升级";
            else if (lv >= maxLv || cost < 0) costTxt = "已是满级 Lv" + lv;
            else
            {
                int coins = SaveManager.Data.coins;
                costTxt = $"当前 Lv{lv}  →  升级消耗 {cost} 金币（现有 {coins}）";
            }
            _costText = UIHelper.Text(panel, "Cost", costTxt, 26,
                new Color(1f, 0.85f, 0.5f), FontStyle.Bold, TextAnchor.MiddleCenter);
            UIHelper.Place(_costText.rectTransform, new Vector2(0.5f, 0.34f), new Vector2(560, 70), Vector2.zero);

            // 升级按钮（可升级时）
            if (_skill != null && lv < maxLv && cost >= 0)
            {
                if (_upBtn != null) { Object.Destroy(_upBtn.gameObject); _upBtn = null; }
                bool affordable = SaveManager.Data.coins >= cost;
                _upBtn = MakeButton(panel, "Up", "升 级", () =>
                {
                    if (SaveManager.Data.coins < cost)
                    {
                        RefreshInfo();
                        return;
                    }
                    SaveManager.AddCoins(-cost);
                    SaveManager.SetFairySkillLevel(FairyId, lv + 1);
                    Debug.Log($"[精灵] {FairyId} 技能升级 Lv{lv + 1}（花 {cost} 金币）");
                    RefreshInfo();
                }, new Vector2(0, -84), affordable ? new Color(0.95f, 0.78f, 0.32f) : new Color(0.5f, 0.5f, 0.55f));
            }
        }

        private Button MakeButton(Transform parent, string name, string label,
            System.Action onClick, Vector2 pos, Color color)
        {
            var bt = UIHelper.Button(parent, name, onClick);
            UIHelper.Place((RectTransform)bt.transform, new Vector2(0.5f, 0.5f), new Vector2(260, 64), pos);
            var img = bt.gameObject.GetComponent<Image>();
            img.color = color;
            var t = UIHelper.Text(bt.transform, "L", label, 28, new Color(0.3f, 0.18f, 0.05f), FontStyle.Bold);
            UIHelper.Stretch(t.rectTransform);
            return bt;
        }

        public override void Refresh() { }
    }
}