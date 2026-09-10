using System.IO;
using UnityEditor;
using UnityEngine;

namespace Xio.EditorTools
{
    /// <summary>在编辑器中运行完整启动链路（不开批处理也能验证）。</summary>
    public static class RunBootstrap
    {
        [MenuItem("Xio/运行-游戏启动链路(101关)")]
        public static void Run()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            var go = GameObject.Find("Bootstrap") ?? new GameObject("Bootstrap");
            go.AddComponent<Xio.Game.GameBootstrap>();
            var bs = go.GetComponent<Xio.Game.GameBootstrap>();
            bs.RunNow();
            Debug.Log("[Xio] 启动链路运行完成 场景=" + scene.name);
        }
    }
}
