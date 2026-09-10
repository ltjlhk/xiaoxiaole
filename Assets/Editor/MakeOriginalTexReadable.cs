using System.IO;
using UnityEditor;
using UnityEngine;

namespace Xio.EditorTools
{
    /// <summary>
    /// 把 Resources/OriginalTex 下导出的原版 PNG 全部设为 Read/Write Enabled（isReadable），
    /// 这样运行时 Resources.Load 即可直接 Sprite.Create，无需 GPU blit 转换。
    /// </summary>
    public static class MakeOriginalTexReadable
    {
        [MenuItem("Xio/原版PNG设为可读")]
        public static void Run()
        {
            string dir = "Assets/Resources/OriginalTex";
            if (!Directory.Exists(dir)) { Debug.LogError("[Readable] 目录不存在: " + dir); return; }
            int n = 0;
            foreach (var f in Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories))
            {
                var importer = AssetImporter.GetAtPath(f) as TextureImporter;
                if (importer == null) continue;
                if (!importer.isReadable)
                {
                    importer.isReadable = true;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    n++;
                }
            }
            AssetDatabase.Refresh();
            Debug.Log($"[Readable] 已设置 {n} 个原版 PNG 为可读");
        }
    }
}
