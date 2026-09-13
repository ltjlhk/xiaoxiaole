using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>周榜条目（1:1 复刻 res_RankItemWeek_12170：皮肤立绘 + 名次/名字/分数）。</summary>
    public sealed class RankItemWeek
    {
        public RectTransform Root { get; private set; }

        public RankItemWeek(Transform parent, int rank, string name, int score, int level)
        {
            Root = UIHelper.NewRect(parent, "RankItemWeek");
            Root.anchorMin = Root.anchorMax = new Vector2(0.5f, 0.5f);
            Root.pivot = new Vector2(0.5f, 0.5f);
            Root.sizeDelta = new Vector2(718, 110);
            Root.anchoredPosition = Vector2.zero;

            Img(Root, "imgMotif", "myrankingboard", new Vector2(645, 110), new Vector2(36.5f, -1.8f));
            // 皮肤立绘（mjskin1 缺省用透明占位）
            Img(Root, "imgSkin", "mjskin1", new Vector2(67, 92), new Vector2(-322.5f, 0));
            Img(Root, "imgElves", null, new Vector2(80, 80), new Vector2(295, 0)).gameObject.SetActive(false);
            Img(Root, "imgRank", "img_rank1", new Vector2(82, 77), new Vector2(-245.6f, 0)).gameObject.SetActive(false);

            // 头像 + 圆框
            Img(Root, "imgIcon", "transparent", new Vector2(64, 64), new Vector2(-206.2f, 3.5f));
            Img(Root, "imgCircle", "rankcommon", new Vector2(72, 72), new Vector2(-206.2f, 1.5f));

            // 名次底板（隐藏）+ 分数底板
            Img(Root, "Image", "numbottomplate", new Vector2(150, 37.8f), new Vector2(-54, -21)).gameObject.SetActive(false);
            Img(Root, "Image (3)", "numbottomplate", new Vector2(150, 37.8f), new Vector2(152.3f, 2.6f));
            Img(Root, "Image (4)", "output_icon_1", new Vector2(64, 64), new Vector2(79, 4.6f));

            Txt(Root, "txtName", name, Fs(60), new Vector2(200, 60), new Vector2(-62.6f, 4.1f), true);
            Txt(Root, "txtScore", score.ToString(), Fs(65), new Vector2(120, 65), new Vector2(165.3f, 3.9f), true);
            Txt(Root, "txtRank", rank.ToString(), Fs(65), new Vector2(80, 65), new Vector2(-321.7f, 2.7f), true);
            Txt(Root, "txtLevel", "通关" + level, Fs(40), new Vector2(140, 40), new Vector2(3, -11.2f), true)
                .gameObject.SetActive(false);
        }

        private static int Fs(float h) => Mathf.Clamp(Mathf.RoundToInt(h / 2.2f), 20, 44);

        private static Image Img(Transform parent, string name, string sp, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name, sp != null ? OriginalAssets.GetUi(sp) : null);
            if (img.sprite == null) img.color = new Color(1f, 1f, 1f, 0.28f);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return img;
        }

        private static Text Txt(Transform parent, string name, string content, int fontSize,
            Vector2 size, Vector2 pos, bool outline = false)
        {
            var t = UIHelper.Text(parent, name, content, fontSize, Color.white, FontStyle.Bold);
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            if (outline)
            {
                var o = t.gameObject.AddComponent<Outline>();
                o.effectColor = Color.black;
                o.effectDistance = new Vector2(2f, -2f);
            }
            return t;
        }
    }
}
