using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>好友榜条目（1:1 复刻 res_FameRankItem_12171：好友头像 + 日期装饰线 + 分数）。</summary>
    public sealed class FameRankItem
    {
        public RectTransform Root { get; private set; }

        public FameRankItem(Transform parent, int rank, string name, int score, string time)
        {
            Root = UIHelper.NewRect(parent, "FameRankItem");
            Root.anchorMin = Root.anchorMax = new Vector2(0.5f, 0.5f);
            Root.pivot = new Vector2(0.5f, 0.5f);
            Root.sizeDelta = new Vector2(718, 130);
            Root.anchoredPosition = Vector2.zero;

            Img(Root, "imgMotif", "myrankingboard", new Vector2(645, 110), new Vector2(36.5f, -21.3f));
            Img(Root, "imgSkin", "mjskin1", new Vector2(67, 92), new Vector2(-322.4f, -19.3f));
            Img(Root, "imgElves", null, new Vector2(80, 80), new Vector2(295, -19.5f)).gameObject.SetActive(false);
            // imgCircle：前三名奖牌（rank1-3），非前三隐藏
            var circle = Img(Root, "imgCircle", "rank" + Mathf.Clamp(rank, 1, 3), new Vector2(81, 84), new Vector2(-213, -17.8f));
            circle.gameObject.SetActive(rank <= 3);
            Img(Root, "imgIcon", "transparent", new Vector2(64, 64), new Vector2(-213, -17.8f));
            Img(Root, "Image (4)", "rankcommon", new Vector2(72, 72), new Vector2(-213, -19));

            // 分数底板
            Img(Root, "Image (1)", "numbottomplate", new Vector2(150, 37.8f), new Vector2(152.3f, -20.1f));
            Img(Root, "Image (2)", "output_icon_1", new Vector2(64, 64), new Vector2(79, -18.1f));

            // 顶部装饰线（deocrated_line 左右）
            Img(Root, "Image", "deocrated_line", new Vector2(53, 15), new Vector2(-312.1f, 54.7f));
            Img(Root, "Image (3)", "deocrated_line", new Vector2(53, 15), new Vector2(-118.7f, 54.7f));

            Txt(Root, "txtScore", score.ToString(), Fs(65), new Vector2(120, 65), new Vector2(165.3f, -18.8f), true);
            Txt(Root, "txtName", name, Fs(50), new Vector2(200, 50), new Vector2(-65.4f, -16.8f), true);
            Txt(Root, "txtTime", time, Fs(45), new Vector2(140, 45), new Vector2(-215.3f, 54.7f), true);
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
