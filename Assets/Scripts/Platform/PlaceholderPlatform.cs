using System;

namespace Xio.Platform
{
    /// <summary>占位平台实现：编辑器/WebGL 下全部走 WxApi 桩（日志 + 模拟成功），后续接入微信 SDK 时替换。</summary>
    public class PlaceholderPlatform : IPlatform
    {
        public void Init() => WxApi.Init();

        public void Share(string title, string text, Action onDone)
        {
            WxApi.Share(title, text);
            onDone?.Invoke();
        }

        public void ShowRewardedAd(Action<bool> onEnd)
        {
            WxApi.ShowRewardedAd(onEnd);
        }

        public void SubmitScore(int score) => WxApi.SubmitScore(score);
    }
}