using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>收藏领取奖励弹窗（1:1 复刻 res_CollectReward_11390）。
    /// 条目复刻 res_imgRewardItem_9643 / res_RewardItem_9644（imgIcon scale0.2 + txtNum）。</summary>
    public sealed class CollectRewardPanel : UIPanel
    {
        private Image _imgItem;
        private Text _txtName;
        private Text _txtExplain;
        private RectTransform _rewardList;

        protected override void Build()
        {
            var bgMask = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)bgMask.transform);
            bgMask.color = new Color(0f, 0f, 0f, 0.6f);

            var panel = StretchNode(Root, "Panel");

            // BlueSkeletonGraphic → spine 占位
            SpinePlaceholder(panel, "BlueSkeletonGraphic", new Vector2(0f, 384f));

            UiImg(panel, "Image", "lightbg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 92f), new Vector2(352f, 353f));
            _imgItem = Node(panel, "imgItem", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 96f), new Vector2(240f, 240f)).gameObject.AddComponent<Image>();

            // motif（dump 默认隐藏）
            var motif = Node(panel, "motif", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 92f), new Vector2(422f, 376f));
            motif.localScale = new Vector3(0.8f, 0.8f, 1f);
            motif.gameObject.AddComponent<RectMask2D>();
            UiImg(motif, "Image", "theme_Panel_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(682f, 376f));
            UiImg(motif, "imgMotif", "theme_picture_03", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -19f), new Vector2(653f, 274f));
            Txt(motif, "txtMotifName", "", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 150.6f), new Vector2(340f, 60f));
            motif.gameObject.SetActive(false);

            // 星光
            var starRoot = Node(panel, "Image", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 112f), new Vector2(100f, 100f));
            UiImg(starRoot, "Image", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-138f, 30f), new Vector2(33.6f, 44.4f));
            UiImg(starRoot, "Image (1)", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-21f, -99f), new Vector2(39.2f, 51.8f));
            UiImg(starRoot, "Image (2)", "xing_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(124.1f, 5.1f), new Vector2(28f, 37f));

            // btnDraw 374×103 @(0,-333)
            var btnDraw = ImgBtn(panel, "btnDraw", "Btn_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -333f), new Vector2(374f, 103f), () => PanelManager.Instance.Pop());
            Txt(btnDraw.transform, "Text", "领取", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -4.3f), new Vector2(180f, 70f), true);

            _txtName = Txt(panel, "txtName", "物品名", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -137f), new Vector2(550f, 80f));
            _txtExplain = Txt(panel, "txtExplain", "可在收藏领取奖励", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -223f), new Vector2(550f, 80f));
            Txt(panel, "Text (Legacy)", "恭喜获得", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 383f), new Vector2(300f, 95f), true);

            // 多奖励小图列表（原版单图展示；列表条目复刻 imgRewardItem）
            _rewardList = Node(panel, "RewardList", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 20f), new Vector2(600f, 110f));
            var hlg = _rewardList.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
        }

        /// <summary>填充奖励（大图 + 计数；多奖励时追加小图列表）。</summary>
        public void SetRewards(List<string> spriteNames, List<int> counts)
        {
            if (spriteNames == null || spriteNames.Count == 0) return;

            var sp = OriginalAssets.GetUi(spriteNames[0]);
            if (sp != null) _imgItem.sprite = sp;
            else _imgItem.color = new Color(1f, 1f, 1f, 0.25f);   // GetUi null → 半透明兜底
            _txtName.text = spriteNames[0];
            if (counts != null && counts.Count > 0 && counts[0] > 0)
                _txtExplain.text = "可在收藏领取奖励 x" + counts[0];

            for (int i = _rewardList.childCount - 1; i >= 0; i--)
                Object.Destroy(_rewardList.GetChild(i).gameObject);
            if (spriteNames.Count > 1)
            {
                for (int i = 0; i < spriteNames.Count; i++)
                    MakeImgRewardItem(_rewardList, "imgRewardItem" + (i + 1), spriteNames[i],
                        counts != null && i < counts.Count ? counts[i] : 0);
            }
        }

        /// <summary>复刻 res_imgRewardItem_9643：imgIcon(252×261 scale0.2) + txtNum(100×60)。</summary>
        private void MakeImgRewardItem(Transform parent, string name, string spName, int count)
        {
            var root = Node(parent, name, V(0f, 0f), V(0f, 0f), Vector2.zero, new Vector2(80f, 100f));
            var icon = Node(root, "imgIcon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 21.2f), new Vector2(252f, 261f));
            icon.localScale = new Vector3(0.2f, 0.2f, 1f);
            var img = icon.gameObject.AddComponent<Image>();
            var sp = OriginalAssets.GetUi(spName);
            if (sp != null) img.sprite = sp;
            else img.color = new Color(1f, 1f, 1f, 0.25f);
            Txt(root, "txtNum", count > 0 ? count.ToString() : "", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -27.1f), new Vector2(100f, 60f));
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

        private static Button ImgBtn(Transform parent, string name, string sp,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, System.Action onClick, Vector2? pivot = null)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size, pivot);
            var img = rt.gameObject.AddComponent<Image>();
            var s = OriginalAssets.GetUi(sp);
            if (s != null) img.sprite = s;
            else img.color = new Color(1f, 1f, 1f, 0.25f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        private static RectTransform SpinePlaceholder(Transform parent, string name, Vector2 pos)
        {
            var rt = Node(parent, name, V(0.5f, 0.5f), V(0.5f, 0.5f), pos, new Vector2(100f, 100f));
            UIHelper.NewRect(rt, "Renderer0");   // TODO spine: SkeletonGraphic 渲染占位
            return rt;
        }
    }
}
