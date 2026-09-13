using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>添加桌面三步引导（dump res_AddDesktop_10538 1:1）：
    /// buzhou 步骤圆形编号 + 说明文字 + zhiyin/zhiyin_2 指引图 + hand 手型 + “我知道了”关闭。</summary>
    public sealed class AddDesktopPanel : UIPanel
    {
        /// <summary>关闭回调（外部注入；未注入走 Pop）。</summary>
        public System.Action OnClose;

        protected override bool BlockClick => true;

        protected override void Build()
        {
            // backgroup（全屏半透明黑，隔离下层）
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch(back.rectTransform);
            back.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            // 指引气泡（zhiyin 大图）：右上角“三个点”
            var zhiyin = Img(panel, "Image", "zhiyin", new Vector2(201, 298), new Vector2(188, 433));
            Img(zhiyin.transform, "Image (1)", "wx_shezhi", new Vector2(181, 72), new Vector2(-38.6f, 127));

            // 步骤2底部图（zhiyin_2）内含手型
            var zhiyin2 = Img(panel, "Image (2)", "zhiyin_2", new Vector2(395, 201), new Vector2(48, -154));
            Img(zhiyin2.transform, "imgHand", "hand", new Vector2(70, 87), new Vector2(42, -28));

            // 三个步骤圆（buzhou）在此之前创建以保持在指引图下层
            MakeStep(panel, 2, "Text (Legacy) (3)", new Vector2(69, 68), new Vector2(-210, 5), "2", new Vector2(50, 60), new Vector2(-210, 9.1f), "点击 添加小程序 即可");
            MakeStep(panel, 1, "Text (Legacy) (4)", new Vector2(69, 68), new Vector2(-210, -334), "3", new Vector2(50, 60), new Vector2(-210, -329.8f), "从我的小程序进入游戏即可领取");
            MakeStep(panel, 0, "Text (Legacy) (2)", new Vector2(69, 68), new Vector2(-210, 235), "1", new Vector2(50, 60), new Vector2(-210, 239.2f), "点击右上角 三个点");

            // 关闭按钮“我知道了”
            var close = Btn(panel, "Close", null, new Vector2(320, 80), new Vector2(0, -481), OnCloseClick);
            Txt(close.transform, "Text (Legacy)", "我知道了", Fs(80), new Vector2(320, 80), Vector2.zero, true);
        }

        private void MakeStep(Transform panel, int _idx, string imgName, Vector2 imgSize, Vector2 imgPos,
            string num, Vector2 numSize, Vector2 numPos, string explain)
        {
            Img(panel, imgName, "buzhou", imgSize, imgPos);
            Txt(panel, "Text (" + _idx + ")", num, Fs(60), numSize, numPos, true);
            // 说明文案紧随该步骤编号
            textPositions(_idx, panel, explain);
        }

        private void textPositions(int _idx, Transform panel, string explain)
        {
            switch (_idx)
            {
                case 0: Txt(panel, "Text (" + 1 + ")", explain, Fs(60), new Vector2(400, 60), new Vector2(50, 241), true); break;
                case 1: Txt(panel, "txtExplain2", explain, Fs(60), new Vector2(500, 60), new Vector2(100, -328), true); break;
                case 2: Txt(panel, "txtExplain", explain, Fs(60), new Vector2(400, 60), new Vector2(50, 11), true); break;
            }
        }

        private void OnCloseClick()
        {
            if (OnClose != null) OnClose();
            else PanelManager.Instance.Pop();
        }

        // ===== 搭建小工具（基于 UIHelper，与其它面板一致） =====

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