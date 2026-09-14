/**
 * 就你会消除 - 微信小游戏开放数据域（好友排行榜渲染）。
 * 微信开发者工具配置：project.config.json 的 openDataContext 指向本目录。
 *
 * 流程：主域 game.js 的 _WxFetchFriendRanking → 本域收到 {type:'renderRanking'} →
 * wx.getFriendCloudStorage 拉好友 "star" KV → 用 sharedCanvas 绘制榜单。
 */
(function () {
  'use strict';

  var sharedCanvas = wx.getSharedCanvas();
  var ctx = sharedCanvas.getContext('2d');

  var W = 750;   // 与主域 Canvas 宽度一致
  var H = 600;   // 排行榜可视高度（由主域 RankView 开放数据域视图决定）

  function drawRanking(list) {
    ctx.clearRect(0, 0, W, H);
    // 背景
    ctx.fillStyle = 'rgba(20, 26, 40, 0.9)';
    ctx.fillRect(0, 0, W, H);
    // 标题
    ctx.fillStyle = '#ffffff';
    ctx.font = 'bold 34px sans-serif';
    ctx.textAlign = 'center';
    ctx.fillText('好友排行', W / 2, 60);

    if (!list || list.length === 0) {
      ctx.fillStyle = '#9aa5b8';
      ctx.font = '26px sans-serif';
      ctx.fillText('暂无好友数据，快去邀请好友吧', W / 2, 140);
      return;
    }

    // 每行：名次 头像 昵称 分数
    var y0 = 120;
    var rowH = 72;
    list.forEach(function (item, i) {
      var y = y0 + i * rowH;
      if (y > H - rowH) return;
      // 名次
      ctx.fillStyle = i < 3 ? '#ffd35a' : '#ffffff';
      ctx.font = 'bold 28px sans-serif';
      ctx.textAlign = 'center';
      ctx.fillText(String(i + 1), 40, y + 34);
      // 头像
      if (item.avatarUrl) {
        var img = wx.createImage();
        img.onload = function () {
          ctx.save();
          ctx.beginPath();
          ctx.arc(90, y + 20, 26, 0, Math.PI * 2);
          ctx.closePath();
          ctx.clip();
          ctx.drawImage(img, 62, y - 8, 56, 56);
          ctx.restore();
        };
        img.src = item.avatarUrl;
      }
      // 昵称
      ctx.fillStyle = '#ffffff';
      ctx.font = '26px sans-serif';
      ctx.textAlign = 'left';
      var nick = item.nickname || '好友';
      if (nick.length > 8) nick = nick.slice(0, 8) + '…';
      ctx.fillText(nick, 130, y + 34);
      // 分数
      ctx.fillStyle = '#ffd35a';
      ctx.font = 'bold 28px sans-serif';
      ctx.textAlign = 'right';
      ctx.fillText(String(item.score || 0), W - 40, y + 34);
    });
  }

  wx.onMessage(function (msg) {
    if (!msg || msg.type !== 'renderRanking') return;
    var key = msg.key || 'star';
    wx.getFriendCloudStorage({
      keyList: [key],
      success: function (res) {
        var list = (res.data || []).map(function (u) {
          var score = 0;
          if (u.KVDataList && u.KVDataList.length > 0) {
            score = parseInt(u.KVDataList[0].value, 10) || 0;
          }
          return {
            nickname: u.nickname,
            avatarUrl: u.avatarUrl,
            score: score
          };
        });
        list.sort(function (a, b) { return b.score - a.score; });
        drawRanking(list);
      },
      fail: function () { drawRanking([]); }
    });
  });
})();
