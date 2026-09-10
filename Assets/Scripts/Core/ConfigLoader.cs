using System.Collections.Generic;
using UnityEngine;

namespace Xio.Game
{
    /// <summary>加载 Resources/Config 下的配置 JSON（Unity JsonUtility 不支持顶层数组，包装成对象字段）。</summary>
    public static class ConfigLoader
    {
        // JsonUtility 不支持 Dictionary，这里统一转 List 方案：配置表条目较少直接用逐条解析。
        // 简单的做法：ReadAllText + 手工分割。为保持骨架简洁，此处提供通用 TryGet。

        public static string LoadRaw(string fileNameWithoutExt)
        {
            var ta = Resources.Load<TextAsset>("Config/" + fileNameWithoutExt);
            return ta != null ? ta.text : null;
        }

        // 通用：按 {key":{...}} 顶层键分割，返回 Map<key, JsonObject>
        public static Dictionary<string, string> SplitTopLevel(string json)
        {
            var map = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(json)) return map;
            int i = 0;
            int n = json.Length;
            while (i < n)
            {
                // 找顶层 key
                while (i < n && (json[i] == ' ' || json[i] == '\t' || json[i] == '\r' || json[i] == '\n' || json[i] == '{' || json[i] == '}' || json[i] == ',')) i++;
                if (i >= n) break;
                if (json[i] != '"') { i++; continue; }
                int ks = i + 1;
                int ke = json.IndexOf('"', ks);
                if (ke < 0) break;
                string key = json.Substring(ks, ke - ks);
                // 跳过 : 和空白
                int p = ke + 1;
                while (p < n && (json[p] == ' ' || json[p] == '\t' || json[p] == '\r' || json[p] == '\n')) p++;
                if (p >= n || json[p] != ':') { i = p + 1; continue; }
                p++;
                while (p < n && (json[p] == ' ' || json[p] == '\t' || json[p] == '\r' || json[p] == '\n')) p++;
                if (p >= n || json[p] != '{') { i = p + 1; continue; }
                // 匹配对象括号（含嵌套）
                int depth = 0;
                int start = p;
                while (p < n)
                {
                    if (json[p] == '{') depth++;
                    else if (json[p] == '}')
                    {
                        depth--;
                        if (depth == 0) { p++; break; }
                    }
                    p++;
                }
                string obj = json.Substring(start, p - start);
                map[key] = obj;
                i = p;
            }
            return map;
        }

        public static T FromJson<T>(string json) => JsonUtility.FromJson<T>(json);
    }
}
