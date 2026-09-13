using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>排行榜主页（dump res_RankView_12521 1:1）：全服/周榜/好友三页签 + 我的排名栏 + 演示条目。</summary>
    public sealed class RankViewPanel : UIPanel
    {
        /// <summary>关闭回调（由外部注入；未注入时关闭按钮走 Pop）。</summary>
        public System.Action OnHome;

        private readonly List<ScrollRect> _scrolls = new List<ScrollRect>();
        private GameObject _tipGo;

        protected override bool BlockClick => true;

        protected override void Build()
        {
            // backgroup（半透明遮罩）
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch(back.rectTransform);
            back.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            // 主底板
            Img(panel, "Image", "Bg_02di", new Vector2(662, 1130), new Vector2(0, -19));

            // Spine 占位
            var spine = Box(panel, "BlueSkeletonGraphic", new Vector2(100, 100), new Vector2(0, 552));
            spine.localScale = new Vector3(1.1f, 1.1f, 1f);
            Box(spine, "Renderer0", new Vector2(100, 100), Vector2.zero);

            // 三页签按钮（世界 0 / 周榜 -187 / 好友 187）
            var tabWorld = Btn(panel, "btnBgWorld", null, new Vector2(179, 51), new Vector2(0, 413), () => SelectTab(0));
            Img(tabWorld.transform, "Image", "quanfubang", new Vector2(142, 46), Vector2.zero);
            var tabWeek = Btn(panel, "btnBgWeek", null, new Vector2(179, 51), new Vector2(-187, 413), () => SelectTab(1));
            Img(tabWeek.transform, "Image", "zhoubang", new Vector2(142, 46), Vector2.zero);
            var tabFriend = Btn(panel, "btnBgFriend", null, new Vector2(179, 51), new Vector2(187, 413), () => SelectTab(2));
            Img(tabFriend.transform, "Image (1)", "haoyoubang", new Vector2(142, 46), Vector2.zero);

            // 关闭
            Btn(panel, "btnClose", "tcclose_01", new Vector2(50, 49), new Vector2(301, 493), OnCloseClick);

            // 去授权（默认隐藏）
            var privacy = Btn(panel, "btnPrivacy", "01tongyong(mediate)", new Vector2(240, 87), new Vector2(0, 78.5f), null);
            Txt(privacy.transform, "Text (Legacy)", "去授权", Fs(67), new Vector2(220, 67), Vector2.zero, true);
            privacy.gameObject.SetActive(false);

            // 我的排名栏
            var selfRank = Plain(panel, "imgSelfRank", new Color(0f, 0f, 0f, 0.35f), new Vector2(598, 129), new Vector2(0, -383.8f));
            var rankDown = Img(selfRank.transform, "imgRankDown", "img_rank1", new Vector2(82, 77), new Vector2(-45.2f, 0));
            rankDown.gameObject.SetActive(false);
            Txt(selfRank.transform, "txtSelf", "我的排名", Fs(65), new Vector2(140, 65), new Vector2(-202, 0), true);
            Txt(selfRank.transform, "txtRankDown", "0", Fs(65), new Vector2(100, 65), new Vector2(-62, 0), true);
            Txt(selfRank.transform, "txtLevelDown", "通关0", Fs(65), new Vector2(140, 65), new Vector2(61, 0), true);
            Txt(selfRank.transform, "txtScoreDown", "0", Fs(65), new Vector2(140, 65), new Vector2(204, 0), true);

            // 邀请好友
            var share = Btn(panel, "btnShareFriend", "01tongyong(mediate)", new Vector2(304, 97), new Vector2(0, -491.5f),
                () => Debug.Log("[RankView] 邀请好友"));
            Txt(share.transform, "Text (Legacy)", "邀请好友", Fs(75), new Vector2(278, 75), Vector2.zero, true);

            // 三个列表（世界 / 好友 / 周）
            _scrolls.Clear();
            _scrolls.Add(MakeScroll(panel, "ScrollViewWorld", new Vector2(590, 630.5f), new Vector2(0, 6)));
            _scrolls.Add(MakeScroll(panel, "ScrollViewWeek", new Vector2(590, 600), new Vector2(0, 22)));
            _scrolls.Add(MakeScroll(panel, "ScrollViewFirend", new Vector2(590, 630.5f), new Vector2(0, 6)));
            _scrolls[1].gameObject.SetActive(false);
            _scrolls[2].gameObject.SetActive(false);

            // RawImage 占位（默认隐藏）
            var body = Box(panel, "RawBody", new Vector2(590, 630), new Vector2(-295, 321.2f));
            body.pivot = new Vector2(0, 0);
            body.gameObject.AddComponent<RawImage>();
            body.gameObject.SetActive(false);

            // 标题与表头
            Txt(panel, "txtTitle", "排行榜", Fs(120), new Vector2(400, 120), new Vector2(0, 548), true, FontStyle.Bold);
            Txt(panel, "Text (Legacy)", "排名", Fs(65), new Vector2(120, 65), new Vector2(-224, 358));
            Txt(panel, "Text (Legacy) (1)", "玩家", Fs(65), new Vector2(120, 65), new Vector2(-49, 358));
            Txt(panel, "Text (Legacy) (2)", "星星", Fs(65), new Vector2(120, 65), new Vector2(208, 358));

            // 好友未授权提示（默认隐藏，切好友页签时显示）
            _tipGo = Txt(panel, "txtTip", "好友排名未授权，无法查看", Fs(150), new Vector2(450, 150), new Vector2(0, -34.5f)).gameObject;
            _tipGo.SetActive(false);
            Txt(panel, "txtTimeRefresh", "每周一<color=#25CD00>00:00</color>点刷新周榜", Fs(55), new Vector2(450, 55), new Vector2(0, -306)).gameObject.SetActive(false);

            // 静态演示数据：世界榜含前三领奖台（topRank），其余各 5 条
            int[] scores = { 980, 860, 745, 690, 520 };
            for (int i = 0; i < 3; i++)
            {
                var content = _scrolls[i].content;
                if (i == 0)
                {
                    // 前三名领奖台（rank2 左 / rank1 中高 / rank3 右）
                    TopRankItem.MakePodium(content, new[] { "玩家1", "玩家2", "玩家3" }, new[] { 980, 860, 745 });
                    // 榜单其余：第 4-8 名
                    for (int n = 3; n < 8; n++)
                        MakeItem(content, "玩家" + (n + 1), scores[3] - (n - 3) * 35, n + 1);
                }
                else
                {
                    for (int n = 0; n < 5; n++)
                        MakeItem(content, "玩家" + (n + 1), scores[n], n + 1);
                }
            }
            SelectTab(0);
        }

        private void SelectTab(int idx)
        {
            for (int i = 0; i < _scrolls.Count; i++)
                if (_scrolls[i] != null) _scrolls[i].gameObject.SetActive(i == idx);
            if (_tipGo != null) _tipGo.SetActive(idx == 2);
        }

        private void OnCloseClick()
        {
            if (OnHome != null) OnHome();
            else PanelManager.Instance.Pop();
        }

        /// <summary>排行条目（dump res_RankItemDaily_12172 1:1）。</summary>
        private void MakeItem(Transform parent, string name, int score, int rank)
        {
            var item = Box(parent, "RankItemDaily", new Vector2(718, 110), Vector2.zero);
            Img(item, "imgMotif", "myrankingboard", new Vector2(645, 110), new Vector2(36.5f, -1.8f));
            Img(item, "imgSkin", "mjskin1", new Vector2(67, 92), new Vector2(-320.9f, -1.3f));
            Box(item, "imgElves", new Vector2(80, 80), Vector2.zero).gameObject.SetActive(false);
            Img(item, "imgRank", "icon1", new Vector2(44, 47), new Vector2(-321.5f, 0)).gameObject.SetActive(false);
            Img(item, "imgIcon", "transparent", new Vector2(64, 64), new Vector2(-206.2f, 3.5f));
            Img(item, "imgCircle", "rankcommon", new Vector2(72, 72), new Vector2(-206.2f, 1.5f));
            Img(item, "Image (1)", "numbottomplate", new Vector2(150, 37.8f), new Vector2(152.3f, 2.6f));
            Img(item, "Image (2)", "output_icon_1", new Vector2(64, 64), new Vector2(79, 4.6f));
            Txt(item, "txtRank", (rank > 3 ? "第" : "") + rank.ToString(), Fs(65), new Vector2(80, 65), new Vector2(-320.2f, 2.7f), true);
            Txt(item, "txtName", name, Fs(60), new Vector2(200, 60), new Vector2(-62.6f, 4.1f), true);
            Txt(item, "txtScore", score.ToString(), Fs(65), new Vector2(120, 65), new Vector2(165.3f, 3.9f), true);
            Txt(item, "txtLevel", "0", Fs(42.8f), new Vector2(120, 42.8f), new Vector2(-64, -18.8f), true).gameObject.SetActive(false);
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

        private static Image Plain(Transform parent, string name, Color color, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name);
            img.color = color;
            Rt(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), size, pos);
            return img;
        }

        private static Image Img(Transform parent, string name, string sp, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name, OriginalAssets.GetUi(sp));
            if (img.sprite == null) img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), size, pos);
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
            lg.childControlWidth = false; lg.childControlHeight = false;
            lg.childForceExpandWidth = false; lg.childForceExpandHeight = false;
            lg.childAlignment = TextAnchor.UpperCenter;
            lg.spacing = 6;
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
