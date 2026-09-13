using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>排行榜前三名领奖台条目（dump res_topRank_9645 1:1）：
    /// 奖牌（rank1/2/3）+ 顶部装饰（icon1/2/3）+ 名称 + 底牌数字与星星数。
    /// MakePodium 生成居中排列的三条。</summary>
    public sealed class TopRankItem
    {
        /// <summary>条目根节点。</summary>
        public RectTransform Root { get; private set; }

        public TopRankItem(Transform parent, int rank, string name, int stars)
        {
            Root = UIHelper.NewRect(parent, "topRank");
            Rt(Root, new Vector2(0.5f, 0.5f), new Vector2(70, 70), Vector2.zero);

            // 底牌（名次数字底板）
            Img(Root, "Image", "numbottomplate", new Vector2(142, 36), new Vector2(0, -140.9f));

            // 头像/奖牌组
            var icon = Img(Root, "imgIcon", "transparent", new Vector2(80, 80), new Vector2(0, -11));
            var medal = "rank" + Mathf.Clamp(rank, 1, 3);
            Img(icon.transform, "Image (1)", medal, new Vector2(90, 90), new Vector2(0, -1.9f));
            medalTop(icon.transform, rank);
            Txt(icon.transform, "txtName", name, Fs(40), new Vector2(140, 40), new Vector2(0, 64), true);

            // 星星图标 + 数量
            var star = Img(Root, "imgStar", "xingxinglogo", new Vector2(44.5f, 43), new Vector2(-40.7f, -140));
            star.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            Txt(Root, "txtStarNum", stars.ToString(), Fs(40), new Vector2(90, 40), new Vector2(20.6f, -140), true);

            // 名次数字（原版默认隐藏）
            Txt(Root, "txtTopRank", rank.ToString(), Fs(40), new Vector2(40, 40), new Vector2(0, -32), true)
                .gameObject.SetActive(false);
        }

        private void medalTop(Transform icon, int rank)
        {
            if (rank > 3) return;
            Img(icon.transform, "imgTop", "icon" + rank, new Vector2(44, 47), new Vector2(43, 30));
        }

        /// <summary>生成长条领奖台：右侧容器内 rank2 左 / rank1 中高 / rank3 右。</summary>
        public static void MakePodium(Transform parent, string[] names, int[] stars)
        {
            if (names == null || stars == null) return;
            int n = Mathf.Min(3, Mathf.Min(names.Length, stars.Length));
            if (n <= 0) return;
            var row = UIHelper.NewRect(parent, "TopRankRow");
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 0.5f);
            row.pivot = new Vector2(0.5f, 0.5f);
            row.sizeDelta = new Vector2(560, 250);
            row.anchoredPosition = Vector2.zero;

            // 原版三名：亚军左、冠军中高、季军右
            var ranks = new[] { 2, 1, 3 };
            var offsets = new[] { -175f, 0f, 175f };
            var heights = new[] { 0f, 55f, 0f };
            for (int i = 0; i < n; i++)
            {
                var it = new TopRankItem(row, ranks[i], names[i], stars[i]);
                it.Root.anchoredPosition = new Vector2(offsets[i], heights[i]);
            }
        }

        // ===== 小工具 =====
        private static int Fs(float h) => Mathf.Clamp(Mathf.RoundToInt(h / 2.2f), 20, 44);

        private static RectTransform Rt(RectTransform rt, Vector2 anchor, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        private static Image Img(Transform parent, string name, string sp, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name, OriginalAssets.GetUi(sp));
            if (img.sprite == null) img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, new Vector2(0.5f, 0.5f), size, pos);
            return img;
        }

        private static Text Txt(Transform parent, string name, string content, int fontSize,
            Vector2 size, Vector2 pos, bool outline = false)
        {
            var t = UIHelper.Text(parent, name, content, fontSize, Color.white);
            Rt(t.rectTransform, new Vector2(0.5f, 0.5f), size, pos);
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