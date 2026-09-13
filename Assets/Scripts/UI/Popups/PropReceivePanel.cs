using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>道具到账弹窗（dump res_PropReceive_9866 1:1）：
    /// “恭喜获得” + Spine 光效占位 + 大道具图（缩放 0.7）+ xN + “领取”按钮。
    /// 领取即入账并关闭（获取入账在 PropGuide 已发生，这里负责展示确认，可二次叠加）。</summary>
    public sealed class PropReceivePanel : UIPanel
    {
        /// <summary>道具 Id（1清 2刷 3合 4冻）。</summary>
        public int PropId = 3;

        /// <summary>数量。</summary>
        public int Count = 1;

        /// <summary>确认回调（外部注入）。</summary>
        public System.Action OnConfirm;

        protected override bool BlockClick => true;

        private static readonly string[] SlotSprites = { "clear", "shuffle", "compose", "freeze" };

        protected override void Build()
        {
            // backgroup
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch(back.rectTransform);
            back.color = new Color(0f, 0f, 0f, 0.72f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            // Spine 光效占位（原版 BlueSkeletonGraphic）
            var spine = Box(panel, "BlueSkeletonGraphic", new Vector2(100, 100), new Vector2(0, 352));
            Box(spine, "Renderer0", new Vector2(100, 100), Vector2.zero);

            // 光效 Skeleton（默认隐藏）+ 三个渲染层占位
            var light = Box(panel, "lightSkeleton", new Vector2(100, 100), new Vector2(20, 145));
            light.localScale = new Vector3(0.6f, 0.6f, 1f);
            for (int i = 0; i < 4; i++) Box(light, "Renderer" + i, new Vector2(100, 100), Vector2.zero);
            light.gameObject.SetActive(false);

            // 大道具图（缩放 0.7）
            var sp = SlotSprites[Mathf.Clamp(PropId - 1, 0, 3)];
            var item = Img(panel, "imgItem", sp, new Vector2(239, 296), new Vector2(0, 103));
            item.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

            // 数量
            Txt(panel, "txtNum", "x" + Count, Fs(80), new Vector2(300, 80), new Vector2(0, -70), true);

            // 恭喜获得
            var gw = Txt(panel, "Text (Legacy)", "恭喜获得", Fs(95), new Vector2(300, 95), new Vector2(0, 350), true);

            // 领取按钮
            var draw = Btn(panel, "btnDraw", "Btn_01", new Vector2(339.3f, 103), new Vector2(0, -235), OnDrawClick);
            Txt(draw.transform, "Text", "领取", Fs(70), new Vector2(240, 70), new Vector2(0, 1), true);
        }

        private void OnDrawClick()
        {
            // 入账（幂等：PropGuide 可能已记账；这里再叠加一次 == 原版引导后到账弹窗只此一处入账）
            SaveManager.AddItem(PropId, Count);
            Debug.Log("[PropReceive] 领取 propId=" + PropId + " x" + Count + " 库存=" + SaveManager.GetItemCount(PropId));
            if (OnConfirm != null) OnConfirm();
            PanelManager.Instance.Pop();
        }

        // ===== 搭建小工具 =====

        private static int Fs(float h) => Mathf.Clamp(Mathf.RoundToInt(h / 2.2f), 20, 44);

        private static RectTransform Rt(RectTransform rt, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        private static RectTransform Box(Transform parent, string name, Vector2 size, Vector2 pos)
            => Rt(UIHelper.NewRect(parent, name), size, pos);

        private static Image Img(Transform parent, string name, string sp, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name, OriginalAssets.GetUi(sp));
            if (img.sprite == null) img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, size, pos);
            return img;
        }

        private static Text Txt(Transform parent, string name, string content, int fontSize,
            Vector2 size, Vector2 pos, bool outline = false)
        {
            var t = UIHelper.Text(parent, name, content, fontSize, Color.white);
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
    }
}