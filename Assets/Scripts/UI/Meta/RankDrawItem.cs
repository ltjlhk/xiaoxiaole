using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>抽奖排行条目（1:1 复刻 res_RankDrawItem_9972：名次/时间/奖励 + 未领取/已领取按钮）。</summary>
    public sealed class RankDrawItem
    {
        public RectTransform Root { get; private set; }

        public enum DrawState { Unclaimed, Received }

        public RankDrawItem(Transform parent, string rank, string time, string reward, string rewardNum, DrawState state)
        {
            Root = UIHelper.NewRect(parent, "RankDrawItem");
            Root.anchorMin = Root.anchorMax = new Vector2(0.5f, 0.5f);
            Root.pivot = new Vector2(0.5f, 0.5f);
            Root.sizeDelta = new Vector2(550, 64);
            Root.anchoredPosition = new Vector2(0, 31);

            // 分隔线
            Img(Root, "Image (2)", "dividing_line", new Vector2(520, 5), new Vector2(0, -24));

            // itemReward（图标 + 数量，默认隐藏——由奖励文本 ParentReward 显示）
            var itemReward = Img(Root, "itemReward", "gold_big", new Vector2(80, 84), new Vector2(220, -30.8f));
            itemReward.rectTransform.anchorMin = new Vector2(0, 1);
            itemReward.rectTransform.anchorMax = new Vector2(0, 1);
            itemReward.rectTransform.pivot = new Vector2(0, 0.5f);
            itemReward.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            var num = Txt(itemReward.transform, "txtNum", "x" + rewardNum, Fs(30), new Vector2(40, 30), new Vector2(20, -13.5f));
            itemReward.gameObject.SetActive(false);

            // ParentReward：奖励文本（HorizontalLayoutGroup + ContentSizeFitter）
            var pr = UIHelper.NewRect(Root, "ParentReward");
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.pivot = new Vector2(0.5f, 0.5f);
            pr.sizeDelta = new Vector2(0, 70);
            pr.anchoredPosition = new Vector2(50, 9.2f);
            var hlg = pr.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 4;
            pr.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            Txt(pr, "T", reward, Fs(60), new Vector2(160, 40), Vector2.zero, true);

            // 未领取按钮
            var draw = Btn(Root, "btnDraw", "Btn_01", new Vector2(120, 48), new Vector2(210, 9.2f), null);
            Txt(draw.transform, "Text (Legacy) (1)", "未领取", Fs(60), new Vector2(120, 60), Vector2.zero, true);

            // 已领取按钮（默认隐藏）
            var received = Btn(Root, "btnReceived", "Btn_01", new Vector2(120, 48), new Vector2(210, 9.2f), null);
            Txt(received.transform, "Text (Legacy) (1)", "已领取", Fs(60), new Vector2(120, 60), Vector2.zero, true);
            received.gameObject.SetActive(false);

            // 名次 / 时间（锚左）
            var rankT = Txt(Root, "txtRank", rank, Fs(60), new Vector2(70, 60), new Vector2(-254, 9.2f), false);
            rankT.rectTransform.anchorMin = new Vector2(0, 0.5f);
            rankT.rectTransform.anchorMax = new Vector2(0, 0.5f);
            rankT.rectTransform.pivot = new Vector2(0, 0.5f);
            var timeT = Txt(Root, "txtTime", time, Fs(60), new Vector2(120, 60), new Vector2(-175, 9.2f), false);
            timeT.rectTransform.anchorMin = new Vector2(0, 0.5f);
            timeT.rectTransform.anchorMax = new Vector2(0, 0.5f);
            timeT.rectTransform.pivot = new Vector2(0, 0.5f);

            // 状态切换
            draw.gameObject.SetActive(state == DrawState.Unclaimed);
            received.gameObject.SetActive(state == DrawState.Received);
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
            var t = UIHelper.Text(parent, name, content, fontSize, Color.white);
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

        private static Button Btn(Transform parent, string name, string sp, Vector2 size, Vector2 pos,
            System.Action onClick)
        {
            var b = UIHelper.Button(parent, name, onClick);
            var img = b.gameObject.GetComponent<Image>();
            var s = sp != null ? OriginalAssets.GetUi(sp) : null;
            if (s != null) { img.sprite = s; img.type = Image.Type.Sliced; }
            else img.color = new Color(1f, 1f, 1f, 0.28f);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return b;
        }
    }
}
