using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>日榜奖励弹窗（dump res_RankRewardView_9925 1:1）：名次奖励表 + 我的排行奖励 + 领取全部。</summary>
    public sealed class RankRewardViewPanel : UIPanel
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

            // 主底板（pivot 0.5,1）
            ImgX(panel, "Image", "Bg_01di", new Vector2(662, 1007), new Vector2(0, 461));
            ImgX(panel, "Image (1)", "boarddark", new Vector2(610, 354.1f), new Vector2(0, 297));

            // 顶部装饰板
            var head = ImgX(panel, "Image (2)", "prompt_board", new Vector2(628, 126), new Vector2(0, 347.1f));
            Img(head.transform, "Image", "flower", new Vector2(41, 45), new Vector2(263, -17.1f));
            Img(head.transform, "Image (2)", "flower", new Vector2(41, 45), new Vector2(-263, -17.1f)).transform.localScale = new Vector3(-1, 1, 1);
            Img(head.transform, "Image (3)", "dividingline", new Vector2(207, 45), new Vector2(-127, 24.4f));
            Img(head.transform, "Image (1)", "dividingline", new Vector2(207, 45), new Vector2(127, 24.4f)).transform.localScale = new Vector3(-1, 1, 1);
            Img(head.transform, "Image (4)", "deocrated_pattern", new Vector2(31, 34), new Vector2(0, 44));

            // Spine 占位
            var spine = BoxX(panel, "BlueSkeletonGraphic", new Vector2(100, 100), new Vector2(0, 467));
            spine.localScale = new Vector3(1.1f, 1.1f, 1f);
            Box(spine, "Renderer0", new Vector2(100, 100), Vector2.zero);

            // 关闭
            Btn(panel, "btnClose", "tcclose_02", new Vector2(50, 49), new Vector2(301, 408), () => PanelManager.Instance.Pop());

            // 名次奖励表
            var rewardContent = MakeScroll(panel, "ScrollViewReward", new Vector2(580, 275), new Vector2(0, 235)).content;

            Txt(panel, "txtTitle", "日榜奖励", Fs(120), new Vector2(400, 120), new Vector2(0, 463), true, FontStyle.Bold);
            Txt(panel, "txt1", "名次区间", Fs(45), new Vector2(140, 45), new Vector2(-205.8f, 260));
            Txt(panel, "txt1 (1)", "奖励", Fs(45), new Vector2(140, 45), new Vector2(217, 260));

            // 我的排行奖励
            var myBg = ImgX(panel, "rewardBg", "boardkhaki", new Vector2(610, 304.9f), new Vector2(0, -83.8f));
            Img(myBg.transform, "Image", "deocrated_line2", new Vector2(53, 15), new Vector2(-128, 123));
            Img(myBg.transform, "Image (1)", "deocrated_line2", new Vector2(53, 15), new Vector2(128, 123));
            Img(myBg.transform, "Image (2)", "pattern_flower", new Vector2(138, 161), new Vector2(0, -48));
            Txt(myBg.transform, "txt1 (2)", "我的排行奖励", Fs(50), new Vector2(190, 50), new Vector2(0, 124.2f), true);

            // 可领取区
            var able = BoxX(panel, "ableReceive", new Vector2(600, 390), new Vector2(0, -329));
            MakeScroll(able.transform, "ScrollViewDraw", new Vector2(580, 200), new Vector2(0, 147.6f));
            var receive = Btn(able.transform, "btnReceive", "Btn_01", new Vector2(360, 102), new Vector2(0, -124),
                () => Debug.Log("[RankReward] 领取全部"));
            Txt(receive.transform, "Text", "领取全部", Fs(80), new Vector2(300, 80), Vector2.zero, true);
            Txt(able.transform, "txt1 (3)", "名次", Fs(50), new Vector2(100, 50), new Vector2(-236.9f, 176.3f), true);
            Txt(able.transform, "txt1 (4)", "时间", Fs(50), new Vector2(100, 50), new Vector2(-113, 176.3f), true);
            Txt(able.transform, "txt1 (5)", "奖励", Fs(50), new Vector2(100, 50), new Vector2(51, 176.3f), true);
            Txt(able.transform, "txt1 (6)", "状态", Fs(50), new Vector2(100, 50), new Vector2(220, 176.3f), true);

            // 不可领取区（默认隐藏）
            var unable = BoxX(panel, "unableReceive", new Vector2(600, 390), new Vector2(0, -326.5f));
            var grey = Btn(unable.transform, "btnReceive", "grey", new Vector2(330, 108), Vector2.zero, null);
            var greyRt = (RectTransform)grey.transform;
            greyRt.pivot = new Vector2(0.5f, 1f);
            greyRt.anchoredPosition = new Vector2(0, -74.7f);
            grey.image.color = new Color(1f, 1f, 1f, 0.28f);
            Txt(grey.transform, "Text", "暂无奖励", Fs(100), new Vector2(300, 100), Vector2.zero, true);
            unable.gameObject.SetActive(false);

            // 提示
            Txt(panel, "txtTip", "每日截止<color=#EDFF10>23:50</color>分，对当日获得星", Fs(80),
                new Vector2(406.1f, 80), new Vector2(0, 334.7f), true);

            // 静态演示数据：4 条名次奖励
            MakeItem(rewardContent, "1", "金币×100");
            MakeItem(rewardContent, "2", "金币×60");
            MakeItem(rewardContent, "3", "金币×30");
            MakeItem(rewardContent, "4-10", "金币×10");
        }

        /// <summary>名次奖励条目（dump res_RankRewardItem_9974 1:1）。</summary>
        private void MakeItem(Transform parent, string rank, string reward)
        {
            var item = Box(parent, "RankRewardItem", new Vector2(550, 64), Vector2.zero);
            Img(item, "Image (2)", "dividing_line", new Vector2(522.6f, 5), new Vector2(0, -25));

            // itemReward（默认隐藏：图标 + 数量）
            var itemReward = BoxX(item.transform, "itemReward", new Vector2(70, 60), new Vector2(165, 8.2f));
            itemReward.pivot = new Vector2(0, 0.5f);
            var icon = Img(itemReward, "imgIcon", "gold_big", new Vector2(80, 84), new Vector2(-6.4f, 0));
            icon.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            Txt(itemReward, "txtNum", "x1", Fs(30), new Vector2(40, 30), new Vector2(20, -13.5f));
            itemReward.gameObject.SetActive(false);

            // ParentReward：奖励文本横排
            var pr = BoxX(item.transform, "ParentReward", new Vector2(0, 70), new Vector2(-10, 8.2f));
            pr.anchorMin = pr.anchorMax = new Vector2(1, 0.5f);
            pr.pivot = new Vector2(1, 0.5f);
            var hlg = pr.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.spacing = 8;
            pr.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            Txt(pr, "R", reward, Fs(60), new Vector2(160, 40), Vector2.zero, true);

            var t = Txt(item, "txtRank", rank, Fs(60), new Vector2(150, 60), new Vector2(-252, 8.2f), true);
            t.alignment = TextAnchor.MiddleLeft;
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
            => BoxX(parent, name, size, pos);

        private static RectTransform BoxX(Transform parent, string name, Vector2 size, Vector2 pos)
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
            var rt = img.rectTransform;
            var oldPos = pos;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = oldPos;
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

        /// <summary>竖向滚动列表（Background/Viewport/Content/Scrollbar，与 dump 结构一致）。</summary>
        private static ScrollRect MakeScroll(Transform parent, string name, Vector2 size, Vector2 pos)
        {
            var root = Box(parent, name, size, pos);
            var bgImg = root.gameObject.AddComponent<Image>();
            var bgSp = OriginalAssets.GetUi("Background");
            if (bgSp != null) { bgImg.sprite = bgSp; bgImg.type = Image.Type.Sliced; }
            else bgImg.color = new Color(0.1f, 0.12f, 0.18f, 0.6f);
            var sc = root.gameObject.AddComponent<ScrollRect>();

            var vp = Box(root, "Viewport", Vector2.zero, Vector2.zero);
            UIHelper.Stretch(vp);
            var vpImg = vp.gameObject.AddComponent<Image>();
            var mSp = OriginalAssets.GetUi("UIMask");
            if (mSp != null) vpImg.sprite = mSp; else vpImg.color = new Color(1f, 1f, 1f, 0f);
            vp.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = Box(vp, "Content", Vector2.zero, Vector2.zero);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0, 1);
            var lg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            lg.childControlWidth = true; lg.childControlHeight = false;
            lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
            lg.spacing = 2;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sbRoot = Box(root, "Scrollbar Vertical", new Vector2(12, 0), Vector2.zero);
            sbRoot.anchorMin = new Vector2(1, 0);
            sbRoot.anchorMax = new Vector2(1, 1);
            sbRoot.pivot = new Vector2(1, 1);
            var sbImg = sbRoot.gameObject.AddComponent<Image>();
            if (bgSp != null) { sbImg.sprite = bgSp; sbImg.type = Image.Type.Sliced; }
            else sbImg.color = new Color(1f, 1f, 1f, 0.1f);
            var area = UIHelper.Stretch(Box(sbRoot, "Sliding Area", Vector2.zero, Vector2.zero));
            var handle = UIHelper.Stretch(Box(area, "Handle", Vector2.zero, Vector2.zero));
            var hImg = handle.gameObject.AddComponent<Image>();
            var hSp = OriginalAssets.GetUi("UISprite");
            if (hSp != null) hImg.sprite = hSp; else hImg.color = new Color(1f, 1f, 1f, 0.35f);
            var sb = sbRoot.gameObject.AddComponent<Scrollbar>();
            sb.targetGraphic = hImg;
            sb.direction = Scrollbar.Direction.BottomToTop;

            sc.viewport = vp;
            sc.content = content;
            sc.verticalScrollbar = sb;
            sc.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            sc.horizontal = false;
            sc.vertical = true;
            sc.movementType = ScrollRect.MovementType.Clamped;
            sc.scrollSensitivity = 24f;
            return sc;
        }
    }
}
