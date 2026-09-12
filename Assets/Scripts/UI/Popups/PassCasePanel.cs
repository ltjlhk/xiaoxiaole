using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>魔盒延时弹窗（1:1 复刻 res_PassCase_10566）。</summary>
    public sealed class PassCasePanel : UIPanel
    {
        protected override bool BlockClick => true;

        protected override void Build()
        {
            var backgroup = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)backgroup.transform);
            backgroup.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = StretchNode(Root, "Panel");

            var lightbg = UiImg(panel, "Image", "lightbg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 99f), V(352f, 353f));   // dump: LightEffectControl 动效脚本不复刻

            UiImg(panel, "Image", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-145f, 190f), V(42f, 55.5f));
            UiImg(panel, "Image (1)", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-106f, 36f), V(28f, 37f));
            UiImg(panel, "Image (2)", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(134f, 124f), V(36.4f, 48.1f));

            var btnVideo = ImgBtn(panel, "btnVideo", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -293f), V(374f, 103f), () => PanelManager.Instance.Pop());
            UiImg(btnVideo.transform, "Image", "video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-75.5f, 0f), V(47f, 39f));
            Txt(btnVideo.transform, "Text", "延时120秒", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(39.8f, 2f), V(180f, 70f), true);

            ImgBtn(panel, "btnClose", "close_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(273f, 465f), V(71f, 70f), () => PanelManager.Instance.Pop());

            Txt(panel, "txtTip", "是否延时存在时间，以获取装扮碎片？", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -154f), V(600f, 80f));
            Txt(panel, "txtTitle", "魔盒即将消失", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 393f), V(440f, 100f), true);

            Node(panel, "imgPos", V(0.5f, 0.5f), V(0.5f, 0.5f), V(0f, 105f), V(100f, 100f));
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
