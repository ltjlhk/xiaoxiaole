using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>商店弹窗：承载 1:1 复刻 res_StoreItem_11544 的商品条目
    /// （iconParent/icon/星光/进度条/btnVideo 免费按钮/btnCoin 金币按钮/txtName/txtNum）。</summary>
    public sealed class StorePanel : UIPanel
    {
        public System.Action OnBuy;     // 购买回调（btnVideo / btnCoin 均触发）

        protected override void Build()
        {
            var bgMask = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)bgMask.transform);
            bgMask.color = new Color(0f, 0f, 0f, 0.6f);

            var panel = StretchNode(Root, "Panel");

            // 承载容器（dump 只有 StoreItem 条目，底板/标题/关闭为弹窗化所需）
            var bgT = UiImg(panel, "bg", "Bg_01di", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(640f, 560f)).transform;
            Txt(bgT, "txtTitle", "商店", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 300f), new Vector2(340f, 80f), true);
            ImgBtn(bgT, "btnClose", "tcclose_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(279f, 254f), new Vector2(50f, 49f), () => PanelManager.Instance.Pop());

            MakeStoreItem(bgT, "StoreItem1", new Vector2(0f, 110f), "金币", 100, 100);
            MakeStoreItem(bgT, "StoreItem2", new Vector2(0f, -70f), "金币", 550, 500);
            MakeStoreItem(bgT, "StoreItem3", new Vector2(0f, -250f), "金币", 1200, 1000);
        }

        /// <summary>复刻 res_StoreItem_11544：640×175 bg_store 条目。</summary>
        private void MakeStoreItem(Transform parent, string name, Vector2 pos,
            string label, int num, int cost)
        {
            var root = UiImg(parent, name, "bg_store", V(0.5f, 0.5f), V(0.5f, 0.5f),
                pos, new Vector2(640f, 175f));

            var iconParent = Node(root.transform, "iconParent", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-227f, 0f), new Vector2(100f, 100f));
            var icon = UiImg(iconParent, "icon", "gold", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(247f, 256f));
            icon.rectTransform.localScale = new Vector3(0.4f, 0.4f, 1f);

            // 星光（LightEffectControl → 静态底 + xing_01）
            var fx = Node(root.transform, "Image (1)", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-249.5f, 22.8f), new Vector2(110f, 110f));
            fx.gameObject.AddComponent<Image>();
            UiImg(fx, "Image (2)", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-29f, 18.4f), new Vector2(28f, 37f));
            UiImg(fx, "Image (3)", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-0.7f, -51.2f), new Vector2(28f, 37f));
            UiImg(fx, "Image (4)", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(70.4f, 2.3f), new Vector2(28f, 37f));

            var prog = UiImg(root.transform, "Image", "jmbaoxiangjindutiaoE", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-81.3f, -28.4f), new Vector2(90f, 34f));
            prog.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1f);

            // btnVideo 187×60 @(176,-35)
            var btnVideo = ImgBtn(root.transform, "btnVideo", "Btn_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(176f, -35f), new Vector2(187f, 60f), OnBuyClicked);
            var vid = UiImg(btnVideo.transform, "Image", "icon_video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-35f, 0f), new Vector2(47f, 40f));
            vid.rectTransform.localScale = new Vector3(0.9f, 0.9f, 1f);
            Txt(btnVideo.transform, "Text", "免费", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(24.7f, 1.8f), new Vector2(85f, 45f), true);

            // btnCoin 187×60 @(176,35)
            var btnCoin = ImgBtn(root.transform, "btnCoin", "blackbg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(176f, 35f), new Vector2(187f, 60f), OnBuyClicked);
            var gc = UiImg(btnCoin.transform, "Image", "gold_con", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-36.8f, 0f), new Vector2(51f, 53f));
            gc.rectTransform.localScale = new Vector3(0.8f, 0.8f, 1f);
            Txt(btnCoin.transform, "txtCost", cost.ToString(), V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(21.3f, 1f), new Vector2(85f, 45f), true);

            Txt(root.transform, "txtName", label, V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-30.2f, 30.3f), new Vector2(185f, 70f), true);
            Txt(root.transform, "txtNum", "x" + num, V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-82.7f, -25.8f), new Vector2(185f, 70f), true);
        }

        private void OnBuyClicked()
        {
            if (OnBuy != null) OnBuy();
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
