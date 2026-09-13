using System;
using UnityEngine;

namespace Xio.Platform
{
    /// <summary>微信小游戏桥接：登录 / 好友分享 / 朋友圈 / 激励视频 / 好友榜 / 云存档。
    /// 打包微信小游戏时（UNITY_WEBGL && !UNITY_EDITOR）经 DllImport("__Internal") 转发到 minigame/game.js 的 _Wx* 原生函数；
    /// game.js 的结果经 SendMessage → WxBridge → WxBridgeProxy 回传。
    /// 编辑器/其他平台：全部模拟成功，保证业务链路可跑。</summary>
    public static class WxApi
    {
        /// <summary>微信环境检测结果（非微信平台恒 false）。</summary>
        public static bool IsWeChat { get; private set; }

        public static void Init()
        {
            IsWeChat = false;
#if UNITY_WEBGL && !UNITY_EDITOR
            try { IsWeChat = _WxCheck(); } catch (Exception) { IsWeChat = false; }
            // 挂 WxBridge 中转（game.js 用 SendMessage 回传结果）
            var go = new GameObject("WxBridge");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<WxBridge>();
#endif
        }

        // ==================== 登录 ====================

        /// <summary>静默登录 + 拉取昵称（编辑器模拟返回"测试用户"）。</summary>
        public static void Login(Action<string> onDone)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WxBridgeProxy.OnLogin = onDone;
            try { _WxLogin(); }
            catch (Exception e) { Debug.LogWarning("[Wx] 登录调用失败 " + e.Message); onDone?.Invoke(null); }
#else
            Debug.Log("[Wx] 登录(模拟)：测试用户");
            onDone?.Invoke("测试用户");
#endif
        }

        // ==================== 分享 ====================

        /// <summary>分享到微信好友（转发卡片）。完成回调。</summary>
        public static void Share(string title, string text, Action onDone)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WxBridgeProxy.OnShareDone = ok => onDone?.Invoke();
            try { _WxShareAppMessage(title, text); }
            catch (Exception e) { Debug.LogWarning("[Wx] 好友分享失败 " + e.Message); onDone?.Invoke(); }
#else
            Debug.Log($"[Wx] 好友分享(模拟): {title} - {text}");
            onDone?.Invoke();
#endif
        }

        /// <summary>分享到朋友圈。完成回调。</summary>
        public static void ShareTimeline(string title, Action onDone)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WxBridgeProxy.OnShareDone = ok => onDone?.Invoke();
            try { _WxShareTimeline(title); }
            catch (Exception e) { Debug.LogWarning("[Wx] 朋友圈分享失败 " + e.Message); onDone?.Invoke(); }
#else
            Debug.Log($"[Wx] 朋友圈分享(模拟): {title}");
            onDone?.Invoke();
#endif
        }

        // ==================== 激励视频 ====================

        /// <summary>激励视频广告：onEnd(bool 完整观看)。</summary>
        public static void ShowRewardedAd(Action<bool> onEnd)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WxBridgeProxy.OnAdEnd = onEnd;
            try { _WxShowRewardedAd(); }
            catch (Exception e) { Debug.LogWarning("[Wx] 激励视频调用失败 " + e.Message); onEnd?.Invoke(false); }
#else
            Debug.Log("[Wx] 激励视频(模拟)：直接成功。");
            onEnd?.Invoke(true);
#endif
        }

        // ==================== 好友排行榜 ====================

        /// <summary>提交分数（wx.setUserCloudStorage，KV 上限 128 字节）。</summary>
        public static void SubmitScore(int v)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try { _WxSubmitScore(v); }
            catch (Exception e) { Debug.LogWarning("[Wx] 提交分数失败 " + e.Message); }
#else
            Debug.Log($"[Wx] 好友榜分数(模拟): {v}");
#endif
        }

        /// <summary>拉取好友排行（开放数据域渲染 + 回传结果）。</summary>
        public static void FetchFriendRanking(Action<bool> onDone)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WxBridgeProxy.OnRankingDone = onDone;
            try { _WxFetchFriendRanking(); }
            catch (Exception e) { Debug.LogWarning("[Wx] 好友榜拉取失败 " + e.Message); onDone?.Invoke(false); }
#else
            Debug.Log("[Wx] 好友榜拉取(模拟)：返回演示数据。");
            onDone?.Invoke(true);
#endif
        }

        // ===== 导入到 minigame/game.js 的原生函数（DllImport("__Internal") 名称对应）=====
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _WxCheck();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _WxLogin();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _WxShareAppMessage(string title, string text);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _WxShareTimeline(string title);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _WxShowRewardedAd();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _WxSubmitScore(int v);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _WxFetchFriendRanking();
#endif
    }

    /// <summary>微信桥接结果代理：game.js 通过 SendMessage("WxBridge", "XxxResult", value) 回传。
    /// 静态 Action 仅单实例回调用（登录/广告/分享每次调用后即置空，无并发冲突）。</summary>
    public static class WxBridgeProxy
    {
        public static Action<bool> OnAdEnd;
        public static Action<string> OnLogin;
        public static Action<bool> OnShareDone;
        public static Action<bool> OnRankingDone;

        public static void AdResult(string finished)
        {
            var cb = OnAdEnd; OnAdEnd = null;
            cb?.Invoke(finished == "1");
        }

        public static void LoginResult(string nickname)
        {
            var cb = OnLogin; OnLogin = null;
            cb?.Invoke(string.IsNullOrEmpty(nickname) ? null : nickname);
        }

        public static void ShareResult(string ok)
        {
            var cb = OnShareDone; OnShareDone = null;
            cb?.Invoke(ok == "1");
        }

        public static void RankingResult(string ok)
        {
            var cb = OnRankingDone; OnRankingDone = null;
            cb?.Invoke(ok == "1");
        }
    }

    /// <summary>SendMessage 中转 MonoBehaviour（game.js 调 unityInstance.SendMessage("WxBridge", ...)）。</summary>
    public class WxBridge : MonoBehaviour
    {
        public void AdResult(string v) => WxBridgeProxy.AdResult(v);
        public void LoginResult(string v) => WxBridgeProxy.LoginResult(v);
        public void ShareResult(string v) => WxBridgeProxy.ShareResult(v);
        public void RankingResult(string v) => WxBridgeProxy.RankingResult(v);
    }
}
