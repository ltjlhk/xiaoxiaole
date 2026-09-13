using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;
using Xio.Platform;
using Spine.Unity;

namespace Xio.UI
{
    /// <summary>
    /// 主城：按原版 PanelHome + PanelButton 一比一复刻 ——
    /// bg3 背景 + 精灵 Spine 看板 → 关卡/星星宝箱 → 两侧 6 个功能按钮
    /// → 开始游戏 + 赛季入口 → 顶部资源条（设置/精力/金币/星星）+ 底部 5 键导航。
    /// 坐标与贴图名严格取自 tools/prefab_dump/level1_Canvas_1015.txt。
    /// </summary>
    public class MainCityPanel : UIPanel
    {
        // 顶栏资源条
        private Text _coinText;
        private Text _starText;
        private Text _spiritText;
        private Text _spiritTime;
        // 开始按钮
        private Text _levelText;
        // 关卡/星星宝箱
        private Image _levelProFill;
        private Text _levelProText;
        private Image _starProFill;
        private Text _starProText;

        private float _tickAcc;

        protected override void Build()
        {
            MakeBackground();
            MakeFairyShow();
            MakeChestBoxes();
            MakeSideButtons();
            MakeStartButton();
            MakeSeasonEntry();
            MakeBottomNav();   // PanelButton：顶部资源条 + 底部导航，最后绘制=最上层

            Refresh();

            // 精力倒计时秒级刷新
            var ticker = Root.gameObject.AddComponent<MainCityScript>();
            ticker.panel = this;
        }

        // ==================== 背景 ====================
        private void MakeBackground()
        {
            // 原版主城背景 sp=bg3（赛季主题缺省回退纯色）
            var bgTex = OriginalAssets.GetBackground("bg3");
            var bg = UIHelper.Image(Root, "background", bgTex != null
                ? Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f), 100f)
                : null);
            UIHelper.Stretch((RectTransform)bg.transform);
        }

        // ==================== 精灵看板 ====================
        private void MakeFairyShow()
        {
            // 当前精灵 Spine（dump：skeletonGraphic 位于 (0,-3) 屏中央）
            var fairy = FairySpineMap.SpineFor(SaveManager.Data.currentFairy);
            var sg = SpineView.Play(Root, "skeletonGraphic", fairy, null, true, new Vector2(420, 560));
            if (sg != null)
            {
                UIHelper.Place((RectTransform)sg.transform, new Vector2(0.5f, 0.5f), new Vector2(420, 560), new Vector2(0, -3));
                var anim = sg.skeletonAnimation as Spine.Unity.SkeletonAnimation;
                var names = SpineAssets.AnimationNames(sg.skeletonDataAsset);
                if (anim != null && names.Count > 0)
                {
                    anim.AnimationState.SetAnimation(0, names[0], true);
                }
            }
            else
            {
                // 无 Spine 降级静态立绘
                var icon = UIHelper.Image(Root, "skeletonGraphic", FairySpineMap.IconFor(SaveManager.Data.currentFairy));
                UIHelper.Place((RectTransform)icon.transform, new Vector2(0.5f, 0.5f), new Vector2(420, 560), new Vector2(0, -3));
            }
        }

        // ==================== 关卡宝箱 + 星星宝箱 ====================
        private void MakeChestBoxes()
        {
            // 左：关卡宝箱 (0,-354) 342×121 sp=boxbutton2 →点击开宝箱
            var lv = UIHelper.Button(Root, "imgLevelBox", () => PanelManager.Instance.Push<BoxRewardPanel>());
            UIHelper.Place((RectTransform)lv.transform, new Vector2(0f, 1f), new Vector2(342, 121), new Vector2(0, -354));
            var bg = OriginalAssets.GetUi("boxbutton2");
            if (bg != null) { var img = lv.GetComponent<Image>(); img.sprite = bg; img.type = Image.Type.Sliced; img.color = Color.white; }

            // 宝箱图（baoxiang blue）
            var lvBox = UIHelper.Image(lv.transform, "levelBox", OriginalAssets.GetUi("baoxiang_blue"));
            UIHelper.Place((RectTransform)lvBox.transform, new Vector2(0.5f, 1f), new Vector2(104, 84), new Vector2(55.9f, -29.2f));

            // 进度条（Progress_1 底 + Progress_2 填充 0..1）
            var lvPro = UIHelper.Image(lv.transform, "Progress", OriginalAssets.GetUi("Progress_1"));
            UIHelper.Place((RectTransform)lvPro.transform, new Vector2(0.5f, 0.5f), new Vector2(143.7f, 24.3f), new Vector2(74.8f, -24.8f));
            var lvFill = UIHelper.Image(lvPro.transform, "proLevel", OriginalAssets.GetUi("Progress_2"));
            UIHelper.Stretch((RectTransform)lvFill.transform);
            _levelProFill = lvFill;
            _levelProFill.type = Image.Type.Filled;
            _levelProFill.fillMethod = Image.FillMethod.Horizontal;
            _levelProText = UIHelper.Text(lvPro.transform, "txtLevelPro", "0/5", 18, Color.white, FontStyle.Bold);
            UIHelper.Stretch(_levelProText.rectTransform);

            var lvLabel = UIHelper.Text(lv.transform, "Label", "关卡宝箱", 24, new Color(0.45f, 0.25f, 0.08f), FontStyle.Bold);
            UIHelper.Place(lvLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(120, 40), new Vector2(131.2f, 21.4f));

            // 右：星星宝箱 (3.5,-356.1) 342×121 锚右
            var st = UIHelper.Button(Root, "imgStarBox", () => PanelManager.Instance.Push<BoxRewardPanel>());
            UIHelper.Place((RectTransform)st.transform, new Vector2(1f, 1f), new Vector2(342, 121), new Vector2(3.5f, -356.1f));
            var sbg = OriginalAssets.GetUi("boxbutton2");
            if (sbg != null) { var img = st.GetComponent<Image>(); img.sprite = sbg; img.type = Image.Type.Sliced; img.color = Color.white; }

            var stBox = UIHelper.Image(st.transform, "starBox", OriginalAssets.GetUi("baoxiang_purple"));
            UIHelper.Place((RectTransform)stBox.transform, new Vector2(0.5f, 1f), new Vector2(106, 86), new Vector2(-59.7f, -28.6f));

            var stPro = UIHelper.Image(st.transform, "Progress", OriginalAssets.GetUi("Progress_1"));
            UIHelper.Place((RectTransform)stPro.transform, new Vector2(0.5f, 0.5f), new Vector2(143.7f, 24.3f), new Vector2(-78.6f, -25.1f));
            var stFill = UIHelper.Image(stPro.transform, "proStar", OriginalAssets.GetUi("Progress_2"));
            UIHelper.Stretch((RectTransform)stFill.transform);
            _starProFill = stFill;
            _starProFill.type = Image.Type.Filled;
            _starProFill.fillMethod = Image.FillMethod.Horizontal;
            _starProText = UIHelper.Text(stPro.transform, "txtStarPro", "0/1000", 18, Color.white, FontStyle.Bold);
            UIHelper.Stretch(_starProText.rectTransform);

            var stLabel = UIHelper.Text(st.transform, "Label", "星星宝箱", 24, new Color(0.45f, 0.25f, 0.08f), FontStyle.Bold);
            UIHelper.Place(stLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(120, 40), new Vector2(-136.7f, 20.6f));
        }

        // ==================== 两侧功能按钮 ====================
        private void MakeSideButtons()
        {
            // 左列（锚左上）：分享奖励 / 超值豪礼 / 朋友圈
            MakeSideButton(118, -487, "btnShareReward", "basket-gift", "分享奖励", () => ShareReward());
            MakeSideButton(118, -671, "btnSuperGift", "luxurygifts", "超值豪礼", () => PanelManager.Instance.Push<SuperGiftPanel>());
            MakeSideButton(118, -855, "btnOpenGamePage", "pengyouquan", "朋友圈", () => ShareMoments());
            // 右列（锚右上）：在线奖励 / 每日挑战 / 福利礼盒
            MakeSideButton(-118, -487, "btnTimeReward", "gift", "在线奖励", () => PanelManager.Instance.Push<TimeRewardPanel>());
            MakeSideButton(-118, -671, "btnDailyChallenge", "dailychallengeicon", "每日挑战", () => PanelManager.Instance.Push<DailyChallengePanel>());
            MakeSideButton(-118, -855, "btnWelfare", "fuli", "福利礼盒", () => PanelManager.Instance.Push<WelfareViewPanel>());
        }

        /// <summary>功能按钮：Home_bg01 底 + 图标 + Home_bar_01 文字条（原版 160×162，图标 (0,2) 文字条 (0,-55)）。</summary>
        private void MakeSideButton(float x, float y, string name, string icon, string label, UnityEngine.Events.UnityAction onClick)
        {
            var btn = UIHelper.Button(Root, name, () => onClick());
            var anchor = x < 0 ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            UIHelper.Place((RectTransform)btn.transform, anchor, new Vector2(160, 162), new Vector2(x, y));
            var bg = OriginalAssets.GetUi("Home_bg01");
            if (bg != null) { var img = btn.GetComponent<Image>(); img.sprite = bg; img.color = Color.white; }

            var ic = UIHelper.Image(btn.transform, "Icon", OriginalAssets.GetUi(icon));
            UIHelper.Place((RectTransform)ic.transform, new Vector2(0.5f, 0.5f), new Vector2(115, 115), new Vector2(0, 2));

            var bar = UIHelper.Image(btn.transform, "Bar", OriginalAssets.GetUi("Home_bar_01"));
            UIHelper.Place((RectTransform)bar.transform, new Vector2(0.5f, 0.5f), new Vector2(148, 57), new Vector2(0, -55));

            var t = UIHelper.Text(btn.transform, "Label", label, 22, new Color(1f, 0.95f, 0.7f), FontStyle.Bold);
            UIHelper.Place(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(120, 45), new Vector2(0, -57));
            var o = t.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0.3f, 0.12f, 0.05f);
            o.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private void ShareReward()
        {
            SharePromptPanel.Show("分享奖励", "分享给好友一起领奖！", "就你会消除", "快来一起通关拿奖励！", () =>
            {
                SaveManager.AddCoins(500);
                Refresh();
                Debug.Log("[主城] 分享成功 +500 金币");
            });
        }

        private void ShareMoments()
        {
            // 朋友圈：走平台分享通道（微信=shareTimeline，编辑器=模拟）
            PlatformService.Current.ShareTimeline("就你会消除", () =>
            {
                UIHint.Inst.Show("已分享到朋友圈");
            });
        }

        // ==================== 开始游戏 ====================
        private void MakeStartButton()
        {
            // btnStartGame (0,-365) 432×148.5 sp=greenbg
            var btn = UIHelper.Button(Root, "btnStartGame", OnClickStart);
            UIHelper.Place((RectTransform)btn.transform, new Vector2(0.5f, 0.5f), new Vector2(432, 148.5f), new Vector2(0, -365));
            var bg = OriginalAssets.GetUi("greenbg");
            if (bg != null) { var img = btn.GetComponent<Image>(); img.sprite = bg; img.color = Color.white; }

            _levelText = UIHelper.Text(btn.transform, "txtLevel", "关卡 1", 46, new Color(1f, 0.98f, 0.8f), FontStyle.Bold);
            UIHelper.Stretch(_levelText.rectTransform);
            var o = _levelText.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0.2f, 0.45f, 0.2f);
            o.effectDistance = new Vector2(2, -2);
        }

        private void OnClickStart()
        {
            // 原版：开始游戏直接进关（消耗 1 精力，不足弹精力不足）
            if (!SaveManager.TryConsumeSpirit())
            {
                PanelManager.Instance.Push<SpiritExpendPanel>();
                return;
            }
            int id = NextLevelId();
            Debug.Log($"[主城] 开始游戏 → 关卡 {id}");
            PanelManager.Instance.Push<GameplayPanel>(gp => gp.LevelId = id);
        }

        /// <summary>下一关 Id（全局序 maxPassedLevel+1）。</summary>
        private static int NextLevelId()
        {
            var all = LevelSegmentModel.All;
            if (all.Count == 0) return 101;
            int idx = Mathf.Clamp(SaveManager.Data.maxPassedLevel + 1, 0, all.Count - 1);
            return all[idx].Id;
        }

        /// <summary>原版段内显示号：Id>=1000 ? Id%1000 : Id%100（101→1，199→99，1100→100）。</summary>
        private static int DisplayNo(int id) => id >= 1000 ? id % 1000 : id % 100;

        // ==================== 赛季入口 ====================
        private void MakeSeasonEntry()
        {
            // btnSeason (0,-165) 374.7×128.8 入口框
            var btn = UIHelper.Button(Root, "btnSeason", () => PanelManager.Instance.Push<SeasonPanel>());
            UIHelper.Place((RectTransform)btn.transform, new Vector2(0.5f, 0.5f), new Vector2(374.7f, 128.8f), new Vector2(0, -165));
            var frame = OriginalAssets.GetUi("entrance_frame");
            if (frame != null) { var img = btn.GetComponent<Image>(); img.sprite = frame; img.color = Color.white; }

            // 赛季名牌（subheading）+"梦之花"
            var plate = UIHelper.Image(btn.transform, "imgSeasonName", OriginalAssets.GetUi("subheading"));
            UIHelper.Place((RectTransform)plate.transform, new Vector2(0.5f, 0.5f), new Vector2(182.6f, 40.6f), new Vector2(-9.2f, 26.4f));
            var nameT = UIHelper.Text(btn.transform, "txtSeasonName", "梦之花", 22, new Color(1f, 0.97f, 0.75f), FontStyle.Bold);
            UIHelper.Place(nameT.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(170f, 34f), new Vector2(-9.2f, 26.4f));
            nameT.gameObject.AddComponent<Outline>().effectColor = new Color(0.35f, 0.18f, 0.06f);

            // 倒计时（赛季剩余天数，SeasonPanel 接管详情）
            var cd = UIHelper.Text(btn.transform, "txtCountDown", "", 20, new Color(1f, 0.96f, 0.8f));
            UIHelper.Place(cd.rectTransform, new Vector2(0.5f, 0f), new Vector2(290f, 40f), new Vector2(0, 14));
            cd.gameObject.AddComponent<Outline>().effectColor = new Color(0.3f, 0.15f, 0.05f);
        }

        // ==================== PanelButton：资源条 + 底部导航 ====================
        private void MakeBottomNav()
        {
            // 顶部资源条（dump PanelButton：btnSet (69,-136) / 精力 (209,-136) / 金币 (311,-136) / 星星 (492,-136)）
            MakeSetButton();

            // ==== 精力条 ====
            var spirit = UIHelper.Button(Root, "imgSpirit", () =>
            {
                // 看视频补 1 精力（原版：弹视频确认框 → 激励视频）
                VideoPromptPanel.Show("当前精力不足，看视频获得 1 点精力", () =>
                {
                    SaveManager.AddSpirit(1);
                    Refresh();
                    UIHint.Inst.Show("精力 +1");
                });
            });
            UIHelper.Place((RectTransform)spirit.transform, new Vector2(0f, 1f), new Vector2(152, 59), new Vector2(209, -136));
            var spBg = OriginalAssets.GetUi("buttonback");
            if (spBg != null) { var img = spirit.GetComponent<Image>(); img.sprite = spBg; img.color = Color.white; }
            var life = UIHelper.Image(spirit.transform, "Life", OriginalAssets.GetUi("lifeicon"));
            UIHelper.Place((RectTransform)life.transform, new Vector2(0.5f, 0.5f), new Vector2(52, 52), new Vector2(-58, 0));
            var spMark = UIHelper.Image(spirit.transform, "SpiritMark", OriginalAssets.GetUi("jmtili"));
            UIHelper.Place((RectTransform)spMark.transform, new Vector2(0.5f, 0.5f), new Vector2(32, 33), new Vector2(-46, -15));
            _spiritText = UIHelper.Text(spirit.transform, "txtSpirit", "", 22, Color.white, FontStyle.Bold);
            UIHelper.Place(_spiritText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(50, 50), new Vector2(-62, 4));
            _spiritTime = UIHelper.Text(spirit.transform, "txtTime", "", 17, Color.white, FontStyle.Normal, TextAnchor.MiddleLeft);
            UIHelper.Place(_spiritTime.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(100, 40), new Vector2(22, 0));

            // ==== 金币条 ====
            var gold = UIHelper.Button(Root, "imgGold", () => PanelManager.Instance.Push<StorePanel>());
            UIHelper.Place((RectTransform)gold.transform, new Vector2(0f, 1f), new Vector2(152, 59), new Vector2(311.4f, -136));
            var gBg = OriginalAssets.GetUi("buttonback");
            if (gBg != null) { var img = gold.GetComponent<Image>(); img.sprite = gBg; img.color = Color.white; }
            var goldIcon = UIHelper.Image(gold.transform, "Gold", OriginalAssets.GetUi("gold_big"));
            UIHelper.Place((RectTransform)goldIcon.transform, new Vector2(0.5f, 0.5f), new Vector2(52, 52), new Vector2(-56, 0));
            _coinText = UIHelper.Text(gold.transform, "txtGold", "", 22, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place(_coinText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(105, 50), new Vector2(14, 0));

            // ==== 星星条 ====
            var star = UIHelper.Button(Root, "imgStar", () => PanelManager.Instance.Push<RankViewPanel>());
            UIHelper.Place((RectTransform)star.transform, new Vector2(0f, 1f), new Vector2(152, 59), new Vector2(492.2f, -136));
            var sBg = OriginalAssets.GetUi("buttonback");
            if (sBg != null) { var img = star.GetComponent<Image>(); img.sprite = sBg; img.color = Color.white; }
            var starIcon = UIHelper.Image(star.transform, "Star", OriginalAssets.GetUi("xingxinglogo"));
            UIHelper.Place((RectTransform)starIcon.transform, new Vector2(0.5f, 0.5f), new Vector2(52, 52), new Vector2(-54, 0));
            _starText = UIHelper.Text(star.transform, "txtStar", "", 22, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
            UIHelper.Place(_starText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(105, 50), new Vector2(17, 0));

            // ==== 底部 5 键导航（原版 BtnParent 底部 160 高，均分 5 栏）====
            string[] icons = { "Home_icon_shop", "Home-icon-jingling", "Home_icon_home", "rankiconbottom", "Home_icon_collection" };
            string[] labels = { "Home_icon_shop_wenzi", "Home-icon-jingling_wenzi", "Home_icon_home_wenzi", "paihang", "Home_icon_collection_wenzi" };
            System.Action[] acts =
            {
                () => PanelManager.Instance.Push<StorePanel>(),
                () => PanelManager.Instance.Push<FairyPanel>(),
                () => BackToMainCity(),
                () => PanelManager.Instance.Push<RankViewPanel>(),
                () => PanelManager.Instance.Push<CollectPanel>(),
            };
            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                var slot = UIHelper.Button(Root, "btn" + new[] { "Store", "Fairy", "Home", "Rank", "Collect" }[i], () => acts[idx]());
                UIHelper.Place((RectTransform)slot.transform, new Vector2(0f, 0f), new Vector2(150, 160), new Vector2(-262.5f + 150 * idx, 80));
                var navBg = OriginalAssets.GetUi("Home_panel_01");
                if (navBg != null) { var img = slot.GetComponent<Image>(); img.sprite = navBg; img.type = Image.Type.Sliced; img.color = Color.white; }

                bool isRank = idx == 3;
                var ic = UIHelper.Image(slot.transform, "Icon", OriginalAssets.GetUi(icons[idx]));
                UIHelper.Place((RectTransform)ic.transform, new Vector2(0.5f, 0.5f), isRank ? new Vector2(77, 104) : new Vector2(120, 124), new Vector2(0, 17.2f));
                var lb = UIHelper.Image(slot.transform, "Label", OriginalAssets.GetUi(labels[idx]));
                UIHelper.Place((RectTransform)lb.transform, new Vector2(0.5f, 0.5f), new Vector2(88, 48), new Vector2(0, -62.9f));
            }
        }

        private void BackToMainCity()
        {
            while (PanelManager.Instance.Count > 1) PanelManager.Instance.Pop();
        }

        private void MakeSetButton()
        {
            // btnSet (69,-136) 72×73 sp=shezhi → 设置
            var set = UIHelper.Button(Root, "btnSet", () => PanelManager.Instance.Push<SetViewPanel>());
            UIHelper.Place((RectTransform)set.transform, new Vector2(0f, 1f), new Vector2(72, 73), new Vector2(69, -136));
            var ic = OriginalAssets.GetUi("shezhi");
            if (ic != null) { var img = set.GetComponent<Image>(); img.sprite = ic; img.color = Color.white; }
        }

        // ==================== 数据刷新 ====================
        public override void Refresh()
        {
            if (_coinText != null) _coinText.text = SaveManager.Data.coins.ToString();
            if (_starText != null) _starText.text = SaveManager.Data.stars.ToString();
            if (_spiritText != null) _spiritText.text = SaveManager.Spirit.ToString();

            if (_levelText != null)
                _levelText.text = "关卡 " + DisplayNo(NextLevelId());

            // 宝箱进度：关卡宝箱=每 5 关小结算；星星宝箱=累计星星/1000
            int lvDone = SaveManager.Data.stars % 5;
            if (_levelProText != null) _levelProText.text = lvDone + "/5";
            if (_levelProFill != null) _levelProFill.fillAmount = lvDone / 5f;
            int starTot = Mathf.Min(SaveManager.Data.stars, 1000);
            if (_starProText != null) _starProText.text = starTot + "/1000";
            if (_starProFill != null) _starProFill.fillAmount = starTot / 1000f;

            UpdateCountdown();
        }

        /// <summary>秒级刷新精力倒计时（MainCityScript 调，1 秒一跳）。</summary>
        public void Tick(float dt)
        {
            _tickAcc += dt;
            if (_tickAcc >= 1f)
            {
                _tickAcc = 0f;
                UpdateCountdown();
            }
        }

        private void UpdateCountdown()
        {
            if (_spiritTime == null) return;
            var s = SaveManager.Data;
            if (s.spirit >= SaveManager.SpiritMax)
            {
                _spiritTime.text = "已满";
                return;
            }
            int sec = Mathf.CeilToInt(SaveManager.SpiritRecoverLeft);
            _spiritTime.text = string.Format("{0:00}:{1:00}", sec / 60, sec % 60);
        }
    }
}