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
    /// 玩法场景视觉诊断：Play Demo（直进关卡）→ 截 Game 视图存 C:\xiaoxiaole\diag_shot.png
    /// + 3D 场景状态 dump（相机/材质/贴图/shader/灯/色域/分辨率）到 diag_shot.txt。
    /// 自动模式：外部创建 C:\xiaoxiaole\diag_pending.txt 后，Unity 一重编译即自动执行（用户无需点菜单）。
    /// </summary>
    public static class DiagShot
    {
        private const string ShotPath = @"C:\xiaoxiaole\diag_shot.png";
        private const string LogPath = @"C:\xiaoxiaole\diag_shot.txt";
        private const string Marker = @"C:\xiaoxiaole\diag_pending.txt";
        private static int _waitFrames = 150;

        [MenuItem("Xio/诊断-截图玩法场景")]
        public static void Run()
        {
            File.Delete(Marker);   // 防循环
            try
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Demo.unity");
                var boot = UnityEngine.Object.FindObjectOfType<GameBootstrap>();
                if (boot != null)
                {
                    boot.StartLevel = 103;   // 精灵1段 第3关
                    boot.DirectEnter = true; // 跳过主城直进关卡
                }
                EditorApplication.isPlaying = true;
                EditorApplication.update += WaitAndShot;
                Debug.Log("[DiagShot] play started, wait " + _waitFrames + " frames");
            }
            catch (Exception e)
            {
                File.WriteAllText(LogPath, "EXCEPTION(Run): " + e);
            }
        }

        private static void WaitAndShot()
        {
            if (!Application.isPlaying) { EditorApplication.update -= WaitAndShot; return; }
            if (Time.frameCount < _waitFrames) return;
            EditorApplication.update -= WaitAndShot;

            var sb = new StringBuilder();
            sb.AppendLine("=== diag " + DateTime.Now + " frame=" + Time.frameCount + " ===");

            var cam = Camera.main;
            sb.AppendLine(cam != null
                ? $"cam: pos={cam.transform.position} rot={cam.transform.eulerAngles} ortho={cam.orthographic} size={cam.orthographicSize} bg={cam.backgroundColor} depth={cam.depth}"
                : "cam: NULL!");

            sb.AppendLine($"Scene3D: {(Scene3D.Inst != null ? "OK BlockParent子节点=" + (Scene3D.Inst.BlockParent != null ? Scene3D.Inst.BlockParent.childCount.ToString() : "null") : "NULL")}");

            // 灯与色域（灰块关键因子）
            sb.AppendLine($"lights={UnityEngine.Object.FindObjectsOfType<Light>().Length} ambient={RenderSettings.ambientLight} colorSpace={PlayerSettings.colorSpace}");

            int nullMat = 0, faceNoTex = 0, faceOK = 0, mr = 0;
            foreach (var r in UnityEngine.Object.FindObjectsOfType<MeshRenderer>())
            {
                mr++;
                var m = r.sharedMaterial;
                if (m == null || m.shader == null) { nullMat++; continue; }
                if (r.name == "Face")
                {
                    var t = m.mainTexture;
                    if (t == null) { faceNoTex++; sb.AppendLine($"Face NO-TEX mat={m.name} shader={m.shader.name}"); }
                    else { faceOK++; if (faceOK <= 2) sb.AppendLine($"Face: shader={m.shader.name} tex={t.name} {t.width}x{t.height} scale={m.mainTextureScale}"); }
                }
            }
            sb.AppendLine($"MeshRenderers={mr} nullMat={nullMat} faceOK={faceOK} faceNoTex={faceNoTex}");

            var blk = UnityEngine.Object.FindObjectsOfType<Block3D>();
            sb.AppendLine($"Block3D={blk.Length}");
            if (blk.Length > 0)
            {
                var b0 = blk[0];
                sb.AppendLine($"sample: tex={b0.TexName} pos={b0.transform.position} scale={b0.transform.localScale} clickable={b0.Clickable}");
                var face = b0.transform.Find("Face");
                if (face != null)
                {
                    var fm = face.GetComponent<MeshRenderer>().sharedMaterial;
                    sb.AppendLine($"sampleFace: shader={(fm != null ? fm.shader.name : "null")} tex={(fm != null && fm.mainTexture != null ? fm.mainTexture.name : "null")} active={face.gameObject.activeInHierarchy}");
                }
            }

            var gp = UnityEngine.Object.FindObjectOfType<GamePanel>();
            sb.AppendLine($"GamePanel={(gp != null ? "OK state=" + (gp.Game != null ? gp.Game.State.ToString() : "null") + " slots=" + (gp.Game != null ? gp.Game.Slots.Count.ToString() : "?") : "NULL")}");

            var scaler = UnityEngine.Object.FindObjectOfType<UnityEngine.UI.CanvasScaler>();
            if (scaler != null) sb.AppendLine($"scaler: ref={scaler.referenceResolution} mode={scaler.uiScaleMode} match={scaler.matchWidthOrHeight}");
            sb.AppendLine($"Screen: {Screen.width}x{Screen.height}");

            ScreenCapture.CaptureScreenshot(ShotPath);
            sb.AppendLine("shot -> " + ShotPath);
            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[DiagShot] " + sb.ToString().Replace("\n", " | "));

            _shotPending = 90;   // 等截图写盘
            EditorApplication.update += ExitSoon;
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

    /// <summary>外部触发：C:\xiaoxiaole\diag_pending.txt 存在时，Unity 编译后自动跑诊断。</summary>
    [InitializeOnLoad]
    public static class DiagAuto
    {
        static DiagAuto()
        {
            if (File.Exists(DiagShot_Marker()))
                EditorApplication.delayCall += DiagShot.Run;
        }
        private static string DiagShot_Marker() { return @"C:\xiaoxiaole\diag_pending.txt"; }
    }
}
