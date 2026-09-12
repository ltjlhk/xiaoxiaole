using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>在线礼包弹窗（1:1 复刻 res_TimeReward_10312）。
    /// 奖励条目复刻 res_imgRewardItem_9643。</summary>
    public sealed class TimeRewardPanel : UIPanel
    {
        public System.Action OnClaim;   // 领取回调（直接/双倍领取均触发）

        protected override void Build()
        {
            var bgMask = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)bgMask.transform);
            bgMask.color = new Color(0f, 0f, 0f, 0.6f);

            var panel = StretchNode(Root, "Panel");

            // bg 575×633 @(0,50)
            var bgT = UiImg(panel, "bg", "Bg_01di", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 50f), new Vector2(575f, 633f)).transform;

            UiImg(bgT, "Image", "incrementalgold", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 55f), new Vector2(496f, 314f));
            SpinePlaceholder(bgT, "BlueSkeletonGraphic", new Vector2(0f, 306f));
            UiImg(bgT, "Image", "Bg_nei2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -215f), new Vector2(496f, 116f));
            UiImg(bgT, "Image", "whitebantou", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -63f), new Vector2(485.6f, 46f));

            // btnVideo 317.9×100 @(-162,-391)（dump 默认隐藏，此处激活供领取）
            var btnVideo = ImgBtn(bgT, "btnVideo", "Btn_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-162f, -391f), new Vector2(317.9f, 100f), ClaimClicked);
            UiImg(btnVideo.transform, "Image", "icon_video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-84f, -0.9f), new Vector2(47f, 40f));
            Txt(btnVideo.transform, "Text", "双倍领取", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(38.2f, 0f), new Vector2(160f, 60f), true);

            // btnDraw 317.9×100 @(162,-391)
            var btnDraw = ImgBtn(bgT, "btnDraw", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(162f, -391f), new Vector2(317.9f, 100f), ClaimClicked);
            Txt(btnDraw.transform, "Text", "直接领取", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(160f, 60f), true);

            ImgBtn(bgT, "btnClose", "tcclose_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(279.4f, 254f), new Vector2(50f, 49f), () => PanelManager.Instance.Pop());

            // imgItemParent：奖励横向排列（复刻 res_imgRewardItem_9643）
            var itemParent = Node(bgT, "imgItemParent", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -215f), new Vector2(496f, 116f));
            var hlg = itemParent.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 40f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            MakeImgRewardItem(itemParent, "imgRewardItem1", "gold", 500);

            Txt(bgT, "txtOverTime", "00:00:00", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -59.7f), new Vector2(220f, 50f), true);
            Txt(bgT, "txtExplain", "计时完毕可领取", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -132f), new Vector2(420f, 50f));
            Txt(bgT, "txtTitle", "在线礼包", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 302f), new Vector2(340f, 80f), true);
            Txt(bgT, "Text (1)", "保持在线                      增加奖励", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-0.2f, -59.1f), new Vector2(500f, 60f), true);
        }

        private void ClaimClicked()
        {
            if (OnClaim != null) OnClaim();
            PanelManager.Instance.Pop();
        }

        /// <summary>复刻 res_imgRewardItem_9643：imgIcon(252×261 scale0.2) + txtNum(100×60)。</summary>
        private void MakeImgRewardItem(Transform parent, string name, string spName, int count)
        {
            var root = Node(parent, name, V(0f, 0f), V(0f, 0f), Vector2.zero, new Vector2(80f, 100f));
            var icon = Node(root, "imgIcon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 21.2f), new Vector2(252f, 261f));
            icon.localScale = new Vector3(0.2f, 0.2f, 1f);
            var img = icon.gameObject.AddComponent<Image>();
            var sp = OriginalAssets.GetUi(spName);
            if (sp != null) img.sprite = sp;
            else img.color = new Color(1f, 1f, 1f, 0.25f);
            Txt(root, "txtNum", count > 0 ? count.ToString() : "", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -27.1f), new Vector2(100f, 60f));
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
