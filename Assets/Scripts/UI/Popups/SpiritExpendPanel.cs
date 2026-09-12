using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>退出扣体力确认弹窗（1:1 复刻 res_SpiritExpend_12907）。
    /// 损失条目复刻 res_itemReward_12769 结构（imgIcon + txtSpiritNum）。</summary>
    public sealed class SpiritExpendPanel : UIPanel
    {
        public System.Action OnContinue;    // 继续玩
        public System.Action OnQuit;        // 退出（扣体力）

        protected override void Build()
        {
            var bgMask = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)bgMask.transform);
            bgMask.color = new Color(0f, 0f, 0f, 0.6f);

            var panel = StretchNode(Root, "Panel");

            // 底板 575×560 @(0,12)
            var bgT = UiImg(panel, "Image", "Bg_02di", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 12f), new Vector2(575f, 560f)).transform;

            SpinePlaceholder(bgT, "BlueSkeletonGraphic", new Vector2(0f, 298.5f));

            // Image（dump 默认隐藏）
            var i1 = Node(bgT, "Image", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 72.5f), new Vector2(505f, 244f));
            i1.gameObject.AddComponent<Image>();
            i1.gameObject.SetActive(false);
            // Image（whitebantou，dump 默认隐藏）
            var i2 = UiImg(bgT, "Image", "whitebantou", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -7.5f), new Vector2(436f, 46f));
            i2.gameObject.SetActive(false);

            UiImg(bgT, "Image (2)", "fail01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 65.5f), new Vector2(209f, 126f));

            // itemParent 480×120 @(0,-49)：Bg_nei2 底 + 横向排列
            var itemParent = UiImg(bgT, "itemParent", "Bg_nei2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -49f), new Vector2(480f, 120f));
            var hlg = itemParent.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // itemReward 模板（dump 默认隐藏，保留原坐标）
            var tpl = Node(bgT, "itemReward", V(0f, 1f), V(0f, 1f),
                new Vector2(375f, -714.5f), new Vector2(80f, 110f));
            UiImg(tpl, "imgIcon", "output_icon_1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 16.3f), new Vector2(70f, 70f));
            Txt(tpl, "txtSpiritNum", "-1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -29.2f), new Vector2(100f, 50f), true);
            tpl.gameObject.SetActive(false);

            // btnContinue 254×92 @(-135,-174.5)
            var btnContinue = ImgBtn(bgT, "btnContinue", "Btn_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-135f, -174.5f), new Vector2(254f, 92f), ContinueClicked);
            Txt(btnContinue.transform, "Text", "继续", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 1.5f), new Vector2(150f, 76f), true);

            // btnQuit 232×92 @(128,-174.6)
            var btnQuit = ImgBtn(bgT, "btnQuit", "grey", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(128f, -174.6f), new Vector2(232f, 92f), QuitClicked);
            var spirit = UiImg(btnQuit.transform, "imgSpirit", "spiritxin", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-60.4f, 0f), new Vector2(80f, 65f));
            Txt(spirit.transform, "Text", "-1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-1f, 3f), new Vector2(90f, 76f), true);
            spirit.gameObject.SetActive(false);
            Txt(btnQuit.transform, "txtState", "退出", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 1.5f), new Vector2(150f, 76f), true);

            // txtExplain（dump 默认隐藏）
            Txt(bgT, "txtExplain", "将重置本关进度！", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -7.5f), new Vector2(300f, 80f), true).gameObject.SetActive(false);

            Txt(bgT, "txtExplainTip", "退出将损失以下奖励！", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 154.5f), new Vector2(480f, 60f), true);
            Txt(bgT, "txtTitle", "退出?", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(10f, 300.5f), new Vector2(240f, 100f), true);

            // 损失条目（复刻 itemReward：imgIcon + txtSpiritNum）
            MakeItemReward(itemParent.transform, "itemReward1", "output_icon_1", "-1");
        }

        private void ContinueClicked()
        {
            if (OnContinue != null) OnContinue();
            PanelManager.Instance.Pop();
        }

        private void QuitClicked()
        {
            if (OnQuit != null) OnQuit();
            PanelManager.Instance.Pop();
        }

        /// <summary>复刻 itemReward 条目：imgIcon(70×70) + txtSpiritNum(100×50)。</summary>
        private void MakeItemReward(Transform parent, string name, string spName, string numText)
        {
            var root = Node(parent, name, V(0f, 0f), V(0f, 0f), Vector2.zero, new Vector2(80f, 110f));
            UiImg(root, "imgIcon", spName, V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 16.3f), new Vector2(70f, 70f));
            Txt(root, "txtSpiritNum", numText, V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -29.2f), new Vector2(100f, 50f), true);
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
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var rt = UIHelper.NewRect(parent, name);
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        private static Image UiImg(Transform parent, string name, string sp,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            var s = OriginalAssets.GetUi(sp);
            if (s != null) img.sprite = s;
            else img.color = new Color(1f, 1f, 1f, 0.25f);   // GetUi null → 半透明纯色兜底
            return img;
        }

        private static Text Txt(Transform parent, string name, string content,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, bool outline = false)
        {
            int fs = Mathf.Clamp(Mathf.RoundToInt(size.y / 2.2f), 20, 44);
            var t = UIHelper.Text(parent, name, content, fs, Color.white);
            var rt = t.rectTransform;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            if (outline) rt.gameObject.AddComponent<Outline>();
            return t;
        }

        private static Button ImgBtn(Transform parent, string name, string sp,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, System.Action onClick)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            var s = OriginalAssets.GetUi(sp);
            if (s != null) img.sprite = s;
            else img.color = new Color(1f, 1f, 1f, 0.25f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        private static RectTransform SpinePlaceholder(Transform parent, string name, Vector2 pos)
        {
            var rt = Node(parent, name, V(0.5f, 0.5f), V(0.5f, 0.5f), pos, new Vector2(100f, 100f));
            UIHelper.NewRect(rt, "Renderer0");   // TODO spine: SkeletonGraphic 渲染占位
            return rt;
        }
    }
}
