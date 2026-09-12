using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>装扮试用弹窗（原版 SkinTryOut #12725 + SkinItem #12408 逆向 1:1）。
    /// 弹窗本体按 dump 摆放；底部 SkinItem 条目带横向选择（点击切换 imgSkinIcon 展示）。
    /// 精灵立绘原版为 Spine，当前用皮肤精灵/纯色占位。</summary>
    public sealed class SkinTryOutPanel : UIPanel
    {
        // 静态皮肤条目（4 条）：皮肤名 / 立绘精灵名 / 是否已拥有
        private static readonly string[] SkinNames = { "简约蓝", "樱粉", "森绿", "暮紫" };
        private static readonly string[] SkinSprites = { "skin1", "skin2", "skin3", "skin4" };
        private static readonly bool[] SkinOwned = { true, false, false, false };

        private static readonly Color Fallback = new Color(0.16f, 0.2f, 0.3f, 0.9f);

        private Image _skinIconImg;
        private Image _skinProImg;
        private Image _tryStamp;
        private readonly List<Image> _lightImgs = new List<Image>();

        protected override void Build()
        {
            // backgroup（原版无精灵 → 半透明遮罩兜底）
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)back.transform);
            back.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            // View1（原版 inactive：装扮使用标题版）
            var view1 = UIHelper.NewRect(panel, "View1");
            UIHelper.Stretch(view1);
            view1.gameObject.SetActive(false);
            Img(view1, "imgTitle", "zhuangbanshiyong", V(0.5f, 0.5f), V(366, 90), V(0, 412));
            var tExp2 = Txt(view1, "txtExplain2", "恭喜在本关获得1次试用机会", 80, V(0.5f, 0.5f), V(600, 80), V(0, -125));
            Outline(tExp2);

            // View2（默认显示）
            var view2 = UIHelper.NewRect(panel, "View2");
            UIHelper.Stretch(view2);

            // TODO spine: 原版为 SkeletonGraphic+SwapBanner 蓝色横幅动画（Renderer0=RawImage）
            var spine = UIHelper.NewRect(view2, "BlueSkeletonGraphic");
            UIHelper.Place(spine, V(0.5f, 0.5f), V(100, 100), V(0, 412));
            var renderer0 = UIHelper.NewRect(spine, "Renderer0");
            UIHelper.Place(renderer0, V(0.5f, 0.5f), V(100, 100), Vector2.zero);

            SpriteBtn(view2, "btnClose", "tcclose_02", V(0.5f, 0.5f), V(50, 49), V(279, 357),
                () => PanelManager.Instance.Pop());
            var tTitle = Txt(view2, "txtTitle", "装扮试用", 95, V(0.5f, 0.5f), V(300, 95), V(0, 410));
            Outline(tTitle);
            var tExp = Txt(view2, "txtExplain", "恭喜在本关获得1次试用机会", 80, V(0.5f, 0.5f), V(600, 80), V(0, 278));
            Outline(tExp);
            var tTip = Txt(view2, "txtTip", "每看1次广告会额外获得1个装扮碎片", 60, V(0.5f, 0.5f), V(500, 60), V(0, -127));
            Outline(tTip);

            // 皮肤展示区（光效底 + 立绘 + 三颗星）
            Img(panel, "Image", "lightbg", V(0.5f, 0.5f), V(342, 343), V(0, 116));
            _skinIconImg = Img(panel, "imgSkinIcon", "skin1", V(0.5f, 0.5f), V(210, 221), V(0, 116));
            // TODO spine: imgSkinIcon 原版为精灵立绘 Spine，先用皮肤精灵/纯色占位
            var stars = UIHelper.NewRect(panel, "Image (1)");
            UIHelper.Place(stars, V(0.5f, 0.5f), V(352, 353), V(13, 127));
            Img(stars, "Image", "xing_01", V(0.5f, 0.5f), V(33.6f, 44.4f), V(-125.2f, 60.3f));
            Img(stars, "Image (1)", "xing_01", V(0.5f, 0.5f), V(28, 37), V(-17, -64));
            Img(stars, "Image (2)", "xing_01", V(0.5f, 0.5f), V(42, 55.5f), V(106.9f, 71.7f));

            // 试用按钮 + 已领取戳（原版 imgTryOut inactive）
            var btnTryOut = UIHelper.Button(panel, "btnTryOut", () =>
            {
                if (_tryStamp != null) _tryStamp.gameObject.SetActive(true);
                Debug.Log("[SkinTryOut] 使用试用机会");
            });
            UIHelper.Place((RectTransform)btnTryOut.transform, V(0.5f, 0.5f), V(60, 60), V(-87, -569.7f));
            Img(btnTryOut.transform, "Image", "blankwhite", V(0.5f, 0.5f), V(29, 30), Vector2.zero);
            _tryStamp = Img(panel, "imgTryOut", "reward_lingqu", V(0.5f, 0.5f), V(33, 33), V(-87, -569.7f));
            _tryStamp.gameObject.SetActive(false);

            // 碎片进度条
            Img(panel, "Image2", "deepblue", V(0.5f, 0.5f), V(190, 25), V(0, -58));
            _skinProImg = Img(panel, "imgSkinPro", "greenprogressbar", V(0.5f, 0.5f), V(190, 25), V(0, -58));
            _skinProImg.type = Image.Type.Filled;
            _skinProImg.fillMethod = Image.FillMethod.Horizontal;
            _skinProImg.fillAmount = 0f;
            var imgFrag = Img(panel, "imgFrag", "ad2", V(0.5f, 0.5f), V(47, 56), V(-88, -57));
            imgFrag.transform.localScale = V(0.8f, 0.8f);
            var imgCoin = Img(panel, "imgCoin", "gold_big", V(0.5f, 0.5f), V(50, 54), V(-88, -57));
            imgCoin.transform.localScale = V(0.8f, 0.8f);
            imgCoin.gameObject.SetActive(false);

            // 看广告试用 / 免费试用（inactive）/ 暂时不用
            var btnVideo = SpriteBtn(panel, "btnVideo", "Btn_01", V(0.5f, 0.5f), V(339.3f, 103), V(143, -290),
                () => Debug.Log("[SkinTryOut] 看广告获得试用"));
            Img(btnVideo.transform, "Image", "video", V(0.5f, 0.5f), V(47, 39), V(-76.1f, 0));
            var tUse = Txt(btnVideo.transform, "Text", "试用", 70, V(0.5f, 0.5f), V(180, 70), V(28.4f, 1));
            Outline(tUse);

            var btnFree = SpriteBtn(panel, "btnFree", "Btn_02", V(0.5f, 0.5f), V(339.3f, 103), V(143, -290),
                () => Debug.Log("[SkinTryOut] 免费试用"));
            btnFree.gameObject.SetActive(false);
            var tFree = Txt(btnFree.transform, "Text", "免费试用", 70, V(0.5f, 0.5f), V(180, 70), V(0, 1));
            Outline(tFree);

            var btnGiveUp = SpriteBtn(panel, "btnGiveUp", "Btn_02", V(0.5f, 0.5f), V(250, 103), V(-186, -290),
                () => PanelManager.Instance.Pop());
            var tGiveUp = Txt(btnGiveUp.transform, "Text", "暂时不用", 70, V(0.5f, 0.5f), V(200, 70), V(0, 1));
            Outline(tGiveUp);

            var tPro = Txt(panel, "txtSkinPro", "0/0", 60, V(0.5f, 0.5f), V(160, 60), V(0, -56.1f));
            Outline(tPro);
            var tTryOut = Txt(panel, "txtTryOut", "今日不再试用", 60, V(0.5f, 0.5f), V(240, 60), V(25, -569));
            Outline(tTryOut);

            // 皮肤条目选择带（参考 SkinItem #12408，位于按钮与进度文案之间的空档）
            var list = UIHelper.NewRect(panel, "SkinList");
            UIHelper.Stretch(list);
            float[] xs = { -237f, -79f, 79f, 237f };
            for (int i = 0; i < SkinNames.Length; i++)
            {
                int idx = i;
                var item = MakeSkinItem(list, "SkinItem" + (i + 1), xs[i], i);
                item.onClick.AddListener(() => Select(idx));
            }
            Select(0);
        }

        /// <summary>按 SkinItem #12408 dump 生成一条皮肤条目（缩放 0.65 放入选择带）。</summary>
        private Button MakeSkinItem(Transform list, string name, float x, int idx)
        {
            bool owned = SkinOwned[idx];
            var item = UIHelper.Button(list, name, null);
            var rt = (RectTransform)item.transform;
            UIHelper.Place(rt, V(0.5f, 0.5f), V(227, 277), V(x, -355f));
            rt.pivot = new Vector2(0.5f, 1f);
            rt.localScale = V(0.65f, 0.65f);

            _lightImgs.Add(Img(item.transform, "imgLight", "showboard", V(0.5f, 0.5f), V(230, 259), V(0, 14.2f)));
            Img(item.transform, "Image", "showboardn", V(0.5f, 0.5f), V(204, 232), V(0, 14.5f));

            // TODO spine: imgSkin 原版为精灵立绘 Spine，先用皮肤精灵/纯色占位
            var imgSkin = Img(item.transform, "imgSkin", SkinSprites[idx], V(0.5f, 0.5f), V(210, 221), V(0, 21.7f));
            imgSkin.transform.localScale = V(0.8f, 0.8f);
            if (imgSkin.sprite == null) imgSkin.color = Fallback;

            var imgLock = Img(item.transform, "imgLock", "unlockblack", V(0.5f, 0.5f), V(209, 47), V(0, 12));
            Img(imgLock.transform, "imgLock1", "Game_Lock_1_Locked", V(0.5f, 0.5f), V(44, 51), V(0, -0.2f));
            imgLock.gameObject.SetActive(!owned);

            var imgRed = UIHelper.Image(item.transform, "imgRed");
            UIHelper.Place((RectTransform)imgRed.transform, V(0.5f, 0.5f), V(30, 30), V(64, 100.4f));
            imgRed.gameObject.SetActive(false);

            // 合成进度组（原版 inactive）
            var imgMake = UIHelper.NewRect(item.transform, "imgMake");
            UIHelper.Place(imgMake, V(0.5f, 0.5f), V(140, 40), V(0, -125.5f));
            Img(imgMake, "imgProLight", "deepbluelight", V(0.5f, 0.5f), V(217, 52), Vector2.zero);
            Img(imgMake, "imgGray", "deepblue", V(0.5f, 0.5f), V(190, 25), Vector2.zero);
            var progress = Img(imgMake, "imgProgress", "greenprogressbar", V(0.5f, 0.5f), V(192, 25), Vector2.zero);
            progress.type = Image.Type.Filled;
            progress.fillMethod = Image.FillMethod.Horizontal;
            progress.fillAmount = idx * 0.25f;
            var mkCoin = Img(imgMake, "imgCoin", "gold_big", V(0.5f, 0.5f), V(50, 52), V(-93.9f, 0));
            mkCoin.transform.localScale = V(0.8f, 0.8f);
            var mkFrag = Img(imgMake, "imgFrag", "pic", V(0.5f, 0.5f), V(52, 52), V(-93.9f, 0));
            mkFrag.transform.localScale = V(0.8f, 0.8f);
            mkFrag.gameObject.SetActive(false);
            var mkVideo = Img(imgMake, "imgVideo", "ad2", V(0.5f, 0.5f), V(47, 56), V(-93.9f, 0));
            mkVideo.transform.localScale = V(0.8f, 0.8f);
            mkVideo.gameObject.SetActive(false);
            var tProgress = Txt(imgMake, "txtProgress", "0/2", 35, V(0.5f, 0.5f), V(120, 35), V(0, 0.3f));
            Outline(tProgress);
            imgMake.gameObject.SetActive(false);

            var tName = Txt(item.transform, "txtName", SkinNames[idx], 45, V(0.5f, 0.5f), V(120, 45), V(0, -73.1f));
            Outline(tName);
            var tState = Txt(item.transform, "txtState", owned ? "已拥有" : "未拥有", 45, V(0.5f, 0.5f), V(120, 45), V(0, -123.6f));
            Outline(tState);
            var imgSelect = Img(tState.transform, "imgSelect", "pet_icon_Equipped", V(0.5f, 0.5f), V(42, 35), V(-62, 0));
            imgSelect.gameObject.SetActive(false);

            return item;
        }

        /// <summary>选中皮肤 → 高亮底板 + 刷新立绘展示。</summary>
        private void Select(int idx)
        {
            for (int i = 0; i < _lightImgs.Count; i++)
                if (_lightImgs[i] != null)
                    _lightImgs[i].color = i == idx ? new Color(1f, 0.92f, 0.45f, 1f) : Color.white;
            if (_skinIconImg != null)
            {
                var sp = OriginalAssets.GetUi(SkinSprites[idx]);
                _skinIconImg.sprite = sp;
                _skinIconImg.color = sp != null ? Color.white : Fallback;
            }
            Debug.Log("[SkinTryOut] 选中皮肤: " + SkinNames[idx]);
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
