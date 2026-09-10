using UnityEngine;

namespace Xio.Platform
{
    /// <summary>
    /// 微信小游戏桥接：分享 / 激励视频 / 好友榜 / 云存档。
    /// 打包微信小游戏时由 WeChat Mini Game SDK 接管；WebGL 构建下转发到 game.js。
    /// 骨架阶段提供安全桩，后续在 minigame/game.js 中实现 wx 调用。
    /// </summary>
    public static class WxApi
    {
        public static bool IsWeChat { get; private set; }

        public static void Init()
        {
            IsWeChat = false;
#if UNITY_WEBGL && !UNITY_EDITOR
            try { IsWeChat = _WxCheck(); } catch (System.Exception) { IsWeChat = false; }
#endif
        }

        // 分享
        public static void Share(string title, string text)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _WxShare(title, text);
#else
            Debug.Log($"[Wx] 分享: {title} - {text}");
#endif
        }

        // 激励视频：onEnd(bool completed)
        public static void ShowRewardedAd(System.Action<bool> onEnd)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _WxShowRewardedAd();
            // 结果回调由 game.js 通过 SendMessage 写入 WxBridgeProxy.OnAdResult
#else
            Debug.Log("[Wx] 激励视频(模拟)：直接成功。");
            onEnd?.Invoke(true);
#endif
        }

        public static void SubmitScore(int v)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _WxSubmitScore(v);
#else
            Debug.Log($"[Wx] 好友榜分数: {v}");
#endif
        }

        // ===== 导入到 game.js 的原生函数 =====
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _WxCheck();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _WxShare(string title, string text);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _WxShowRewardedAd();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _WxSubmitScore(int v);
#endif
    }
}
