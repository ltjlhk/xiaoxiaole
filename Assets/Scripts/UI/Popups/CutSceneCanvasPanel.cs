using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>拼图花语过场（dump res_CutSceneCanvas_10124 1:1）：
    /// huaditu 画卷从 upPos 下滑至 downPos 展开 + 粒子过渡（占位）。
    /// 完成/点击跳过即关闭。动画由 CutSceneScroll 驱动。</summary>
    public sealed class CutSceneCanvasPanel : UIPanel
    {
        /// <summary>画卷贴图（外部注入；null 时用 huaditu 原图）。</summary>
        public Sprite Painting;

        /// <summary>过场结束回调（外部注入）。</summary>
        public System.Action OnDone;

        /// <summary>滚动时长（秒）。</summary>
        public float Duration = 2.6f;

        protected override bool BlockClick => true;

        protected override void Build()
        {
            // 全屏黑底
            var bg = UIHelper.Image(Root, "bg");
            UIHelper.Stretch(bg.rectTransform);
            bg.color = Color.black;

            // 画卷（pivot 顶部，从 upPos 滑到 downPos）
            var huaditu = UIHelper.NewRect(Root, "huaditu");
            huaditu.anchorMin = huaditu.anchorMax = new Vector2(0.5f, 0.5f);
            huaditu.pivot = new Vector2(0.5f, 1f);
            huaditu.sizeDelta = new Vector2(1080, 2200);
            huaditu.anchoredPosition = new Vector2(0, 1113);
            var huImg = huaditu.gameObject.AddComponent<Image>();
            var sp = Painting != null ? Painting : OriginalAssets.GetUi("huaditu");
            if (sp != null) { huImg.sprite = sp; huImg.type = Image.Type.Sliced; }
            else huImg.color = new Color(0.2f, 0.25f, 0.35f, 1f);

            // 粒子过渡（占位粒子系统，隐藏）
            var lizi = UIHelper.NewRect(Root, "Transition_lizi");
            UIHelper.Stretch(lizi);
            for (int i = 1; i <= 3; i++)
            {
                var p = UIHelper.Image(lizi, "Particle System" + i, null);
                UIHelper.Stretch((RectTransform)p.transform);
            }
            lizi.gameObject.SetActive(false);

            // 滚动动画驱动
            var scroller = Root.gameObject.AddComponent<CutSceneScroll>();
            scroller.target = huaditu;
            scroller.from = new Vector2(0, 1113);
            scroller.to = new Vector2(0, -967);
            scroller.duration = Duration;
            scroller.onDone = OnDone;

            // 点击任意处跳过
            var skip = UIHelper.Button(Root, "Skip", () =>
            {
                if (scroller != null) scroller.CompleteNow();
                CloseSelf();
            });
            UIHelper.Stretch((RectTransform)skip.transform);
        }

        private void CloseSelf()
        {
            PanelManager.Instance.Pop();
        }
    }

    /// <summary>画卷滚动动画（upPos → downPos，EaseIn）。</summary>
    public sealed class CutSceneScroll : MonoBehaviour
    {
        public RectTransform target;
        public Vector2 from;
        public Vector2 to;
        public float duration = 2.6f;
        public System.Action onDone;

        private float _t;
        private bool _done;

        private void Update()
        {
            if (target == null) { if (!_done) CompleteNow(); return; }
            _t += Time.deltaTime / Mathf.Max(0.01f, duration);
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_t));
            target.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
            if (_t >= 1f) Complete();
        }

        public void CompleteNow()
        {
            if (target != null) target.anchoredPosition = to;
            Complete();
        }

        private void Complete()
        {
            if (_done) return;
            _done = true;
            if (onDone != null) onDone();
        }
    }
}