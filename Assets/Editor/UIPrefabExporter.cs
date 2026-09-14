using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Xio.UI;

namespace Xio.EditorTools
{
    /// <summary>
    /// 导出全部 UIPanel 子类为真实 .prefab 资产（Assets/Prefabs/UI/{类名}.prefab）。
    /// 对齐原版 Addressables Prefabs/UI/* 目录结构，解决"工程里看不到预制体"的观感问题。
    /// 面板由代码动态构建（PanelManager.Push 语义等价），导出后可在 Project 窗口预览层级/组件。
    /// </summary>
    public static class UIPrefabExporter
    {
        private const string OutDir = "Assets/Prefabs/UI";

        public static void Run()
        {
            if (!Directory.Exists(OutDir)) Directory.CreateDirectory(OutDir);

            // 隔离：批处理先切空场景，避免 Demo 里已有对象干扰
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 运行时依赖：要有一个 Canvas（PanelManager.CanvasRoot 查找场景中已有 Canvas）
            var cv = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (cv == null)
            {
                var go = new GameObject("BootCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
                go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            var root = (RectTransform)UnityEngine.Object.FindObjectOfType<Canvas>().transform;

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.Contains("Assembly-CSharp"))
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(UIPanel).IsAssignableFrom(t))
                .OrderBy(t => t.Name);

            int ok = 0, fail = 0;
            foreach (var t in types)
            {
                try
                {
                    var panel = (UIPanel)Activator.CreateInstance(t);
                    panel.Open(root);
                    panel.Refresh();
                    string path = OutDir + "/" + t.Name + ".prefab";
                    PrefabUtility.SaveAsPrefabAsset(panel.Root.gameObject, path);
                    panel.Close();
                    ok++;
                    Debug.Log("[UIPrefabExporter] " + t.Name + " -> " + path);
                }
                catch (Exception e)
                {
                    fail++;
                    Debug.LogWarning("[UIPrefabExporter] FAIL " + t.Name + ": " + e.Message);
                }
            }
            Debug.Log($"[UIPrefabExporter] done ui={ok} fail={fail} -> {OutDir}");
            AssetDatabase.Refresh();
        }
    }
}