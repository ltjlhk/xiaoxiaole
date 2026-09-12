using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>暂停弹窗（1:1 复刻 res_PauseView_11631）。</summary>
    public sealed class PauseViewPanel : UIPanel
    {
        public System.Action OnResume, OnRestart, OnHome;

        protected override bool BlockClick => true;

        protected override void Build()
        {
            var backgroup = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)backgroup.transform);
            backgroup.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = StretchNode(Root, "Panel");

            // 底板（dump pivot=(0.5,1)）
            UiImg(panel, "Image", "Bg_02di", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 370f), V(575f, 790f), V(0.5f, 1f));
            UiImg(panel, "Image", "Bg_nei2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 265.3f), V(505f, 392f), V(0.5f, 1f));

            // BlueSkeletonGraphic → spine 占位（Renderer0 为 SkeletonGraphic 渲染子节点）
            SpinePlaceholder(panel, "BlueSkeletonGraphic", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 379.8f), V(100f, 100f));

            ImgBtn(panel, "btnClose", "tcclose_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(282.2f, 331.8f), V(50f, 49f), () => PanelManager.Instance.Pop());

            var btnStart = ImgBtn(panel, "btnStart", "shezhi", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-98f, -200f), V(170f, 90f),
                () => { OnHome?.Invoke(); PanelManager.Instance.Pop(); });
            UiImg(btnStart.transform, "Image", "home-icon-stroke", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(63f, 59f));

            var btnAgain = ImgBtn(panel, "btnAgain", "shezhi", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(98f, -200f), V(170f, 90f),
                () => { OnRestart?.Invoke(); PanelManager.Instance.Pop(); });
            UiImg(btnAgain.transform, "Image", "replay-icon-stroke", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(54f, 57f));

            var btnBack = ImgBtn(panel, "btnBack", "kaishi", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -324f), V(365f, 90f),
                () => { OnResume?.Invoke(); PanelManager.Instance.Pop(); });
            UiImg(btnBack.transform, "Image", "play-icon-thin", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(5f, 0f), V(43f, 60f));

            UiImg(panel, "Image", "xian", V(0.5f, 0.5f), V(0.5f, 0.5f), V(0f, 178.8f), V(400f, 4f));
            UiImg(panel, "Image (3)", "xian", V(0.5f, 0.5f), V(0.5f, 0.5f), V(0f, 102.8f), V(400f, 4f));
            UiImg(panel, "Image (4)", "xian", V(0.5f, 0.5f), V(0.5f, 0.5f), V(0f, 31.6f), V(400f, 4f));
            UiImg(panel, "Image (5)", "xian", V(0.5f, 0.5f), V(0.5f, 0.5f), V(0f, -46.7f), V(400f, 4f));

            // 震动
            var imgVibrate = UiImg(panel, "imgVibrate", "vibration-on-icon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-147f, 65.8f), V(65f, 62f));
            UiImg(imgVibrate.transform, "Image (1)", "vibration_zi", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(89.8f, 1f), V(66f, 37f));
            ImgBtn(imgVibrate.transform, "btnVibrate", "kai", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(256f, 2f), V(161f, 44f), null);

            // 音效
            var imgSound = UiImg(panel, "imgSound", "soundeffects-on-icon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-147f, 216.8f), V(65f, 62f));
            UiImg(imgSound.transform, "Image", "soundeffects_zi", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(89.8f, -1f), V(66f, 37f));
            ImgBtn(imgSound.transform, "btnSound", "kai", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(256f, 0f), V(161f, 44f), null);

            // 音乐
            var imgMusic = UiImg(panel, "imgMusic", "sound-on-icon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-147f, 140.8f), V(65f, 62f));
            UiImg(imgMusic.transform, "Image (2)", "sound_zi", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(89.8f, -1f), V(66f, 37f));
            ImgBtn(imgMusic.transform, "btnMusic", "kai", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(256f, 0f), V(161f, 44f), null);

            // 语音
            var imgVoice = UiImg(panel, "imgVoice", "yuyinon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-147f, -10.2f), V(65f, 62f));
            UiImg(imgVoice.transform, "Image (3)", "yuyin", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(89.8f, 1f), V(66f, 37f));
            ImgBtn(imgVoice.transform, "btnVoice", "kai", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(256f, 2f), V(161f, 44f), null);

            // 精灵
            var imgElves = UiImg(panel, "imgElves", "jinglingon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-147f, -86.9f), V(56f, 55f));
            UiImg(imgElves.transform, "Image (3)", "jingling", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(89.8f, 1f), V(67f, 37f));
            ImgBtn(imgElves.transform, "btnElves", "kai", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(256f, 2f), V(161f, 44f), null);

            Txt(panel, "Text (Legacy)", "暂停", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 372.8f), V(240f, 80f), true);
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
