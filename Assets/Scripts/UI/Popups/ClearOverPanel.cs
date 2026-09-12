using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>空间不足/栏位满弹窗（1:1 复刻 res_ClearOver_11960，view1/view2 双视图）。</summary>
    public sealed class ClearOverPanel : UIPanel
    {
        protected override bool BlockClick => true;

        protected override void Build()
        {
            var backgroup = UIHelper.Image(Root, "backgroup");   // dump: Image + RectGuide（引导脚本不复刻）
            UIHelper.Stretch((RectTransform)backgroup.transform);
            backgroup.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = StretchNode(Root, "Panel");

            // ---- view2（默认显示）----
            var view2 = UiImg(panel, "view2", "Bg_02di", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -46f), V(575f, 740f));

            // BlueSkeletonGraphic → spine 占位
            SpinePlaceholder(view2.transform, "BlueSkeletonGraphic", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 370f), V(100f, 100f));

            var imgQuit = UiImg(view2.transform, "imgQuit", "quitbg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 122f), V(505f, 241f));
            imgQuit.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
            imgQuit.gameObject.SetActive(false);

            var btnCoin2 = ImgBtn(view2.transform, "btnCoin2", "blackbg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -143f), V(374f, 103f), () => PanelManager.Instance.Pop());
            UiImg(btnCoin2.transform, "imgCoin", "gold_con", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(51f, 53f));
            Txt(btnCoin2.transform, "txtCost", "100", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(79.4f, 0f), V(100f, 60f), true);
            Txt(btnCoin2.transform, "Text (Legacy)", "继续", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-76.7f, 0f), V(100f, 60f), true);

            var btnVideo2 = ImgBtn(view2.transform, "btnVideo2", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -267f), V(374f, 103f), () => PanelManager.Instance.Pop());
            UiImg(btnVideo2.transform, "Image", "video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-57.3f, 0f), V(47f, 39f));
            Txt(btnVideo2.transform, "Text (Legacy)", "继续", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(41.5f, 0f), V(100f, 60f), true);

            ImgBtn(view2.transform, "btnClose", "tcclose_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(281f, 320f), V(50f, 49f), () => PanelManager.Instance.Pop());

            var btnRevive2 = ImgBtn(view2.transform, "btnRevive2", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -195f), V(374f, 103f), () => PanelManager.Instance.Pop());
            Txt(btnRevive2.transform, "Text (Legacy)", "立即复活", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(300f, 70f), true);
            btnRevive2.gameObject.SetActive(false);

            UiImg(view2.transform, "Image (2)", "fail01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 106.5f), V(209f, 126f));

            // itemReward（dump 默认 inactive）
            var itemReward = Node(view2.transform, "itemReward", V(0f, 1f), V(0f, 1f),
                V(287.5f, -379f), V(80f, 110f));
            itemReward.gameObject.SetActive(false);
            UiImg(itemReward, "imgIcon", "output_icon_1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 16.3f), V(70f, 70f));
            Txt(itemReward, "txtSpiritNum", "-1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -29.2f), V(100f, 50f), true);

            var itemParent = UiImg(view2.transform, "itemParent", "Bg_nei2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -9f), V(480f, 120f));
            var ilg = itemParent.gameObject.AddComponent<HorizontalLayoutGroup>();
            ilg.spacing = 24f;
            ilg.childAlignment = TextAnchor.MiddleCenter;
            ilg.childForceExpandWidth = false;
            ilg.childForceExpandHeight = false;

            var image1 = UiImg(view2.transform, "image1", "whitebantou", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 223.2f), V(475.4f, 46f));
            image1.gameObject.SetActive(false);

            var txtReviveTip = Txt(view2.transform, "txtReviveTip", "必须清理出更多的空间", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -51.8f), V(450f, 60f), true);
            txtReviveTip.gameObject.SetActive(false);

            Txt(view2.transform, "txtTitle2", "空间不足", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 367f), V(300f, 80f), true);

            var txtQuitTip = Txt(view2.transform, "txtQuitTip", "损失1点体力和60%星星并重置关卡进度",
                V(0.5f, 0.5f), V(0.5f, 0.5f), V(0f, 224f), V(500f, 80f), true);
            txtQuitTip.gameObject.SetActive(false);

            Txt(view2.transform, "txtWinOverTip", "放弃2连胜将中断并损失以下奖励", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 227f), V(500f, 80f), true);

            var imageNoSp = Node(view2.transform, "Image (1)", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(118.8f, 172.1f), V(41.5f, 40.1f));
            imageNoSp.gameObject.AddComponent<Image>();          // dump: Image 无 sprite
            imageNoSp.gameObject.SetActive(false);

            var txtStarNum = Txt(view2.transform, "txtStarNum", "0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(178.8f, 173f), V(100f, 50f), true);
            txtStarNum.gameObject.SetActive(false);

            var goldItem = UiImg(view2.transform, "goldItem", "buttonback", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-17f, 536f), V(186f, 87.4f));
            UiImg(goldItem.transform, "gold", "gold_con", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-55.7f, 2.1f), V(51f, 53f));
            Txt(goldItem.transform, "txtGold", "0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(18.7f, 3.6f), V(120f, 60f));

            var spiritItem = UiImg(view2.transform, "spiritItem", "buttonback", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-214f, 536f), V(163.5f, 87.4f));
            UiImg(spiritItem.transform, "aixin", "output_icon_5", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-39.5f, 1.4f), V(60f, 60f));
            Txt(spiritItem.transform, "txtSpirit", "5", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(28.1f, 3.6f), V(80f, 60f));

            // ---- view1（dump 默认 inactive，铺满）----
            var view1 = Node(panel, "view1", V(0f, 0f), V(1f, 1f), V(0f, 0f), V(0f, 0f));
            view1.gameObject.SetActive(false);

            UiImg(view1, "Image", "Bg_ding3", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 364f), V(688f, 168f));
            UiImg(view1, "imgType1", "blocknot1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 106f), V(518f, 284f));
            var imgType2 = UiImg(view1, "imgType2", "blocknot2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-9.3f, 110.3f), V(456f, 275f));
            imgType2.gameObject.SetActive(false);
            UiImg(view1, "Image", "dialoguebubble", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-168f, 167f), V(203f, 140f));

            ImgBtn(view1, "btnClose", "tanchuang_0019", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(279f, 344f), V(50f, 49f), () => PanelManager.Instance.Pop());

            UiImg(view1, "Image (1)", "blueframe", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(14.3f, -127.9f), V(360f, 45f));
            UiImg(view1, "imgLevelPro", "greenbar", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(14.3f, -126.7f), V(352f, 36f));
            var flag = UiImg(view1, "Image", "flag", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(243f, -115f), V(100f, 100f));
            flag.rectTransform.localScale = new Vector3(0.7f, 0.7f, 1f);

            Txt(view1, "Text (Legacy)", "通关进度：", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-215f, -124.6f), V(140f, 50f));
            Txt(view1, "txtLevelPro", "0%", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(14.8f, -126f), V(120f, 50f), true);
            Txt(view1, "txtTitle1", "栏位满了", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 387.2f), V(350f, 80f), true);
            Txt(view1, "txtOverTip", "只差一点点就过关啦！", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-166f, 178.7f), V(180f, 100f));

            var btnCoin1 = ImgBtn(view1, "btnCoin1", "blackbg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-180f, -247f), V(348f, 100f), () => PanelManager.Instance.Pop());
            UiImg(btnCoin1.transform, "imgCoin", "gold_con", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(51f, 53f));
            Txt(btnCoin1.transform, "txtCost", "100", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(79.4f, 0f), V(100f, 60f), true);
            Txt(btnCoin1.transform, "Text (Legacy)", "清空", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-76.7f, 0f), V(100f, 60f), true);

            var btnVideo1 = ImgBtn(view1, "btnVideo1", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(180f, -247f), V(348f, 100f), () => PanelManager.Instance.Pop());
            UiImg(btnVideo1.transform, "Image", "video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-57.3f, 0f), V(47f, 39f));
            Txt(btnVideo1.transform, "Text (Legacy)", "清空", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(41.5f, 0f), V(100f, 60f), true);

            var btnRevive1 = ImgBtn(view1, "btnRevive1", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -249f), V(374f, 103f), () => PanelManager.Instance.Pop());
            Txt(btnRevive1.transform, "Text (Legacy)", "立即复活", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(300f, 70f), true);
            btnRevive1.gameObject.SetActive(false);

            var imgLight = UiImg(view1, "imgLight", "lightframe", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -437f), V(745f, 165f));
            var lightInner = Node(imgLight.transform, "Image", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(725f, 145f));
            lightInner.gameObject.AddComponent<Image>();         // dump: Image 无 sprite

            var goldItem1 = UiImg(view1, "goldItem (1)", "Home_icon_bg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-211f, 490f), V(186f, 60.8f));
            UiImg(goldItem1.transform, "gold", "gold_con", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-55.7f, 2.1f), V(51f, 53f));
            Txt(goldItem1.transform, "txtGold", "0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(18.7f, 3.6f), V(120f, 60f));
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
