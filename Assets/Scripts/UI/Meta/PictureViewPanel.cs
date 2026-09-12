using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>花语图鉴面板（原版 PictureView #11834 + PictureItem #10178 逆向 1:1）。
    /// ScrollView 内 GridLayoutGroup 摆放静态条目，缩略图用 GetPuzzleTexture(levelId)；
    /// 点击条目刷新上方大图详情（imgIcon/txtName/txtIntro）。</summary>
    public sealed class PictureViewPanel : UIPanel
    {
        // 静态图鉴条目（6 条）：关卡 Id / 花名
        private static readonly int[] LevelIds = { 101, 102, 103, 104, 105, 106 };
        private static readonly string[] FlowerNames = { "郁金香", "兰花", "玫瑰", "向日葵", "百合", "樱花" };

        private static readonly Color Fallback = new Color(0.16f, 0.2f, 0.3f, 0.9f);

        private RawImage _detailIcon;
        private Text _detailName;
        private Text _detailIntro;
        private Text _maskProText;

        protected override void Build()
        {
            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            // basemap（原版无精灵 → 半透明底兜底）
            var basemap = UIHelper.Image(panel, "basemap");
            UIHelper.Stretch((RectTransform)basemap.transform);
            basemap.color = new Color(0f, 0f, 0f, 0.55f);

            // 顶部横幅（锚 (0,1)~(1,1)，scale y=-1 翻转）
            var imgTop = UIHelper.NewRect(panel, "imgTop");
            imgTop.anchorMin = new Vector2(0f, 1f);
            imgTop.anchorMax = new Vector2(1f, 1f);
            imgTop.pivot = new Vector2(0.5f, 0.5f);
            imgTop.sizeDelta = new Vector2(0f, 190f);
            imgTop.anchoredPosition = new Vector2(0f, -95f);
            imgTop.localScale = new Vector3(1f, -1f, 1f);
            var topSp = OriginalAssets.GetUi("Home_panel_01");
            var topImg = imgTop.gameObject.AddComponent<Image>();
            if (topSp != null) topImg.sprite = topSp;
            else topImg.color = Fallback;

            SpriteBtn(panel, "btnClose", "collection_icon_01", V(0f, 1f), V(73, 73), V(74, -107),
                () => PanelManager.Instance.Pop());

            var tPicture = Txt(panel, "txtPicture", "花语", 80, V(0.5f, 1f), V(320, 80), V(0, -109));
            Outline(tPicture);

            BuildDetail(panel);
            BuildMask(panel);
            BuildScrollView(panel);

            var imgIntro = Img(panel, "imgIntro", "title01", V(0.5f, 1f), V(699, 122), V(0, -668));
            imgIntro.rectTransform.pivot = new Vector2(0.5f, 0f);
            _detailIntro = Txt(imgIntro.transform, "txtIntro", "兰花奇珍", 70, V(0.5f, 0.5f), V(380, 70), Vector2.zero);

            Select(0);
        }

        /// <summary>详情大图区（imgTitle：相框/大图/品质角/两行说明/礼包按钮）。</summary>
        private void BuildDetail(Transform panel)
        {
            var imgTitle = Img(panel, "imgTitle", "collection_bg_02", V(0.5f, 1f), V(700, 405), V(0, -401));
            imgTitle.rectTransform.pivot = new Vector2(0.5f, 0f);

            Img(imgTitle.transform, "imgCase", "Valuable_bg", V(0.5f, 0.5f), V(246, 265), V(-172, -28));
            _detailIcon = RawImg(imgTitle.transform, "imgIcon", null, V(0.5f, 0.5f), V(140, 140), V(-173.5f, -33));
            Img(imgTitle.transform, "imgQuality", "Valuable_sign", V(0.5f, 0.5f), V(76, 81), V(-249.5f, 40.8f));
            Img(imgTitle.transform, "Image", "collection_bg_03", V(0.5f, 0.5f), V(320, 50), V(123, 19.7f));
            Img(imgTitle.transform, "Image (1)", "collection_bg_03", V(0.5f, 0.5f), V(320, 100), V(123, -66));

            _detailName = Txt(imgTitle.transform, "txtName", "郁金香", 60, V(0.5f, 0.5f), V(260, 60), V(0, 158.4f));
            Outline(_detailName);
            Txt(imgTitle.transform, "txtOtherName", "别名：", 60, V(0.5f, 0.5f), V(306, 60), V(122, 19.7f));
            Txt(imgTitle.transform, "txtExplain", "花语:", 100, V(0.5f, 0.5f), V(306, 100), V(122, -67.6f));

            // 礼包按钮（原版 inactive）
            var btnDrawReward = SpriteBtn(imgTitle.transform, "btnDrawReward", "collection_icon_gift",
                V(0.5f, 0.5f), V(113, 92), V(253.3f, -115), () => Debug.Log("[PictureView] 领取图鉴奖励"));
            btnDrawReward.gameObject.SetActive(false);
        }

        /// <summary>解锁进度遮罩（原版 inactive：锁头 + 进度条 + 说明）。</summary>
        private void BuildMask(Transform panel)
        {
            var imgMask = Img(panel, "imgMask", "collection_bg_10", V(0.5f, 1f), V(700, 405), V(0, -401));
            imgMask.rectTransform.pivot = new Vector2(0.5f, 0f);
            imgMask.gameObject.SetActive(false);

            Img(imgMask.transform, "imgUnlock", "lockyellow", V(0.5f, 0.5f), V(71, 92), V(0, 37));
            Img(imgMask.transform, "Image", "collection_bar01", V(0.5f, 0.5f), V(466, 40), V(0, -39));
            Img(imgMask.transform, "imgPro", "collection_bar", V(0.5f, 0.5f), V(461, 40), V(0, -39));
            _maskProText = Txt(imgMask.transform, "txtPro", "0/35", 45, V(0.5f, 0.5f), V(160, 45), V(0, -38.8f));
            Txt(imgMask.transform, "txtUnlockExplain", "", 60, V(0.5f, 0.5f), V(550, 60), V(0, -115));
        }

        /// <summary>滚动区：ScrollView → Viewport(Mask) → Content(GridLayout) → 静态条目。</summary>
        private void BuildScrollView(Transform panel)
        {
            var scroll = Img(panel, "ScrollView", "Background",
                V(0.5f, 0.5f), V(650, -742), V(0, -361));
            var srt = (RectTransform)scroll.transform;
            srt.anchorMin = new Vector2(0.5f, 0f);
            srt.anchorMax = new Vector2(0.5f, 1f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(650f, -742f);
            srt.anchoredPosition = new Vector2(0f, -361f);

            var viewport = UIHelper.NewRect(srt, "Viewport");
            UIHelper.Stretch(viewport);
            viewport.pivot = new Vector2(0f, 1f);
            var vpImg = viewport.gameObject.AddComponent<Image>();
            var vpSp = OriginalAssets.GetUi("UIMask");
            if (vpSp != null) vpImg.sprite = vpSp;
            else vpImg.color = Fallback;
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = UIHelper.NewRect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.sizeDelta = new Vector2(0f, 0f);
            content.anchoredPosition = Vector2.zero;

            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(304f, 284f);
            grid.spacing = new Vector2(14f, 16f);
            grid.padding = new RectOffset(14, 14, 20, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = scroll.gameObject.AddComponent<ScrollRect>();
            sr.viewport = viewport;
            sr.content = content;
            sr.vertical = true;
            sr.horizontal = false;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 30f;

            // Scrollbar Vertical（原版结构，尺寸按 dump 全 0，仅保留层级）
            var sb = Img(srt, "Scrollbar Vertical", "Background", V(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            ((RectTransform)sb.transform).anchorMin = new Vector2(1f, 0f);
            ((RectTransform)sb.transform).anchorMax = new Vector2(1f, 1f);
            ((RectTransform)sb.transform).pivot = new Vector2(1f, 1f);
            var sliding = UIHelper.NewRect(sb.transform, "Sliding Area");
            UIHelper.Stretch(sliding);
            var handle = Img(sliding, "Handle", "UISprite", V(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            ((RectTransform)handle.transform).anchorMin = Vector2.zero;
            ((RectTransform)handle.transform).anchorMax = Vector2.zero;

            for (int i = 0; i < LevelIds.Length; i++)
            {
                int idx = i;
                var item = MakePictureItem(content, i);
                item.onClick.AddListener(() => Select(idx));
            }
        }

        /// <summary>按 PictureItem #10178 dump 生成一条图鉴条目。</summary>
        private Button MakePictureItem(Transform content, int idx)
        {
            int levelId = LevelIds[idx];
            string flower = FlowerNames[idx];
            int cur = Mathf.Clamp(SaveManager.Data.maxPassedLevel - idx * 20, 0, 20);

            var item = SpriteBtn(content, "PictureItem" + (idx + 1), "collection_bg_01",
                V(0.5f, 0.5f), V(332, 310), Vector2.zero, null);

            RawImg(item.transform, "imgIcon", OriginalAssets.GetPuzzleTexture(levelId),
                V(0.5f, 0.5f), V(308, 195), V(0, -4.4f));

            var imgRed = Img(item.transform, "imgRed", "warning", V(0.5f, 0.5f), V(34, 35), V(155.8f, 144.4f));
            imgRed.gameObject.SetActive(false);

            Img(item.transform, "imgProBg", "collection_bar01", V(0.5f, 0.5f), V(274, 34), V(0, -125));
            var imgPro = Img(item.transform, "imgPro", "collection_bar", V(0.5f, 0.5f), V(272, 32), V(0, -125));
            imgPro.type = Image.Type.Filled;
            imgPro.fillMethod = Image.FillMethod.Horizontal;
            imgPro.fillAmount = cur / 20f;

            var tName = Txt(item.transform, "txtName", flower, 50, V(0.5f, 0.5f), V(160, 50), V(0, 125.5f));
            Outline(tName);

            var tPro = Txt(item.transform, "txtPro", cur + "/20", 45, V(0.5f, 0.5f), V(160, 45), V(0, -125));
            var tComplete = Txt(item.transform, "txtComplete", "完成", 50, V(0.5f, 0.5f), V(160, 50), V(0, -125));
            Outline(tComplete);
            tComplete.gameObject.SetActive(false);
            if (cur >= 20)
            {
                tPro.gameObject.SetActive(false);
                tComplete.gameObject.SetActive(true);
            }
            return item;
        }

        /// <summary>选中条目 → 刷新详情大图/花名/花语横幅。</summary>
        private void Select(int idx)
        {
            var tex = OriginalAssets.GetPuzzleTexture(LevelIds[idx]);
            if (_detailIcon != null)
            {
                _detailIcon.texture = tex;
                _detailIcon.color = tex != null ? Color.white : Fallback;
            }
            if (_detailName != null) _detailName.text = FlowerNames[idx];
            if (_detailIntro != null) _detailIntro.text = FlowerNames[idx] + "奇珍";
            if (_maskProText != null)
            {
                int cur = Mathf.Clamp(SaveManager.Data.maxPassedLevel - idx * 20, 0, 20);
                _maskProText.text = cur + "/35";
            }
        }

        // ===== 本文件内的小工具（sprite 缺失 → 半透明纯色兜底） =====

        private static Vector2 V(float x, float y) { return new Vector2(x, y); }

        private static Image Img(Transform parent, string name, string sprite,
            Vector2 anchor, Vector2 size, Vector2 pos)
        {
            var sp = sprite != null ? OriginalAssets.GetUi(sprite) : null;
            var img = UIHelper.Image(parent, name, sp);
            if (sp == null && sprite != null) img.color = Fallback;
            UIHelper.Place((RectTransform)img.transform, anchor, size, pos);
            return img;
        }

        private static RawImage RawImg(Transform parent, string name, Texture tex,
            Vector2 anchor, Vector2 size, Vector2 pos)
        {
            var rt = UIHelper.NewRect(parent, name);
            UIHelper.Place(rt, anchor, size, pos);
            var raw = rt.gameObject.AddComponent<RawImage>();
            raw.texture = tex;
            if (tex == null) raw.color = Fallback;
            raw.raycastTarget = false;
            return raw;
        }

        private static Text Txt(Transform parent, string name, string content, float boxH,
            Vector2 anchor, Vector2 size, Vector2 pos)
        {
            int fs = Mathf.Clamp(Mathf.RoundToInt(boxH / 2.2f), 20, 44);
            var t = UIHelper.Text(parent, name, content, fs, Color.white);
            UIHelper.Place((RectTransform)t.transform, anchor, size, pos);
            return t;
        }

        private static void Outline(Graphic g)
        {
            var o = g.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.85f);
        }

        private static Button SpriteBtn(Transform parent, string name, string sprite,
            Vector2 anchor, Vector2 size, Vector2 pos, System.Action onClick)
        {
            var img = Img(parent, name, sprite, anchor, size, pos);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }
    }
}
