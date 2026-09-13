using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>世界榜条目（1:1 复刻 res_RankItemWorld_12169：根自带 myrankingboard 底板）。</summary>
    public sealed class RankItemWorld
    {
        public RectTransform Root { get; private set; }

        public RankItemWorld(Transform parent, int rank, string name, int score, int level)
        {
            Root = UIHelper.NewRect(parent, "RankItemWorld");
            Root.anchorMin = Root.anchorMax = new Vector2(0.5f, 0.5f);
            Root.pivot = new Vector2(0.5f, 0.5f);
            Root.sizeDelta = new Vector2(710, 110);
            Root.anchoredPosition = Vector2.zero;
            var rootImg = Root.gameObject.AddComponent<Image>();
            var board = OriginalAssets.GetUi("myrankingboard");
            if (board != null) { rootImg.sprite = board; rootImg.color = Color.white; }
            else rootImg.color = new Color(1f, 1f, 1f, 0.28f);

            // imgRank（前三名奖牌图，默认隐藏；World 榜 >3 用 txtRank 数字）
            var imgRank = Img(Root, "imgRank", "img_rank1", new Vector2(82, 77), new Vector2(-245.6f, 3.5f));
            imgRank.gameObject.SetActive(false);

            // 头像 + 圆框
            Img(Root, "imgIcon", "transparent", new Vector2(64, 64), new Vector2(-155.2f, 3.5f));
            Img(Root, "imgCircle", "rankcommon", new Vector2(73, 73), new Vector2(-155.2f, 1.5f));

            // 名次底板（隐藏）+ 分数底板
            Img(Root, "Image", "numbottomplate", new Vector2(150, 37.8f), new Vector2(-0.9f, -11.6f)).gameObject.SetActive(false);
            Img(Root, "Image (1)", "numbottomplate", new Vector2(150, 37.8f), new Vector2(216.3f, 0));
            Img(Root, "Image (2)", "output_icon_1", new Vector2(55, 55), new Vector2(143, 1));

            // 文本（原版字号：名次 65/名字 45/通关 40/分数 65）
            Txt(Root, "txtRank", rank.ToString(), Fs(65), new Vector2(80, 65), new Vector2(-258.3f, 6), true);
            Txt(Root, "txtName", name, Fs(45), new Vector2(200, 45), new Vector2(24, 25.9f), true);
            Txt(Root, "txtLevel", "通关" + level, Fs(40), new Vector2(140, 40), new Vector2(3, -11.2f), true);
            Txt(Root, "txtScore", score.ToString(), Fs(65), new Vector2(120, 65), new Vector2(222.3f, 0), true);
        }

        private static int Fs(float h) => Mathf.Clamp(Mathf.RoundToInt(h / 2.2f), 20, 44);

        private static Image Img(Transform parent, string name, string sp, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name, OriginalAssets.GetUi(sp));
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
