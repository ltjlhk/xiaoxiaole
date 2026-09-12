using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace Xio.Assets
{
    /// <summary>
    /// 原版 Spine 骨骼动画加载：从 Resources/Config/{name}.json + {name}.atlas.json 运行时构建 SkeletonDataAsset。
    /// 贴图来自 Resources/Original/ui/{page}.png（atlas page 名即骨架皮肤贴图名，与短名同名）。
    /// 懒加载 + 缓存，范式同 OriginalAssets。
    /// </summary>
    public static class SpineAssets
    {
        private const string ConfigDir = "Config/";
        private const string UiDir = "Original/ui/";

        private static readonly Dictionary<string, SkeletonDataAsset> _cache = new Dictionary<string, SkeletonDataAsset>();

        /// <summary>按短名取 SkeletonDataAsset（如 "jingling_1"/"hua"/"baozha"）。失败返回 null。</summary>
        public static SkeletonDataAsset Get(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (_cache.TryGetValue(name, out var cached) && cached != null) return cached;

            var jsonTa = Resources.Load<TextAsset>(ConfigDir + name);                       // jingling_1.json
            var atlasTa = Resources.Load<TextAsset>(ConfigDir + name + ".atlas.json")    // jingling_1.atlas.json
                        ?? Resources.Load<TextAsset>(ConfigDir + name + ".atlas");        // 兜底文本 atlas
            if (jsonTa == null || atlasTa == null)
            {
                Debug.LogWarning($"[SpineAssets] 缺配置: {name} json={(jsonTa != null)} atlas={(atlasTa != null)}");
                return null;
            }

            // atlas page 名（如 jingling_1.png → jingling_1）→ 从 ui 目录取同名牌贴图
            Texture2D tex = Resources.Load<Texture2D>(UiDir + name);
            var atlasAsset = SpineAtlasAsset.CreateRuntimeInstance(atlasTa,
                new[] { tex }, (Shader)null, false);
            if (tex == null)
            {
                Debug.LogWarning($"[SpineAssets] 缺贴图: ui/{name}");
                return null;
            }

            var sda = SkeletonDataAsset.CreateRuntimeInstance(jsonTa,
                new[] { atlasAsset }, true, 0.01f);
            _cache[name] = sda;
            return sda;
        }

        /// <summary>动画名列表（首帧冒烟用）。</summary>
        public static List<string> AnimationNames(SkeletonDataAsset sda)
        {
            var list = new List<string>();
            if (sda == null || sda.GetSkeletonData(false) == null) return list;
            var anims = sda.GetSkeletonData(false).Animations;
            if (anims != null)
                foreach (var a in anims)
                    if (a != null && !string.IsNullOrEmpty(a.Name)) list.Add(a.Name);
            return list;
        }
    }
}