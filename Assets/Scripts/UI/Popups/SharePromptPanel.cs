using System;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>
    /// 分享确认弹窗（原版 SharePrompt）：Wx_tanchuang 框 + 标题/文案 + 取消/分享按钮。
    /// 点分享调用平台层 Share，回调完成时执行 onDone（用于发奖励等）。
    /// </summary>
    public class SharePromptPanel : UIPanel
    {
        private Action _onDone;

        /// <summary>静态入口：压栈弹出分享确认框。</summary>
        public static SharePromptPanel Show(string title, string prompt, string shareTitle, string shareText, Action onDone = null)
        {
            return PanelManager.Instance.Push<SharePromptPanel>(p =>
            {
                p._title = title;
                p._prompt = prompt;
                p._shareTitle = shareTitle;
                p._shareText = shareText;
                p._onDone = onDone;
            });
        }

        private string _title = "分享";
        private string _prompt = "分享给好友一起玩";
        private string _shareTitle = "就你会消除";
        private string _shareText = "快来一起通关拿奖励！";

        protected override void Build()
        {
            var mask = UIHelper.Image(Root, "bg", null);
            UIHelper.Stretch((RectTransform)mask.transform);
            mask.color = new Color(0f, 0f, 0f, 0.55f);

            var frame = UIHelper.NewRect(Root, "Frame");
            UIHelper.Place(frame, new Vector2(0.5f, 0.5f), new Vector2(600, 400), new Vector2(0, 48));
            var fImg = frame.gameObject.AddComponent<Image>();
            var fSp = OriginalAssets.GetUi("Wx_tanchuang");
            if (fSp != null) { fImg.sprite = fSp; fImg.type = Image.Type.Sliced; fImg.color = Color.white; }

            // 分隔线
            var line = UIHelper.Image(frame, "Line", OriginalAssets.GetUi("Wx_xian"));
            UIHelper.Place((RectTransform)line.transform, new Vector2(0.5f, 0.5f), new Vector2(600, 4), new Vector2(0, 149));

            var title = UIHelper.Text(frame, "txtTitle", _title, 34, Color.white, FontStyle.Bold);
            UIHelper.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(480, 80), new Vector2(0, 194));
            title.gameObject.AddComponent<Outline>().effectColor = new Color(0.2f, 0.12f, 0.05f);

            var prompt = UIHelper.Text(frame, "txtPrompt", _prompt, 28, Color.white);
            UIHelper.Place(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(480, 60), new Vector2(0, 51));

            // 取消 / 分享
            var close = UIHelper.Button(frame, "btnClose", () => PanelManager.Instance.Pop());
            UIHelper.Place((RectTransform)close.transform, new Vector2(0.5f, 0.5f), new Vector2(234, 94), new Vector2(-145, -89));
            var cSp = OriginalAssets.GetUi("Wx_quxiao");
            if (cSp != null) { var ci = close.GetComponent<Image>(); ci.sprite = cSp; ci.color = Color.white; }

            var share = UIHelper.Button(frame, "btnShare", OnClickShare);
            UIHelper.Place((RectTransform)share.transform, new Vector2(0.5f, 0.5f), new Vector2(234, 94), new Vector2(145, -89));
            var sSp = OriginalAssets.GetUi("Wx_fenxiang");
            if (sSp != null) { var si = share.GetComponent<Image>(); si.sprite = sSp; si.color = Color.white; }
        }

        private void OnClickShare()
        {
            PanelManager.Instance.Pop();
            Xio.Platform.PlatformService.Current.Share(_shareTitle, _shareText, () =>
            {
                UIHint.Inst.Show("分享成功");
                _onDone?.Invoke();
            });
        }
    }
}