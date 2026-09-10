using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Xio.Game;

namespace Xio.EditorTools
{
    /// <summary>
    /// 一键验证：生成 Demo 场景 + 完整跑一遍引导链路（配置→原版bundle贴图→可读转换→切碎片→开局）。
    /// 结果写入 C:\xiaoxiaole\verify_result.txt（避免 -quit 丢日志），并显式退出码。
    /// </summary>
    public static class VerifyGame
    {
        private const string ResultPath = @"C:\xiaoxiaole\verify_result.txt";

        [MenuItem("Xio/验证-游戏可运行(101关)")]
        public static void Run()
        {
            var steps = new System.Text.StringBuilder();
            void Step(string s) { steps.AppendLine(s); Debug.Log("[Verify] " + s); File.AppendAllText(ResultPath, s + Environment.NewLine); }
            try
            {
                File.WriteAllText(ResultPath, "start " + DateTime.Now + Environment.NewLine);

                // 0) 先把导出的原版 PNG 设为可读（零转换）
                MakeOriginalTexReadable.Run();
                Step("png readable ok");

                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var boot = new GameObject("GameBootstrap", typeof(GameBootstrap)).GetComponent<GameBootstrap>();
                boot.StartLevel = 101;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, "Assets/Scenes/Demo.unity");
                BuildDemoScene.AddToBuildSettings("Assets/Scenes/Demo.unity");
                Step("scene ok");

                // 无 Play 模式直接跑完整启动链路
                boot.RunNow();
                Step("runnow ok");

                var panel = boot.GetComponent<GamePanel>();
                bool ok = panel != null && panel.Game != null && panel.Game.Level != null && panel.Game.State == GameState.Playing;
                if (!ok)
                {
                    Step("FAIL: GamePanel 未初始化或状态异常");
                    EditorApplication.Exit(1);
                    return;
                }
                Step($"关卡 {panel.Game.Level.Id} 棋盘 {panel.Game.Level.Rows}x{panel.Game.Level.Cols} 状态={panel.Game.State} 分数={panel.Game.Score}");
                Step($"UI 元素 {UnityEngine.Object.FindObjectsOfType<Image>().Length} 个");
                Step("PASS：场景+配置+原版贴图+碎片棋盘全链路可用");
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Step("EXCEPTION: " + e);
                EditorApplication.Exit(2);
            }
        }
    }
}
