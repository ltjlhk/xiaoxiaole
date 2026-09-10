using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Xio.Game;

namespace Xio.EditorTools
{
    /// <summary>批处理入口：Unity -executeMethod Xio.EditorTools.BatchRun.Run</summary>
    public static class BatchRun
    {
        public static void Run()
        {
            string logFile = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "batch_result.txt");
            File.WriteAllText(logFile, "");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cam.tag = "MainCamera";
            cam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            cam.GetComponent<Camera>().backgroundColor = new Color(0.1f, 0.16f, 0.22f);

            var bootstrap = new GameObject("Bootstrap").AddComponent<GameBootstrap>();
            bootstrap.RunNow();

            string report = "";
            var panel = Object.FindObjectOfType<GamePanel>();
            if (panel == null) report += "FAIL: GamePanel not created\n";
            else
            {
                var g = panel.Game;
                report += $"OK: level={g.Level.Id} {g.Level.Rows}x{g.Level.Cols} state={g.State}\n";
                report += $"  lit={g.LitCells.Count(b => b)}/{g.LitCells.Length} pending={g.PendingCells.Count} cards={g.Cards.Count} stacks={g.Stacks.Count}\n";

                // 模拟：连点 3 张同图案卡（跨格收集）→ 消除 → 点亮
                int beforeLit = g.LitCells.Count(b => b);
                int pid = g.Cards[0].PatternId;
                foreach (var c in g.Cards.Where(c => c.PatternId == pid).Take(3).ToList())
                    g.ClickCard(c);
                int afterLit = g.LitCells.Count(b => b);
                report += $"  3-click match: lit {beforeLit}->{afterLit} slots={g.Slots.Count} score={g.Score} state={g.State}\n";

                // 道具：洗牌（叠堆重洗）
                g.UseTool(ToolType.Shuffle);
                report += $"  shuffle: cards={g.Cards.Count} stacks={g.Stacks.Count}\n";

                var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                report += "  eventSystem=" + (es != null ? "OK" : "MISSING") + "\n";

                // 全通关 → 下一关流转
                while (g.State == GameState.Playing && g.Cards.Count > 0)
                {
                    // 点堆顶直到全消
                    bool clicked = false;
                    foreach (var grid in g.Stacks.Keys.ToList())
                    {
                        var top = g.TopCard(grid);
                        if (top != null) { g.ClickCard(top); clicked = true; break; }
                    }
                    if (!clicked) break;
                    if (g.State == GameState.Lose) break;
                }
                report += $"  force-play state={g.State} lit={g.LitCells.Count(b => b)}/{g.LitCells.Length}\n";
            }

            // 音频：BGM mp3 + 原版 WAV 音效（bundle AAC 解码而来）+ 花牌专属消除音（ItemSoundv2）
            var bgm = Resources.Load<AudioClip>("Audio/puzzle_bgm");
            var sfxPick = Xio.Game.SoundAssets.Get("puzzle_pickUp", "pick");
            var sfxMerge = Xio.Game.SoundAssets.Get("puzzle_merge", "merge");
            var sfxLight = Xio.Game.SoundAssets.Get("getsuipian");
            var sfxWin = Xio.Game.SoundAssets.Get("puzzle_complete", "win");
            var sfxLose = Xio.Game.SoundAssets.Get("lose");
            var sfxClean = Xio.Game.SoundAssets.Get("clean");
            var flowerSnd = Resources.Load<AudioClip>("Audio/Item/MG_001"); // 花牌专属音（MG_001 有 mp3）
            report += $"  audio: bgm={(bgm != null ? "OK" : "MISS")} " +
                      $"pick={(sfxPick != null ? "OK" : "MISS")} merge={(sfxMerge != null ? "OK" : "MISS")} " +
                      $"light={(sfxLight != null ? "OK" : "MISS")} win={(sfxWin != null ? "OK" : "MISS")} " +
                      $"lose={(sfxLose != null ? "OK" : "MISS")} clean={(sfxClean != null ? "OK" : "MISS")} " +
                      $"flowerOwn={(flowerSnd != null ? "OK" : "MISS")}\n";

            // 关卡号换算：段内连续（101→1, 199→99, 1100→100, 501→1）
            System.Func<int, int> levelNo = id => id >= 1000 ? id % 1000 : id % 100;
            int ln101 = levelNo(101), ln199 = levelNo(199), ln1100 = levelNo(1100), ln501 = levelNo(501);
            var lvProbe = new PuzzleLevel { Id = 101 };
            bool levelNoOk = ln101 == 1 && ln199 == 99 && ln1100 == 100 && ln501 == 1
                             && lvProbe.LevelNo == 1;
            report += $"  levelNo: 101->{ln101} 199->{ln199} 1100->{ln1100} 501->{ln501} prop101={lvProbe.LevelNo} => {(levelNoOk ? "OK" : "FAIL")}\n";

            // 贴图轮换：4170 关缺图 → fallback
            var t4170 = Xio.Assets.OriginalAssets.GetPuzzleTexture(4170);
            var tFallback = Xio.Assets.OriginalAssets.GetPuzzleTexture(109); // (4170-101)%20=68 → 101+68%20=109
            report += $"  tex4170={(t4170 != null ? "direct" : "null")} fallbackExpect=109 got={(tFallback != null ? tFallback.width + "x" + tFallback.height : "null")}\n";

            // 下一关流转：4170 → 101（循环）
            bootstrap.StartLevel = 4170;
            bootstrap.RunNow();
            var pEnd = Object.FindObjectOfType<GamePanel>();
            report += "  level4170=" + (pEnd != null && pEnd.Game.Level.Id == 4170 ? "OK" : "FAIL") + "\n";

            File.WriteAllText(logFile, report);
            Debug.Log("[BatchRun]\n" + report);
        }
    }
}
