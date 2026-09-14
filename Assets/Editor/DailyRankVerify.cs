using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Xio.UI;

namespace Xio.EditorTools
{
    /// <summary>
    /// 每日榜验证：开 Demo 场景 → Push RankViewPanel → 断言
    /// 四个页签按钮齐全、每日滚动列表内 RankItemDaily 条目数 &lt; 1 要求、
    /// 切“每日”页签后 ScrollViewDaily 激活（其余隐藏）。输出 C:\xiaoxiaole\daily_rank.txt。
    /// 编辑模式跑（不进 Play），逻辑与 DiagShot 同源。
    /// </summary>
    public static class DailyRankVerify
    {
        private const string LogPath = @"C:\xiaoxiaole\daily_rank.txt";

        [MenuItem("Xio/诊断-每日榜验证")]
        public static void Run()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("daily rank verify " + DateTime.Now);
            try
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    EditorApplication.delayCall += Run;
                    return;
                }
                EditorSceneManager.OpenScene("Assets/Scenes/Demo.unity", OpenSceneMode.Single);

                var panel = PanelManager.Instance.Push<RankViewPanel>();
                panel.Open(PanelManager.Instance.CanvasRoot);
                panel.Refresh();

                sb.AppendLine("panel=" + (panel != null ? panel.GetType().Name : "NULL"));

                // 页签按钮：btnBgDaily / btnBgWorld / btnBgWeek / btnBgFriend
                foreach (var name in new[] { "btnBgDaily", "btnBgWorld", "btnBgWeek", "btnBgFriend" })
                {
                    var btn = FindChild(panel.Root, name);
                    sb.AppendLine("tab " + name + " = " + (btn != null ? "OK" : "MISSING"));
                }

                // 每日滚动列表内容
                var dailyScroll = FindChild(panel.Root, "ScrollViewDaily");
                sb.AppendLine("ScrollViewDaily active=" + (dailyScroll != null && dailyScroll.gameObject.activeSelf));
                var dailyContent = dailyScroll != null ? FindChild(dailyScroll, "Content") : null;
                int dailyItems = dailyContent != null ? dailyContent.childCount : 0;
                sb.AppendLine("ScrollViewDaily activeSelf=" + (dailyScroll != null && dailyScroll.gameObject.activeSelf)
                              + " items=" + dailyItems + " (expect 7)");
                if (dailyContent != null)
                    for (int i = 0; i < dailyContent.childCount; i++)
                        sb.AppendLine("  row" + i + "=" + dailyContent.GetChild(i).name);

                // 世界列表内容（入口默认页）
                var worldScroll = FindChild(panel.Root, "ScrollViewWorld");
                var worldContent = worldScroll != null ? FindChild(worldScroll, "Content") : null;
                sb.AppendLine("ScrollViewWorld active=" + (worldScroll != null && worldScroll.gameObject.activeSelf)
                              + " items=" + (worldContent != null ? worldContent.childCount : -1));

                // 找每日按钮模拟点击切换
                var btnDaily = FindChild(panel.Root, "btnBgDaily");
                if (btnDaily != null && btnDaily.GetComponent<UnityEngine.UI.Button>() != null)
                {
                    btnDaily.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                }
                sb.AppendLine("after click daily: scroll active="
                              + (dailyScroll != null && dailyScroll.gameObject.activeSelf)
                              + " world active=" + (worldScroll != null && worldScroll.gameObject.activeSelf));
            }
            catch (Exception e)
            {
                sb.AppendLine("EXCEPTION: " + e);
            }
            sb.AppendLine("done");
            File.WriteAllText(LogPath, sb.ToString());
            UnityEngine.Debug.Log("[DailyRankVerify]\n" + sb);
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var r = FindChild(root.GetChild(i), name);
                if (r != null) return r;
            }
            return null;
        }
    }
}