using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>道具获取引导（dump res_PropGuide_9785 1:1）：
    /// tittle 标题 + lightbg 光效 + 大 AA道具图（hecheng）+ 四道具槽 + handGuide 手型 +
    /// “获取”按钮（观看视频/金币购买 → 入账后跳 PropReceive）。</summary>
    public sealed class PropGuidePanel : UIPanel
    {
        /// <summary>道具 Id（1清 2刷 3合 4冻，与存档 items 一致）。</summary>
        public int PropId = 3;

        /// <summary>获取后追加回调（如跳转 PropReceive）。</summary>
        public System.Action<int, int> OnGranted;

        protected override bool BlockClick => true;

        // 道具槽：id / 槽内小图标 sprite / 说明文案 / 大图 sprite
        private static readonly int[] SlotIds = { 1, 2, 3, 4 };
        private static readonly string[] SlotSprites = { "clear", "shuffle", "compose", "freeze" };
        private static readonly string[] SlotTips =
        {
            "使用后清除无可消除的麻将",
            "使用后重新洗牌",
            "使用后立即合成一组麻将",
            "使用后冻结时间",
        };

        protected override void Build()
        {
            // backgroup
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch(back.rectTransform);
            back.color = new Color(0f, 0f, 0f, 0.72f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            // 标题
            Img(panel, "Image", "tittle", new Vector2(388, 92), new Vector2(0, 384));

            // 光效（RectMask2D 裁切 + 旋转 LightEffect）
            var maskBox = Box(panel, "Image", new Vector2(354, 176), new Vector2(0, 174));
            maskBox.gameObject.AddComponent<RectMask2D>();
            var light = Img(maskBox, "Image", "lightbg", new Vector2(352, 353), new Vector2(0, -89));
            light.gameObject.AddComponent<LightSpin>();

            // 大道具图（hecheng 组合图，随 PropId 替换）
            var itemTxt = SlotSprites[Mathf.Clamp(PropId - 1, 0, 3)];
            Img(panel, "imgItem", itemTxt, new Vector2(512, 350), new Vector2(0, 97));

            // 星星装饰 x3（覆盖在大图上）
            var deco = Box(panel, "Image (1)", new Vector2(352, 353), new Vector2(0, 82));
            Img(deco, "Image", "xing_01", new Vector2(33.6f, 44.4f), new Vector2(-94.9f, 39.8f));
            Img(deco, "Image (1)", "xing_01", new Vector2(28, 37), new Vector2(-25, -43));
            Img(deco, "Image (2)", "xing_01", new Vector2(42, 55.5f), new Vector2(59, 56));

            // 说明文案
            Txt(panel, "txtTip", SlotTips[Mathf.Clamp(PropId - 1, 0, 3)], Fs(80), new Vector2(450, 80),
                new Vector2(0, -145), true);

            // 获取按钮
            var draw = Btn(panel, "btnDraw", "Btn_02", new Vector2(345, 103), new Vector2(0, -267), OnDrawClick);
            Txt(draw.transform, "Text", "获取", Fs(70), new Vector2(240, 70), new Vector2(0, 1), true);

            // 四道具槽（底部，默认全部隐藏，原版点击对应槽位出引导）
            var prop = Box(panel, "prop", new Vector2(750, 160), new Vector2(0, 80));
            prop.anchorMin = new Vector2(0.5f, 0f);
            prop.anchorMax = new Vector2(0.5f, 0f);
            prop.pivot = new Vector2(0.5f, 0.5f);
            _slots.Clear();
            for (int i = 0; i < SlotIds.Length; i++)
            {
                var x = new[] { -258.8f, -86.2f, 86.2f, 258.8f }[i];
                var s = Btn(prop, "prop" + (i + 1), null, new Vector2(145, 145), new Vector2(x, 3), () => { });
                Img(s.transform, "imgProp", SlotSprites[i], SlotSize(i), new Vector2(0, 13));
                s.gameObject.SetActive(false);
                _slots.Add(s);
            }

            // 手型引导（底部第一个槽附近，默认隐藏）
            var handBox = Box(prop, "handGuide", new Vector2(70, 87), new Vector2(-276, 50));
            handBox.anchorMin = new Vector2(0f, 1f);
            handBox.anchorMax = new Vector2(0f, 1f);
            handBox.pivot = new Vector2(0f, 1f);
            Img(handBox, "hand", "guidehand", new Vector2(70, 87), new Vector2(-22, 39)).gameObject.SetActive(false);
            handBox.gameObject.SetActive(false);

            // 底部大图影子（默认隐藏，原版展示动画用）
            var propImg = Img(panel, "propImg", itemTxt, new Vector2(239, 296), new Vector2(0, 82));
            propImg.rectTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
            propImg.gameObject.SetActive(false);
        }

        private static Vector2 SlotSize(int i)
        {
            switch (i)
            {
                case 0: return new Vector2(76.5f, 94.7f);   // compose
                case 1: return new Vector2(95f, 85.4f);     // shuffle
                case 2: return new Vector2(80.6f, 100.5f);  // clear
                default: return new Vector2(85.4f, 92.2f);  // freeze
            }
        }

        private void OnDrawClick()
        {
            Debug.Log("[PropGuide] 获取道具 propId=" + PropId);
            // 入账统一在 PropReceivePanel 领取时发生（与 OnGranted 二选一）
            if (OnGranted != null)
            {
                OnGranted(PropId, 1);
                return;
            }
            PanelManager.Instance.Pop();
            PanelManager.Instance.Push<PropReceivePanel>(p => { p.PropId = PropId; p.Count = 1; });
        }

        private readonly List<Button> _slots = new List<Button>();

        // ===== 搭建小工具 =====

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
            Vector2 size, Vector2 pos, bool outline = false)
        {
            var t = UIHelper.Text(parent, name, content, fontSize, Color.white);
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
    }

    /// <summary>LightEffectControl 简易版：光效图缓缓旋转。</summary>
    public sealed class LightSpin : MonoBehaviour
    {
        private float _speed;
        private void Start() { _speed = Random.Range(40f, 70f) * (Random.value > 0.5f ? 1f : -1f); }
        private void Update()
        {
            var t = transform as RectTransform;
            if (t == null) return;
            t.Rotate(0f, 0f, _speed * Time.deltaTime);
        }
    }
}