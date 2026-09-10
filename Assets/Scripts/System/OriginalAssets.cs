using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Xio.Assets
{
    /// <summary>从 Resources/Original 加载原版导出精灵（Python 工具批量导出，文件名带 path_id 后缀）。</summary>
    public static class OriginalAssets
    {
        public const string ResDir = "Original";

        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, string> _nameMap = new Dictionary<string, string>();
        private static bool _mapBuilt;

        private static void EnsureNameMap()
        {
            if (_mapBuilt) return;
            _mapBuilt = true;
            BuildNameMap("puzzle");
            BuildNameMap("bg");
            BuildNameMap("fairybg");
            BuildNameMap("flower");
            BuildNameMap("ui");
        }

        private static void BuildNameMap(string category)
        {
            var dir = Path.Combine(Application.dataPath, "Resources", ResDir, category);
            if (!Directory.Exists(dir)) return;
            foreach (var f in Directory.GetFiles(dir, "*.png"))
            {
                var fn = Path.GetFileNameWithoutExtension(f); // e.g. "101__730..."
                var sep = fn.IndexOf("__");
                var shortName = sep < 0 ? fn : fn.Substring(0, sep);
                if (!_nameMap.ContainsKey(category + "/" + shortName))
                    _nameMap[category + "/" + shortName] = fn;
            }
        }

        /// <summary>加载 Resources/Original/{category}/{name} 精灵，自动裁成方形 sprite。</summary>
        public static Sprite Get(string category, string name)
        {
            EnsureNameMap();
            string key = category + "/" + name;
            if (_cache.TryGetValue(key, out var s) && s != null) return s;
            string actualName = _nameMap.TryGetValue(key, out var mapped) ? mapped : name;
            var tex = Resources.Load<Texture2D>(ResDir + "/" + category + "/" + actualName);
            if (tex == null)
            {
                Debug.LogWarning("[OriginalAssets] missing: " + key + " (tried " + actualName + ")");
                return null;
            }
            s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = s;
            return s;
        }

        /// <summary>加载拼图关卡原图。</summary>
        public static Texture2D GetPuzzleTexture(int imageId)
        {
            EnsureNameMap();
            string key = "puzzle/" + imageId;
            string actual = _nameMap.TryGetValue(key, out var mapped) ? mapped : imageId.ToString();
            return Resources.Load<Texture2D>(ResDir + "/puzzle/" + actual);
        }

        /// <summary>加载主数据包导出的 UI 图（title/card_back/topbase/gold/mp_board/shufflesmall 等）。</summary>
        public static Sprite GetUi(string name)
        {
            return Get("ui", name);
        }

        /// <summary>加载背景（bg01..bg12）。</summary>
        public static Texture2D GetBackground(string name)
        {
            EnsureNameMap();
            string bgKey = "bg/" + name;
            string actual = _nameMap.TryGetValue(bgKey, out var mapped) ? mapped : name;
            var t = Resources.Load<Texture2D>(ResDir + "/bg/" + actual);
            if (t != null) return t;
            string fKey = "fairybg/" + name;
            actual = _nameMap.TryGetValue(fKey, out mapped) ? mapped : name;
            return Resources.Load<Texture2D>(ResDir + "/fairybg/" + actual);
        }

        /// <summary>把非可读贴图转换成可读 RGBA32 贴图。</summary>
        public static Texture2D ToReadable(Texture2D src)
        {
            if (src == null) return null;
            if (src.isReadable) return src;
            var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
            var prev = RenderTexture.active;
            Graphics.Blit(src, rt);
            RenderTexture.active = rt;
            var copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
            copy.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return copy;
        }

        public static string[] ListAllFlowerIds()
        {
            var dir = Path.Combine(Application.dataPath, "Resources/Original/flower");
            if (!Directory.Exists(dir)) return new string[0];
            var files = Directory.GetFiles(dir, "*.png");
            var set = new HashSet<string>();
            foreach (var f in files)
            {
                var fn = Path.GetFileNameWithoutExtension(f);
                var sep = fn.IndexOf("__");
                var shortName = sep < 0 ? fn : fn.Substring(0, sep);
                set.Add(shortName);
            }
            var arr = new List<string>(set);
            arr.Sort();
            return arr.ToArray();
        }
    }
}
