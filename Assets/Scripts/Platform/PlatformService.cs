namespace Xio.Platform
{
    /// <summary>平台服务入口：业务代码统一通过本类访问平台能力，不感知具体实现。
    /// 后续接微信小游戏 SDK 只需把 Resolve() 返回的实例换成 WeChatPlatform。</summary>
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
            // TODO 后续: 微信小游戏构建时 return new WeChatPlatform();
            return new PlaceholderPlatform();
        }
    }
}