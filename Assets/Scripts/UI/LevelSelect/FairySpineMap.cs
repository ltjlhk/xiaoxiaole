using UnityEngine;
using Xio.Assets;

namespace Xio.Game
{
    /// <summary>精灵展示映射：fairyId → Spine 骨架短名 / 立绘图标。数据源 FairyConfig。</summary>
    public static class FairySpineMap
    {
        public static FairyInfo Info(int fairyId)
        {
            foreach (var f in LevelSegmentModel.FairyList)
                if (f.id == fairyId) return f;
            return null;
        }

        /// <summary>Spine 骨架短名。原版配置为 bundle 全路径（Prefabs/Model/ElvesSkeleton1），
        /// 本地按短名导出（jingling_1/jingling2），做末段截取 + 别名映射，无配置回退 jingling_1。</summary>
        public static string SpineFor(int fairyId)
        {
            var f = Info(fairyId);
            string raw = f != null ? f.FairySpineSkeleton : "";
            if (string.IsNullOrEmpty(raw)) return "jingling_1";
            string shortName = raw.Substring(raw.LastIndexOf('/') + 1);
            if (shortName.Length == 0) return "jingling_1";
            if (shortName == "ElvesSkeleton1") return "jingling_1";
            if (shortName == "ElvesSkeleton2") return "jingling2";
            if (shortName == "ElvesSkeleton3") return "jingling_1";
            return shortName;
        }

        /// <summary>立绘图标 Sprite（主城看板降级用）。</summary>
        public static Sprite IconFor(int fairyId)
        {
            var f = Info(fairyId);
            string name = f != null && !string.IsNullOrEmpty(f.FairyIcon) ? f.FairyIcon : "fairy_1";
            var sp = OriginalAssets.Get("ui", name);
            return sp;
        }
    }
}