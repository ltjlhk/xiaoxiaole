using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>连胜奖励结算弹窗（1:1 复刻 res_RewardOver_12509）。</summary>
    public sealed class RewardOverPanel : UIPanel
    {
        protected override bool BlockClick => true;

        protected override void Build()
        {
            var backgroup = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)backgroup.transform);
            backgroup.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = StretchNode(Root, "Panel");

            // BlueSkeletonGraphic → spine 占位
            SpinePlaceholder(panel, "BlueSkeletonGraphic", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 301f), V(100f, 100f));

            var btnVideo = ImgBtn(panel, "btnVideo", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -259f), V(344f, 108f), () => PanelManager.Instance.Pop());
            UiImg(btnVideo.transform, "Image", "video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-89f, -1f), V(47f, 40f));
            Txt(btnVideo.transform, "Text (1)", "领取奖励", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(33.5f, 1f), V(200f, 70f), true);

            // btnClose（放弃，dump 默认 inactive）
            var btnGiveUp = ImgBtn(panel, "btnClose", "Btn_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-177f, -259f), V(250f, 108f), () => PanelManager.Instance.Pop());
            Txt(btnGiveUp.transform, "Text (1)", "放弃", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 1f), V(200f, 70f), true);
            btnGiveUp.gameObject.SetActive(false);

            // btnClose（右上角 X，本体 Image 无 sprite）
            var btnCloseX = Node(panel, "btnClose", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(279.7f, 251f), V(100f, 100f));
            var xImg = btnCloseX.gameObject.AddComponent<Image>();
            xImg.color = new Color(1f, 1f, 1f, 0f);              // dump: Image 无 sprite
            var xBtn = btnCloseX.gameObject.AddComponent<Button>();
            xBtn.targetGraphic = xImg;
            xBtn.onClick.AddListener(() => PanelManager.Instance.Pop());
            UiImg(btnCloseX, "Image", "tcclose_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(50f, 49f));

            var imgParent = Node(panel, "imgParent", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(500f, 180f));
            var ilg = imgParent.gameObject.AddComponent<HorizontalLayoutGroup>();
            ilg.spacing = 24f;
            ilg.childAlignment = TextAnchor.MiddleCenter;
            ilg.childForceExpandWidth = false;
            ilg.childForceExpandHeight = false;

            Txt(panel, "Text (Legacy)", "恭喜获得", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 299f), V(300f, 95f), true);
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

        private static RectTransform SpinePlaceholder(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size);   // TODO spine: SkeletonGraphic 占位
            UIHelper.NewRect(rt, "Renderer0");                    // TODO spine: 渲染占位
            return rt;
        }
    }
}
