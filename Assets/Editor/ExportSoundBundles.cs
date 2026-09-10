using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Xio.EditorTools
{
    /// <summary>导出原版 sound bundle 的 AudioClip 到 Resources/Audio/Bundle，并列出名字清单。</summary>
    public static class ExportSoundBundles
    {
        private static readonly string[] Bundles =
        {
            "sound_assets_sound_short_d573664d9633fd4bf9431ae43741c70a.bundle",
            "sound_jl_assets_sound_jl_ec44be2aac42446182510be4567af8d0.bundle",
        };

        [MenuItem("Xio/导出原版游戏音效")]
        public static void ExportAll()
        {
            string outDir = Path.Combine(Application.dataPath, "Resources/Audio/Bundle");
            Directory.CreateDirectory(outDir);
            var sb = new StringBuilder();

            foreach (var b in Bundles)
            {
                string path = Path.Combine(Application.streamingAssetsPath, b);
                if (!File.Exists(path)) { Debug.LogWarning("[声音] 缺 " + b); continue; }
                var ab = AssetBundle.LoadFromFile(path);
                if (ab == null) { Debug.LogWarning("[声音] 加载失败 " + b); continue; }
                foreach (var clip in ab.LoadAllAssets<AudioClip>())
                {
                    if (clip == null) continue;
                    sb.AppendLine($"{Path.GetFileNameWithoutExtension(b)}\t{clip.name}\t{clip.frequency}Hz\t{clip.length:F2}s\tch={clip.channels}");
                    string file = Path.Combine(outDir, Sanitize(clip.name) + ".wav");
                    SavWav(file, clip);
                }
                ab.Unload(false);
            }
            File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "sound_list.txt"), sb.ToString());
            AssetDatabase.Refresh();
            Debug.Log("[声音] 导出完成，清单见 sound_list.txt");
        }

        private static string Sanitize(string n)
        {
            var s = n.Replace("assets/sound/", "").Replace(".asset", "").Replace("/", "_");
            foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }

        // ===== WAV 导出（AudioClip 非 readable 也能跑：编辑器下 GetData 可用）=====
        private static void SavWav(string path, AudioClip clip)
        {
            const int headerSize = 44;
            var samples = new float[clip.samples * clip.channels];
            if (!clip.GetData(samples, 0))
            {
                Debug.LogWarning("[声音] GetData 失败: " + clip.name);
                return;
            }
            var freq = clip.frequency;
            var chans = clip.channels;
            using var fs = new FileStream(path, FileMode.Create);
            using var bw = new BinaryWriter(fs);
            int dataSize = samples.Length * 2;
            byte[] ascii(string s) => Encoding.ASCII.GetBytes(s);
            bw.Write(ascii("RIFF")); bw.Write(headerSize + dataSize); bw.Write(ascii("WAVE"));
            bw.Write(ascii("fmt ")); bw.Write(16); bw.Write((short)1); bw.Write((short)chans);
            bw.Write(freq); bw.Write(freq * chans * 2); bw.Write((short)(chans * 2)); bw.Write((short)16);
            bw.Write(ascii("data")); bw.Write(dataSize);
            foreach (var s in samples)
            {
                short v = (short)Mathf.Clamp(Mathf.RoundToInt(s * 32767f), short.MinValue, short.MaxValue);
                bw.Write(v);
            }
        }
    }
}
