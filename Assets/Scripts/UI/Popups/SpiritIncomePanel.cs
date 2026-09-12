using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>获取体力弹窗（1:1 复刻 res_SpiritIncome_12278）。</summary>
    public sealed class SpiritIncomePanel : UIPanel
    {
        public System.Action OnClaim;   // 领取回调（视频免费领取）

        protected override void Build()
        {
            var bgMask = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)bgMask.transform);
            bgMask.color = new Color(0f, 0f, 0f, 0.6f);

            var panel = StretchNode(Root, "Panel");

            // bg 575×522 @(0,-6)
            var bgT = UiImg(panel, "bg", "Bg_02di", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -6f), new Vector2(575f, 522f)).transform;

            UiImg(bgT, "Image", "Bg_nei2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 57f), new Vector2(480f, 290f));
            SpinePlaceholder(bgT, "BlueSkeletonGraphic", new Vector2(0f, 301f));

            // imgLove（体力爱心）+ 数量
            var love = UiImg(bgT, "imgLove", "lifeicon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 55f), new Vector2(98f, 83f));
            love.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1f);
            var txtNum = Txt(love.transform, "txtNum", "10", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 5.3f), new Vector2(80f, 80f), true);
            txtNum.rectTransform.localScale = new Vector3(0.7f, 0.7f, 1f);

            // btnVideo 374×103 @(0,-167)
            var btnVideo = ImgBtn(bgT, "btnVideo", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -167f), new Vector2(374f, 103f), ClaimClicked);
            UiImg(btnVideo.transform, "Image", "video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-2.4f, 0f), new Vector2(47f, 39f));
            UiImg(btnVideo.transform, "Image (1)", "spiritxin", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-89f, 0f), new Vector2(80f, 65f));
            Txt(btnVideo.transform, "txtVideoNum", "+1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-88f, 2.1f), new Vector2(60f, 60f), true);
            Txt(btnVideo.transform, "Text", "免费", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(78.6f, 2f), new Vector2(120f, 60f), true);

            // btnCoin（dump 默认隐藏）
            var btnCoin = ImgBtn(bgT, "btnCoin", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -62.3f), new Vector2(374f, 103f), ClaimClicked);
            var gb = UiImg(btnCoin.transform, "Image", "gold_big", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(80f, 84f));
            gb.rectTransform.localScale = new Vector3(0.9f, 0.9f, 1f);
            var sx = UiImg(btnCoin.transform, "Image (1)", "spiritxin", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-129.4f, 0f), new Vector2(80f, 65f));
            sx.rectTransform.localScale = new Vector3(0.9f, 0.9f, 1f);
            Txt(btnCoin.transform, "txtCoinNum", "+2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-72.1f, 0f), new Vector2(60f, 65f), true);
            Txt(btnCoin.transform, "txtCoinCost", "200", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(95.4f, 0f), new Vector2(110f, 65f), true);
            btnCoin.gameObject.SetActive(false);

            ImgBtn(bgT, "btnClose", "tcclose_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(282f, 251f), new Vector2(50f, 49f), () => PanelManager.Instance.Pop());

            // txtTime（dump 默认隐藏）
            Txt(bgT, "txtTime", "00:00", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -40.3f), new Vector2(240f, 60f)).gameObject.SetActive(false);

            Txt(bgT, "Text (Legacy)", "获取体力", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 296f), new Vector2(340f, 90f), true);
        }

        private void ClaimClicked()
        {
            if (OnClaim != null) OnClaim();
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

        private static RectTransform SpinePlaceholder(Transform parent, string name, Vector2 pos)
        {
            var rt = Node(parent, name, V(0.5f, 0.5f), V(0.5f, 0.5f), pos, new Vector2(100f, 100f));
            UIHelper.NewRect(rt, "Renderer0");   // TODO spine: SkeletonGraphic 渲染占位
            return rt;
        }
    }
}
