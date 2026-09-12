using UnityEngine;
using UnityEngine.UI;
using Xio.UI;

namespace Xio.Game
{
    /// <summary>
    /// 游戏入口（引导层）：确保 Canvas/EventSystem 就绪 → 压栈主城主面板。
    /// 面板栈架构：切场零开销，Spine/音频/存档常驻。
    /// DirectEnter = 直接进某关（调试/批处理冒烟用）。
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        public int StartLevel = 101;
        public bool DirectEnter = false;

        /// <summary>原版全部关卡区间（PuzzleConfig 实际数据）。</summary>
        public const int FirstLevel = 101;
        public const int LastLevel = 4170;

        private void Start()
        {
            Xio.Platform.WxApi.Init();
            TaskTracker.Init();          // M3 赛季任务注册
            RunNow();
        }

        private void Update()
        {
            TaskTracker.Tick(Time.deltaTime);   // 在线时长累计
        }

        public void RunNow()
        {
            EnsureBootCanvas();
            AudioManager.Inst.PlayBgm();

            if (DirectEnter)
            {
                var gp = PanelManager.Instance.Push<GameplayPanel>();
                gp.StartLevel(StartLevel);
            }
            else
            {
                PanelManager.Instance.Clear();
                PanelManager.Instance.Push<MainCityPanel>();
            }
        }

        /// <summary>主城/关卡共用一套竖屏 Canvas（750x1334）。</summary>
        private void EnsureBootCanvas()
        {
            if (FindObjectOfType<Canvas>() != null) return;

            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camGo.tag = "MainCamera";
                var cam = camGo.GetComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.10f, 0.16f, 0.22f);
            }

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(750, 1334);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
                esGo.transform.SetParent(transform, false);
            }
        }
    }
}