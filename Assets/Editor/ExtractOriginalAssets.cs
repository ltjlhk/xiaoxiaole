using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Xio.EditorTools
{
    /// <summary>
    /// 用 Unity 自身加载原版团结引擎 bundle，把贴图/精灵导出为 PNG。
    /// 解决 UnityPy 解析团结引擎(2022.3.48t5)贴图字节偏移 40 的问题。
    /// </summary>
    public static class ExtractOriginalAssets
    {
        private const string BundleDir = "Assets/StreamingAssets";
        private const string OutDir = "Assets/Extracted";

        [MenuItem("Xio/提取原版资源到 Assets/Extracted")]
        public static void ExtractAll()
        {
            if (!Directory.Exists(BundleDir))
            {
                Debug.LogError("未找到 bundle 目录: " + BundleDir);
                return;
            }

            Directory.CreateDirectory(OutDir);
            int totalTex = 0, totalSpr = 0;
            foreach (var bundlePath in Directory.GetFiles(BundleDir, "*.bundle"))
            {
                var bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle == null)
                {
                    Debug.LogWarning("加载失败: " + bundlePath);
                    continue;
                }
                var tag = Path.GetFileNameWithoutExtension(bundlePath).Replace("-", "_");
                tag = tag.Length > 40 ? tag.Substring(0, 40) : tag;
                var dir = Path.Combine(OutDir, tag);
                Directory.CreateDirectory(dir);

                // 1) Sprite
                foreach (var spr in bundle.LoadAllAssets<Sprite>())
                {
                    if (spr == null) continue;
                    SaveTexture(spr.texture, dir, spr.name);
                    totalSpr++;
                }
                // 2) Texture2D
                foreach (var tex in bundle.LoadAllAssets<Texture2D>())
                {
                    if (tex == null) continue;
                    SaveTexture(tex, dir, tex.name);
                    totalTex++;
                }
                // 3) TextAsset（配置/Spine 万一不在主文件）
                foreach (var ta in bundle.LoadAllAssets<TextAsset>())
                {
                    if (ta == null) continue;
                    File.WriteAllText(Path.Combine(dir, Sanitize(ta.name) + ".txt"), ta.text);
                }
                // 4) AudioClip
                foreach (var ac in bundle.LoadAllAssets<AudioClip>())
                {
                    if (ac == null) continue;
                    Debug.Log($"[Extract] AudioClip {ac.name} len={ac.length}s ch={ac.channels}");
                }
                bundle.Unload(false);
            }
            AssetDatabase.Refresh();
            Debug.Log($"[Extract] 完成: textures={totalTex} sprites={totalSpr} 输出到 {OutDir}");
        }

        private static void SaveTexture(Texture2D tex, string dir, string name)
        {
            if (tex == null) return;
            var safe = Sanitize(string.IsNullOrEmpty(name) ? "tex" : name);
            var png = tex.EncodeToPNG();
            if (png == null || png.Length == 0) return;
            File.WriteAllBytes(Path.Combine(dir, safe + ".png"), png);
        }

        private static string Sanitize(string s)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s;
        }
    }
}
