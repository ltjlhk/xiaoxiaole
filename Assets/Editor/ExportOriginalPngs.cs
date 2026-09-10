using System.IO;
using UnityEditor;
using UnityEngine;
using Xio.Assets;

namespace Xio.EditorTools
{
    /// <summary>
    /// 把原版 bundle 里的贴图一次性导出为 PNG 到 Assets/Resources/OriginalTex/，
    /// 让游戏运行时直接 Resources.Load（避免运行时 GPU 拷贝与 bundle 依赖）。
    /// 必须在有 GPU 的编辑器里执行（不带 -nographics）。
    /// </summary>
    public static class ExportOriginalPngs
    {
        private const string OutDir = "Assets/Resources/OriginalTex";

        [MenuItem("Xio/导出原版贴图为PNG到Resources")]
        public static void ExportAll()
        {
            Directory.CreateDirectory(OutDir);
            ExportBundle("ui_puzzle_assets_uipuzzle_1_9ec56723e63d5041aabf285bf4b1510a.bundle", "puzzle");
            ExportBundle("ui_bg_assets_uibg_bg2_81eb69c8ac93882fe360d7101ce35f41.bundle", "bg");
            ExportBundle("ui_flower_assets_ui_flower_98038c8b402d0558693b6961cfc3d496.bundle", "flower");
            AssetDatabase.Refresh();
            Debug.Log("[Export] 全部导出完成 -> " + OutDir);
        }

        private static void ExportBundle(string bundleName, string tag)
        {
            var path = Path.Combine(Application.streamingAssetsPath, bundleName);
            var b = AssetBundle.LoadFromFile(path);
            if (b == null) { Debug.LogError("[Export] 加载失败 " + bundleName); return; }
            int n = 0;
            foreach (var an in b.GetAllAssetNames())
            {
                var tex = b.LoadAsset<Texture2D>(an);
                if (tex == null) continue;
                var copy = Xio.Assets.OriginalAssets.ToReadable(tex);
                if (copy == null) continue;
                var shortName = Path.GetFileNameWithoutExtension(an);
                var png = copy.EncodeToPNG();
                if (png == null || png.Length == 0) continue;
                File.WriteAllBytes(Path.Combine(OutDir, $"{tag}_{shortName}.png"), png);
                Object.DestroyImmediate(copy);
                n++;
            }
            b.Unload(false);
            Debug.Log($"[Export] {bundleName} -> {n} 张贴图");
        }
    }
}
