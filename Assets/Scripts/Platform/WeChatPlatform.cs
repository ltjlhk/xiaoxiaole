using System;

namespace Xio.Platform
{
    /// <summary>微信小游戏平台实现：登录 / 好友分享 / 朋友圈 / 激励视频 / 好友排行榜，
    /// 全部经 WxApi 桥到 minigame/game.js（原生 wx API）。非微信构建不会实例化本类。</summary>
    public class WeChatPlatform : IPlatform
    {
        public bool IsWeChat => WxApi.IsWeChat;
        public string Nickname { get; private set; } = "微信用户";

        public void Init() => WxApi.Init();

        public void Login(Action<bool> onResult)
        {
            WxApi.Login(nick =>
            {
                if (!string.IsNullOrEmpty(nick)) Nickname = nick;
                onResult?.Invoke(!string.IsNullOrEmpty(nick));
            });
        }

        public void Share(string title, string text, Action onDone)
            => WxApi.Share(title, text, onDone);

        public void ShareTimeline(string title, Action onDone)
            => WxApi.ShareTimeline(title, onDone);

        public void ShowRewardedAd(Action<bool> onEnd)
            => WxApi.ShowRewardedAd(onEnd);

        public void SubmitScore(int score)
            => WxApi.SubmitScore(score);

        public void FetchFriendRanking(Action<bool> onDone)
            => WxApi.FetchFriendRanking(onDone);
    }
}
