using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>超值豪礼弹窗（1:1 复刻 res_SuperGift_12756）。
    /// 礼包条目复刻 res_imgRewardItem_9643（imgIcon + txtNum）。</summary>
    public sealed class SuperGiftPanel : UIPanel
    {
        public System.Action OnBuy;     // 立即解锁回调

        protected override void Build()
        {
            var bgMask = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)bgMask.transform);
            bgMask.color = new Color(0f, 0f, 0f, 0.6f);

            var panel = StretchNode(Root, "Panel");

            // bg 575×825 @(0,-12)
            var bgT = UiImg(panel, "bg", "Bg_01di", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -12f), new Vector2(575f, 825f)).transform;

            // BlueSkeletonGraphic → spine 占位
            SpinePlaceholder(bgT, "BlueSkeletonGraphic", new Vector2(0f, 406f));

            UiImg(bgT, "Image (2)", "chaozhihaoli", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 31f), new Vector2(498f, 543f));
            UiImg(bgT, "Image", "gift_bg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -151f), new Vector2(469f, 136f));

            // itemParent：礼包内容横向排列
            var itemParent = Node(bgT, "itemParent", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -151f), new Vector2(469f, 136f));
            var hlg = itemParent.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // imgMask（未解锁态，dump 默认隐藏）
            var imgMask = UiImg(bgT, "imgMask", "gift_bg1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -151f), new Vector2(469f, 136f));
            UiImg(imgMask.transform, "imgLock", "Game_Lock_1_Locked", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 24f), new Vector2(59f, 73.1f));
            Txt(imgMask.transform, "txtOverTime", "00:00:00", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -38f), new Vector2(240f, 45f));
            imgMask.gameObject.SetActive(false);

            // btnVideo 374×103 @(0,-318)
            var btnVideo = ImgBtn(bgT, "btnVideo", "bule", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -318f), new Vector2(374f, 103f), OnBuyClicked);
            UiImg(btnVideo.transform, "Image", "video1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-79f, -1.5f), new Vector2(47f, 39f));
            Txt(btnVideo.transform, "Text (Legacy)", "立即解锁", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(39f, 1.5f), new Vector2(200f, 60f), true);

            // btnDraw（dump 默认隐藏）
            var btnDraw = ImgBtn(bgT, "btnDraw", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -318f), new Vector2(374f, 103f), null);
            UiImg(btnDraw.transform, "Image (1)", "video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-79f, -1.5f), new Vector2(47f, 39f));
            Txt(btnDraw.transform, "Text (Legacy)", "免费领取", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(33.3f, 1.5f), new Vector2(200f, 60f), true);
            btnDraw.gameObject.SetActive(false);

            // btnClose 50×49 @(280.2,351)
            ImgBtn(bgT, "btnClose", "tcclose_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(280.2f, 351f), new Vector2(50f, 49f), () => PanelManager.Instance.Pop());

            var tips = UiImg(bgT, "Image (1)", "gift_tips", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-139f, 191f), new Vector2(131f, 134f));
            tips.rectTransform.localScale = new Vector3(1.2f, 1.2f, 1f);
            Txt(bgT, "textTip1", "超值!", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-139.7f, 214.7f), new Vector2(100f, 60f), true);
            Txt(bgT, "textTip1 (1)", "500%", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-139.7f, 171f), new Vector2(100f, 60f), true);
            Txt(bgT, "textTip2", "超值豪礼,爽！！！", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-7f, -48f), new Vector2(300f, 70f), true);
            Txt(bgT, "txtTitle", "超值豪礼", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(8f, 404f), new Vector2(300f, 80f), true);

            // 礼包内容占位（原版由运营配置填充；此处默认 4 份金币，条目复刻 imgRewardItem）
            for (int i = 0; i < 4; i++)
                MakeImgRewardItem(itemParent, "imgRewardItem" + (i + 1), "gold", (i + 1) * 500);
        }

        private void OnBuyClicked()
        {
            if (OnBuy != null) OnBuy();
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

        private static RectTransform SpinePlaceholder(Transform parent, string name, Vector2 pos)
        {
            var rt = Node(parent, name, V(0.5f, 0.5f), V(0.5f, 0.5f), pos, new Vector2(100f, 100f));
            UIHelper.NewRect(rt, "Renderer0");   // TODO spine: SkeletonGraphic 渲染占位
            return rt;
        }
    }
}
