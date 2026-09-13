using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>分享有礼弹窗（dump res_LoginShare_9815 1:1）：奖励展示 + 分享奖励按钮。</summary>
    public sealed class LoginSharePanel : UIPanel
    {
        /// <summary>分享回调（由外部注入）。</summary>
        public System.Action OnShare;

        /// <summary>关闭回调（由外部注入；未注入时走 Pop）。</summary>
        public System.Action OnClose;

        protected override bool BlockClick => true;

        protected override void Build()
        {
            // backgroup
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch(back.rectTransform);
            back.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            // 弹窗底板
            var bg = Img(panel, "bg", "Bg_01di", new Vector2(585, 785), new Vector2(0, -32));

            // 顶部奖励图
            Img(bg.transform, "Image", "pic", new Vector2(512, 296), new Vector2(0, 232));

            // Spine 占位
            var spine = Box(bg.transform, "BlueSkeletonGraphic", new Vector2(100, 100), new Vector2(0, 437));
            Box(spine, "Renderer0", new Vector2(100, 100), Vector2.zero);

            // 光效与星星装饰
            var light = Img(bg.transform, "Image (1)", "Signin_light", new Vector2(352, 353), new Vector2(0, 220));
            light.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            Img(bg.transform, "Image (2)", "xing_01", new Vector2(35, 44), new Vector2(-98, 315));
            Img(bg.transform, "Image (3)", "xing_01", new Vector2(28, 37), new Vector2(-85, 159));
            Img(bg.transform, "Image (4)", "xing_01", new Vector2(42, 60), new Vector2(112, 274));

            // 奖励图标
            var icon = Img(bg.transform, "imgIcon", "compose", new Vector2(239, 296), new Vector2(0, 232));
            icon.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            var voice = Img(bg.transform, "Image (5)", "pet_icon_voice", new Vector2(48, 48), new Vector2(94.9f, 220.9f));
            voice.transform.localScale = new Vector3(1.3f, 1.3f, 1f);

            // 底部说明区
            Img(bg.transform, "Image (6)", "Bg_nei2", new Vector2(520, 256), new Vector2(0, -112.5f));
            MakeScroll(bg.transform, "Scroll View", new Vector2(510, 232), new Vector2(0, -112.8f));

            // 分享按钮
            var share = Btn(bg.transform, "btnShare", "Btn_01", new Vector2(374, 106), new Vector2(0, -303), OnShareClick);
            Img(share.transform, "Image", "orange_icon", new Vector2(46, 49), new Vector2(-85.4f, -1.8f));
            Txt(share.transform, "Text", "分享奖励", Fs(60), new Vector2(200, 60), new Vector2(30.7f, 0), true);

            // 继续邀请按钮（默认隐藏；点击进邀请有礼）
            var invite = Btn(bg.transform, "btnInvite", "Btn_02", new Vector2(374, 106), new Vector2(0, -303),
                () => PanelManager.Instance.Push<InvitePanel>());
            Img(invite.transform, "Image", "orange_icon", new Vector2(46, 49), new Vector2(-85.4f, -1.8f));
            Txt(invite.transform, "Text", "继续邀请", Fs(60), new Vector2(200, 60), new Vector2(30.7f, 0), true);
            invite.gameObject.SetActive(false);

            // 关闭
            Btn(bg.transform, "btnClose", "tcclose_02", new Vector2(50, 49), new Vector2(280.2f, 386), OnCloseClick);

            // 邀请提示条
            Img(bg.transform, "Image (7)", "whitebantou", new Vector2(436, 46), new Vector2(0, 130));
            Img(bg.transform, "Image (8)", "line", new Vector2(50, 2), new Vector2(-225, 49));
            Img(bg.transform, "Image (9)", "line", new Vector2(50, 2), new Vector2(225, 49));
            Txt(bg.transform, "txtInviteTip", "邀请玩家进入游戏，领取额外奖励", Fs(60), new Vector2(400, 60), new Vector2(0, 49));

            // 标题与角标
            Txt(bg.transform, "txtTitle", "分享有礼", Fs(80), new Vector2(340, 80), new Vector2(0, 437), true, FontStyle.Bold);
            Txt(bg.transform, "txtNum", "x1", Fs(80), new Vector2(80, 80), new Vector2(95.5f, 224.4f), true);

            // 冷却与描述（默认隐藏）
            Txt(bg.transform, "txtTime", "00:00:00后可分享领取", Fs(60), new Vector2(400, 60), new Vector2(0, 132), true).gameObject.SetActive(false);
            Txt(bg.transform, "txtDes", "今日首次登录奖励", Fs(50), new Vector2(400, 50), new Vector2(0, 132.9f), true).gameObject.SetActive(false);
        }

        private void OnShareClick()
        {
            if (OnShare != null) OnShare();
            PanelManager.Instance.Pop();
        }

        private void OnCloseClick()
        {
            if (OnClose != null) OnClose();
            else PanelManager.Instance.Pop();
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
