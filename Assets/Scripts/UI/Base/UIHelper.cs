using UnityEngine;
using UnityEngine.UI;

namespace Xio.UI
{
    /// <summary>动态搭建 UI 的小工具：快速创建 Image/Text/Button/面板容器。
    /// 字体统一走简易动态字体，避免工程外依赖。</summary>
    public static class UIHelper
    {
        private static Font _font;
        public static Font Font
        {
            get
            {
                if (_font == null)
                    _font = Font.CreateDynamicFontFromOSFont(
                        new[] { "Microsoft YaHei", "SimHei", "Arial", "Arial Unicode MS" }, 32);
                return _font;
            }
        }

        /// <summary>创建带 RectTransform 的空对象。</summary>
        public static RectTransform NewRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>拉伸铺满父节点。</summary>
        public static RectTransform Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        /// <summary>设置锚定（左下/右上/尺寸/中心偏移），传入 anchor 与 pivot=0.5。</summary>
        public static RectTransform Place(RectTransform rt, Vector2 anchor,
            Vector2 sizeDelta, Vector2 anchoredPos)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            return rt;
        }

        /// <summary>创建 Image（可选 sprite，sliced 模式自动开）。</summary>
        public static Image Image(Transform parent, string name, Sprite sprite = null, bool sliced = false)
        {
            var rt = NewRect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                if (sliced) img.type = UnityEngine.UI.Image.Type.Sliced;
            }
            else
            {
                img.color = new Color(1, 1, 1, 0);
            }
            return img;
        }

        /// <summary>创建 Text。</summary>
        public static Text Text(Transform parent, string name, string content,
            int size = 30, Color? color = null, FontStyle style = FontStyle.Normal,
            TextAnchor align = TextAnchor.MiddleCenter)
        {
            var rt = NewRect(parent, name);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color ?? Color.white;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;  // 纯展示文字不拦截点击
            return t;
        }

        /// <summary>创建 Button（透明底即可点）。</summary>
        public static Button Button(Transform parent, string name, System.Action onClick)
        {
            var rt = NewRect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0);
            var btn = rt.gameObject.AddComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }
    }
}