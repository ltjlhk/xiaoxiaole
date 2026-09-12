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

        // JsonUtility 不支持 List<List<int>> 等嵌套数组，这里提供从字段 JSON 里提取并解析的轻量工具。
        /// <summary>从对象 JSON 中提取指定键的值子串（如 "SkillUpGrade" → "[[0,1,1,50],...]"），无则返回 null。</summary>
        public static string ExtractField(string json, string key)
        {
            if (string.IsNullOrEmpty(json)) return null;
            int ki = json.IndexOf("\"" + key + "\"", System.StringComparison.Ordinal);
            if (ki < 0) return null;
            int p = ki + key.Length + 2; // 跳过闭合引号
            int n = json.Length;
            while (p < n && (json[p] == ' ' || json[p] == '\t' || json[p] == '\r' || json[p] == '\n')) p++;
            if (p >= n || json[p] != ':') return null;
            p++;
            while (p < n && (json[p] == ' ' || json[p] == '\t' || json[p] == '\r' || json[p] == '\n')) p++;
            if (p >= n) return null;
            if (json[p] != '[') return null;
            int depth = 0;
            int start = p;
            while (p < n)
            {
                if (json[p] == '[') depth++;
                else if (json[p] == ']')
                {
                    depth--;
                    if (depth == 0) { p++; break; }
                }
                p++;
            }
            return json.Substring(start, p - start);
        }

        /// <summary>解析嵌套整数数组 [[a,b,c],[d,e,f]] → List<List<int>>；空数组返回空 List（非 null）。</summary>
        public static List<List<int>> ParseNestedIntLists(string arr)
        {
            var result = new List<List<int>>();
            if (string.IsNullOrEmpty(arr) || arr.Length < 2) return result;
            int i = 0;
            int n = arr.Length;
            while (i < n)
            {
                if (arr[i] != '[') { i++; continue; }
                i++;
                var row = new List<int>();
                while (i < n && arr[i] != ']')
                {
                    if (arr[i] == '[' || arr[i] == ',') { i++; continue; }
                    int s = i;
                    while (i < n && arr[i] != ',' && arr[i] != ']') i++;
                    if (i > s)
                    {
                        int v;
                        if (int.TryParse(arr.Substring(s, i - s), out v)) row.Add(v);
                    }
                }
                if (i < n) i++; // 跳过 ']'
                result.Add(row);
            }
            return result;
        }
    }
}
