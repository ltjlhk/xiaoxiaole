using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>语音包条目（dump res_VoiceItem_11745 1:1）：语音试听 + 装备/解锁/购买三态按钮。</summary>
    public sealed class VoiceItemPanel : UIPanel
    {
        private Text _txtEquiped;
        private Text _txtEquip;
        private Image _imgSelect;
        private bool _equipped = true;

        protected override bool BlockClick => true;

        protected override void Build()
        {
            // 半透明底（Root 全屏遮罩）
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch(back.rectTransform);
            back.color = new Color(0f, 0f, 0f, 0.55f);

            // 条目本体（dump 根节点）
            var item = Img(Root, "VoiceItem", "pet_bg_01", new Vector2(683, 290), new Vector2(0, 131));
            Img(item.transform, "Image", "pet_bg_02", new Vector2(383, 108), new Vector2(-64.4f, 32.3f));
            Img(item.transform, "Image (1)", "pet_decorate", new Vector2(30, 10), new Vector2(4.1f, -30));
            Img(item.transform, "Image (2)", "pet_decorate", new Vector2(30, 10), new Vector2(-142.9f, -30))
                .transform.localScale = new Vector3(-1, 1, 1);
            Img(item.transform, "imgQuality", "Valuable_sign", new Vector2(100, 100), new Vector2(296.3f, 100.8f));
            Img(item.transform, "imgIcon", "Raccoon", new Vector2(191, 218), new Vector2(248, 13));
            Img(item.transform, "Image (3)", "pet_bg_name", new Vector2(185, 86), new Vector2(230, -89));

            // 文案
            Txt(item.transform, "txtLabel", "普通话", Fs(48), new Vector2(160, 48), new Vector2(-194, 121));
            var sound = Txt(item.transform, "txtSound", "金耀百合，火焰百合，橙霞花，晨曦百合。", 20,
                new Vector2(258, 83.5f), new Vector2(-109.4f, 36.9f));
            sound.horizontalOverflow = HorizontalWrapMode.Wrap;   // 长句自动换行
            sound.verticalOverflow = VerticalWrapMode.Overflow;
            Txt(item.transform, "txtVPack", "语音包", Fs(40), new Vector2(80, 40), new Vector2(-124.4f, 125.2f)).gameObject.SetActive(false);
            Txt(item.transform, "txtListen", "点击试听", Fs(40), new Vector2(80, 40), new Vector2(-70.9f, -28));
            Txt(item.transform, "txtName", "富贵儿", Fs(40), new Vector2(100, 40), new Vector2(237, -99.3f));

            // 试听按钮
            var soundBtn = Btn(item.transform, "btnSound", "pet_icon_voice", new Vector2(48, 48), new Vector2(63, 31.4f),
                () => Debug.Log("[VoiceItem] 试听：" + sound.text));
            Img(soundBtn.transform, "Image (1)", "pet_icon_voice_03", new Vector2(9, 26), new Vector2(-7.8f, 0));
            Img(soundBtn.transform, "Image (2)", "pet_icon_voice_02", new Vector2(8, 18), new Vector2(1.2f, 0));
            Img(soundBtn.transform, "Image (3)", "pet_icon_voice_01", new Vector2(5, 5), new Vector2(10.2f, 0));

            // 装备按钮（已装备态）
            var use = Btn(item.transform, "btnUse", "Btn_01", new Vector2(210, 63), new Vector2(-107, -99), ToggleEquip);
            _imgSelect = Img(use.transform, "imgSelect", "pet_icon_Equipped", new Vector2(42, 35), new Vector2(47.1f, 0));
            _txtEquiped = Txt(use.transform, "txtEquiped", "已装备", Fs(50), new Vector2(80, 50), new Vector2(-16, 2), true);
            _txtEquip = Txt(use.transform, "txtEquip", "装备", Fs(50), new Vector2(80, 50), Vector2.zero, true);
            _txtEquip.gameObject.SetActive(false);

            // 解锁按钮（默认隐藏）
            var unlock = Btn(item.transform, "btnUnlock", "Btn_02", new Vector2(210, 63), new Vector2(-107, -99),
                () => Debug.Log("[VoiceItem] 观看广告解锁"));
            Img(unlock.transform, "Image (1)", "icon_video", new Vector2(41.5f, 30.5f), new Vector2(-62.2f, 0));
            Txt(unlock.transform, "Text (Legacy)", "解锁", Fs(50), new Vector2(60, 50), new Vector2(-7.4f, 2), true);
            Txt(unlock.transform, "txtUnlock", "(0/5)", Fs(50), new Vector2(80, 50), new Vector2(52, 2), true);
            unlock.gameObject.SetActive(false);

            // 购买按钮（默认隐藏）
            var buy = Btn(item.transform, "btnBuy", "Btn_02", new Vector2(210, 63), new Vector2(-107, -99),
                () => Debug.Log("[VoiceItem] 金币购买"));
            Img(buy.transform, "Image (1)", "gold_con", new Vector2(38, 39.5f), new Vector2(0.5f, 0));
            Txt(buy.transform, "Text (Legacy)", "购买", Fs(50), new Vector2(80, 50), new Vector2(-50.2f, 2), true);
            Txt(buy.transform, "txtCost", "500", Fs(50), new Vector2(100, 50), new Vector2(55.1f, 2), true);
            buy.gameObject.SetActive(false);
        }

        /// <summary>装备/卸下切换（演示态）。</summary>
        private void ToggleEquip()
        {
            _equipped = !_equipped;
            if (_imgSelect != null) _imgSelect.gameObject.SetActive(_equipped);
            if (_txtEquiped != null) _txtEquiped.gameObject.SetActive(_equipped);
            if (_txtEquip != null) _txtEquip.gameObject.SetActive(!_equipped);
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
    }
}
