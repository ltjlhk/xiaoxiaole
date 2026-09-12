using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>拼图相册（1:1 复刻 res_PuzzleAlbum_10189 + res_PuzzleAlbumItem_12071）。
    /// 780 关图鉴网格，20 关一页 + 翻页按钮；缩略图走 OriginalAssets.GetPuzzleTexture；
    /// 通关态取 SaveManager.Data.maxPassedLevel。</summary>
    public sealed class PuzzleAlbumPanel : UIPanel
    {
        public int FairyId = 0;             // 指定精灵时标题显示精灵名

        private const int PerPage = 20;
        private const int TotalLevels = 780;

        private readonly List<GameObject> _pageItems = new List<GameObject>();
        private RectTransform _content;
        private Text _completeTxt;
        private Text _pageTxt;
        private Button _prevBtn;
        private Button _nextBtn;
        private int _page;

        protected override void Build()
        {
            Root.gameObject.AddComponent<Image>();   // 原版根节点带 Image

            // Panel（dump sizeDelta=(0,-82.2)；x 向 -755 为滑入动画偏移，不复刻）
            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);
            panel.sizeDelta = new Vector2(0f, -82.2f);

            // Panel1 stretch @(0,-40) sizeDelta=(0,-0.2)，底图 bg
            var p1 = Node(panel, "Panel1", V(0f, 0f), V(1f, 1f),
                new Vector2(0f, -40f), new Vector2(0f, -0.2f));
            var p1Img = p1.gameObject.AddComponent<Image>();
            var bgSp = OriginalAssets.GetUi("bg");
            if (bgSp != null) p1Img.sprite = bgSp;
            else p1Img.color = new Color(1f, 1f, 1f, 0.25f);
            var p1T = p1;

            // Title：HorizontalLayoutGroup + ContentSizeFitter
            var title = Node(p1T, "Title", V(0.5f, 1f), V(0.5f, 1f),
                new Vector2(0f, -65f), new Vector2(0f, 64f));
            var hlg = title.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            var csf = title.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var pat1 = Node(title, "Image (1)", V(0.5f, 0.5f), V(0.5f, 0.5f), Vector2.zero, new Vector2(154f, 32f));
            var pat1Img = pat1.gameObject.AddComponent<Image>();
            var patSp = OriginalAssets.GetUi("pattern");
            if (patSp != null) pat1Img.sprite = patSp;
            var le1 = pat1.gameObject.AddComponent<LayoutElement>();
            le1.preferredWidth = 154f; le1.preferredHeight = 32f;

            var titleTxt = Txt(title, "TitleTxt", TitleText(), V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(238f, 64f));
            var leT = titleTxt.gameObject.AddComponent<LayoutElement>();
            leT.preferredWidth = 238f; leT.preferredHeight = 64f;
            _completeTxt = Txt(titleTxt.transform, "CompleteTxt", "已完成：0/" + TotalLevels,
                V(0.5f, 1f), V(0.5f, 1f), new Vector2(0f, -67.1f), new Vector2(341.6f, 64f));

            var pat2 = Node(title, "Image (2)", V(0.5f, 0.5f), V(0.5f, 0.5f), Vector2.zero, new Vector2(154f, 32f));
            var pat2Img = pat2.gameObject.AddComponent<Image>();
            if (patSp != null) pat2Img.sprite = patSp;
            pat2.localScale = new Vector3(-1f, 1f, 1f);
            var le2 = pat2.gameObject.AddComponent<LayoutElement>();
            le2.preferredWidth = 154f; le2.preferredHeight = 32f;

            // CloseBtn 73×73 @(54.2,-82.5)
            ImgBtn(p1T, "CloseBtn", "collection_icon_01", V(0f, 1f), V(0f, 1f),
                new Vector2(54.2f, -82.5f), new Vector2(73f, 73f), () => PanelManager.Instance.Pop());

            // Scroll View + Viewport + Content(Grid) + Scrollbar
            var scrollView = Node(p1T, "Scroll View", V(0f, 0f), V(1f, 1f),
                new Vector2(3.1f, -66.8f), new Vector2(-29.2f, -133.7f));
            var svImg = scrollView.gameObject.AddComponent<Image>();
            var svSp = OriginalAssets.GetUi("Background");
            if (svSp != null) svImg.sprite = svSp;
            else svImg.color = new Color(1f, 1f, 1f, 0.25f);

            var viewport = Node(scrollView, "Viewport", V(0f, 0f), V(1f, 1f),
                Vector2.zero, Vector2.zero, new Vector2(0f, 1f));
            var vpImg = viewport.gameObject.AddComponent<Image>();
            var vpSp = OriginalAssets.GetUi("UIMask");
            if (vpSp != null) vpImg.sprite = vpSp;
            viewport.gameObject.AddComponent<RectMask2D>();

            _content = Node(viewport, "Content", V(0f, 1f), V(1f, 1f),
                Vector2.zero, Vector2.zero, new Vector2(0f, 1f));
            var grid = _content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(280f, 532f);
            grid.spacing = new Vector2(16f, 18f);
            grid.padding = new RectOffset(72, 72, 20, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.UpperCenter;
            var cCsf = _content.gameObject.AddComponent<ContentSizeFitter>();
            cCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollbar = Node(p1T, "Scrollbar Vertical", V(1f, 0f), V(1f, 1f),
                new Vector2(-1f, -16.3f), new Vector2(20f, -16.3f), new Vector2(1f, 1f));
            var sbImg = scrollbar.gameObject.AddComponent<Image>();
            var sbSp = OriginalAssets.GetUi("progress bar change blank");
            if (sbSp != null) sbImg.sprite = sbSp;
            var sliding = StretchNode(scrollbar, "Sliding Area");
            var handle = StretchNode(sliding, "Handle");
            var handleImg = handle.gameObject.AddComponent<Image>();
            var hSp = OriginalAssets.GetUi("progress bar change gradually");
            if (hSp != null) handleImg.sprite = hSp;
            else handleImg.color = new Color(1f, 1f, 1f, 0.25f);
            var sb = scrollbar.gameObject.AddComponent<Scrollbar>();
            sb.targetGraphic = handleImg;
            sb.handleRect = handle;
            sb.direction = Scrollbar.Direction.BottomToTop;

            var sr = scrollView.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 30f;
            sr.viewport = viewport;
            sr.content = _content;
            sr.verticalScrollbar = sb;
            sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            // 叶片装饰
            UiImg(p1T, "Image (1)", "leaf decoraction", V(1f, 1f), V(1f, 1f),
                new Vector2(-190.3f, 37.7f), new Vector2(357f, 151f));

            // Image（dump 默认隐藏的全屏底图）
            var imgExtra = Node(Root, "Image", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -948f), new Vector2(750f, 1334f));
            imgExtra.gameObject.AddComponent<Image>();
            imgExtra.gameObject.SetActive(false);

            // 翻页（原版无此按钮，任务要求 20 关一页 + 翻页）
            _prevBtn = UIHelper.Button(p1T, "PagePrev", () => ShowPage(_page - 1));
            var pr = (RectTransform)_prevBtn.transform;
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.sizeDelta = new Vector2(150f, 56f);
            pr.anchoredPosition = new Vector2(-180f, -598f);
            var prevTxt = UIHelper.Text(_prevBtn.transform, "L", "上一页", 24, Color.white);
            UIHelper.Stretch(prevTxt.rectTransform);
            _nextBtn = UIHelper.Button(p1T, "PageNext", () => ShowPage(_page + 1));
            var nr = (RectTransform)_nextBtn.transform;
            nr.anchorMin = nr.anchorMax = new Vector2(0.5f, 0.5f);
            nr.sizeDelta = new Vector2(150f, 56f);
            nr.anchoredPosition = new Vector2(180f, -598f);
            var nextTxt = UIHelper.Text(_nextBtn.transform, "L", "下一页", 24, Color.white);
            UIHelper.Stretch(nextTxt.rectTransform);
            _pageTxt = Txt(p1T, "PageTxt", "", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(200f, 50f));

            ShowPage(0);
        }

        private string TitleText()
        {
            var f = FairyId > 0 ? FairySpineMap.Info(FairyId) : null;
            string name = f != null && !string.IsNullOrEmpty(f.FairyName) ? f.FairyName : "拼图";
            return "<color=#49B500>" + name + "</color>的相册";
        }

        private void ShowPage(int page)
        {
            int totalPages = (TotalLevels + PerPage - 1) / PerPage;
            _page = Mathf.Clamp(page, 0, totalPages - 1);

            for (int i = 0; i < _pageItems.Count; i++)
            {
                if (_pageItems[i] == null) continue;
                if (Application.isPlaying) Object.Destroy(_pageItems[i]);
                else Object.DestroyImmediate(_pageItems[i]);
            }
            _pageItems.Clear();

            int passed = SaveManager.Data.maxPassedLevel;
            int start = _page * PerPage;
            for (int i = 0; i < PerPage; i++)
            {
                int levelId = start + i + 1;
                if (levelId > TotalLevels) break;
                var item = MakeAlbumItem(_content, "PuzzleAlbumItem" + levelId, levelId, levelId <= passed);
                _pageItems.Add(item.gameObject);
            }

            _pageTxt.text = (_page + 1) + "/" + totalPages;
            _prevBtn.interactable = _page > 0;
            _nextBtn.interactable = _page < totalPages - 1;
        }

        public override void Refresh()
        {
            int passed = Mathf.Clamp(SaveManager.Data.maxPassedLevel, 0, TotalLevels);
            if (_completeTxt != null) _completeTxt.text = "已完成：" + passed + "/" + TotalLevels;
            ShowPage(_page);
        }

        /// <summary>复刻 res_PuzzleAlbumItem_12071：photo bg/Title/Detail/Mask(缩略图+Lock)/CompleteMark/UncompletedMark/CompleteBg/Slider。</summary>
        private RectTransform MakeAlbumItem(Transform parent, string name, int levelId, bool isPassed)
        {
            var root = Node(parent, name, V(0f, 0f), V(0f, 0f), Vector2.zero, new Vector2(280f, 532f));
            var rootImg = root.gameObject.AddComponent<Image>();
            var bgSp = OriginalAssets.GetUi("photo bg");
            if (bgSp != null) rootImg.sprite = bgSp;
            else rootImg.color = new Color(1f, 1f, 1f, 0.25f);

            Txt(root, "Title", "拼图" + levelId, V(0.5f, 1f), V(0.5f, 1f),
                new Vector2(-1.4f, -383.2f), new Vector2(282.8f, 37.4f));
            Txt(root, "Detail", "", V(0.5f, 1f), V(0.5f, 1f),
                new Vector2(-1.2f, -449.2f), new Vector2(283.3f, 94.2f));

            // Mask：拼图缩略图 + 锁
            var mask = UiImg(root, "Mask", "albumMask", V(0.5f, 1f), V(0.5f, 1f),
                new Vector2(0f, -192.1f), new Vector2(280f, 350f));
            var thumb = Node(mask.transform, "Image (1)", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(280f, 350f));
            var thumbImg = thumb.gameObject.AddComponent<Image>();
            var tex = OriginalAssets.GetPuzzleTexture(levelId);
            if (tex != null)
            {
                thumbImg.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), V(0.5f, 0.5f));
                thumbImg.preserveAspect = true;
            }
            else thumbImg.color = new Color(1f, 1f, 1f, 0.25f);
            var lockImg = UiImg(mask.transform, "Lock", "lockyellow", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(81f, 102f));
            lockImg.gameObject.SetActive(!isPassed);
            int id = levelId;
            var maskBtn = mask.gameObject.AddComponent<Button>();
            maskBtn.targetGraphic = mask;
            maskBtn.onClick.AddListener(() =>
            {
                if (isPassed) PanelManager.Instance.Push<CompletePuzzlePanel>(p => p.LevelId = id);
            });

            // 完成角标
            var cm = UiImg(root, "CompleteMark", "pin", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-137.6f, 216f), new Vector2(80f, 76f));
            cm.gameObject.SetActive(isPassed);
            var um = UiImg(root, "UncompletedMark", "pin green", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(-137.6f, 216f), new Vector2(80f, 76f));
            um.gameObject.SetActive(!isPassed);

            // CompleteBg（dump 默认隐藏；已通关时显示）
            var cb = UiImg(root, "CompleteBg", "status bar", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -59f), new Vector2(159f, 29f));
            Txt(cb.transform, "Text (Legacy)", "已完成", V(0.5f, 0.5f), V(0.5f, 0.5f),
                Vector2.zero, new Vector2(159f, 29f), true);
            cb.gameObject.SetActive(isPassed);

            // Slider：进度条 + 计数
            var sliderRt = Node(root, "Slider", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -61.2f), new Vector2(209.9f, 24.5f));
            var bg = StretchNode(sliderRt, "Background");
            var bgImg = bg.gameObject.AddComponent<Image>();
            var barSp = OriginalAssets.GetUi("collection progress bar");
            if (barSp != null) bgImg.sprite = barSp;
            else bgImg.color = new Color(1f, 1f, 1f, 0.25f);
            var fillArea = Node(sliderRt, "Fill Area", V(0f, 0f), V(1f, 1f),
                new Vector2(0.9f, -0.5f), new Vector2(-8.3f, -1f));
            var fill = Node(fillArea, "Fill", V(0f, 0f), V(0f, 0f),
                new Vector2(0.3f, 0f), new Vector2(4.6f, 0f));
            var fillImg = fill.gameObject.AddComponent<Image>();
            var fSp = OriginalAssets.GetUi("collection progress bar green");
            if (fSp != null) fillImg.sprite = fSp;
            else fillImg.color = new Color(0.35f, 0.8f, 0.35f, 0.9f);
            var icon1 = UiImg(sliderRt, "Image (1)", "picIcon", V(0f, 0.5f), V(0f, 0.5f),
                new Vector2(5f, 2.4f), new Vector2(40.8f, 40.6f));
            icon1.raycastTarget = false;
            Txt(sliderRt, "CountTxt", (isPassed ? 1 : 0) + "/1", V(0f, 0f), V(1f, 1f),
                Vector2.zero, Vector2.zero, true);
            var slider = sliderRt.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.targetGraphic = fillImg;
            slider.interactable = false;
            slider.value = isPassed ? 1f : 0f;

            return root;
        }

        // ---- dump 复刻小工具 ----
        private static Vector2 V(float x, float y) => new Vector2(x, y);

        private static RectTransform StretchNode(Transform parent, string name)
        {
            var rt = UIHelper.NewRect(parent, name);
            UIHelper.Stretch(rt);
            return rt;
        }

        private static RectTransform Node(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            var rt = UIHelper.NewRect(parent, name);
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        private static Image UiImg(Transform parent, string name, string sp,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size, pivot);
            var img = rt.gameObject.AddComponent<Image>();
            var s = OriginalAssets.GetUi(sp);
            if (s != null) img.sprite = s;
            else img.color = new Color(1f, 1f, 1f, 0.25f);   // GetUi null → 半透明纯色兜底
            return img;
        }

        private static Text Txt(Transform parent, string name, string content,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, bool outline = false, Vector2? pivot = null)
        {
            int fs = Mathf.Clamp(Mathf.RoundToInt(size.y / 2.2f), 20, 44);
            var t = UIHelper.Text(parent, name, content, fs, Color.white);
            var rt = t.rectTransform;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            if (outline) rt.gameObject.AddComponent<Outline>();
            return t;
        }

        private static Button ImgBtn(Transform parent, string name, string sp,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, System.Action onClick, Vector2? pivot = null)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size, pivot);
            var img = rt.gameObject.AddComponent<Image>();
            var s = OriginalAssets.GetUi(sp);
            if (s != null) img.sprite = s;
            else img.color = new Color(1f, 1f, 1f, 0.25f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }
    }
}
