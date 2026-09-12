using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>
    /// 精灵养成（M1 提供展示层：解锁状态 + 专属碎片进度；技能升级在 M2 补全）。
    /// 解锁条件：FairyUnlock（通关数）≦ maxPassedLevel+1；碎片 = items[FairyFragment]。
    /// </summary>
    public class FairyPanel : UIPanel
    {
        private Transform _listRoot;
        private readonly List<GameObject> _cards = new List<GameObject>();

        protected override void Build()
        {
            var bgTex = OriginalAssets.GetBackground("bg10");
            var bg = UIHelper.Image(Root, "BG", bgTex != null
                ? Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f), 100f)
                : null);
            UIHelper.Stretch((RectTransform)bg.transform);

            var title = UIHelper.Text(Root, "Title", "精灵图鉴", 40, new Color(1f, 0.98f, 0.85f), FontStyle.Bold);
            UIHelper.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(600, 60), new Vector2(0, -36));
            title.gameObject.AddComponent<Outline>().effectColor = new Color(0.12f, 0.22f, 0.45f);
            title.gameObject.GetComponent<Outline>().effectDistance = new Vector2(2, -2);

            var back = UIHelper.Button(Root, "Back", () => PanelManager.Instance.Pop());
            UIHelper.Place((RectTransform)back.transform, new Vector2(0, 1f), new Vector2(96, 56), new Vector2(70, -38));
            var backIcon = OriginalAssets.GetUi("btn_fanhui");
            if (backIcon != null)
            {
                var bi = back.gameObject.GetComponent<Image>();
                bi.sprite = backIcon;
                bi.color = Color.white;
                bi.SetNativeSize();
            }
            else
            {
                var l = UIHelper.Text(back.transform, "L", "返回", 26, Color.white, FontStyle.Bold);
                UIHelper.Stretch(l.rectTransform);
            }

            _listRoot = UIHelper.NewRect(Root, "List");
            UIHelper.Place((RectTransform)_listRoot, new Vector2(0.5f, 1f), new Vector2(700, 1100), new Vector2(0, -140));
            Rebuild();
        }

        private void Rebuild()
        {
            for (int i = _cards.Count - 1; i >= 0; i--)
                if (_cards[i] != null) SafeDestroy(_cards[i]);
            _cards.Clear();

            var fairies = LevelSegmentModel.FairyList;
            int passed = SaveManager.Data.maxPassedLevel;
            for (int i = 0; i < fairies.Count; i++)
            {
                var f = fairies[i];
                var card = UIHelper.NewRect(_listRoot, "Fairy_" + f.id);
                UIHelper.Place(card, new Vector2(0.5f, 1f), new Vector2(660, 160), new Vector2(0, -i * 172));
                MakeCard(card, f, i, passed);
                _cards.Add(card.gameObject);
            }
        }

        private void MakeCard(RectTransform card, FairyInfo f, int idx, int passed)
        {
            var board = OriginalAssets.GetUi("mp_board");
            var img = card.gameObject.AddComponent<Image>();
            if (board != null) { img.sprite = board; img.type = Image.Type.Sliced; img.color = Color.white; }
            else img.color = new Color(0.9f, 0.84f, 0.7f);

            bool unlocked = f.FairyUnlock <= passed + 1;

            // 立绘 / Spine
            var icon = FairySpineMap.IconFor(f.id);
            if (icon != null)
            {
                var ic = UIHelper.Image(card, "Icon", icon);
                UIHelper.Place((RectTransform)ic.transform, new Vector2(0, 0.5f), new Vector2(120, 120), new Vector2(85, 0));
            }

            // 名称 / 解锁条件
            var nameT = UIHelper.Text(card, "Name", (f.FairyName ?? "精灵" + (idx + 1)) + (unlocked ? "" : "\n通关 " + f.FairyUnlock + " 关解锁"),
                30, new Color(0.45f, 0.25f, 0.08f), FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place(nameT.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(270, 90), new Vector2(65, 20));

            // 碎片进度（101-106 专属碎片）
            int fragId = f.FairyFragment;
            int have = fragId > 0 && SaveManager.Data.items.ContainsKey(fragId) ? SaveManager.Data.items[fragId] : 0;
            var fragT = UIHelper.Text(card, "Frag", "专属碎片 " + have + "/100", 22,
                new Color(0.3f, 0.32f, 0.4f), FontStyle.Normal, TextAnchor.MiddleLeft);
            UIHelper.Place(fragT.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(270, 30), new Vector2(65, -35));

            // 技能等级 + 升级入口（M2 已开放）
            int lv = FairySkillSystem.LevelOf(f.id);
            var skill = LevelSkill(f);
            string tipTxt = skill != null
                ? $"技能 · {skill.SkillName} Lv{lv}"
                : "技能 · 未配置";
            var tip = UIHelper.Text(card, "Tip", tipTxt, 22,
                new Color(0.55f, 0.35f, 0.15f), FontStyle.Normal, TextAnchor.MiddleCenter);
            UIHelper.Place(tip.rectTransform, new Vector2(1f, 0.5f), new Vector2(150, 40), new Vector2(-35, 0));

            if (unlocked)
            {
                var up = UIHelper.Button(card, "Up", () =>
                {
                    PanelManager.Instance.Push<FairyUpgradePopup>(p => p.FairyId = f.id);
                    Debug.Log($"[精灵] 打开 {f.FairyName}({f.id}) 技能升级");
                });
                UIHelper.Place((RectTransform)up.transform, new Vector2(1f, 0.5f), new Vector2(120, 44), new Vector2(-35, 50));
                var uimg = up.gameObject.GetComponent<Image>();
                uimg.color = new Color(0.95f, 0.78f, 0.32f);
                var ul = UIHelper.Text(up.transform, "L", "升级", 22, new Color(0.3f, 0.18f, 0.05f), FontStyle.Bold);
                UIHelper.Stretch(ul.rectTransform);
            }

            if (!unlocked)
            {
                var mask = UIHelper.Image(card, "Mask");
                UIHelper.Stretch((RectTransform)mask.transform);
                mask.color = new Color(0, 0, 0, 0.45f);
            }
        }

        private static FairySkillInfo LevelSkill(FairyInfo f)
        {
            var skills = FairySkillSystem.SkillsOf(f.id);
            return skills.Count > 0 ? skills[0] : null;
        }

        public override void Refresh() => Rebuild();

        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}