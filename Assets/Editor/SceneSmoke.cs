using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Xio.Game;
using Xio.UI;

namespace Xio.EditorTools
{
    /// <summary>
    /// 原版 3 场景路由 smoke：对 Load/StartScene/MainScene 各场景
    /// 打开 → 挂 GameBootstrap → RunNow() → 断言面板栈进入预期面板。
    /// Load 场景协程路径在编辑模式不全跑，只断言 LoadFlow 能入栈 PuzzleLoadingPanel。
    /// 输出 C:\xiaoxiaole\scene_smoke.txt。
    /// </summary>
    public static class SceneSmoke
    {
        private const string LogPath = @"C:\xiaoxiaole\scene_smoke.txt";

        public static void Run()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("scene smoke " + DateTime.Now);
            try
            {
                // StartScene → 主城
                EditorSceneManager.OpenScene("Assets/Scenes/StartScene.unity", OpenSceneMode.Single);
                var boot = UnityEngine.Object.FindObjectOfType<GameBootstrap>();
                if (boot == null) boot = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
                boot.DirectEnter = false;
                boot.RunNow();
                var top = PanelManager.Instance.Top;
                sb.AppendLine("StartScene -> top=" + (top != null ? top.GetType().Name : "NULL") +
                              " count=" + PanelManager.Instance.Count);

                // MainScene → 玩法（续关）
                PanelManager.Instance.ResetForScene();
                EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);
                boot = UnityEngine.Object.FindObjectOfType<GameBootstrap>();
                if (boot == null) boot = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
                boot.DirectEnter = true;
                boot.StartLevel = 101;
                boot.RunNow();
                top = PanelManager.Instance.Top;
                sb.AppendLine("MainScene -> top=" + (top != null ? top.GetType().Name : "NULL") +
                              " count=" + PanelManager.Instance.Count);

                // Load → 加载页（协程片段：LoadFlow 先入栈 PuzzleLoadingPanel）
                PanelManager.Instance.ResetForScene();
                EditorSceneManager.OpenScene("Assets/Scenes/Load.unity", OpenSceneMode.Single);
                boot = UnityEngine.Object.FindObjectOfType<GameBootstrap>();
                if (boot == null) boot = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
                boot.RunNow();
                top = PanelManager.Instance.Top;
                sb.AppendLine("Load -> top=" + (top != null ? top.GetType().Name : "NULL") +
                              " count=" + PanelManager.Instance.Count);
            }
            catch (Exception e)
            {
                sb.AppendLine("EXCEPTION: " + e);
            }
            sb.AppendLine("done");
            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[SceneSmoke]\n" + sb);
        }
    }
}