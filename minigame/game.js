/**
 * 就你会消除 - 微信小游戏构建桥（Unity WebGL 导出后放置于微信开发者工具工程根目录，与 game.json 同级）。
 *
 * 与 C# 侧 Xio.Platform.WxApi 的 DllImport("__Internal") 一一对应：
 *   _WxCheck / _WxLogin / _WxShareAppMessage / _WxShareTimeline /
 *   _WxShowRewardedAd / _WxSubmitScore / _WxFetchFriendRanking
 * 结果通过 unityInstance.SendMessage("WxBridge", "XxxResult", value) 回传 C#。
 *
 * 依赖：
 *  - game.json 需配置 openDataContext（本目录 openDataContext/）
 *  - 需要用户授权时补充 wx.authorize 处理
 */
(function () {
  'use strict';

  var unityInstance = null;   // Unity 2022 WebGL 实例（模板在 index.html 中赋值）
  var rewardedVideoAd = null;

  function send(obj, method, value) {
    if (unityInstance && typeof unityInstance.SendMessage === 'function') {
      unityInstance.SendMessage(obj, method, value === undefined ? '' : value);
    } else if (window.Module && typeof Module.SendMessage === 'function') {
      Module.SendMessage(obj, method, value === undefined ? '' : value);
    }
  }

  // ============ 环境检测 ============
  function _WxCheck() {
    return typeof wx !== 'undefined' ? 1 : 0;
  }

  // ============ 登录（wx.login + wx.getUserInfo 拉取昵称）============
  function _WxLogin() {
    if (typeof wx === 'undefined') { send('WxBridge', 'LoginResult', '测试用户'); return; }
    wx.login({
      success: function () {
        // 获取用户信息（部分环境需在用户点击后调用；失败则回传默认昵称）
        wx.getUserInfo({
          success: function (res) {
            var nick = (res.userInfo && res.userInfo.nickName) || '微信用户';
            send('WxBridge', 'LoginResult', nick);
          },
          fail: function () { send('WxBridge', 'LoginResult', '微信用户'); }
        });
      },
      fail: function () { send('WxBridge', 'LoginResult', null); }
    });
  }

  // ============ 好友分享（转发卡片）============
  function _WxShareAppMessage(title, text) {
    if (typeof wx === 'undefined') { send('WxBridge', 'ShareResult', '1'); return; }
    wx.shareAppMessage({
      title: title || '就你会消除',
      imageUrl: '',
      query: '',
      success: function () { send('WxBridge', 'ShareResult', '1'); },
      fail: function () { send('WxBridge', 'ShareResult', '0'); }
    });
  }

  // ============ 朋友圈分享（右上角菜单 + shareTimeline 通道）============
  function _WxShareTimeline(title) {
    if (typeof wx === 'undefined') { send('WxBridge', 'ShareResult', '1'); return; }
    // 打开转发菜单（含朋友圈 shareTimeline）
    wx.showShareMenu({
      withShareTicket: true,
      menus: ['shareAppMessage', 'shareTimeline'],
      success: function () {
        // 提示用户从右上角选择"分享到朋友圈"
        wx.showToast({ title: '请点击右上角分享到朋友圈', icon: 'none' });
        send('WxBridge', 'ShareResult', '1');
      },
      fail: function () { send('WxBridge', 'ShareResult', '0'); }
    });
  }

  // ============ 激励视频 ============
  function _WxShowRewardedAd() {
    if (typeof wx === 'undefined') { send('WxBridge', 'AdResult', '1'); return; }
    if (rewardedVideoAd) {
      rewardedVideoAd.show().catch(function () {
        // 加载失败重试一次
        rewardedVideoAd.load().then(function () { return rewardedVideoAd.show(); })
          .catch(function () { send('WxBridge', 'AdResult', '0'); });
      });
      return;
    }
    rewardedVideoAd = wx.createRewardedVideoAd({ adUnitId: 'REPLACE_WITH_REWARDED_AD_UNIT_ID' });
    rewardedVideoAd.onClose(function (res) {
      send('WxBridge', 'AdResult', (res && res.isEnded) ? '1' : '0');
    });
    rewardedVideoAd.onError(function () { send('WxBridge', 'AdResult', '0'); });
    rewardedVideoAd.load().then(function () { return rewardedVideoAd.show(); })
      .catch(function () { send('WxBridge', 'AdResult', '0'); });
  }

  // ============ 好友排行榜：提交分数 ============
  function _WxSubmitScore(v) {
    if (typeof wx === 'undefined') { return; }
    // 写入 KV 数据（开放数据域读取"star"排名）
    wx.setUserCloudStorage({
      KVDataList: [{ key: 'star', value: String(v || 0) }],
      fail: function (e) { console.warn('[Wx] setUserCloudStorage fail', e); }
    });
  }

  // ============ 好友排行榜：拉取（通知开放数据域渲染 + 回传）============
  function _WxFetchFriendRanking() {
    if (typeof wx === 'undefined') { send('WxBridge', 'RankingResult', '1'); return; }
    // 开放数据域（openDataContext）监听本消息后调用 wx.getFriendCloudStorage 并渲染
    var oc = wx.getOpenDataContext();
    if (oc) {
      oc.postMessage({ type: 'renderRanking', key: 'star' });
      send('WxBridge', 'RankingResult', '1');
    } else {
      send('WxBridge', 'RankingResult', '0');
    }
  }

  // 暴露给 Unity "__Internal"（Module 上挂载为全局函数）
  window._WxCheck = _WxCheck;
  window._WxLogin = _WxLogin;
  window._WxShareAppMessage = _WxShareAppMessage;
  window._WxShareTimeline = _WxShareTimeline;
  window._WxShowRewardedAd = _WxShowRewardedAd;
  window._WxSubmitScore = _WxSubmitScore;
  window._WxFetchFriendRanking = _WxFetchFriendRanking;

  // 供 Unity index.html 模板赋值实例
  window._setUnityInstance = function (inst) { unityInstance = inst; };
})();
