using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>通用奖励弹窗（1:1 复刻 res_CommonReward_10465）。
    /// 奖励条目由 SetRewards 动态填充进 rewardGroup（HorizontalLayoutGroup）。</summary>
    public sealed class CommonRewardPanel : UIPanel
    {
        private RectTransform _rewardGroup;

        protected override bool BlockClick => true;

        protected override void Build()
        {
            var backgroup = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)backgroup.transform);
            backgroup.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = StretchNode(Root, "Panel");

            // BlueSkeletonGraphic → spine 占位
            SpinePlaceholder(panel, "BlueSkeletonGraphic", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 344f), V(100f, 100f));

            // effectSkeleton（dump 默认 inactive，scale0.7）→ spine 占位
            var effectSkeleton = SpinePlaceholder(panel, "effectSkeleton", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(22f, 105f), V(100f, 100f), new Vector3(0.7f, 0.7f, 1f));
            effectSkeleton.gameObject.SetActive(false);

            var rewardGroup = Node(panel, "rewardGroup", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 15f), V(0f, 260f));
            var ilg = rewardGroup.gameObject.AddComponent<HorizontalLayoutGroup>();
            ilg.spacing = 24f;
            ilg.childAlignment = TextAnchor.MiddleCenter;
            ilg.childForceExpandWidth = false;
            ilg.childForceExpandHeight = false;
            var csf = rewardGroup.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            _rewardGroup = rewardGroup;

            var btnDraw = ImgBtn(panel, "btnDraw", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -291f), V(374f, 103f), () => PanelManager.Instance.Pop());
            Txt(btnDraw.transform, "Text", "领取", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 1f), V(240f, 70f), true);

            Txt(panel, "Text (Legacy)", "恭喜获得", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 342f), V(300f, 95f), true);
        }

        /// <summary>填充奖励图标与数量（条目复刻 res_Over 的 reward1/txtStarNum 结构）。</summary>
        public void SetRewards(List<string> spriteNames, List<int> counts)
        {
            if (_rewardGroup == null) return;
            for (int i = _rewardGroup.childCount - 1; i >= 0; i--)
                Object.Destroy(_rewardGroup.GetChild(i).gameObject);
            if (spriteNames == null) return;

            int n = counts != null ? Mathf.Min(spriteNames.Count, counts.Count) : spriteNames.Count;
            for (int i = 0; i < n; i++)
            {
                var item = UiImg(_rewardGroup, "reward" + i, spriteNames[i],
                    V(0.5f, 0.5f), V(0.5f, 0.5f), V(0f, 0f), V(75f, 75f));
                Txt(item.transform, "txtNum", counts != null ? "x" + counts[i] : "",
                    V(0.5f, 0.5f), V(0.5f, 0.5f), V(0f, -47f), V(120f, 60f));
            }
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
            var s = sp == null ? null : OriginalAssets.GetUi(sp);
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

        private static RectTransform SpinePlaceholder(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Vector3? scale = null)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size);   // TODO spine: SkeletonGraphic 占位
            if (scale.HasValue) rt.localScale = scale.Value;
            UIHelper.NewRect(rt, "Renderer0");                    // TODO spine: 渲染占位
            return rt;
        }
    }
}
