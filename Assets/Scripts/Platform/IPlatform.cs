using System;

namespace Xio.Platform
{
    /// <summary>平台能力抽象：分享 / 激励视频 / 排行榜 / 初始化。
    /// 编辑器与 WebGL 通用实现为 PlaceholderPlatform；接微信 SDK 时替换 PlatformService.Resolve() 返回 WeChatPlatform。</summary>
    public interface IPlatform
    {
        /// <summary>启动初始化（微信登录/环境检测等）。</summary>
        void Init();

        /// <summary>通用分享（带完成回调）。</summary>
        void Share(string title, string text, Action onDone);

        /// <summary>激励视频广告：结束后回调是否完整观看。</summary>
        void ShowRewardedAd(Action<bool> onEnd);

        /// <summary>好友排行榜提交分数。</summary>
        void SubmitScore(int score);
    }
}