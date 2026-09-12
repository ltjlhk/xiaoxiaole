using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>游戏内特效层（对应原版 Game 内 FX 对象）：
    /// - sendCardEffect/BlueSkeletonGraphic → Spine SkeletonGraphic（baozha/kapailizi/effect_3/5/6）
    /// - Hit1/Projectile/Shine_pink/SkillEft/Smoke/Transition_lizi/WaterOrbitSphere → 粒子替身（Spine/精灵缩放闪烁）
    /// - star/StarControl → 星飞收集；GuideFinger → 引导手（guidehand/zhiyin）；冻结 → dongjie 闪现
    /// 全部挂 Canvas 下动态生成，播完自毁。</summary>
    public static class GameFX
    {
        private static Canvas _canvas;
        public static Canvas Canvas
        {
            get { return _canvas; }
            set { _canvas = value; }
        }

        private static Canvas Resolve()
        {
            if (_canvas == null)
            {
                var go = GameObject.Find("Canvas");
                if (go != null) _canvas = go.GetComponent<Canvas>();
            }
            return _canvas;
        }

        private static Transform Root
        {
            get
            {
                var c = Resolve();
                return c != null ? c.transform : null;
            }
        }

        // ===== Spine 特效（原版 sendCardEffect/BlueSkeletonGraphic 同类） =====
        /// <summary>播放一个骨骼特效并自毁。skeleton 名：baozha(爆炸)/kapailizi(卡牌粒子)/effect_3/5/6。</summary>
        public static void PlaySpineFx(string skeleton, Vector2 anchoredPos, float scale = 1f, string anim = null)
        {
            var root = Root;
            if (root == null) return;

            var sg = SpineView.Play(root, "Fx_" + skeleton, skeleton, anim, false, new Vector2(100, 100));
            if (sg == null) return;

            UIHelper.Place((RectTransform)sg.transform, new Vector2(0.5f, 0.5f), new Vector2(100, 100), anchoredPos);
            sg.transform.localScale = Vector3.one * scale;

            // 未指定动画时默认播最后一个（通常是攻击/爆发）
            if (string.IsNullOrEmpty(anim))
            {
                var animCtl = sg.GetComponent<SkeletonAnimation>();
                var anims = SpineAssets.AnimationNames(sg.skeletonDataAsset);
                if (animCtl != null && anims.Count > 0)
                    animCtl.AnimationState.SetAnimation(0, anims[anims.Count - 1], false);
            }

            float dur = 1f;
            var track = sg.GetComponent<SkeletonAnimation>()?.AnimationState?.GetTrack(0);
            if (track != null && track.Animation != null) dur = track.Animation.Duration;
            var mb = sg.gameObject.AddComponent<FxSelfDestroy>();
            mb.Life = dur + 0.15f;
        }

        // ===== 飘字（消除得分/连击，对应原版 combo 区域 (0,-457) 与结算飘分） =====
        public static void PopText(Vector2 pos, string txt, Color color, int size = 40)
        {
            var root = Root;
            if (root == null || string.IsNullOrEmpty(txt)) return;
            var t = UIHelper.Text(root, "FxPop_" + txt, txt, size, color, FontStyle.Bold, TextAnchor.MiddleCenter);
            UIHelper.Place(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(400, 60), pos);
            var o = t.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0.2f, 0.1f, 0.02f);
            o.effectDistance = new Vector2(2, -2);
            t.rectTransform.SetAsLastSibling();
            var mb = t.gameObject.AddComponent<FxFloatUp>();
            mb.Life = 0.8f;
            mb.Rise = 90f;
        }

        // ===== 星飞收集（对应原版 star/StarControl：从牌位置飞向顶栏星标 (-34,-129 顶锚)） =====
        public static void FlyStar(Vector2 from, Vector2 to, System.Action onArrive = null)
        {
            var root = Root;
            if (root == null) return;
            var sp = OriginalAssets.GetUi("FX_Star_2") ?? OriginalAssets.GetUi("star");
            if (sp == null) { onArrive?.Invoke(); return; }
            var img = UIHelper.Image(root, "FxStar", sp);
            UIHelper.Place((RectTransform)img.transform, new Vector2(0.5f, 0.5f), new Vector2(58, 60), from);
            var mb = img.gameObject.AddComponent<FxStarFly>();
            mb.From = from;
            mb.To = to;
            mb.OnArrive = onArrive;
        }

        // ===== 冻结特效（对应原版 SkillEft 冻结 + dongjie 图标闪现 + 蓝闪） =====
        public static void FreezeFlash()
        {
            var root = Root;
            if (root == null) return;
            // 全屏蓝闪
            var flash = UIHelper.Image(root, "FxFreeze");
            UIHelper.Stretch((RectTransform)flash.transform);
            flash.color = new Color(0.45f, 0.75f, 1f, 0.0f);
            flash.raycastTarget = false;
            flash.rectTransform.SetAsLastSibling();
            var fb = flash.gameObject.AddComponent<FxFadeOut>();
            fb.Life = 0.5f;
            fb.TargetAlpha = 0.35f;

            // 中央 dongjie 图标缩放弹出
            var icon = OriginalAssets.GetUi("dongjie");
            if (icon != null)
            {
                var img2 = UIHelper.Image(root, "FxDongjie", icon);
                img2.preserveAspect = true;
                UIHelper.Place((RectTransform)img2.transform, new Vector2(0.5f, 0.5f), new Vector2(220, 220), Vector2.zero);
                img2.raycastTarget = false;
                var pb = img2.gameObject.AddComponent<FxPop>();
                pb.Life = 0.9f;
            }
        }

        // ===== 藤蔓/障碍提示（对应原版 vine/AnimationFly：tengmantiety 图标弹出） =====
        public static void VinePop(Vector2 pos)
        {
            var sp = OriginalAssets.GetUi("tengmantiety");
            if (sp == null) return;
            var root = Root;
            if (root == null) return;
            var img = UIHelper.Image(root, "FxVine", sp);
            img.preserveAspect = true;
            UIHelper.Place((RectTransform)img.transform, new Vector2(0.5f, 0.5f), new Vector2(160, 160), pos);
            img.raycastTarget = false;
            var pb = img.gameObject.AddComponent<FxPop>();
            pb.Life = 0.8f;
        }

        // ===== 引导手（对应原版 GuideFinger：guidehand 手势 + zhiyin 圈，呼吸/移动动画） =====
        public static GameObject GuideFinger(Vector2 pos, Vector2 moveDelta = default(Vector2))
        {
            var root = Root;
            if (root == null) return null;
            var sp = OriginalAssets.GetUi("guidehand") ?? OriginalAssets.GetUi("hand");
            if (sp == null) return null;
            var hand = UIHelper.Image(root, "FxGuideHand", sp);
            hand.preserveAspect = true;
            hand.raycastTarget = false;
            UIHelper.Place((RectTransform)hand.transform, new Vector2(0.5f, 0.5f), new Vector2(110, 110), pos);
            var mb = hand.gameObject.AddComponent<FxGuideHand>();
            mb.BasePos = pos;
            mb.MoveDelta = moveDelta == default(Vector2) ? new Vector2(0, -60) : moveDelta;
            return hand.gameObject;
        }

        public static void HideGuide()
        {
            var root = Root;
            if (root == null) return;
            var t = root.Find("FxGuideHand");
            if (t != null) Object.Destroy(t.gameObject);
        }
    }

    // ===== 自毁/动画协件 =====
    internal class FxSelfDestroy : MonoBehaviour
    {
        public float Life = 1f;
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(Life);
            Destroy(gameObject);
        }
    }

    internal class FxFloatUp : MonoBehaviour
    {
        public float Life = 0.8f;
        public float Rise = 90f;
        private Text _t;
        private Graphic _g;
        private Vector3 _start;
        private IEnumerator Start()
        {
            _start = transform.position;
            _t = GetComponent<Text>();
            _g = GetComponent<Graphic>();
            float t = 0;
            while (t < Life)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / Life);
                transform.position = _start + Vector3.up * Rise * k;
                if (_g != null) _g.color = new Color(_g.color.r, _g.color.g, _g.color.b, 1f - k);
                yield return null;
            }
            Destroy(gameObject);
        }
    }

    internal class FxStarFly : MonoBehaviour
    {
        public Vector2 From, To;
        public System.Action OnArrive;
        private const float Dur = 0.55f;
        private IEnumerator Start()
        {
            var rt = (RectTransform)transform;
            float t = 0;
            while (t < Dur)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / Dur));
                Vector2 p = Vector2.Lerp(From, To, k);
                p.y += Mathf.Sin(k * Mathf.PI) * 120f;   // 抛物线
                rt.anchoredPosition = p;
                rt.localScale = Vector3.one * (1f + 0.5f * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            rt.anchoredPosition = To;
            OnArrive?.Invoke();
            Destroy(gameObject);
        }
    }

    internal class FxFadeOut : MonoBehaviour
    {
        public float Life = 0.5f;
        public float TargetAlpha = 0.35f;
        private Graphic _g;
        private IEnumerator Start()
        {
            _g = GetComponent<Graphic>();
            float t = 0;
            while (t < Life)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / Life);
                float a = k < 0.25f ? TargetAlpha * (k / 0.25f) : TargetAlpha * (1f - (k - 0.25f) / 0.75f);
                if (_g != null) _g.color = new Color(_g.color.r, _g.color.g, _g.color.b, a);
                yield return null;
            }
            Destroy(gameObject);
        }
    }

    internal class FxPop : MonoBehaviour
    {
        public float Life = 0.9f;
        private RectTransform _rt;
        private Graphic _g;
        private IEnumerator Start()
        {
            _rt = (RectTransform)transform;
            _g = GetComponent<Graphic>();
            float t = 0;
            while (t < Life)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / Life);
                float s = k < 0.3f ? Mathf.Lerp(0.3f, 1.15f, k / 0.3f) : Mathf.Lerp(1.15f, 1f, (k - 0.3f) / 0.7f);
                _rt.localScale = Vector3.one * s;
                if (_g != null && k > 0.6f) _g.color = new Color(_g.color.r, _g.color.g, _g.color.b, 1f - (k - 0.6f) / 0.4f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }

    internal class FxGuideHand : MonoBehaviour
    {
        public Vector2 BasePos;
        public Vector2 MoveDelta;
        private RectTransform _rt;
        private IEnumerator Start()
        {
            _rt = (RectTransform)transform;
            float t = 0;
            while (true)
            {
                t += Time.deltaTime;
                float k = (Mathf.Sin(t * 3f) + 1f) * 0.5f;   // 0..1 往复
                _rt.anchoredPosition = BasePos + MoveDelta * k;
                yield return null;
            }
        }
    }
}
