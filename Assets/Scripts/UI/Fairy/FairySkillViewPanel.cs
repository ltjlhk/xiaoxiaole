using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>精灵技能展示弹窗（1:1 复刻 res_FairySkillView_12958）。
    /// 数据：FairySpineMap.Info(FairyId) + FairySkillSystem；升级写存档。</summary>
    public sealed class FairySkillViewPanel : UIPanel
    {
        public int FairyId = 1;     // PanelManager.Push 时注入

        private FairyInfo _fairy;
        private FairySkillInfo _skill;

        protected override void Build()
        {
            _fairy = FairySpineMap.Info(FairyId);
            var skills = FairySkillSystem.SkillsOf(FairyId);
            _skill = skills.Count > 0 ? skills[0] : null;

            // backgroup：全屏遮罩（点击空白关闭由 Close 层承接）
            var bgMask = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)bgMask.transform);
            bgMask.color = new Color(0f, 0f, 0f, 0.6f);

            var panel = StretchNode(Root, "Panel");

            // bg 575×480.1 @(0,68.4)
            var bgT = UiImg(panel, "bg", "Bg_01di", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 68.4f), new Vector2(575f, 480.1f)).transform;

            // Icon 352×353 @(0,-32)
            var icon = Node(bgT, "Icon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -32f), new Vector2(352f, 353f));
            UiImg(icon, "Image", "lightbg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(394.2f, 395.3f));
            string iconSp = _skill != null && !string.IsNullOrEmpty(_skill.SkillIcon)
                ? _skill.SkillIcon : "skill_autoCombine";
            var skillIcon = UiImg(icon, "imgSkillIcon", iconSp, V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 9f), new Vector2(383.7f, 387.1f));
            skillIcon.rectTransform.localScale = new Vector3(0.7f, 0.7f, 1f);
            // photoTip（dump 默认隐藏）
            var photoTip = UiImg(icon, "photoTip", "photoTip", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 123f), new Vector2(437.9f, 373f));
            photoTip.rectTransform.localScale = new Vector3(0.7f, 0.7f, 1f);
            photoTip.gameObject.SetActive(false);

            // 说明两行
            string explain = _skill != null && !string.IsNullOrEmpty(_skill.SkillDesc)
                ? string.Format(_skill.SkillDesc ?? "{0}", _skill.TriggerParam1)
                : "进入关卡后，自动将牌侧、背面牌翻至正面状态";
            Txt(bgT, "txtExplain", explain, V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -272f), new Vector2(686.8f, 106.4f));
            Txt(bgT, "txtExplain (1)", "完成<color=#62A5F5>普通</color>和<color=#BC2E",
                V(0.5f, 0.5f), V(0.5f, 0.5f), new Vector2(0f, -340f), new Vector2(686.8f, 106.4f));

            // 冷却行（dump 默认隐藏）
            Txt(bgT, "txtCurrentCd", "当前冷却：120秒", V(0f, 0f), V(0f, 0f),
                new Vector2(131f, 173.2f), new Vector2(214f, 62.4f)).gameObject.SetActive(false);
            Txt(bgT, "txtNextCd", "下次冷却：<color=red>120秒</color>", V(1f, 0f), V(1f, 0f),
                new Vector2(-138f, 173.2f), new Vector2(214f, 62.4f)).gameObject.SetActive(false);

            // btnUpgrade 374×103 @(0,91)（dump 默认隐藏；有可升级档位时显示）
            var btnUpgrade = ImgBtn(bgT, "btnUpgrade", "Btn_02", V(0.5f, 0f), V(0.5f, 0f),
                new Vector2(0f, 91f), new Vector2(374f, 103f), null);
            UiImg(btnUpgrade.transform, "imgCoin", "picIcon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(50.5f, 50.5f));
            Txt(btnUpgrade.transform, "buy", "升级", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-86f, 0f), new Vector2(120f, 80f), true);
            var costText = Txt(btnUpgrade.transform, "txtCost", "100", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(91.3f, 0f), new Vector2(120f, 80f), true);
            btnUpgrade.gameObject.SetActive(false);

            string title = _skill != null && !string.IsNullOrEmpty(_skill.SkillName)
                ? _skill.SkillName
                : (_fairy != null && !string.IsNullOrEmpty(_fairy.FairyName) ? _fairy.FairyName : "自动翻牌");
            Txt(bgT, "txtTitle", title, V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 267.1f), new Vector2(536.7f, 134.2f), true);

            Txt(panel, "CloseTxt", "点 击 任 意 空 白 处 关 闭", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -560f), new Vector2(340.9f, 49.8f));

            // Image（dump 默认隐藏的全屏底图）
            var imgExtra = Node(panel, "Image", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(750f, 1334f));
            imgExtra.gameObject.AddComponent<Image>();
            imgExtra.gameObject.SetActive(false);

            // Close：全屏点击任意空白关闭
            var close = UIHelper.Button(Root, "Close", () => PanelManager.Instance.Pop());
            UIHelper.Stretch((RectTransform)close.transform);

            // 数据装配：升级入口（金币消耗走 FairySkillSystem）
            if (_skill != null)
            {
                int lv = FairySkillSystem.LevelOf(FairyId);
                if (lv < FairySkillSystem.MaxLevel(_skill))
                {
                    int cost = FairySkillSystem.UpgradeGoldCost(_skill, lv);
                    if (cost >= 0)
                    {
                        btnUpgrade.gameObject.SetActive(true);
                        costText.text = cost.ToString();
                        btnUpgrade.onClick.AddListener(() =>
                        {
                            if (SaveManager.Data.coins < cost) return;
                            SaveManager.AddCoins(-cost);
                            SaveManager.SetFairySkillLevel(FairyId, lv + 1);
                            int next = FairySkillSystem.UpgradeGoldCost(_skill, lv + 1);
                            if (next < 0) btnUpgrade.gameObject.SetActive(false);
                            else costText.text = next.ToString();
                        });
                    }
                }
            }
        }

        public override void Refresh() { }

        // ---- dump 复刻小工具 ----
        private static Vector2 V(float x, float y) => new Vector2(x, y);

        private static RectTransform StretchNode(Transform parent, string name)
        {
            var rt = UIHelper.NewRect(parent, name);
            UIHelper.Stretch(rt);
            return rt;
        }

        private static RectTransform Node(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            var rt = UIHelper.NewRect(parent, name);
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        private static Image UiImg(Transform parent, string name, string sp,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size, pivot);
            var img = rt.gameObject.AddComponent<Image>();
            var s = OriginalAssets.GetUi(sp);
            if (s != null) img.sprite = s;
            else img.color = new Color(1f, 1f, 1f, 0.25f);   // GetUi null → 半透明纯色兜底
            return img;
        }

        private static Text Txt(Transform parent, string name, string content,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, bool outline = false, Vector2? pivot = null)
        {
            int fs = Mathf.Clamp(Mathf.RoundToInt(size.y / 2.2f), 20, 44);
            var t = UIHelper.Text(parent, name, content, fs, Color.white);
            var rt = t.rectTransform;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            if (outline) rt.gameObject.AddComponent<Outline>();
            return t;
        }

        private static Button ImgBtn(Transform parent, string name, string sp,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, System.Action onClick, Vector2? pivot = null)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size, pivot);
            var img = rt.gameObject.AddComponent<Image>();
            var s = OriginalAssets.GetUi(sp);
            if (s != null) img.sprite = s;
            else img.color = new Color(1f, 1f, 1f, 0.25f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }
    }
}
