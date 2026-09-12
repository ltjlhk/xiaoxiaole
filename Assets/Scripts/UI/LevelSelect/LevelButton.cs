using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>单关按钮：显示段内关卡号；已通带星，未解锁置灰。点击回调由外部注入。</summary>
    public static class LevelButton
    {
        public static Button Create(Transform parent, string name, PuzzleLevel level,
            System.Action onClick, Vector2 size)
        {
            bool passed = LevelSegmentModel.IsPassed(level.Id);
            bool unlocked = LevelSegmentModel.IsUnlocked(level.Id);

            var rt = UIHelper.NewRect(parent, name);
            rt.sizeDelta = size;

            var board = OriginalAssets.GetUi("mp_board");
            var img = rt.gameObject.AddComponent<Image>();
            if (board != null) { img.sprite = board; img.type = Image.Type.Sliced; img.color = Color.white; }
            else img.color = new Color(0.95f, 0.78f, 0.32f);

            var btn = rt.gameObject.AddComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            // 关卡号
            var t = UIHelper.Text(rt, "No", level.LevelNo.ToString(), 26,
                new Color(0.45f, 0.25f, 0.08f), FontStyle.Bold);
            var trt = t.rectTransform;
            trt.anchorMin = trt.anchorMax = passed ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0.62f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(size.x, 30);
            trt.anchoredPosition = Vector2.zero;

            // 已通关星标
            if (passed)
            {
                var star = UIHelper.Image(rt, "Star", OriginalAssets.GetUi("star"));
                var srt = (RectTransform)star.transform;
                srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.28f);
                srt.pivot = new Vector2(0.5f, 0.5f);
                srt.sizeDelta = new Vector2(size.x * 0.45f, size.x * 0.45f);
                srt.anchoredPosition = Vector2.zero;
            }
            // 未解锁锁头
            if (!unlocked)
            {
                var mask = UIHelper.Image(rt, "LockMask");
                var mrt = (RectTransform)mask.transform;
                UIHelper.Stretch(mrt);
                mask.color = new Color(0, 0, 0, 0.55f);
                var lockIcon = UIHelper.Image(rt, "Lock", OriginalAssets.GetUi("suojin"));
                if (lockIcon != null)
                {
                    var lrt = (RectTransform)lockIcon.transform;
                    lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
                    lrt.pivot = new Vector2(0.5f, 0.5f);
                    lrt.sizeDelta = new Vector2(36, 44);
                    lrt.anchoredPosition = Vector2.zero;
                }
            }
            return btn;
        }
    }
}