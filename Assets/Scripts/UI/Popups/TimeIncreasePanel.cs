using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>加时间道具弹窗（1:1 复刻 res_TimeIncrease_10177）。</summary>
    public sealed class TimeIncreasePanel : UIPanel
    {
        public System.Action OnUse;     // 使用回调

        protected override void Build()
        {
            var bgMask = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)bgMask.transform);
            bgMask.color = new Color(0f, 0f, 0f, 0.6f);

            var panel = StretchNode(Root, "Panel");

            UiImg(panel, "Image", "lightbg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 99f), new Vector2(352f, 353f));

            // imgclock（dump 默认隐藏）
            var clock = UiImg(panel, "imgclock", "timebig", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 96f), new Vector2(256f, 293f));
            clock.rectTransform.localScale = new Vector3(0.7f, 0.7f, 1f);
            clock.gameObject.SetActive(false);

            // imgmagnet（默认展示的道具图）
            var magnet = UiImg(panel, "imgmagnet", "matcbig", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 96f), new Vector2(276f, 283f));
            magnet.rectTransform.localScale = new Vector3(0.7f, 0.7f, 1f);

            UiImg(panel, "Image", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-145f, 190f), new Vector2(42f, 55.5f));
            UiImg(panel, "Image (1)", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-106f, 36f), new Vector2(28f, 37f));
            UiImg(panel, "Image (2)", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(134f, 124f), new Vector2(36.4f, 48.1f));

            // btnShare 374×103 @(0,-293)
            var btnShare = ImgBtn(panel, "btnShare", "Btn_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -293f), new Vector2(374f, 103f), UseClicked);
            UiImg(btnShare.transform, "imgShare", "invite_icon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-61.3f, 0f), new Vector2(46f, 49f));
            Txt(btnShare.transform, "Text", "使用", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(26.8f, 3f), new Vector2(120f, 70f), true);

            // btnVideo（dump 默认隐藏）
            var btnVideo = ImgBtn(panel, "btnVideo", "Btn_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -293f), new Vector2(374f, 103f), UseClicked);
            UiImg(btnVideo.transform, "imgVideo", "icon_video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-61f, 0f), new Vector2(47f, 40f));
            Txt(btnVideo.transform, "Text", "使用", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(26.8f, 3f), new Vector2(120f, 70f), true);
            btnVideo.gameObject.SetActive(false);

            ImgBtn(panel, "btnClose", "close_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(273f, 465f), new Vector2(71f, 70f), () => PanelManager.Instance.Pop());

            Txt(panel, "txtTip", "使用时间可以增加30秒的时间", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -154f), new Vector2(600f, 80f), true);
            Txt(panel, "txtTitle", "加时间", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 393f), new Vector2(340f, 100f), true);
        }

        private void UseClicked()
        {
            if (OnUse != null) OnUse();
            PanelManager.Instance.Pop();
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
    }
}
