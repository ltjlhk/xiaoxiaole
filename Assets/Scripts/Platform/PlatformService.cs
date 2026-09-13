namespace Xio.Platform
{
    /// <summary>平台服务入口：业务代码统一通过本类访问平台能力，不感知具体实现。
    /// 微信小游戏构建（UNITY_WEBGL && !UNITY_EDITOR）返回 WeChatPlatform；其余返回 PlaceholderPlatform。</summary>
    public static class PlatformService
    {
        private static IPlatform _platform;

        public static IPlatform Current
        {
            get { return _platform ?? (_platform = Resolve()); }
            set { _platform = value; }
        }

        private static IPlatform Resolve()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new WeChatPlatform();
#else
            return new PlaceholderPlatform();
#endif
        }
    }
}
