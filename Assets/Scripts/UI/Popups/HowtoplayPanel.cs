using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>拼图玩法说明（1:1 复刻 res_Howtoplay_10188，点击任意空白处关闭）。</summary>
    public sealed class HowtoplayPanel : UIPanel
    {
        protected override bool BlockClick => true;

        protected override void Build()
        {
            // 根节点本体即 Image + Button（全屏点击关闭）
            var rootImg = Root.gameObject.AddComponent<Image>();
            rootImg.color = new Color(1f, 1f, 1f, 0f);
            var rootBtn = Root.gameObject.AddComponent<Button>();
            rootBtn.targetGraphic = rootImg;
            rootBtn.onClick.AddListener(() => PanelManager.Instance.Pop());

            var panel = StretchNode(Root, "Panel");

            Txt(panel, "Title", "拼图玩法", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-0.0f, 533f), V(313.9f, 96.1f));

            var step1 = UiImg(panel, "Step1", "step1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-167.4f, 297f), V(350f, 191f));
            Txt(step1.transform, "Text (Legacy)", "①通关可获得<color=#3F8EEB>随机</color>精灵碎片",
                V(0.5f, 0f), V(0.5f, 0f), V(2.1f, -28.9f), V(350f, 54.1f));

            UiImg(panel, "arrow", "leadiing icon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(80f, 172f), V(76f, 71f));

            var step2 = UiImg(panel, "Step2", "step2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(179.1f, -24f), V(326f, 239f));
            Txt(step2.transform, "Text (Legacy)", "②<color=#3F8EEB>精灵界面</color>选择进入拼图",
                V(0.5f, 0f), V(0.5f, 0f), V(-9f, -18f), V(350f, 54.1f));

            var arrow1 = UiImg(panel, "arrow (1)", "leadiing icon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-62f, -170f), V(76f, 71f));
            arrow1.rectTransform.localScale = new Vector3(-1f, 1f, 1f);

            var step3 = UiImg(panel, "Step3", "step3", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-172f, -348f), V(325f, 182f));
            Txt(step3.transform, "Text (Legacy)", "③将精灵碎片<color=#3F8EEB>兑换</color>拼图碎片",
                V(0.5f, 0f), V(0.5f, 0f), V(9f, -27f), V(350f, 54.1f));

            var arrow2 = UiImg(panel, "arrow (2)", "leadiing icon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(102.1f, -151.3f), V(76f, 71f));
            arrow2.gameObject.SetActive(false);

            var step4 = UiImg(panel, "Step4", "step4", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(171f, -370f), V(414f, 415f));
            step4.gameObject.SetActive(false);
            Txt(step4.transform, "Text (Legacy)", "④完成<color=#3F8EEB>任意拼图</color>解锁精灵",
                V(0.5f, 0f), V(0.5f, 0f), V(1f, 42.3f), V(350f, 54.1f));
            var step4Tip = Txt(step4.transform, "Text (Legacy)", "稀有精灵可在排行榜上展示哦！",
                V(0.5f, 0.5f), V(0.5f, 0.5f), V(11.7f, -198f), V(312.9f, 48.9f));
            step4Tip.gameObject.SetActive(false);

            Txt(panel, "CloseTxt", "点 击 任 意 空 白 处 关 闭", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -611f), V(340.9f, 49.8f));
        }

        public override void Refresh() { }

        // ---- dump 复刻小工具 ----
        private static Vector2 V(float x, float y) => new Vector2(x, y);

        private static RectTransform StretchNode(Transform parent, string name)
        {
            var rt = UIHelper.NewRect(parent, name);
            UIHelper.Stretch(rt);
            return rt;
        }

        private static RectTransform Node(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            var rt = UIHelper.NewRect(parent, name);
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        private static Image UiImg(Transform parent, string name, string sp,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size, pivot);
            var img = rt.gameObject.AddComponent<Image>();
            var s = OriginalAssets.GetUi(sp);
            if (s != null) img.sprite = s;
            else img.color = new Color(1f, 1f, 1f, 0.25f);   // GetUi null → 半透明纯色兜底
            return img;
        }

        private static Text Txt(Transform parent, string name, string content,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, bool outline = false, Vector2? pivot = null)
        {
            int fs = Mathf.Clamp(Mathf.RoundToInt(size.y / 2.2f), 20, 44);
            var t = UIHelper.Text(parent, name, content, fs, Color.white);
            var rt = t.rectTransform;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            if (outline) rt.gameObject.AddComponent<Outline>();
            return t;
        }
    }
}
