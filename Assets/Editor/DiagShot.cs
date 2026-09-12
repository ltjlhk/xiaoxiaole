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
    /// 玩法场景视觉诊断（编辑模式，不进 Play——避免 domain reload 丢回调）：
    /// 开 Demo → 直进 103 关（RunNow）→ Camera.Render 到 RT(750×1334) → 存 PNG
    /// + dump 相机/材质/贴图/法线/灯 + 直接采样方块顶面像素 RGB（文本铁证）。
    /// 自动触发：存在 C:\xiaoxiaole\diag_pending.txt 时 Unity 编译完自动跑。
    /// </summary>
    public static class DiagShot
    {
        private const string ShotPath = @"C:\xiaoxiaole\diag_shot.png";
        private const string LogPath = @"C:\xiaoxiaole\diag_shot.txt";
        private const string Marker = @"C:\xiaoxiaole\diag_pending.txt";

        [MenuItem("Xio/诊断-截图玩法场景")]
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Run;
                return;
            }
            try
            {
                File.Delete(Marker);
                File.WriteAllText(LogPath, "diag(edit) start " + DateTime.Now + "\n");
                EditorSceneManager.OpenScene("Assets/Scenes/Demo.unity", OpenSceneMode.Single);

                var bootGo = new GameObject("DiagBoot", typeof(GameBootstrap));
                var boot = bootGo.GetComponent<GameBootstrap>();
                boot.StartLevel = 103;
                boot.DirectEnter = true;
                boot.RunNow();   // 编辑模式构建面板 + 3D 场景 + 牌堆

                // 批处理 -quit 模式不执行 delayCall → 必须同步截图
                Capture();
            }
            catch (Exception e)
            {
                File.AppendAllText(LogPath, "EXCEPTION(Run): " + e + "\n");
            }
        }

        private static void Capture()
        {
            try
            {
                // 2026-09-13 手动触发：对比原版复刻检查
                Canvas.ForceUpdateCanvases();
                var sb = new StringBuilder();
                var cam = Camera.main;
                sb.AppendLine("=== capture " + DateTime.Now + " ===");
                sb.AppendLine(cam == null
                    ? "cam: NULL"
                    : $"cam pos={cam.transform.position} rot={cam.transform.eulerAngles} ortho={cam.orthographic} size={cam.orthographicSize} bg={cam.backgroundColor}");
                sb.AppendLine($"lights={UnityEngine.Object.FindObjectsOfType<Light>().Length} colorSpace={PlayerSettings.colorSpace}");
                sb.AppendLine($"Scene3D={(Scene3D.Inst != null ? "OK" : "NULL")}");

                const int W = 750, H = 1334;
                var rt = new RenderTexture(W, H, 24);
                cam.targetTexture = rt;
                RenderTexture.active = rt;

                // ScreenSpaceOverlay 不经过 Camera.Render() → 截图无 UI。
                // 临时切到 Camera 模式（planeDistance 5，位于 3D 之上）连 UI 一起渲染，拍完恢复。
                var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
                var changed = new System.Collections.Generic.List<Canvas>();
                foreach (var cv in canvases)
                    if (cv.renderMode == RenderMode.ScreenSpaceOverlay && cv.isRootCanvas)
                    {
                        cv.renderMode = RenderMode.ScreenSpaceCamera;
                        cv.worldCamera = cam;
                        cv.planeDistance = 5f;
                        changed.Add(cv);
                    }
                Canvas.ForceUpdateCanvases();
                cam.Render();
                foreach (var cv in changed) cv.renderMode = RenderMode.ScreenSpaceOverlay;
                var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
                tex.Apply();
                File.WriteAllBytes(ShotPath, tex.EncodeToPNG());
                sb.AppendLine("png -> " + ShotPath);

                // 方块状态 + 顶面像素采样（法线可见性 + 颜色铁证）
                var blks = UnityEngine.Object.FindObjectsOfType<Block3D>();
                sb.AppendLine($"Block3D={blks.Length}");
                int n = 0;
                foreach (var b in blks)
                {
                    if (n >= 5) break;
                    var face = b.transform.Find("Face");
                    if (face == null) { sb.AppendLine($"blk[{n}] {b.TexName}: NO Face child"); n++; continue; }
                    var fm = face.GetComponent<MeshRenderer>().sharedMaterial;
                    var worldNormal = face.TransformDirection(Vector3.forward);
                    var sp = cam.WorldToScreenPoint(face.position + Vector3.up * 0.1f);
                    int px = Mathf.Clamp((int)sp.x, 0, W - 1), py = Mathf.Clamp((int)sp.y, 0, H - 1);
                    var c = tex.GetPixel(px, py);
                    sb.AppendLine($"blk[{n}] tex={b.TexName} pos={b.transform.position} faceNormal(world)={worldNormal} " +
                                  $"shader={(fm != null ? fm.shader.name : "null")} tex={(fm != null && fm.mainTexture != null ? fm.mainTexture.name + " " + fm.mainTexture.width + "x" + fm.mainTexture.height : "NULL")} " +
                                  $"pixel({px},{py})=RGBA({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})");
                    n++;
                }

                var gp = UnityEngine.Object.FindObjectOfType<GamePanel>();
                sb.AppendLine($"GamePanel={(gp != null ? "OK state=" + (gp.Game != null ? gp.Game.State.ToString() : "null") : "NULL")}");

                cam.targetTexture = null;
                RenderTexture.active = null;
                rt.Release();
                File.AppendAllText(LogPath, sb.ToString());
                Debug.Log("[DiagShot] done\n" + sb);
            }
            catch (Exception e)
            {
                File.AppendAllText(LogPath, "EXCEPTION(Capture): " + e + "\n");
            }
            finally
            {
                // 清理：重开场景丢弃运行时构建的对象（不保存）
                EditorSceneManager.OpenScene("Assets/Scenes/Demo.unity", OpenSceneMode.Single);
            }
        }
    }

    /// <summary>外部触发：标记文件存在 → 编译完成后自动跑诊断（编辑模式安全）。</summary>
    [InitializeOnLoad]
    public static class DiagAuto
    {
        static DiagAuto()
        {
            if (File.Exists(@"C:\xiaoxiaole\diag_pending.txt"))
                EditorApplication.delayCall += DiagShot.Run;
        }
    }
}
