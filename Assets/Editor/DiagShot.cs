using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Xio.Game;

namespace Xio.EditorTools
{
    /// <summary>
    /// 玩法场景视觉诊断：Play Demo（直进 101 关）→ 第 120 帧截 Game 视图存
    /// C:\xiaoxiaole\diag_shot.png + 打印 3D 场景状态（方块数/材质/贴图/shader）。
    /// 用于定位"灰块"类渲染问题（读图定生死，不猜）。
    /// </summary>
    public static class DiagShot
    {
        private const string ShotPath = @"C:\xiaoxiaole\diag_shot.png";
        private const string LogPath = @"C:\xiaoxiaole\diag_shot.txt";
        private static int _waitFrames = 120;

        [MenuItem("Xio/诊断-截图玩法场景")]
        public static void Run()
        {
            var sb = new StringBuilder();
            try
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Demo.unity");
                EditorApplication.isPlaying = true;
                EditorApplication.update += WaitAndShot;
                sb.AppendLine("play started, waiting " + _waitFrames + " frames");
            }
            catch (Exception e)
            {
                sb.AppendLine("EXCEPTION: " + e);
                File.WriteAllText(LogPath, sb.ToString());
            }
            finally
            {
                if (sb.Length > 0) File.AppendAllText(LogPath, sb.ToString());
            }
        }

        private static void WaitAndShot()
        {
            if (!Application.isPlaying) { EditorApplication.update -= WaitAndShot; return; }
            if (Time.frameCount < _waitFrames) return;
            EditorApplication.update -= WaitAndShot;

            var sb = new StringBuilder();
            sb.AppendLine("=== diag " + DateTime.Now + " frame=" + Time.frameCount + " ===");

            // 相机状态
            var cam = Camera.main;
            if (cam != null)
                sb.AppendLine($"cam: pos={cam.transform.position} rot={cam.transform.eulerAngles} ortho={cam.orthographic} size={cam.orthographicSize} bg={cam.backgroundColor}");
            else
                sb.AppendLine("cam: NULL!");

            // Scene3D 状态
            if (Scene3D.Inst != null)
            {
                sb.AppendLine($"Scene3D OK: BlockParent={(Scene3D.Inst.BlockParent != null ? Scene3D.Inst.BlockParent.childCount.ToString() : "null")} " +
                              $"Droplocation={(Scene3D.Inst.Droplocation != null ? Scene3D.Inst.Droplocation.name : "null")}");
            }
            else sb.AppendLine("Scene3D: NULL!");

            // 全部 MeshRenderer + 材质 + 贴图状态（灰块定位核心）
            int nullMat = 0, noTex = 0, mr = 0;
            foreach (var r in UnityEngine.Object.FindObjectsOfType<MeshRenderer>())
            {
                mr++;
                var m = r.sharedMaterial;
                if (m == null || m.shader == null) { nullMat++; continue; }
                var t = m.mainTexture;
                if (r.name == "Face")
                {
                    if (t == null) noTex++;
                    else sb.AppendLine($"Face: mat={m.name} shader={m.shader.name} tex={(t != null ? t.name + " " + t.width + "x" + t.height : "null")}");
                }
            }
            sb.AppendLine($"MeshRenderers={mr} nullMat/shader={nullMat} faceNoTex={noTex}");
            var blk = UnityEngine.Object.FindObjectsOfType<Block3D>();
            sb.AppendLine($"Block3D count={blk.Length}");
            if (blk.Length > 0) sb.AppendLine($"sample: tex={blk[0].TexName} clickable={blk[0].Clickable} pos={blk[0].transform.position}");
            var gp = UnityEngine.Object.FindObjectOfType<GamePanel>();
            sb.AppendLine($"GamePanel={(gp != null ? "OK game=" + (gp.Game != null ? gp.Game.State.ToString() : "null") : "NULL")}");

            // Game 视图分辨率（CanvasScaler 匹配）
            var scaler = UnityEngine.Object.FindObjectOfType<UnityEngine.UI.CanvasScaler>();
            if (scaler != null) sb.AppendLine($"scaler: ref={scaler.referenceResolution} mode={scaler.uiScaleMode} match={scaler.matchWidthOrHeight}");
            sb.AppendLine($"Screen: {Screen.width}x{Screen.height}");

            // 截图（异步落盘，多等 10 帧确保写完）
            ScreenCapture.CaptureScreenshot(ShotPath);
            sb.AppendLine("shot requested -> " + ShotPath);
            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[DiagShot] " + sb.ToString().Replace("\n", " | "));

            EditorApplication.delayCall += () =>
            {
                // 等 60 帧让截图写盘再退出 Play
                _shotPending = 60;
                EditorApplication.update += ExitSoon;
            };
        }

        private static int _shotPending;

        private static void ExitSoon()
        {
            if (!Application.isPlaying) { EditorApplication.update -= ExitSoon; return; }
            if (--_shotPending > 0) return;
            EditorApplication.update -= ExitSoon;
            EditorApplication.isPlaying = false;
        }
    }
}
