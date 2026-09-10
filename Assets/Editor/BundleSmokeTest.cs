using UnityEditor;
using UnityEngine;

namespace Xio.EditorTools
{
    /// <summary>冒烟测试：验证原版 bundle 能否加载出拼图贴图（引擎自身解码，非 UnityPy）。</summary>
    public static class BundleSmokeTest
    {
        [MenuItem("Xio/冒烟测试-加载原版拼图bundle")]
        public static void Run()
        {
            const string bundleName = "ui_puzzle_assets_uipuzzle_1_9ec56723e63d5041aabf285bf4b1510a.bundle";
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, bundleName);
            Debug.Log("[Smoke] 路径: " + path);
            var b = AssetBundle.LoadFromFile(path);
            if (b == null) { Debug.LogError("[Smoke] bundle 加载失败"); return; }
            Debug.Log($"[Smoke] bundle OK, {b.GetAllAssetNames().Length} assets");
            foreach (var n in b.GetAllAssetNames())
                Debug.Log("  asset: " + n);
            var names = b.GetAllAssetNames();
            if (names.Length > 0)
            {
                var tex = b.LoadAsset<Texture2D>(names[0]);
                if (tex != null)
                {
                    Debug.Log($"[Smoke] 贴图 {tex.name} {tex.width}x{tex.height} 格式={tex.format} readable={tex.isReadable}");
                    // 尝试读像素验证非洋红（需要 readable；若不可读则跳过像素验证）
                    if (tex.isReadable)
                    {
                        var px = tex.GetPixel(tex.width / 2, tex.height / 2);
                        Debug.Log($"[Smoke] 中心像素 RGBA=({px.r:F2},{px.g:F2},{px.b:F2},{px.a:F2})");
                    }
                }
                var spr = b.LoadAllAssets<Sprite>();
                Debug.Log($"[Smoke] sprites={spr.Length}");
            }
            b.Unload(false);
        }
    }
}
