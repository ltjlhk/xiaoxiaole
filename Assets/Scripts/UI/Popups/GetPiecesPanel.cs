using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>获取碎片引导弹窗（1:1 复刻 res_GetPieces_12753）。</summary>
    public sealed class GetPiecesPanel : UIPanel
    {
        /// <summary>“去通关”跨面板跳转钩子。</summary>
        public System.Action OnGoLevel;

        protected override bool BlockClick => true;

        protected override void Build()
        {
            var backgroup = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)backgroup.transform);
            backgroup.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = StretchNode(Root, "Panel");

            var bg = UiImg(panel, "bg", "Bg_02di", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 36.1f), V(575f, 594.9f));

            var step1 = UiImg(bg.transform, "Image (2)", "step1", V(0.5f, 1f), V(0.5f, 1f),
                V(0f, -235.8f), V(515f, 284.3f));
            var statusBar = UiImg(step1.transform, "Image (1)", "status bar", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-0.0f, -98.1f), V(435f, 51.4f));
            Txt(statusBar.transform, "Text (Legacy)", "通关可获得不同精灵碎片",
                V(0f, 0f), V(1f, 1f), V(0f, 0f), V(0f, 0f), true);

            // BlueSkeletonGraphic → spine 占位（Renderer1/Renderer2 为 inactive 渲染子节点）
            var spine = SpinePlaceholder(bg.transform, "BlueSkeletonGraphic", V(0.5f, 1f), V(0.5f, 1f),
                V(0f, -6f), V(100f, 100f));
            Txt(spine, "txtTitle", "获取碎片", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(8f, -2f), V(300f, 80f), true);
            ImgBtn(spine, "btnClose", "tcclose_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(280.8f, -55f), V(36.7f, 35.9f), () => PanelManager.Instance.Pop());

            var tipText = Txt(bg.transform, "Text (Legacy)", "稀有精灵可在排行榜上展示哦！",
                V(0.5f, 0.5f), V(0.5f, 0.5f), V(0f, -92.6f), V(483.8f, 50.5f));
            tipText.gameObject.SetActive(false);

            var buttons = Node(bg.transform, "Buttons", V(0.5f, 0f), V(0.5f, 0f),
                V(0f, 99f), V(516.1f, 94f));
            var blg = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            blg.spacing = 24f;
            blg.childAlignment = TextAnchor.MiddleCenter;
            blg.childForceExpandWidth = false;
            blg.childForceExpandHeight = false;

            // btnShare（dump 默认 inactive）
            var btnShare = ImgBtn(buttons, "btnShare", "Btn_01", V(0f, 1f), V(0f, 1f),
                V(130f, -47f), V(260f, 100f), () => PanelManager.Instance.Pop());
            UiImg(btnShare.transform, "Image", "orange_icon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-78.8f, 0f), V(46f, 49f));
            Txt(btnShare.transform, "Text (Legacy)", "获得", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-17.4f, 0f), V(102.4f, 60f), true);
            var shareTip = Txt(btnShare.transform, "Text (Legacy) (2)", "每次可得", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-23f, 69.7f), V(201.9f, 60f), true);
            shareTip.gameObject.SetActive(false);
            UiImg(shareTip.transform, "Image (1)", "JL_001", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(64.7f, 0f), V(30f, 30f));
            Txt(shareTip.transform, "Text (Legacy)", "x1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(88.8f, -3.8f), V(47f, 60f));
            var shareIcon = UiImg(btnShare.transform, "Image (1)", "JL_001", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(43.9f, 2.4f), V(74.7f, 74.7f));
            Txt(shareIcon.transform, "Text (Legacy) (1)", "x1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(35.9f, -23f), V(47f, 46f), true);
            btnShare.gameObject.SetActive(false);

            // btnVideo1（dump 默认 inactive）
            var btnVideo1 = ImgBtn(buttons, "btnVideo1", "Btn_01", V(0f, 1f), V(0f, 1f),
                V(130f, -47f), V(260f, 100f), () => PanelManager.Instance.Pop());
            UiImg(btnVideo1.transform, "Image", "video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-78.8f, 0f), V(47f, 39f));
            Txt(btnVideo1.transform, "Text (Legacy)", "获得", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-17.4f, 0f), V(142.6f, 60f), true);
            var videoTip = Txt(btnVideo1.transform, "Text (Legacy) (2)", "每次可得", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-23f, 69.7f), V(201.9f, 60f), true);
            videoTip.gameObject.SetActive(false);
            UiImg(videoTip.transform, "Image (1)", "picIcon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(64.7f, 0f), V(30f, 30f));
            Txt(videoTip.transform, "Text (Legacy)", "x1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(88.8f, -3.8f), V(47f, 60f));
            var videoIcon = UiImg(btnVideo1.transform, "Image (1)", "JL_001", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(43.9f, 2.4f), V(74.7f, 74.7f));
            Txt(videoIcon.transform, "Text (Legacy) (1)", "x1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(35.9f, -23f), V(47f, 46f), true);
            btnVideo1.gameObject.SetActive(false);

            // btnGoToLevel
            var btnGoToLevel = ImgBtn(buttons, "btnGoToLevel", "Btn_02", V(0f, 0f), V(0f, 0f),
                V(0f, 0f), V(260f, 100f),
                () => { OnGoLevel?.Invoke(); PanelManager.Instance.Pop(); });
            Txt(btnGoToLevel.transform, "Text (Legacy)", "去通关", V(0f, 0f), V(1f, 1f),
                V(0f, 0f), V(0f, 0f), true);

            // 全屏 Image（dump 默认 inactive，无 sprite）
            var fullImg = Node(Root, "Image", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(750f, 1334f));
            fullImg.gameObject.AddComponent<Image>();
            fullImg.gameObject.SetActive(false);
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
