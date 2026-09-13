using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>邀请任务行状态。</summary>
    public enum InviteState
    {
        /// <summary>未达成：灰底“未完成”。</summary>
        NotReceived,

        /// <summary>可邀请：Btn_02“邀请”。</summary>
        Invite,

        /// <summary>已达成可领：Btn_01“领取”。</summary>
        Draw,

        /// <summary>已领取：Btn_01“已领取”。</summary>
        Received,
    }

    /// <summary>邀请行条目（dump res_ItemInvite_9973 1:1）：
    /// 头像/序号 + 奖励区 + 状态按钮（未完成/邀请/领取/已领取）+ 分隔线。
    /// 供邀请有礼列表动态生成。</summary>
    public sealed class InviteItem
    {
        /// <summary>行根节点。</summary>
        public RectTransform Root { get; private set; }

        /// <summary>邀请点击（委托：微信邀请）。</summary>
        public System.Action OnInvite;

        /// <summary>领取点击（委托：发奖）。</summary>
        public System.Action OnDraw;

        private GameObject _notReceived, _btnInvite, _btnDraw, _btnReceived;

        public InviteItem(Transform parent, int index, Sprite rewardIcon, string rewardNum)
        {
            Root = UIHelper.NewRect(parent, "ItemInvite");
            Rt(Root, new Vector2(0.5f, 0.5f), new Vector2(500, 80), new Vector2(0, 31));

            // 头像
            Img(Root, "Image", "head", new Vector2(52, 56), new Vector2(-212.1f, 0));
            Img(Root, "Image (1)", "linebrown", new Vector2(2, 60), new Vector2(-174.3f, 0)).gameObject.SetActive(false);

            // 序号
            Txt(Root, "txtIndex", index.ToString(), Fs(60), new Vector2(60, 60), new Vector2(-213f, -5.4f), true);

            // 分隔线
            var line = UIHelper.Image(Root, "Image (2)", null);
            Rt(line.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(480, 2), new Vector2(0, -33.2f));
            line.color = new Color(1f, 1f, 1f, 0.14f);

            // 奖励区（横向排布）
            var reward = UIHelper.NewRect(Root, "ParentReward");
            Rt(reward, new Vector2(0.5f, 0.5f), new Vector2(280, 60), new Vector2(-23.3f, 0));
            var lg = reward.gameObject.AddComponent<HorizontalLayoutGroup>();
            lg.childControlHeight = true; lg.childControlWidth = false;
            lg.childForceExpandWidth = false; lg.childForceExpandHeight = true;
            lg.childAlignment = TextAnchor.MiddleCenter;
            lg.spacing = 4;
            if (rewardIcon != null)
            {
                var ic = UIHelper.Image(reward, "imgRewardIcon", rewardIcon);
                Rt(ic.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(52, 54), Vector2.zero);
            }
            Txt(reward, "txtNum", rewardNum, Fs(60), new Vector2(120, 60), Vector2.zero, true);

            // 状态按钮
            _notReceived = Btn(Root, "NotReceived", "grey", new Vector2(120, 48), new Vector2(184.3f, 0), null);
            Txt(_notReceived.transform, "Text (Legacy) (1)", "未完成", Fs(50), new Vector2(100, 50), new Vector2(0, 2), true);

            _btnInvite = Btn(Root, "btnInvite", "Btn_02", new Vector2(120, 48), new Vector2(184.3f, 0), () =>
            {
                if (OnInvite != null) OnInvite();
            });
            Txt(_btnInvite.transform, "Text (Legacy) (1)", "邀请", Fs(50), new Vector2(120, 60), Vector2.zero, true);

            _btnDraw = Btn(Root, "btnDraw", "Btn_01", new Vector2(120, 48), new Vector2(184.3f, 0), () =>
            {
                if (OnDraw != null) OnDraw();
            });
            Txt(_btnDraw.transform, "Text (Legacy) (1)", "领取", Fs(50), new Vector2(120, 60), Vector2.zero, true);

            _btnReceived = Btn(Root, "btnReceived", "Btn_01", new Vector2(120, 48), new Vector2(184.3f, 0), null);
            Txt(_btnReceived.transform, "Text (Legacy) (1)", "已领取", Fs(50), new Vector2(120, 60), Vector2.zero, true);

            SetState(InviteState.NotReceived);
        }

        /// <summary>切换状态按钮。</summary>
        public void SetState(InviteState s)
        {
            bool b1 = s == InviteState.NotReceived;
            bool b2 = s == InviteState.Invite;
            bool b3 = s == InviteState.Draw;
            bool b4 = s == InviteState.Received;
            if (_notReceived != null) _notReceived.SetActive(b1);
            if (_btnInvite != null) _btnInvite.SetActive(b2);
            if (_btnDraw != null) _btnDraw.SetActive(b3);
            if (_btnReceived != null) _btnReceived.SetActive(b4);
        }

        // ===== 小工具 =====
        private static int Fs(float h) => Mathf.Clamp(Mathf.RoundToInt(h / 2.2f), 20, 44);

        private static RectTransform Rt(RectTransform rt, Vector2 anchor, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        private static Image Img(Transform parent, string name, string sp, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name, OriginalAssets.GetUi(sp));
            if (img.sprite == null) img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, new Vector2(0.5f, 0.5f), size, pos);
            return img;
        }

        private static Text Txt(Transform parent, string name, string content, int fontSize,
            Vector2 size, Vector2 pos, bool outline = false)
        {
            var t = UIHelper.Text(parent, name, content, fontSize, Color.white);
            Rt(t.rectTransform, new Vector2(0.5f, 0.5f), size, pos);
            if (outline)
            {
                var o = t.gameObject.AddComponent<Outline>();
                o.effectColor = Color.black;
                o.effectDistance = new Vector2(2f, -2f);
            }
            return t;
        }

        private static GameObject Btn(Transform parent, string name, string sp, Vector2 size, Vector2 pos,
            System.Action onClick)
        {
            var b = UIHelper.Button(parent, name, onClick);
            var img = b.gameObject.GetComponent<Image>();
            var s = sp != null ? OriginalAssets.GetUi(sp) : null;
            if (s != null) { img.sprite = s; img.type = Image.Type.Sliced; }
            else img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, new Vector2(0.5f, 0.5f), size, pos);
            return b.gameObject;
        }
    }

    /// <summary>邀请有礼（ItemInvite 宿主，原版按邀请人数阶段领奖）：
    /// 标题 + 滚动列表（3 档：邀请 1/3/5 人领金币）+ 关闭。微信层未接入时代码演示。</summary>
    public sealed class InvitePanel : UIPanel
    {
        /// <summary>关闭回调（外部注入）。</summary>
        public System.Action OnClose;

        protected override bool BlockClick => true;

        protected override void Build()
        {
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch(back.rectTransform);
            back.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            // 底板
            var bg = Img(panel, "bg", "Bg_01di", new Vector2(585, 720), new Vector2(0, -40));

            // 标题
            Txt(bg.transform, "txtTitle", "邀请有礼", Fs(80), new Vector2(340, 80), new Vector2(0, 312), true, FontStyle.Bold);
            Btn(bg.transform, "btnClose", "tcclose_02", new Vector2(50, 49), new Vector2(280.2f, 336), OnCloseClick);

            // 提示条
            Txt(bg.transform, "txtInviteTip", "邀请玩家进入游戏，累计达标领奖", Fs(55), new Vector2(480, 55), new Vector2(0, 240), true);

            // 滚动列表
            var sc = MakeScroll(bg.transform, "Scroll View", new Vector2(520, 480), new Vector2(0, -55));
            var gold = OriginalAssets.GetUi("gold_big");
            var rewards = new[] { "x100金币", "x300金币", "x800金币" };
            for (int i = 0; i < rewards.Length; i++)
            {
                var row = new InviteItem(sc.content, i + 1, gold, rewards[i]);
                var stage = i;
                row.OnInvite = () => Debug.Log("[Invite] 邀请第" + (stage + 1) + "档（微信邀请）");
                row.OnDraw = () =>
                {
                    // 演示：领取后置“已领取”
                    row.SetState(InviteState.Received);
                    Debug.Log("[Invite] 领取第" + (stage + 1) + "档奖励");
                };
                row.SetState(i == 0 ? InviteState.Invite : InviteState.NotReceived);
            }
        }

        private void OnCloseClick()
        {
            if (OnClose != null) OnClose();
            else PanelManager.Instance.Pop();
        }

        // ===== 小工具 =====
        private static int Fs(float h) => Mathf.Clamp(Mathf.RoundToInt(h / 2.2f), 20, 44);

        private static RectTransform Rt(RectTransform rt, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        private static Image Img(Transform parent, string name, string sp, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name, OriginalAssets.GetUi(sp));
            if (img.sprite == null) img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, size, pos);
            return img;
        }

        private static Text Txt(Transform parent, string name, string content, int fontSize,
            Vector2 size, Vector2 pos, bool outline = false, FontStyle style = FontStyle.Normal)
        {
            var t = UIHelper.Text(parent, name, content, fontSize, Color.white, style);
            Rt(t.rectTransform, size, pos);
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
            Rt(img.rectTransform, size, pos);
            return b;
        }

        /// <summary>竖向滚动列表（与 LoginSharePanel 同一套结构）。</summary>
        private static ScrollRect MakeScroll(Transform parent, string name, Vector2 size, Vector2 pos)
        {
            var root = UIHelper.NewRect(parent, name);
            Rt(root, size, pos);
            root.gameObject.AddComponent<Image>();
            var sc = root.gameObject.AddComponent<ScrollRect>();

            var vp = UIHelper.NewRect(root, "Viewport");
            UIHelper.Stretch(vp);
            var vpImg = vp.gameObject.AddComponent<Image>();
            var maskSp = OriginalAssets.GetUi("UIMask");
            if (maskSp != null) vpImg.sprite = maskSp; else vpImg.color = new Color(1f, 1f, 1f, 0);
            vp.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = UIHelper.NewRect(vp, "Content");
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0, 1);
            var lg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            lg.childControlWidth = true; lg.childControlHeight = false;
            lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
            lg.spacing = 4;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sc.viewport = vp;
            sc.content = content;
            sc.horizontal = false;
            sc.vertical = true;
            sc.movementType = ScrollRect.MovementType.Clamped;
            sc.scrollSensitivity = 24f;
            return sc;
        }
    }
}