using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>每日挑战弹窗（dump res_DailyChallenge_10496 1:1）：挑战入口 + 次数说明 + 视频获次/领奖态。</summary>
    public sealed class DailyChallengePanel : UIPanel
    {
        protected override bool BlockClick => true;

        protected override void Build()
        {
            // backgroup
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch(back.rectTransform);
            back.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            // 弹窗底板（pivot 0.5,1）
            var bg = ImgX(panel, "bg", "Bg_02di", new Vector2(575, 810), new Vector2(0, 390));

            // Spine 占位
            var spine = Box(bg.transform, "BlueSkeletonGraphic", new Vector2(100, 100), new Vector2(0, 403));
            Box(spine, "Renderer0", new Vector2(100, 100), Vector2.zero);

            // 挑战图与次数底板
            Img(bg.transform, "Image", "tiaozhan", new Vector2(496, 314), new Vector2(0, 100.8f));
            Img(bg.transform, "Image (1)", "Bg_nei2", new Vector2(496, 116), new Vector2(0, -128.5f));

            // 挑战奖励条目容器（横排，由外部按天填充）
            var items = Box(bg.transform, "itemParent", new Vector2(496, 116), new Vector2(0, -128.5f));
            var hlg = items.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 12;

            // 前往挑战按钮
            var challenge = Btn(bg.transform, "btnChallenge", "greenbg", new Vector2(370, 122), new Vector2(0, -311.9f),
                () => Debug.Log("[DailyChallenge] 前往挑战"));
            Txt(challenge.transform, "txtEnter", "前往挑战", Fs(65), new Vector2(320, 65), new Vector2(0, 3), true);
            Txt(challenge.transform, "txtUnlock", "第2关解锁", Fs(65), new Vector2(320, 65), new Vector2(0, 5), true)
                .gameObject.SetActive(false);

            // 视频获取次数按钮（默认隐藏）
            var video = Btn(bg.transform, "btnVideo", "greenbg", new Vector2(370, 122), new Vector2(0, -311.9f),
                () => Debug.Log("[DailyChallenge] 观看视频获取次数"));
            Img(video.transform, "Image", "icon_video", new Vector2(47, 40), new Vector2(-79.9f, 0));
            Txt(video.transform, "txtUnlock", "获取次数", Fs(80), new Vector2(200, 80), new Vector2(40, 3), true);
            video.gameObject.SetActive(false);

            // 领取奖励按钮（默认隐藏）
            var draw = Btn(bg.transform, "btnDraw", "Btn_01", new Vector2(380, 112), new Vector2(0, -310),
                () => Debug.Log("[DailyChallenge] 领取奖励"));
            Txt(draw.transform, "txtUnlock", "领取奖励", Fs(70), new Vector2(280, 70), Vector2.zero, true);
            draw.gameObject.SetActive(false);

            // 关闭
            Btn(bg.transform, "btnClose", "tcclose_01", new Vector2(50, 49), new Vector2(278.4f, 352),
                () => PanelManager.Instance.Pop());

            // 文案
            Txt(bg.transform, "txtTitle", "每日挑战", Fs(80), new Vector2(320, 80), new Vector2(0, 397), true, FontStyle.Bold);
            Txt(bg.transform, "txtPeopleNum", "今天通关人数：0", Fs(60), new Vector2(320, 60), new Vector2(0, 296));
            Txt(bg.transform, "txtChallengeNum", "挑战次数：1", Fs(60), new Vector2(340, 60), new Vector2(0, -218));
            Txt(bg.transform, "txtDes", "每日00:00刷新挑战次数", Fs(60), new Vector2(420, 60), new Vector2(0, -447));
        }

        public override void Refresh() { }

        // ===== 搭建小工具（基于 UIHelper） =====

        private static int Fs(float h) => Mathf.Clamp(Mathf.RoundToInt(h / 2.2f), 20, 44);

        private static RectTransform Rt(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot,
            Vector2 size, Vector2 pos)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            return rt;
        }

        private static RectTransform Box(Transform parent, string name, Vector2 size, Vector2 pos)
            => Rt(UIHelper.NewRect(parent, name), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), size, pos);

        private static Image Img(Transform parent, string name, string sp, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name, OriginalAssets.GetUi(sp));
            if (img.sprite == null) img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), size, pos);
            return img;
        }

        /// <summary>带自定义 pivot 的 Image（dump 中 pivot=(0.5,1) 的节点）。</summary>
        private static Image ImgX(Transform parent, string name, string sp, Vector2 size, Vector2 pos)
        {
            var img = Img(parent, name, sp, size, Vector2.zero);
            img.rectTransform.pivot = new Vector2(0.5f, 1f);
            img.rectTransform.anchoredPosition = pos;
            return img;
        }

        private static Text Txt(Transform parent, string name, string content, int fontSize,
            Vector2 size, Vector2 pos, bool outline = false, FontStyle style = FontStyle.Normal)
        {
            var t = UIHelper.Text(parent, name, content, fontSize, Color.white, style);
            Rt(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), size, pos);
            if (outline)
            {
                var o = t.gameObject.AddComponent<Outline>();
                o.effectColor = Color.black;
                o.effectDistance = new Vector2(2f, -2f);
            }
            return t;
        }

        private static Button Btn(Transform parent, string name, string sp, Vector2 size, Vector2 pos,
            System.Action onClick)
        {
            var b = UIHelper.Button(parent, name, onClick);
            var img = b.gameObject.GetComponent<Image>();
            var s = sp != null ? OriginalAssets.GetUi(sp) : null;
            if (s != null) { img.sprite = s; img.type = Image.Type.Sliced; }
            else img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), size, pos);
            return b;
        }
    }
}
