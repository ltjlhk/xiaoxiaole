using UnityEngine;
using UnityEngine.UI;

namespace Xio.UI
{
    /// <summary>
    /// 提示条（原版提示条组：greenTip 双边镜像 + 白字 + 可选图标，自动淡出 ~1.8s）。
    /// 全局单例挂载，任意面板可 UIHint.Show(text[, icon]) 弹一条。
    /// </summary>
    public class UIHint : MonoBehaviour
    {
        private static UIHint _inst;

        /// <summary>保证存在挂载层（Canvas 全屏子节点，置顶层）。</summary>
        public static UIHint Inst
        {
            get
            {
                if (_inst == null)
                {
                    var holder = new GameObject("HintLayer", typeof(UIHint));
                    var cv = Object.FindObjectOfType<Canvas>();
                    if (cv != null) holder.transform.SetParent(cv.transform, false);
                    _inst = holder.GetComponent<UIHint>();
                    var rt = (RectTransform)holder.transform;
                    Stretch(rt);
                    holder.transform.SetAsLastSibling();
                }
                return _inst;
            }
        }

        /// <summary>弹一条提示条（可带图标）。</summary>
        public void Show(string text, Sprite icon = null)
        {
            var bar = UIHelper.NewRect(transform, "Hint_" + Time.frameCount);
            UIHelper.Place(bar, new Vector2(0.5f, 0.5f), new Vector2(600, 70), new Vector2(0, 330));

            // greenTip 双边镜像（左 + 右翻转）
            var left = UIHelper.Image(bar, "Left", Xio.Assets.OriginalAssets.GetUi("greenTip"), true);
            UIHelper.Stretch((RectTransform)left.transform);
            var right = UIHelper.Image(bar, "Right", Xio.Assets.OriginalAssets.GetUi("greenTip"), true);
            UIHelper.Stretch((RectTransform)right.transform);
            var rt = (RectTransform)right.transform;
            rt.localScale = new Vector3(-1f, 1f, 1f);

            // 图标（可选，靠左）
            if (icon != null)
            {
                var ic = UIHelper.Image(bar, "Icon", icon);
                UIHelper.Place((RectTransform)ic.transform, new Vector2(0f, 0.5f), new Vector2(44, 44), new Vector2(28, 0));
            }

            // 文案
            var t = UIHelper.Text(bar, "txtPrompt", text, 24, Color.white, FontStyle.Bold);
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            t.rectTransform.sizeDelta = new Vector2(560, 54);
            t.rectTransform.anchoredPosition = new Vector2(icon != null ? 16 : 0, 0);

            var fade = bar.gameObject.AddComponent<HintFade>();
            fade.life = 1.8f;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    /// <summary>提示条自动淡出并销毁。</summary>
    internal class HintFade : MonoBehaviour
    {
        public float life = 1.8f;
        private float _acc;
        private CanvasGroup _cg;

        private void Awake()
        {
            _cg = gameObject.AddComponent<CanvasGroup>();
        }

        private void Update()
        {
            _acc += Time.deltaTime;
            if (_acc >= life)
            {
                if (_cg != null) _cg.alpha -= Time.deltaTime * 3f;
                if (_cg == null || _cg.alpha <= 0f)
                    Destroy(gameObject);
            }
        }
    }
}