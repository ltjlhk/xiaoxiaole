using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>福利面板（原版 WelfareView #11750 逆向 1:1）。
    /// 三条静态任务条目（朋友圈/添加小程序/添加到桌面），点击领取后盖"已领取"戳。</summary>
    public sealed class WelfareViewPanel : UIPanel
    {
        private static readonly Color Fallback = new Color(0.16f, 0.2f, 0.3f, 0.9f);

        protected override void Build()
        {
            // backgroup（原版无精灵 → 半透明遮罩兜底）
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)back.transform);
            back.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            var bg = Img(panel, "bg", "Bg_01di", V(0.5f, 0.5f), V(575, 653), Vector2.zero);
            Img(bg.transform, "Image", "Bg_nei2", V(0.5f, 0.5f), V(510, 492), V(0, -35));

            // TODO spine: 原版为 SkeletonGraphic+SwapBanner 蓝色横幅动画（Renderer0=RawImage）
            var spine = UIHelper.NewRect(bg.transform, "BlueSkeletonGraphic");
            UIHelper.Place(spine, V(0.5f, 0.5f), V(100, 100), V(0, 313.5f));
            var renderer0 = UIHelper.NewRect(spine, "Renderer0");
            UIHelper.Place(renderer0, V(0.5f, 0.5f), V(100, 100), Vector2.zero);

            SpriteBtn(bg.transform, "btnClose", "tcclose_02", V(0.5f, 0.5f), V(40, 39), V(285, 266),
                () => PanelManager.Instance.Pop());

            // 静态条目（按 dump 逐条：图标 / 奖励图 / 奖励数 / 两行文案）
            MakeWelfareItem(bg.transform, "Item1", 119.5f, "wx_pyq", "gold_big", 84, "x15", "朋友圈", "首次加入朋友圈");
            MakeWelfareItem(bg.transform, "Item2", -30.5f, "wx_shoucang", "clearSamll", 87, "x1", "添加小程序", "游戏添加到我的小程序");
            MakeWelfareItem(bg.transform, "Item3", -184.5f, "wx_zhuomian", "clearSamll", 87, "x1", "添加到桌面", "游戏添加到桌面");

            var tTitle = Txt(bg.transform, "txtTitle", "福利礼盒", 100, V(0.5f, 0.5f), V(400, 100), V(4, 310.5f));
            Outline(tTitle);
        }

        /// <summary>按 dump 生成一条福利条目（bg_store 底板 + 图标 + 奖励框 + 两行文案）。</summary>
        private void MakeWelfareItem(Transform bg, string name, float yPos,
            string iconName, string rewardIcon, float rewardIconH,
            string rewardNum, string line1, string line2)
        {
            var item = SpriteBtn(bg, name, "bg_store", V(0.5f, 0.5f), V(448, 122.5f), V(0, yPos), null);

            Img(item.transform, "Image", iconName, V(0.5f, 0.5f), V(86, 82), V(-162, 0));

            var reward = Img(item.transform, "Reward", "reward_di", V(0.5f, 0.5f), V(88, 90), V(147, 0));
            var icon = Img(reward.transform, "imgRewardIcon", rewardIcon, V(0.5f, 0.5f), V(80, rewardIconH), V(0, 8.6f));
            icon.transform.localScale = V(0.7f, 0.7f);
            Txt(reward.transform, "txtRewardNum", rewardNum, 35, V(0.5f, 0.5f), V(60, 35), V(0, -25));
            // 已领取戳（原版 inactive，领取后激活）
            var imgCmp = Img(reward.transform, "imgCmp", "reward_lingqu", V(0.5f, 0.5f), V(88, 90), Vector2.zero);
            imgCmp.gameObject.SetActive(false);

            Txt(item.transform, "text1", line1, 50, V(0.5f, 0.5f), V(200, 50), V(-6.7f, 22.2f));
            Txt(item.transform, "text2", line2, 40, V(0.5f, 0.5f), V(200, 40), V(-6.7f, -20.6f));

            item.onClick.AddListener(() =>
            {
                imgCmp.gameObject.SetActive(true);
                item.interactable = false;
                Debug.Log("[WelfareView] 领取福利: " + line1);
            });
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
