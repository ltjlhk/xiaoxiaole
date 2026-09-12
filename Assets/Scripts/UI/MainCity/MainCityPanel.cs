using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;
using Spine.Unity;

namespace Xio.UI
{
    /// <summary>主城：精灵看板（当前精灵 Spine 动画）+ 金币/星星角标 + 关卡/精灵/赛季入口。</summary>
    public class MainCityPanel : UIPanel
    {
        private Text _coinText;
        private Text _starText;

        protected override void Build()
        {
            // 背景（赛季主题 bg10，缺省纯色）
            var bgTex = OriginalAssets.GetBackground("bg10");
            var bg = UIHelper.Image(Root, "BG", bgTex != null
                ? Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f), 100f)
                : null);
            UIHelper.Stretch((RectTransform)bg.transform);

            // 标题栏（原文案）
            var title = UIHelper.Text(Root, "Title", "就你会消除", 44, new Color(1f, 0.98f, 0.85f), FontStyle.Bold);
            UIHelper.Place((RectTransform)title.transform, new Vector2(0.5f, 1f), new Vector2(600, 70), new Vector2(0, -50));
            var outline = title.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.22f, 0.45f);
            outline.effectDistance = new Vector2(2, -2);

            // 顶部金币/星星
            MakeTopBar();

            // 精灵看板（当前精灵 Spine）居中
            MakeFairyShow();

            // 底部三大入口
            MakeEntries();
        }

        private void MakeTopBar()
        {
            float y = -130;
            // 金币
            var goldBg = UIHelper.Image(Root, "GoldBg", OriginalAssets.GetUi("mp_board"), true);
            UIHelper.Place((RectTransform)goldBg.transform, new Vector2(0.5f, 1f), new Vector2(170, 56), new Vector2(-190, y));
            var goldIcon = UIHelper.Image(Root, "GoldIcon", OriginalAssets.GetUi("bigbig_gold"));
            UIHelper.Place((RectTransform)goldIcon.transform, new Vector2(0.5f, 1f), new Vector2(40, 40), new Vector2(-255, y));
            _coinText = UIHelper.Text(Root, "CoinText", "", 30, new Color(0.2f, 0.12f, 0.05f), FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place((RectTransform)_coinText.transform, new Vector2(0.5f, 1f), new Vector2(120, 44), new Vector2(-175, y));
            var coinOut = _coinText.gameObject.AddComponent<Outline>();
            coinOut.effectColor = new Color(1f, 0.95f, 0.6f, 0.8f);
            coinOut.effectDistance = new Vector2(1.5f, -1.5f);

            // 星星
            var starBg = UIHelper.Image(Root, "StarBg", OriginalAssets.GetUi("mp_board"), true);
            UIHelper.Place((RectTransform)starBg.transform, new Vector2(0.5f, 1f), new Vector2(170, 56), new Vector2(190, y));
            var starIcon = UIHelper.Image(Root, "StarIcon", OriginalAssets.GetUi("star"));
            UIHelper.Place((RectTransform)starIcon.transform, new Vector2(0.5f, 1f), new Vector2(40, 40), new Vector2(125, y));
            _starText = UIHelper.Text(Root, "StarText", "", 30, new Color(0.2f, 0.12f, 0.05f), FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place((RectTransform)_starText.transform, new Vector2(0.5f, 1f), new Vector2(120, 44), new Vector2(205, y));
            var starOut = _starText.gameObject.AddComponent<Outline>();
            starOut.effectColor = new Color(1f, 0.9f, 0.55f, 0.8f);
            starOut.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private void MakeFairyShow()
        {
            // 当前精灵 Spine 看板（精灵1 → jingling_1；映射表见 FairySpineMap）
            var fairy = FairySpineMap.SpineFor(SaveManager.Data.currentFairy);
            var sg = SpineView.Play(Root, "FairyShow", fairy, null, true, new Vector2(420, 560));
            if (sg != null)
            {
                UIHelper.Place((RectTransform)sg.transform, new Vector2(0.5f, 0.5f), new Vector2(420, 560), new Vector2(0, 130));
                // 首个动画名（冒烟）
                var anim = sg.skeletonAnimation as Spine.Unity.SkeletonAnimation;
                var names = SpineAssets.AnimationNames(sg.skeletonDataAsset);
                if (anim != null && names.Count > 0)
                {
                    string first = names[0];
                    anim.AnimationState.SetAnimation(0, first, true);
                    Debug.Log($"[主城] 精灵 Spine 播放: {fairy} / {first}（共 {names.Count} 个动画）");
                }
                else if (anim == null)
                {
                    Debug.LogWarning($"[主城] {fairy} 动画组件缺失");
                }
                else
                {
                    Debug.LogWarning($"[主城] {fairy} 无动画");
                }
            }
            else
            {
                // 无 Spine 时降级静态立绘
                var icon = UIHelper.Image(Root, "FairyShow", FairySpineMap.IconFor(SaveManager.Data.currentFairy));
                UIHelper.Place((RectTransform)icon.transform, new Vector2(0.5f, 0.5f), new Vector2(380, 460), new Vector2(0, 130));
            }
        }

        private void MakeEntries()
        {
            string[] labels = { "关卡", "精灵", "赛季" };
            for (int i = 0; i < labels.Length; i++)
            {
                int idx = i;
                var btn = UIHelper.Button(Root, "Entry_" + labels[i], () =>
                {
                    Debug.Log($"[主城] 进入{labels[idx]}");
                    if (idx == 0) PanelManager.Instance.Push<LevelSelectPanel>();
                    else if (idx == 1) PanelManager.Instance.Push<FairyPanel>();
                    else PanelManager.Instance.Push<SeasonPanel>();
                });
                var brt = UIHelper.Place((RectTransform)btn.transform, new Vector2(0.5f, 0f),
                    new Vector2(200, 150), new Vector2(-250 + 250 * i, 90));
                var board = OriginalAssets.GetUi("mp_board");
                if (board != null)
                {
                    var img = btn.GetComponent<Image>();
                    img.sprite = board;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }
                var label = UIHelper.Text(btn.transform, "Label", labels[i], 32, new Color(0.45f, 0.25f, 0.08f), FontStyle.Bold);
                UIHelper.Stretch(label.rectTransform);
            }
        }

        public override void Refresh()
        {
            if (_coinText != null) _coinText.text = SaveManager.Data.coins.ToString();
            if (_starText != null) _starText.text = SaveManager.Data.stars.ToString();
        }
    }
}