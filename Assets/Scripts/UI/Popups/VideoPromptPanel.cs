using System;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>
    /// 看视频确认弹窗（原版 VideoPrompt）：Wx_tanchuang 框 + 文案 + “30秒”提示 + 取消/看视频按钮。
    /// 确认后调平台激励视频，完整观看回调 onReward。
    /// </summary>
    public class VideoPromptPanel : UIPanel
    {
        private Action _onReward;

        /// <summary>静态入口：压栈弹出，观看完整后执行 onReward。</summary>
        public static VideoPromptPanel Show(string prompt, Action onReward)
        {
            return PanelManager.Instance.Push<VideoPromptPanel>(p =>
            {
                p._onReward = onReward;
                p._prompt = prompt;
            });
        }

        private string _prompt = "需要观看广告";

        protected override void Build()
        {
            // 半透明遮罩
            var mask = UIHelper.Image(Root, "bg", null);
            UIHelper.Stretch((RectTransform)mask.transform);
            mask.color = new Color(0f, 0f, 0f, 0.55f);

            // 弹窗框（原版 600×375）
            var frame = UIHelper.NewRect(Root, "Frame");
            UIHelper.Place(frame, new Vector2(0.5f, 0.5f), new Vector2(600, 375), new Vector2(0, 27));
            var fImg = frame.gameObject.AddComponent<Image>();
            var fSp = OriginalAssets.GetUi("Wx_tanchuang");
            if (fSp != null) { fImg.sprite = fSp; fImg.type = Image.Type.Sliced; fImg.color = Color.white; }

            // 文案
            var prompt = UIHelper.Text(frame, "txtPrompt", _prompt, 30, Color.white, FontStyle.Bold);
            UIHelper.Place(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(480, 60), new Vector2(0, 148.5f));
            var o = prompt.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0.2f, 0.12f, 0.05f);

            // “30秒” 大字
            var sec = UIHelper.Text(frame, "Sec", "30秒", 34, new Color(1f, 0.85f, 0.3f), FontStyle.Bold);
            UIHelper.Place(sec.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(200, 80), new Vector2(0, 81.4f));
            sec.gameObject.AddComponent<Outline>().effectColor = new Color(0.25f, 0.15f, 0.03f);

            var tail = UIHelper.Text(frame, "Tail", "才能领取奖励", 26, Color.white);
            UIHelper.Place(tail.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(480, 60), new Vector2(0, 6.5f));

            // 取消 / 看视频
            var close = UIHelper.Button(frame, "btnClose", () => PanelManager.Instance.Pop());
            UIHelper.Place((RectTransform)close.transform, new Vector2(0.5f, 0.5f), new Vector2(234, 94), new Vector2(-145, -86.5f));
            var cSp = OriginalAssets.GetUi("Wx_quxiao");
            if (cSp != null) { var ci = close.GetComponent<Image>(); ci.sprite = cSp; ci.color = Color.white; }

            var video = UIHelper.Button(frame, "btnVideo", () => OnClickVideo());
            UIHelper.Place((RectTransform)video.transform, new Vector2(0.5f, 0.5f), new Vector2(234, 94), new Vector2(145, -86.5f));
            var vSp = OriginalAssets.GetUi("Wx_video");
            if (vSp != null) { var vi = video.GetComponent<Image>(); vi.sprite = vSp; vi.color = Color.white; }
        }

        private void OnClickVideo()
        {
            // 关闭弹窗并调平台激励视频；完整观看 → 发放奖励 + 提示条
            PanelManager.Instance.Pop();
            Xio.Platform.PlatformService.Current.ShowRewardedAd(ok =>
            {
                if (!ok) return;
                UIHint.Inst.Show("奖励已领取");
                _onReward?.Invoke();
            });
        }
    }
}