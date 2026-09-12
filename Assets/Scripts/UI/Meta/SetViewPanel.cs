using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>设置面板（原版 SetView #11605 逆向 1:1）。
    /// 震动/音效/音乐/语音/精灵五组开关按钮按 dump 摆放，点击切换明暗态（原版 SetViewControl 行为）。</summary>
    public sealed class SetViewPanel : UIPanel
    {
        /// <summary>跨面板钩子：返回主页（由主城/外部注入调用方逻辑；原版面板内无主页按钮节点）。</summary>
        public System.Action OnHome;

        private static readonly Color Fallback = new Color(0.16f, 0.2f, 0.3f, 0.9f);

        // 开关态（无存档字段，仅运行时切换明暗）
        private readonly List<Image> _toggleImgs = new List<Image>();
        private readonly List<bool> _toggleOn = new List<bool>();

        protected override void Build()
        {
            // backgroup（原版无精灵 → 半透明遮罩兜底）
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)back.transform);
            back.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            // 底板（pivot 0.5,1：顶端定位 y=365，向下延伸 780）
            var bgImg = Img(panel, "Image", "Bg_02di", V(0.5f, 0.5f), V(575, 780), V(0, 365));
            bgImg.rectTransform.pivot = new Vector2(0.5f, 1f);

            // TODO spine: 原版为 SkeletonGraphic+SwapBanner 蓝色横幅动画（Renderer0=RawImage）
            var spine = UIHelper.NewRect(panel, "BlueSkeletonGraphic");
            UIHelper.Place(spine, V(0.5f, 0.5f), V(100, 100), V(0, 378));
            var renderer0 = UIHelper.NewRect(spine, "Renderer0");
            UIHelper.Place(renderer0, V(0.5f, 0.5f), V(100, 100), Vector2.zero);

            Img(panel, "Image2", "Bg_nei2", V(0.5f, 0.5f), V(505, 390), V(0, 71));

            // 震动
            var rowVibrate = UIHelper.NewRect(panel, "imgVibrate");
            UIHelper.Place(rowVibrate, V(0.5f, 0.5f), V(65, 62), V(-147, 67));
            Img(rowVibrate, "icon", "vibration-on-icon", V(0.5f, 0.5f), V(65, 62), Vector2.zero);
            Img(rowVibrate.transform, "Image", "vibration_zi", V(0.5f, 0.5f), V(66, 37), V(89.8f, 0));
            AddToggle(rowVibrate, "btnVibrate", "kai", V(0.5f, 0.5f), V(161, 44), V(256, 2), "震动");

            // 音效
            var rowSound = UIHelper.NewRect(panel, "imgSound");
            UIHelper.Place(rowSound, V(0.5f, 0.5f), V(65, 62), V(-147, 214));
            Img(rowSound, "icon", "soundeffects-on-icon", V(0.5f, 0.5f), V(65, 62), Vector2.zero);
            Img(rowSound.transform, "Image", "soundeffects_zi", V(0.5f, 0.5f), V(66, 37), V(89.8f, -2));
            AddToggle(rowSound, "btnSound", "kai", V(0.5f, 0.5f), V(161, 44), V(256, 0), "音效");

            // 音乐
            var rowMusic = UIHelper.NewRect(panel, "imgMusic");
            UIHelper.Place(rowMusic, V(0.5f, 0.5f), V(65, 62), V(-147, 141.1f));
            Img(rowMusic, "icon", "sound-on-icon", V(0.5f, 0.5f), V(65, 62), Vector2.zero);
            Img(rowMusic.transform, "Image (1)", "sound_zi", V(0.5f, 0.5f), V(66, 37), V(89.8f, -2));
            AddToggle(rowMusic, "btnMusic", "kai", V(0.5f, 0.5f), V(161, 44), V(256, 0), "音乐");

            // 语音
            var rowVoice = UIHelper.NewRect(panel, "imgVoice");
            UIHelper.Place(rowVoice, V(0.5f, 0.5f), V(65, 62), V(-147, -3));
            Img(rowVoice, "icon", "yuyinon", V(0.5f, 0.5f), V(65, 62), Vector2.zero);
            Img(rowVoice.transform, "Image (2)", "yuyin", V(0.5f, 0.5f), V(64, 34), V(89.8f, 0));
            AddToggle(rowVoice, "btnVoice", "kai", V(0.5f, 0.5f), V(161, 44), V(256, 2), "语音");

            // 精灵
            var rowElves = UIHelper.NewRect(panel, "imgElves");
            UIHelper.Place(rowElves, V(0.5f, 0.5f), V(56, 55), V(-147, -72));
            Img(rowElves, "icon", "jinglingon", V(0.5f, 0.5f), V(56, 55), Vector2.zero);
            Img(rowElves.transform, "Image (2)", "jingling", V(0.5f, 0.5f), V(67, 37), V(89.8f, 0));
            AddToggle(rowElves, "btnElves", "kai", V(0.5f, 0.5f), V(161, 44), V(256, 2), "精灵");

            var btnClose = SpriteBtn(panel, "btnClose", "tcclose_01", V(0.5f, 0.5f), V(50, 49), V(281.4f, 326),
                () => PanelManager.Instance.Pop());

            // 意见反馈
            var btnFeedback = SpriteBtn(panel, "btnFeedback", "shezhi", V(0.5f, 0.5f), V(330, 90), V(0, -195),
                () => Debug.Log("[SetView] 意见反馈"));
            Img(btnFeedback.transform, "Image", "fankui", V(0.5f, 0.5f), V(63, 56), V(-92.5f, -2.3f));
            var tFeedback = Txt(btnFeedback.transform, "Text (Legacy)", "意见反馈", 60, V(0.5f, 0.5f), V(160, 60), V(38, 0));
            Outline(tFeedback);

            // 联系我们
            var btnCustomer = SpriteBtn(panel, "btnCustomer", "shezhi", V(0.5f, 0.5f), V(330, 90), V(0, -314),
                () => Debug.Log("[SetView] 联系我们"));
            Img(btnCustomer.transform, "Image", "kefu", V(0.5f, 0.5f), V(63, 56), V(-92.5f, 0));
            var tCustomer = Txt(btnCustomer.transform, "Text (Legacy)", "联系我们", 60, V(0.5f, 0.5f), V(160, 60), V(38, 0));
            Outline(tCustomer);

            var tTitle = Txt(panel, "Text (Legacy)", "设置", 80, V(0.5f, 0.5f), V(240, 80), V(0, 377));
            Outline(tTitle);

            var tVersion = Txt(panel, "txtVersion", "v1.0.0", 45, V(0.5f, 0.5f), V(180, 45), V(153.8f, -370));
            Outline(tVersion);
        }

        /// <summary>开关行按钮：点击切换明暗（开=原精灵，关=压暗）。</summary>
        private void AddToggle(Transform row, string name, string sprite,
            Vector2 anchor, Vector2 size, Vector2 pos, string label)
        {
            var btn = SpriteBtn(row, name, sprite, anchor, size, pos, null);
            int idx = _toggleImgs.Count;
            _toggleImgs.Add(btn.gameObject.GetComponent<Image>());
            _toggleOn.Add(true);
            btn.onClick.AddListener(() =>
            {
                _toggleOn[idx] = !_toggleOn[idx];
                _toggleImgs[idx].color = _toggleOn[idx] ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f);
                Debug.Log("[SetView] " + label + " -> " + (_toggleOn[idx] ? "开" : "关"));
            });
        }

        // ===== 本文件内的小工具（sprite 缺失 → 半透明纯色兜底） =====

        private static Vector2 V(float x, float y) { return new Vector2(x, y); }

        private static Image Img(Transform parent, string name, string sprite,
            Vector2 anchor, Vector2 size, Vector2 pos)
        {
            var sp = sprite != null ? OriginalAssets.GetUi(sprite) : null;
            var img = UIHelper.Image(parent, name, sp);
            if (sp == null && sprite != null) img.color = Fallback;
            UIHelper.Place((RectTransform)img.transform, anchor, size, pos);
            return img;
        }

        private static Text Txt(Transform parent, string name, string content, float boxH,
            Vector2 anchor, Vector2 size, Vector2 pos)
        {
            int fs = Mathf.Clamp(Mathf.RoundToInt(boxH / 2.2f), 20, 44);
            var t = UIHelper.Text(parent, name, content, fs, Color.white);
            UIHelper.Place((RectTransform)t.transform, anchor, size, pos);
            return t;
        }

        private static void Outline(Graphic g)
        {
            var o = g.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.85f);
        }

        private static Button SpriteBtn(Transform parent, string name, string sprite,
            Vector2 anchor, Vector2 size, Vector2 pos, System.Action onClick)
        {
            var img = Img(parent, name, sprite, anchor, size, pos);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }
    }
}
