using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>道具/商店弹窗（原版 PropView #12957 逆向 1:1）。
    /// 原版同一弹窗结构复用于多个道具：此处按静态道具表循环生成卡片，仅当前选中项激活。
    /// 金币购买 / 视频购买都会回调 OnBuy(道具Id)。</summary>
    public sealed class PropViewPanel : UIPanel
    {
        /// <summary>购买钩子：参数 = 道具 Id。</summary>
        public System.Action<int> OnBuy;

        /// <summary>Push 前注入：默认显示的道具序号。</summary>
        public int SelectedIndex;

        // 静态道具条目（4 条）：id / 售价文本
        private static readonly int[] PropIds = { 1, 2, 3, 4 };
        private static readonly string[] PropCosts = { "100", "150", "200", "300" };

        private static readonly Color Fallback = new Color(0.16f, 0.2f, 0.3f, 0.9f);
        private readonly List<Text> _goldTexts = new List<Text>();

        protected override void Build()
        {
            // backgroup（原版无精灵 → 半透明遮罩兜底）
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)back.transform);
            back.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            for (int i = 0; i < PropIds.Length; i++)
            {
                var card = MakePropCard(panel, i);
                card.gameObject.SetActive(i == Mathf.Clamp(SelectedIndex, 0, PropIds.Length - 1));
            }
            Refresh();
        }

        public override void Refresh()
        {
            int coins = SaveManager.Data.coins;
            foreach (var t in _goldTexts)
                if (t != null) t.text = coins.ToString();
        }

        private void Buy(int propId)
        {
            Debug.Log("[PropView] OnBuy propId=" + propId);
            if (OnBuy != null) OnBuy(propId);
            Refresh();
        }

        /// <summary>按 dump 坐标 1:1 生成一张道具卡片。</summary>
        private RectTransform MakePropCard(Transform panel, int idx)
        {
            var card = UIHelper.NewRect(panel, "PropCard" + (idx + 1));
            UIHelper.Stretch(card);
            int propId = PropIds[idx];

            var bg = Img(card, "bg", "Bg_01di", V(0.5f, 0.5f), V(575, 715), V(0, -49));
            Img(bg.transform, "Image", "Bg_nei2", V(0.5f, 0.5f), V(510, 357), V(0, 87));

            // TODO spine: 原版为 SkeletonGraphic+SwapBanner 蓝色横幅动画（Renderer0=RawImage）
            var spine = UIHelper.NewRect(bg.transform, "BlueSkeletonGraphic");
            UIHelper.Place(spine, V(0.5f, 0.5f), V(100, 100), V(0, 368));
            var renderer0 = UIHelper.NewRect(spine, "Renderer0");
            UIHelper.Place(renderer0, V(0.5f, 0.5f), V(100, 100), Vector2.zero);

            var imgProp = Img(bg.transform, "imgProp", "compose", V(0.5f, 0.5f), V(239, 296), V(0, 127));
            imgProp.transform.localScale = V(0.7f, 0.7f);

            var photoTip = Img(bg.transform, "photoTip", "photoTip", V(0.5f, 0.5f), V(437.9f, 373f), V(0, 123));
            photoTip.transform.localScale = V(0.7f, 0.7f);
            photoTip.gameObject.SetActive(false);

            var voice = Img(bg.transform, "Image", "pet_icon_voice", V(0.5f, 0.5f), V(48, 48), V(78, 35));
            var txtPropNum = Txt(voice.transform, "txtPropNum", "+1", 60, V(0.5f, 0.5f), V(60, 60), V(-2, 2.2f));
            Outline(txtPropNum);

            Txt(bg.transform, "txtExplain", "", 80, V(0.5f, 0.5f), V(400, 80), V(4, -41));
            var txtTitle = Txt(bg.transform, "txtTitle", "合成", 100, V(0.5f, 0.5f), V(400, 100), V(4, 365));
            Outline(txtTitle);

            // 金币购买
            var btnCoin = SpriteBtn(bg.transform, "btnCoin", "Btn_02", V(0.5f, 0.5f), V(374, 103), V(0, -152), () => Buy(propId));
            Img(btnCoin.transform, "imgCoin", "gold_con", V(0.5f, 0.5f), V(51, 53), V(0, -2.7f));
            var buy1 = Txt(btnCoin.transform, "buy", "购买", 80, V(0.5f, 0.5f), V(120, 80), V(-86, 0));
            Outline(buy1);
            var txtCost = Txt(btnCoin.transform, "txtCost", PropCosts[idx], 80, V(0.5f, 0.5f), V(120, 80), V(91.3f, 0));
            Outline(txtCost);

            // 视频购买
            var btnVideo = SpriteBtn(bg.transform, "btnVideo", "Btn_01", V(0.5f, 0.5f), V(374, 103), V(0, -269), () => Buy(propId));
            var videoIcon = Img(btnVideo.transform, "Image", "video", V(0.5f, 0.5f), V(47, 39), V(-62.4f, 0));
            videoIcon.transform.localScale = V(1.1f, 1.1f);
            var buy2 = Txt(btnVideo.transform, "buy", "购买", 80, V(0.5f, 0.5f), V(120, 80), V(43.1f, 4));
            Outline(buy2);

            // 抽奖领取（原版 inactive）
            var btnDraw = SpriteBtn(bg.transform, "btnDraw", "Btn_02", V(0.5f, 0.5f), V(374, 103), V(0, -215), () => Buy(propId));
            btnDraw.gameObject.SetActive(false);
            var buy3 = Txt(btnDraw.transform, "buy", "领取", 80, V(0.5f, 0.5f), V(120, 80), V(0, 2));
            Outline(buy3);

            // 橙子分享购买（原版 inactive）
            var btnShare = SpriteBtn(bg.transform, "btnShare", "Btn_01", V(0.5f, 0.5f), V(374, 103), V(0, -269), () => Buy(propId));
            btnShare.gameObject.SetActive(false);
            Img(btnShare.transform, "Image", "orange_icon", V(0.5f, 0.5f), V(46, 49), V(-62.4f, 0));
            var buy4 = Txt(btnShare.transform, "buy", "购买", 80, V(0.5f, 0.5f), V(120, 80), V(43.1f, 4));
            Outline(buy4);

            var btnClose = SpriteBtn(bg.transform, "btnClose", "tcclose_02", V(0.5f, 0.5f), V(40, 39), V(285, 320.5f),
                () => PanelManager.Instance.Pop());

            // 金币栏（悬浮在弹窗上方）
            var goldItem = Img(bg.transform, "goldItem", "Home_icon_bg", V(0.5f, 0.5f), V(186, 60.8f), V(-224, 531));
            Img(goldItem.transform, "gold", "gold_con", V(0.5f, 0.5f), V(51, 53), V(-55.7f, 2.1f));
            var txtGold = Txt(goldItem.transform, "txtGold", "0", 60, V(0.5f, 0.5f), V(120, 60), V(18.7f, 3.6f));
            _goldTexts.Add(txtGold);

            return card;
        }

        // ===== 本文件内的小工具（sprite 缺失 → 半透明纯色兜底） =====

        private static Vector2 V(float x, float y) { return new Vector2(x, y); }

        private static Image Img(Transform parent, string name, string sprite,
            Vector2 anchor, Vector2 size, Vector2 pos)
        {
            var sp = sprite != null ? OriginalAssets.GetUi(sprite) : null;
            var img = UIHelper.Image(parent, name, sp);
            if (sp == null && sprite != null) img.color = Fallback;
            UIHelper.Place((RectTransform)img.transform, anchor, size, pos);
            return img;
        }

        private static Text Txt(Transform parent, string name, string content, float boxH,
            Vector2 anchor, Vector2 size, Vector2 pos)
        {
            int fs = Mathf.Clamp(Mathf.RoundToInt(boxH / 2.2f), 20, 44);
            var t = UIHelper.Text(parent, name, content, fs, Color.white);
            UIHelper.Place((RectTransform)t.transform, anchor, size, pos);
            return t;
        }

        private static void Outline(Graphic g)
        {
            var o = g.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.85f);
        }

        private static Button SpriteBtn(Transform parent, string name, string sprite,
            Vector2 anchor, Vector2 size, Vector2 pos, System.Action onClick)
        {
            var img = Img(parent, name, sprite, anchor, size, pos);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }
    }
}
