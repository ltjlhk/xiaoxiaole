using UnityEditor;
using UnityEngine;
using Xio.Assets;

namespace Xio.EditorTools
{
    /// <summary>验证 ToReadable 全链路：加载原版 ASTC 贴图 → 转可读 → 切碎片 → 出图。</summary>
    public static class ToReadableTest
    {
        [MenuItem("Xio/测试-转可读切碎片")]
        public static void Run()
        {
            const string bundleName = "ui_puzzle_assets_uipuzzle_1_9ec56723e63d5041aabf285bf4b1510a.bundle";
            var b = AssetBundle.LoadFromFile(System.IO.Path.Combine(Application.streamingAssetsPath, bundleName));
            if (b == null) { Debug.LogError("[Test] bundle 加载失败"); return; }
            var tex = b.LoadAsset<Texture2D>("Assets/Resources_moved/UI/Puzzle/101.png");
            if (tex == null) { Debug.LogError("[Test] 贴图 101 加载失败"); return; }
            Debug.Log($"[Test] 原贴图 {tex.name} {tex.width}x{tex.height} format={tex.format} readable={tex.isReadable}");

            var copy = Xio.Assets.OriginalAssets.ToReadable(tex);
            if (copy == null) { Debug.LogError("[Test] ToReadable 失败"); return; }
            Debug.Log($"[Test] 副本 {copy.width}x{copy.height} format={copy.format} readable={copy.isReadable}");
            var px = copy.GetPixel(copy.width / 2, copy.height / 2);
            bool magenta = px.r > 0.5f && px.g < 0.2f && px.b > 0.5f;
            Debug.Log($"[Test] 中心像素 RGBA=({px.r:F2},{px.g:F2},{px.b:F2},{px.a:F2}) 非纯洋红={!magenta}");

            // 切 5x5 碎片
            int rows = 5, cols = 5;
            float fw = copy.width / (float)cols, fh = copy.height / (float)rows;
            var spr = Sprite.Create(copy, new Rect(0, copy.height - fh, fw, fh), new Vector2(0.5f, 0.5f), 100f);
            Debug.Log($"[Test] 碎片 Sprite 创建成功 rect={spr.rect} name={spr.name}");

            // 存盘验证像素
            try
            {
                var png = copy.EncodeToPNG();
                var p = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "puzzle101_readable.png");
                System.IO.File.WriteAllBytes(p, png);
                Debug.Log("[Test] 已存可读图: " + p);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Test] EncodeToPNG 失败: " + e.Message);
            }
            b.Unload(false);
        }
    }
}
