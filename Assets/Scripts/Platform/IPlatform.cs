using System;

namespace Xio.Platform
{
    /// <summary>平台能力抽象：登录 / 分享(好友+朋友圈) / 激励视频 / 排行榜 / 初始化。
    /// 编辑器与 WebGL 通用实现为 PlaceholderPlatform（模拟成功）；
    /// 微信小游戏构建时 PlatformService.Resolve() 返回 WeChatPlatform（走 WxApi + game.js 桥）。</summary>
    public interface IPlatform
    {
        /// <summary>是否微信环境（非微信构建恒 false）。</summary>
        bool IsWeChat { get; }

        /// <summary>启动初始化（微信登录/环境检测等）。</summary>
        void Init();

        /// <summary>静默登录并拉取用户昵称（结果回调：是否成功）。</summary>
        void Login(Action<bool> onResult);

        /// <summary>当前登录昵称（未登录时返回默认值）。</summary>
        string Nickname { get; }

        /// <summary>分享到微信好友（拉起转发卡片）。</summary>
        void Share(string title, string text, Action onDone);

        /// <summary>分享到朋友圈。微信小游戏通过右上角菜单/分享通道实现。</summary>
        void ShareTimeline(string title, Action onDone);

        /// <summary>激励视频广告：结束后回调是否完整观看。</summary>
        void ShowRewardedAd(Action<bool> onEnd);

        /// <summary>好友排行榜提交分数（wx.setUserCloudStorage）。</summary>
        void SubmitScore(int score);

        /// <summary>拉取好友排行数据（开放数据域）。回调是否成功；成功后可读排行榜缓存。</summary>
        void FetchFriendRanking(Action<bool> onDone);
    }
}
