using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>精灵解锁弹窗（1:1 复刻 res_FairyUnlock_9744）。
    /// 条目复刻 res_FairyButtonItem_11363（使用中/未解锁/已拥有/敬请期待）。</summary>
    public sealed class FairyUnlockPanel : UIPanel
    {
        public int FairyId = 1;                 // 当前选中精灵
        public System.Action OnUnlock;          // 立即使用回调

        private Text _detail;
        private readonly List<ItemRefs> _items = new List<ItemRefs>();

        private class ItemRefs
        {
            public int FairyId;
            public int UnlockNeed;
            public GameObject HighLight;
            public Image FairyImage;
            public Text UnlockText, LockText, WaitingText;
        }

        protected override void Build()
        {
            // 根节点 Image（原版 FairyUnlock 根带 Image）
            Root.gameObject.AddComponent<Image>();

            var panel = StretchNode(Root, "Panel");

            // BlueSkeletonGraphic → spine 占位
            var spine = SpinePlaceholder(panel, "BlueSkeletonGraphic", new Vector2(0f, 344f));
            UIHelper.NewRect(spine, "Renderer1").gameObject.SetActive(false);
            UIHelper.NewRect(spine, "Renderer2").gameObject.SetActive(false);
            Txt(spine, "Text (Legacy)", "解锁精灵", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -2f), new Vector2(300f, 95f), true);

            // btnDraw 374×103 @(0,-338)
            var btnDraw = ImgBtn(panel, "btnDraw", "Btn_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -338f), new Vector2(374f, 103f), OnDrawClicked);
            Txt(btnDraw.transform, "Text", "立即使用", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 1f), new Vector2(240f, 70f), true);

            Node(panel, "StartPos", V(0.5f, 0.5f), V(0.5f, 0.5f), new Vector2(-372f, 18f), new Vector2(100f, 100f));
            Node(panel, "EndPos", V(0.5f, 0.5f), V(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(100f, 100f));

            _detail = Txt(panel, "SkillDetail", "", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, -198f), new Vector2(571.1f, 87.2f));

            // 精灵条目（参考 res_FairyButtonItem_11363），一行排布
            var fairyList = LevelSegmentModel.FairyList;
            int n = fairyList != null ? fairyList.Count : 0;
            for (int i = 0; i < n; i++)
            {
                var f = fairyList[i];
                float x = (i - (n - 1) * 0.5f) * 170f;
                MakeFairyButtonItem(panel, "FairyButtonItem" + (i + 1), f, new Vector2(x, 80f));
            }

            Refresh();
        }

        /// <summary>复刻 res_FairyButtonItem_11363：HighLight/RareMark/FairyImage/TextBg(Using|Lock|Unlock|Waiting)。</summary>
        private void MakeFairyButtonItem(Transform parent, string name, FairyInfo f, Vector2 pos)
        {
            var btn = UIHelper.Button(parent, name, () => SelectFairy(f.id));
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(137f, 92f);
            rt.anchoredPosition = pos;

            // HighLight（选中态，dump 默认隐藏）
            var hl = UIHelper.Image(btn.transform, "HighLight");
            var hlRt = (RectTransform)hl.transform;
            hlRt.anchorMin = hlRt.anchorMax = new Vector2(0.5f, 0.5f);
            hlRt.sizeDelta = new Vector2(171f, 117f);
            hlRt.anchoredPosition = new Vector2(0f, 4f);
            hl.gameObject.SetActive(false);

            // RareMark（dump 默认隐藏）
            var rm = UiImg(btn.transform, "RareMark", "corner mark", V(0f, 1f), V(0f, 1f),
                new Vector2(25.5f, -23.5f), new Vector2(52f, 47f));
            rm.gameObject.SetActive(false);

            // FairyImage（立绘）
            var fi = Node(btn.transform, "FairyImage", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 31.9f), new Vector2(108f, 140f));
            var fiImg = fi.gameObject.AddComponent<Image>();
            var iconSp = FairySpineMap.IconFor(f.id);
            if (iconSp != null) fiImg.sprite = iconSp;
            else fiImg.color = new Color(1f, 1f, 1f, 0.25f);

            // TextBg + 四态
            var tb = UiImg(btn.transform, "TextBg", "status bar", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(2.6f, -17.9f), new Vector2(105.2f, 26.2f));
            var usingT = Txt(tb.transform, "Using", "使用中", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(15.1f, 1.2f), new Vector2(74.9f, 28.7f), true);
            var eq = UiImg(usingT.transform, "Image (1)", "pet_icon_Equipped", V(0f, 0.5f), V(0f, 0.5f),
                new Vector2(-12.8f, -2.4f), new Vector2(16.3f, 13.6f));
            eq.raycastTarget = false;
            var lockT = Txt(tb.transform, "Lock", "未解锁", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(11.5f, 1.2f), new Vector2(74.9f, 28.7f), true);
            var lk = UiImg(lockT.transform, "Image (1)", "Game_Lock_1_Locked", V(0f, 0.5f), V(0f, 0.5f),
                new Vector2(-15.4f, 0f), new Vector2(22.7f, 26.3f));
            lk.raycastTarget = false;
            var unlockT = Txt(tb.transform, "Unlock", "已拥有", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 1.2f), new Vector2(105.2f, 28.7f), true);
            var waitT = Txt(tb.transform, "Waiting", "敬请期待", V(0.5f, 0.5f), V(0.5f, 0.5f),
                new Vector2(0f, 1.2f), new Vector2(105.2f, 28.7f), true);
            lockT.gameObject.SetActive(false);
            unlockT.gameObject.SetActive(false);
            waitT.gameObject.SetActive(false);

            _items.Add(new ItemRefs
            {
                FairyId = f.id,
                UnlockNeed = f.FairyUnlock,
                HighLight = hl.gameObject,
                FairyImage = fiImg,
                UnlockText = unlockT,
                LockText = lockT,
                WaitingText = waitT,
            });
        }

        private void SelectFairy(int fairyId)
        {
            FairyId = fairyId;
            Refresh();
        }

        private void OnDrawClicked()
        {
            if (OnUnlock != null) OnUnlock();
            PanelManager.Instance.Pop();
        }

        public override void Refresh()
        {
            int passed = SaveManager.Data.maxPassedLevel;
            for (int i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                bool unlocked = passed >= it.UnlockNeed;
                bool waiting = !unlocked && it.UnlockNeed <= 0;
                it.UnlockText.gameObject.SetActive(unlocked);
                it.LockText.gameObject.SetActive(!unlocked && !waiting);
                it.WaitingText.gameObject.SetActive(waiting);
                if (it.HighLight != null) it.HighLight.SetActive(it.FairyId == FairyId);
                if (it.FairyImage != null)
                {
                    var f = FairySpineMap.Info(it.FairyId);
                    string icon = f != null && !unlocked && !string.IsNullOrEmpty(f.FairyLockedIcon)
                        ? f.FairyLockedIcon : null;
                    var sp = !string.IsNullOrEmpty(icon) ? OriginalAssets.GetUi(icon) : FairySpineMap.IconFor(it.FairyId);
                    if (sp != null) it.FairyImage.sprite = sp;
                }
            }

            var cur = FairySpineMap.Info(FairyId);
            string name = cur != null && !string.IsNullOrEmpty(cur.FairyName) ? cur.FairyName : "精灵" + FairyId;
            int need = cur != null ? cur.FairyUnlock : 0;
            bool canUnlock = SaveManager.Data.maxPassedLevel >= need;
            _detail.text = cur == null
                ? "敬请期待"
                : (canUnlock ? name + "：已可使用！" : string.Format("{0}：通关 {1} 关解锁", name, need));
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

        private static RectTransform SpinePlaceholder(Transform parent, string name, Vector2 pos)
        {
            var rt = Node(parent, name, V(0.5f, 0.5f), V(0.5f, 0.5f), pos, new Vector2(100f, 100f));
            UIHelper.NewRect(rt, "Renderer0");   // TODO spine: SkeletonGraphic 渲染占位
            return rt;
        }
    }
}
