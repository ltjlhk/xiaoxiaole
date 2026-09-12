using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>已完成拼图小部件（1:1 复刻 res_CompletePuzzle_12623：单节点 120×150，
    /// 原版为 RectangleGraphic 绘制，此处用 RawImage 直显关卡原图）。</summary>
    public sealed class CompletePuzzlePanel : UIPanel
    {
        public int LevelId = 1;     // PanelManager.Push 时注入

        protected override void Build()
        {
            var rt = Node(Root, "CompletePuzzle", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(120f, 150f));
            var raw = rt.gameObject.AddComponent<RawImage>();
            var tex = OriginalAssets.GetPuzzleTexture(LevelId);
            if (tex != null) raw.texture = tex;
            else raw.color = new Color(1f, 1f, 1f, 0.25f);   // 兜底半透明
        }

        public override void Refresh() { }

        // ---- dump 复刻小工具 ----
        private static Vector2 V(float x, float y) => new Vector2(x, y);

        private static RectTransform Node(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var rt = UIHelper.NewRect(parent, name);
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }
    }
}
