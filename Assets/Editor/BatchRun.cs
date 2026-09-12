using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Xio.Game;
using Xio.UI;

namespace Xio.EditorTools
{
    /// <summary>批处理入口：Unity -executeMethod Xio.EditorTools.BatchRun.Run</summary>
    public static class BatchRun
    {
        public static void Run()
        {
            string logFile = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "batch_result.txt");
            File.WriteAllText(logFile, "");
            try
            {
                RunInner(logFile);
            }
            catch (System.Exception e)
            {
                File.WriteAllText(logFile, "EXCEPTION:\n" + e);
                Debug.LogError("[BatchRun] " + e);
            }
        }

        static void RunInner(string logFile)
        {
            void CP(string tag) { try { File.AppendAllText(logFile, "[CP:" + tag + "]\n"); } catch { } }

            CP("begin");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cam.tag = "MainCamera";
            cam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            cam.GetComponent<Camera>().backgroundColor = new Color(0.1f, 0.16f, 0.22f);

            var report = "";

            // ===== M1: 关卡分段模型（数据驱动，勿硬编码段号）=====
            CP("m1");
            var all = LevelSegmentModel.All;
            var segs = LevelSegmentModel.Segments;
            var fairies = LevelSegmentModel.FairyList;
            int maxIdx = all.Count > 0 ? LevelSegmentModel.IndexOf(all[all.Count - 1].Id) : 0;
            report += $"== M1 LevelSegment ==\n" +
                      $"  total={all.Count} segments={segs.Count} fairies={fairies.Count} lastIdx={maxIdx}\n";
            if (all.Count == 0 || segs.Count == 0)
                report += "  FAIL: 分段模型为空（PuzzleConfig 解析失败？）\n";
            else
            {
                report += $"  段1: fId={segs[0][0].FairyId} {segs[0].Count}关 首={segs[0][0].Id}(No{segs[0][0].LevelNo}) " +
                          $"末={segs[0][segs[0].Count - 1].Id}(No{segs[0][segs[0].Count - 1].LevelNo})\n";
                // 顺序解锁：101 首关必开
                report += $"  解锁: 101={LevelSegmentModel.IsUnlocked(101)} passed={LevelSegmentModel.IsPassed(101)}\n";
                int nxt = LevelSegmentModel.NextLevelAfter(101);
                report += $"  nextAfter101={nxt}\n";
            }

            // ===== 音频冒烟 =====
            var bgm = Resources.Load<AudioClip>("Audio/puzzle_bgm");
            var sfxPick = SoundAssets.Get("puzzle_pickUp", "pick");
            var sfxMerge = SoundAssets.Get("puzzle_merge", "merge");
            var sfxLight = SoundAssets.Get("getsuipian");
            var sfxWin = SoundAssets.Get("puzzle_complete", "win");
            var sfxLose = SoundAssets.Get("lose");
            var sfxClean = SoundAssets.Get("clean");
            var flowerSnd = Resources.Load<AudioClip>("Audio/Item/MG_001");
            report += $"== Audio ==\n" +
                      $"  bgm={(bgm != null ? "OK" : "MISS")} pick={(sfxPick != null ? "OK" : "MISS")} " +
                      $"merge={(sfxMerge != null ? "OK" : "MISS")} light={(sfxLight != null ? "OK" : "MISS")} " +
                      $"win={(sfxWin != null ? "OK" : "MISS")} lose={(sfxLose != null ? "OK" : "MISS")} " +
                      $"clean={(sfxClean != null ? "OK" : "MISS")} flowerOwn={(flowerSnd != null ? "OK" : "MISS")}\n";

            // ===== 关卡号换算 =====
            var lvProbe = new PuzzleLevel { Id = 101 };
            bool levelNoOk = new PuzzleLevel { Id = 101 }.LevelNo == 1
                             && new PuzzleLevel { Id = 199 }.LevelNo == 99
                             && new PuzzleLevel { Id = 1100 }.LevelNo == 100
                             && new PuzzleLevel { Id = 501 }.LevelNo == 1
                             && lvProbe.LevelNo == 1;
            report += $"== LevelNo ==\n  101->1 199->99 1100->100 501->1 prop101={lvProbe.LevelNo} => {(levelNoOk ? "OK" : "FAIL")}\n";

            // ===== 玩法冒烟（DirectEnter 直达 101）=====
            CP("gameplay");
            var bootstrap = new GameObject("Bootstrap").AddComponent<GameBootstrap>();
            bootstrap.DirectEnter = true;
            bootstrap.StartLevel = 101;
            bootstrap.RunNow();

            CP("afterRunNow");
            var panels = Object.FindObjectsOfType<GamePanel>();
            var panel = panels.Length > 0 ? panels[0] : null;
            if (panel == null) report += "== Gameplay ==\n  FAIL: GamePanel not created\n";
            else
            {
                var g = panel.Game;
                report += $"== Gameplay 101 ==\n" +
                          $"  level={g.Level.Id} {g.Level.Rows}x{g.Level.Cols} state={g.State}\n" +
                          $"  lit={g.LitCells.Count(b => b)}/{g.LitCells.Length} pending={g.PendingCells.Count} " +
                          $"cards={g.Cards.Count} stacks={g.Stacks.Count}\n";

                int beforeLit = g.LitCells.Count(b => b);
                int pid = g.Cards[0].PatternId;
                CP("click3");
                foreach (var c in g.Cards.Where(c => c.PatternId == pid).Take(3).ToList())
                    g.ClickCard(c);
                int afterLit = g.LitCells.Count(b => b);
                report += $"  3-click match: lit {beforeLit}->{afterLit} slots={g.Slots.Count} score={g.Score} state={g.State}\n";

                CP("shuffle");
                g.UseTool(ToolType.Shuffle);
                report += $"  shuffle: cards={g.Cards.Count} stacks={g.Stacks.Count}\n";

                var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                report += "  eventSystem=" + (es != null ? "OK" : "MISSING") + "\n";

                // 全通关 → Win → 存档推进（带迭代护栏，防止意外忙循环）
                CP("force0");
                int guard = 0;
                while (g.State == GameState.Playing && g.Cards.Count > 0 && guard++ < 2000)
                {
                    bool clicked = false;
                    foreach (var grid in g.Stacks.Keys.ToList())
                    {
                        var top = g.TopCard(grid);
                        if (top != null) { g.ClickCard(top); clicked = true; break; }
                    }
                    if (!clicked) break;
                    if (g.State == GameState.Lose) break;
                }
                CP("force1");
                report += $"  force-play state={g.State} lit={g.LitCells.Count(b => b)}/{g.LitCells.Length} " +
                          $"loops={guard} {(guard >= 2000 ? "[GUARD-BREAK]" : "")}\n" +
                          $"  save: maxPassed={SaveManager.Data.maxPassedLevel} stars={SaveManager.Data.stars} " +
                          $"nextUnlocked(102)={LevelSegmentModel.IsUnlocked(102)}\n";
            }

            // ===== 主城冒烟 =====
            CP("maincity");
            PanelManager.Instance.Clear();
            bootstrap.DirectEnter = false;
            bootstrap.RunNow();
            report += $"== MainCity ==\n";
            if (PanelManager.Instance.Top is MainCityPanel)
                report += "  MainCityPanel = OK\n";
            else
                report += "  MainCityPanel = FAIL (top=" +
                          (PanelManager.Instance.Top != null ? PanelManager.Instance.Top.GetType().Name : "null") + ")\n";

            // ===== 关卡选择面板冒烟（切段 + 网格重建；编辑态 Destroy 走 SafeDestroy）
            PanelManager.Instance.Push<LevelSelectPanel>();
            report += $"== LevelSelect ==\n  panels={PanelManager.Instance.Count}\n";

            // ===== M2: 技能系统冒烟 =====
            CP("m2");
            int fairy1 = 1;
            var fSkills = FairySkillSystem.SkillsOf(fairy1);
            var fSkill = fSkills.Count > 0 ? fSkills[0] : null;
            int costL0 = FairySkillSystem.UpgradeGoldCost(fSkill, 0);
            int maxLv = FairySkillSystem.MaxLevel(fSkill);
            report += $"== M2 FairySkill ==\n" +
                      $"  fairy1 skills={fSkills.Count} skill={(fSkill != null ? fSkill.SkillName + "(每" + fSkill.TriggerParam1 + "秒)" : "null")} " +
                      $"costL0={costL0} maxLv={maxLv} lvNow={FairySkillSystem.LevelOf(fairy1)}\n";

            // 升级闭环：给足金币 → 升级 → 等级+1
            int beforeLv = FairySkillSystem.LevelOf(fairy1);
            if (costL0 > 0)
            {
                int needCoins = Mathf.Max(SaveManager.Data.coins, costL0 + 1);
                if (needCoins != SaveManager.Data.coins) SaveManager.AddCoins(needCoins - SaveManager.Data.coins);
                if (SaveManager.Data.coins >= costL0)
                {
                    SaveManager.AddCoins(-costL0);
                    SaveManager.SetFairySkillLevel(fairy1, FairySkillSystem.LevelOf(fairy1) + 1);
                }
            }
            report += $"  upgrade: lv {beforeLv} -> {FairySkillSystem.LevelOf(fairy1)} " +
                      $"coins={SaveManager.Data.coins}\n";

            // ===== M3: 任务追踪冒烟 =====
            CP("m3");
            TaskTracker.Init();
            int s1Tasks = TaskTracker.SeasonTasks(1).Count;
            var actRaw = Xio.Game.ConfigLoader.LoadRaw("SeasonActivityRewardConfig");
            report += $"== M3 Season ==\n" +
                      $"  season1Tasks={s1Tasks} activityRewardCfgLoaded={(string.IsNullOrEmpty(actRaw) ? 0 : 1)}\n";

            // ===== 贴图轮换 =====
            var t4170 = Xio.Assets.OriginalAssets.GetPuzzleTexture(4170);
            var tFallback = Xio.Assets.OriginalAssets.GetPuzzleTexture(109);
            report += $"== Tex ==\n  4170={(t4170 != null ? "direct" : "null")} " +
                      $"fallback109={(tFallback != null ? tFallback.width + "x" + tFallback.height : "null")}\n";

            File.AppendAllText(logFile, "[CP:write]\nREPORT:\n" + report);
            Debug.Log("[BatchRun]\n" + report);
            CP("done");
            PanelManager.Instance.Clear();
        }
    }
}