using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Xio.EditorTools
{
    /// <summary>一键生成可玩场景：Camera + GameBootstrap（加载原版资源跑 101 关）。</summary>
    public static class BuildDemoScene
    {
        [MenuItem("Xio/生成游戏场景(101关)")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var boot = new GameObject("GameBootstrap", typeof(Xio.Game.GameBootstrap));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Demo.unity");
            AddToBuildSettings("Assets/Scenes/Demo.unity");
            Debug.Log("[XioDemo] 场景已生成并保存到 Assets/Scenes/Demo.unity，可打开 Play 运行。");
        }

        public static void AddToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
                if (s.path == path) return;
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
            {
                new EditorBuildSettingsScene(path, true)
            };
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log("[XioDemo] Demo 已加入 Build Settings");
        }
    }
}
