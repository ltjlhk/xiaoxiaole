using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>
    /// 图鉴面板（原版 PanelCollect）：顶部 Home_panel_01 + 场景/花语/装扮 三个 Tab（theme_Paging）。
    /// 每个 Tab 一个可滚动网格；解锁进度按全局最大通关序号线性映射，
    /// 数据驱动的解锁阶梯可以在 CollectRules 里替换成原版 reward 配置。
    /// </summary>
    public class CollectPanel : UIPanel
    {
        private const int TabMotif = 0;
        private const int TabPicture = 1;
        private const int TabSkin = 2;

        private readonly List<GameObject> _tabs = new List<GameObject>();
        private readonly List<Button> _tabButtons = new List<Button>();

        protected override void Build()
        {
            // 遮罩底（半透明黑，隔离下层主城）
            var mask = UIHelper.Image(Root, "basemap", null);
            UIHelper.Stretch((RectTransform)mask.transform);
            mask.color = new Color(0f, 0f, 0f, 0.55f);

            // 顶部横幅（原始 PanelCollect imgTop：顶贴边高 190）
            var top = UIHelper.NewRect(Root, "imgTop");
            top.anchorMin = new Vector2(0f, 1f);
            top.anchorMax = new Vector2(1f, 1f);
            top.pivot = new Vector2(0.5f, 1f);
            top.sizeDelta = new Vector2(0f, 190f);
            top.anchoredPosition = new Vector2(0f, 0f);
            var topImg = top.gameObject.AddComponent<Image>();
            var topTex = OriginalAssets.GetUi("Home_panel_01");
            if (topTex != null) { topImg.sprite = topTex; topImg.type = Image.Type.Sliced; topImg.color = Color.white; }

            var title = UIHelper.Text(Root, "Title", "图鉴", 38, new Color(1f, 0.98f, 0.85f), FontStyle.Bold);
            UIHelper.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(300, 60), new Vector2(0, -100));

            // 返回
            var back = UIHelper.Button(Root, "Back", () => PanelManager.Instance.Pop());
            UIHelper.Place((RectTransform)back.transform, new Vector2(0f, 1f), new Vector2(96, 56), new Vector2(70, -95));
            var backIcon = OriginalAssets.GetUi("btn_fanhui");
            if (backIcon != null) { var bi = back.GetComponent<Image>(); bi.sprite = backIcon; bi.color = Color.white; }

            // 三个 Tab（原版坐标：(0,-263) y 顶对齐，btnMotif(-243)/btnSkin(0)/btnPicture(243)）
            MakeTab(-243, TabMotif, "theme_icon_01", "场景");
            MakeTab(0, TabSkin, "matchingicon", "装扮");
            MakeTab(243, TabPicture, "theme_icon_02", "花语");

            // 内容网格
            MakeGrid(TabMotif, MotifItems());
            MakeGrid(TabPicture, PictureItems());
            MakeGrid(TabSkin, SkinItems());

            SelectTab(TabMotif);
        }

        private void MakeTab(float x, int index, string icon, string label)
        {
            var btn = UIHelper.Button(Root, "Tab_" + index, () => SelectTab(index));
            UIHelper.Place((RectTransform)btn.transform, new Vector2(0.5f, 1f), new Vector2(226, 77), new Vector2(x, -263));
            var img = btn.GetComponent<Image>();
            var sp = OriginalAssets.GetUi(index == 0 ? "theme_Paging_01" : "theme_Paging_02");
            if (sp != null) { img.sprite = sp; img.color = Color.white; }

            var ic = UIHelper.Image(btn.transform, "Icon", OriginalAssets.GetUi(icon));
            UIHelper.Place((RectTransform)ic.transform, new Vector2(0.5f, 0.5f), new Vector2(59, 43), new Vector2(-50, -3.5f));

            var t = UIHelper.Text(btn.transform, "Label", label, 26, new Color(0.45f, 0.25f, 0.08f), FontStyle.Bold);
            UIHelper.Place(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(120, 50), new Vector2(28.5f, -2f));
            var o = t.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(1f, 0.92f, 0.7f, 0.6f);
            o.effectDistance = new Vector2(1f, -1f);

            _tabButtons.Add(btn);
        }

        private struct CollectItem
        {
            public string sprite;
            public string name;
            public int unlockStep;   // 最大通关全局序号达到此值即解锁
        }

        /// <summary>场景：motif01..12（每通 12 关解锁一个场景主题）。</summary>
        private static List<CollectItem> MotifItems()
        {
            var list = new List<CollectItem>();
            for (int i = 1; i <= 12; i++)
                list.Add(new CollectItem { sprite = i < 10 ? "motif0" + i : "motif" + i, name = "场景 " + i, unlockStep = (i - 1) * 12 });
            return list;
        }

        /// <summary>花语：MG_001..012 花牌贴图（每通 20 关解锁一个花语）。</summary>
        private static List<CollectItem> PictureItems()
        {
            var list = new List<CollectItem>();
            for (int i = 1; i <= 12; i++)
                list.Add(new CollectItem { sprite = "MG_" + i.ToString("000"), name = "花语 " + i, unlockStep = (i - 1) * 20 });
            return list;
        }

        /// <summary>装扮：skin1..406 牌背皮肤（每通 24 关解锁一个皮肤）。</summary>
        private static List<CollectItem> SkinItems()
        {
            var list = new List<CollectItem>();
            var names = new[]
            {
                "skin1", "skin2", "skin3", "skin301", "skin4",
                "skin401", "skin402", "skin403", "skin404", "skin405", "skin406"
            };
            for (int i = 0; i < names.Length; i++)
                list.Add(new CollectItem { sprite = names[i], name = "装扮 " + (i + 1), unlockStep = i * 24 });
            return list;
        }

        private GameObject MakeGrid(int tab, List<CollectItem> items)
        {
            var scroll = UIHelper.NewRect(Root, "Scroll_" + tab);
            scroll.anchorMin = scroll.anchorMax = new Vector2(0.5f, 1f);
            scroll.pivot = new Vector2(0.5f, 1f);
            scroll.sizeDelta = new Vector2(690f, 940f);
            scroll.anchoredPosition = new Vector2(0f, -380f);
            var scrollImg = scroll.gameObject.AddComponent<Image>();
            var bg = OriginalAssets.GetUi("Background");
            if (bg != null) { scrollImg.sprite = bg; scrollImg.type = Image.Type.Sliced; scrollImg.color = Color.white; }
            var sr = scroll.gameObject.AddComponent<ScrollRect>();

            var viewport = UIHelper.NewRect(scroll, "Viewport");
            UIHelper.Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = UIHelper.NewRect(viewport, "Content");
            UIHelper.Stretch(content);
            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(148f, 158f);
            grid.spacing = new Vector2(16f, 16f);
            grid.padding = new RectOffset(16, 16, 16, 16);
            var fit = content.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.viewport = viewport;
            sr.content = content;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;

            foreach (var it in items) MakeCell(content, it);

            _tabs.Add(scroll.gameObject);
            return scroll.gameObject;
        }

        private void MakeCell(Transform parent, CollectItem it)
        {
            bool unlocked = SaveManager.Data.maxPassedLevel >= it.unlockStep;

            var cell = UIHelper.NewRect(parent, "Cell_" + it.name);
            var cellImg = cell.gameObject.AddComponent<Image>();
            var frame = OriginalAssets.GetUi("Bg_nei2");
            if (frame != null) { cellImg.sprite = frame; cellImg.type = Image.Type.Sliced; cellImg.color = Color.white; }

            var pic = UIHelper.Image(cell, "Pic", OriginalAssets.GetUi(it.sprite));
            UIHelper.Place((RectTransform)pic.transform, new Vector2(0.5f, 1f), new Vector2(120, 110), new Vector2(0, -62));
            pic.color = unlocked ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f);

            var nameT = UIHelper.Text(cell, "Name", it.name, 20, new Color(0.4f, 0.24f, 0.08f), FontStyle.Bold);
            UIHelper.Place(nameT.rectTransform, new Vector2(0.5f, 0f), new Vector2(140, 30), new Vector2(0, 18));

            var state = UIHelper.Text(cell, "State", unlocked ? "已解锁" : "未解锁", 17,
                unlocked ? new Color(0.2f, 0.7f, 0.25f) : new Color(0.55f, 0.55f, 0.55f));
            UIHelper.Place(state.rectTransform, new Vector2(0.5f, 0f), new Vector2(140, 26), new Vector2(0, -12));
        }

        private void SelectTab(int index)
        {
            for (int i = 0; i < _tabs.Count; i++)
                if (_tabs[i] != null) _tabs[i].SetActive(i == index);
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                if (_tabButtons[i] == null) continue;
                var img = _tabButtons[i].GetComponent<Image>();
                var sp = OriginalAssets.GetUi(i == index ? "theme_Paging_01" : "theme_Paging_02");
                if (sp != null) { img.sprite = sp; img.color = Color.white; }
            }
        }
    }
}