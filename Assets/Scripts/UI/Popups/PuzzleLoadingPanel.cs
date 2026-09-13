using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;

namespace Xio.UI
{
    /// <summary>拼图加载遮罩（dump res_PuzzleLoading_12319 1:1）：
    /// 全屏 Image + 精灵圈底（jingling bg1）+ 黑脸图标（generalBlack）+ “拼图加载中...”。
    /// 加载完成由外部 Pop（常配 Coroutine 演示）。</summary>
    public sealed class PuzzleLoadingPanel : UIPanel
    {
        protected override bool BlockClick => true;

        protected override void Build()
        {
            // 全屏底（原版即 Image，带一点遮罩感）
            var bg = UIHelper.Image(Root, "bg", null);
            UIHelper.Stretch(bg.rectTransform);
            bg.color = new Color(0f, 0f, 0f, 0.82f);

            // 精灵圈底
            var loading = UIHelper.Image(Root, "LoadingImg", OriginalAssets.GetUi("jingling bg1"));
            UIHelper.Place(loading.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(180.1f, 185.9f), new Vector2(0, 100));
            if (loading.sprite == null) loading.color = new Color(1f, 1f, 1f, 0.28f);

            // 内圈黑脸图标
            var inner = UIHelper.Image(loading.transform, "Image", OriginalAssets.GetUi("generalBlack"));
            UIHelper.Place(inner.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(108, 140), Vector2.zero);
            if (inner.sprite == null) inner.color = new Color(0.9f, 0.75f, 0.55f, 1f);

            // 文案
            var t = UIHelper.Text(Root, "Text (Legacy)", "拼图加载中...", 32, Color.white);
            UIHelper.Place(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(317.7f, 97.3f), new Vector2(0, -48.6f));
            var o = t.gameObject.AddComponent<Outline>();
            o.effectColor = Color.black;
            o.effectDistance = new Vector2(2f, -2f);
        }
    }
}