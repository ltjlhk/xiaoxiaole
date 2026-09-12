using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>关卡选择：按精灵 6 段 Tab，段内网格展示；已通带星、未解锁置灰。</summary>
    public class LevelSelectPanel : UIPanel
    {
        private int _tabIndex;
        private readonly List<Button> _tabButtons = new List<Button>();
        private Transform _gridRoot;
        private readonly List<GameObject> _cells = new List<GameObject>();

        protected override void Build()
        {
            // 背景
            var bgTex = Xio.Assets.OriginalAssets.GetBackground("bg10");
            var bg = UIHelper.Image(Root, "BG", bgTex != null
                ? Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f), 100f)
                : null);
            UIHelper.Stretch((RectTransform)bg.transform);

            MakeTopBar();
            MakeTabs();
            MakeGrid();
            SelectTab(SaveActiveTab());
        }

        private int SaveActiveTab()
        {
            // 默认定位到当前进度所在灵片段
            var segs = LevelSegmentModel.Segments;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Count == 0) continue;
                if (LevelSegmentModel.IsUnlocked(segs[i][0].Id)) { _tabIndex = i; return i; }
            }
            _tabIndex = 0;
            return 0;
        }

        private void MakeTopBar()
        {
            var title = UIHelper.Text(Root, "Title", "选择关卡", 40, new Color(1f, 0.98f, 0.85f), FontStyle.Bold);
            UIHelper.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(600, 60), new Vector2(0, -36));
            title.gameObject.AddComponent<Outline>().effectColor = new Color(0.12f, 0.22f, 0.45f);
            title.gameObject.GetComponent<Outline>().effectDistance = new Vector2(2, -2);

            var back = UIHelper.Button(Root, "Back", () => PanelManager.Instance.Pop());
            UIHelper.Place((RectTransform)back.transform, new Vector2(0, 1f), new Vector2(96, 56), new Vector2(70, -38));
            var backIcon = Xio.Assets.OriginalAssets.GetUi("btn_fanhui");
            if (backIcon != null)
            {
                var bimg = back.gameObject.GetComponent<Image>();
                bimg.sprite = backIcon;
                bimg.color = Color.white;
                bimg.SetNativeSize();
            }
            else
            {
                var label = UIHelper.Text(back.transform, "L", "返回", 26, Color.white, FontStyle.Bold);
                UIHelper.Stretch(label.rectTransform);
            }
        }

        private void MakeTabs()
        {
            var fairies = LevelSegmentModel.FairyList;
            var tabRoot = UIHelper.NewRect(Root, "Tabs");
            UIHelper.Place(tabRoot, new Vector2(0.5f, 1f), new Vector2(720, 108), new Vector2(0, -112));

            for (int i = 0; i < fairies.Count; i++)
            {
                int idx = i;
                var bt = UIHelper.Button(tabRoot, "Tab_" + i, () => SelectTab(idx));
                var brt = UIHelper.Place((RectTransform)bt.transform, new Vector2(0.5f, 0.5f),
                    new Vector2(110, 96), new Vector2(-(fairies.Count - 1) * 55 + i * 110, 0));
                var board = Xio.Assets.OriginalAssets.GetUi("mp_board");
                if (board != null)
                {
                    var bi = bt.gameObject.GetComponent<Image>();
                    bi.sprite = board;
                    bi.type = Image.Type.Sliced;
                    bi.color = new Color(1f, 1f, 1f, 1f);
                }

                // 精灵立绘
                var icon = Xio.Game.FairySpineMap.IconFor(fairies[i].id);
                if (icon != null)
                {
                    var ic = UIHelper.Image(bt.transform, "I", icon);
                    UIHelper.Place((RectTransform)ic.transform, new Vector2(0.5f, 1f), new Vector2(64, 64), new Vector2(0, -10));
                }
                // 名称（无 icon 时兜底文字）
                UIHelper.Text(bt.transform, "N",
                    icon == null ? (fairies[i].FairyName ?? "精灵") : "", 18, new Color(0.45f, 0.25f, 0.08f), FontStyle.Bold);

                _tabButtons.Add(bt);
            }
        }

        private void MakeGrid()
        {
            var scGo = UIHelper.NewRect(Root, "Scroll");
            var sc = scGo.gameObject.AddComponent<ScrollRect>();
            UIHelper.Place(scGo, new Vector2(0.5f, 0.5f), new Vector2(720, 1080), new Vector2(0, -60));

            // viewport：裁剪网格
            var vp = UIHelper.NewRect(scGo, "Viewport");
            var vrt = UIHelper.Stretch(vp);
            var mask = vp.gameObject.AddComponent<RectMask2D>();
            mask.enabled = true;

            var content = UIHelper.NewRect(vp, "Content");
            var crt = (RectTransform)content;
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.anchoredPosition = Vector2.zero;

            sc.viewport = vrt;
            sc.content = crt;
            sc.horizontal = false;
            sc.vertical = true;
            sc.movementType = ScrollRect.MovementType.Clamped;
            sc.scrollSensitivity = 24f;
            sc.verticalNormalizedPosition = 1f;

            _gridRoot = crt;
        }

        private void SelectTab(int idx)
        {
            var segs = LevelSegmentModel.Segments;
            if (idx < 0 || idx >= segs.Count) return;
            _tabIndex = idx;

            for (int i = 0; i < _tabButtons.Count; i++)
            {
                if (_tabButtons[i] == null) continue;
                var img = _tabButtons[i].gameObject.GetComponent<Image>();
                if (img != null)
                    img.color = i == idx ? new Color(1f, 0.92f, 0.55f) : new Color(0.75f, 0.72f, 0.68f);
            }

            RebuildGrid(segs[idx]);
        }

        private void RebuildGrid(List<PuzzleLevel> seg)
        {
            for (int i = _cells.Count - 1; i >= 0; i--)
                if (_cells[i] != null) SafeDestroy(_cells[i]);
            _cells.Clear();

            // 6 列网格
            int cols = 6;
            float cw = 104, gap = 12;
            int rows = Mathf.CeilToInt(seg.Count / (float)cols);
            float contentH = rows * (cw + gap) + gap;
            var crt = (RectTransform)_gridRoot;
            crt.sizeDelta = new Vector2(cw * cols + gap * (cols + 1), contentH);

            for (int i = 0; i < seg.Count; i++)
            {
                var lv = seg[i];
                int r = i / cols, c = i % cols;
                var lt = UIHelper.NewRect(_gridRoot, "Lv_" + lv.Id);
                lt.anchorMin = lt.anchorMax = new Vector2(0.5f, 1f);
                lt.pivot = new Vector2(0.5f, 0.5f);
                lt.sizeDelta = new Vector2(cw, cw * 1.3f);
                lt.anchoredPosition = new Vector2((c - cols / 2 + 0.5f) * (cw + gap), -gap - r * (cw + gap) - lt.sizeDelta.y / 2f);

                var lvId = lv.Id;
                var btn = LevelButton.Create(lt, "B", lv, () =>
                {
                    if (!LevelSegmentModel.IsUnlocked(lvId)) return;
                    var gp = PanelManager.Instance.Push<GameplayPanel>();
                    gp.StartLevel(lvId);
                }, lt.sizeDelta);
                _cells.Add(btn.gameObject);
            }
        }

        public override void Refresh()
        {
            // 从关卡返回：刷新当前段的解锁/通关态
            var segs = LevelSegmentModel.Segments;
            if (_tabIndex < segs.Count) RebuildGrid(segs[_tabIndex]);
        }

        /// <summary>编辑态（批处理）用 DestroyImmediate。</summary>
        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}