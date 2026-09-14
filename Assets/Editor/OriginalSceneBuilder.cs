using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Xio.Game;

namespace Xio.EditorTools
{
    /// <summary>
    /// 原版场景结构重建：Load / StartScene / MainScene（对齐原版 BuildSettings:
    /// Assets/Scenes/Load.scene → level0, StartScene.scene → level1, MainScene.unity → level2）。
    /// 每个场景挂 GameBootstrap，运行时按场景名自动路由（Load→加载页→StartScene；MainScene→续关玩法）。
    /// 同时把 3 场景写入 EditorBuildSettings。
    /// </summary>
    public static class OriginalSceneBuilder
    {
        public static void Run()
        {
            string dir = "Assets/Scenes";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string[] scenes = { "Load", "StartScene", "MainScene" };
            foreach (var name in scenes)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var go = new GameObject("GameBootstrap");
                go.AddComponent<GameBootstrap>();
                if (name == "MainScene")
                {
                    // 玩法场景：主城可能也有，这里由 GameBootstrap 按场景名续关直接进玩法
                }
                string path = dir + "/" + name + ".unity";
                EditorSceneManager.SaveScene(scene, path);
                Debug.Log("[SceneBuilder] created " + path);
            }

            AddToBuildSettings(dir, scenes);
            Debug.Log("[SceneBuilder] build settings updated.");
        }

        private static void AddToBuildSettings(string dir, string[] scenes)
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var name in scenes)
                list.Add(new EditorBuildSettingsScene(dir + "/" + name + ".unity", true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}