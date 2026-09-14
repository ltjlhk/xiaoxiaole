using System.Collections;
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
            // 平台初始化：微信环境检测 + 静默登录拉取昵称（编辑器下为模拟用户）
            var pf = Xio.Platform.PlatformService.Current;
            pf.Init();
            pf.Login(null);

            TaskTracker.Init();          // M3 赛季任务注册
            RunNow();
        }

        /// <summary>原版场景名 → 路由语义（对齐原版 BuildSettings：Load/StartScene/MainScene）。
        /// 静态保留给场景切换（Load → StartScene → MainScene）。</summary>
        public static string CurrentSceneName =>
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        public static bool IsLoadScene => CurrentSceneName.StartsWith("Load", System.StringComparison.OrdinalIgnoreCase);
        public static bool IsStartScene => CurrentSceneName.StartsWith("Start", System.StringComparison.OrdinalIgnoreCase);
        public static bool IsMainScene => CurrentSceneName.StartsWith("Main", System.StringComparison.OrdinalIgnoreCase);

        private void Update()
        {
            TaskTracker.Tick(Time.deltaTime);           // 在线时长累计
            SaveManager.TickSpiritRecover(Time.deltaTime);   // 精力按时间恢复
        }

        public void RunNow()
        {
            EnsureBootCanvas();
            AudioManager.Inst.PlayBgm();

            // 原版场景路由：Load→加载页→StartScene；StartScene→主城；MainScene→玩法(续关)
            if (IsLoadScene)
            {
                StartCoroutine(LoadFlow());
                return;
            }
            if (IsMainScene)
            {
                var gp = PanelManager.Instance.Push<GameplayPanel>();
                int cur = SaveManager.Data.maxPassedLevel + 1;
                gp.StartLevel(cur >= FirstLevel ? cur : StartLevel);
                return;
            }

            // 默认/StartScene/Demo：主城
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

        /// <summary>原版 Load 场景：先展示 Loading 面板，随后切 StartScene（主城）。</summary>
        private IEnumerator LoadFlow()
        {
            var loading = PanelManager.Instance.Push<PuzzleLoadingPanel>();
            yield return new WaitForSeconds(1.2f);
            if (loading != null && loading.IsVisible)
                UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
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
            scaler.matchWidthOrHeight = 0f;  // 竖屏：以宽度为基准，高度自适应

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