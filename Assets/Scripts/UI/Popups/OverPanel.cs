using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>关卡结算弹窗（1:1 复刻 res_Over_12490，winPanel/failPanel 按 Win 切换）。</summary>
    public sealed class OverPanel : UIPanel
    {
        public bool Win;
        public int LevelNo;
        public int Stars;
        public System.Action OnNext, OnRetry, OnHome;

        private GameObject _winPanel;
        private GameObject _failPanel;
        private Text _txtWinTip;
        private Text _txtScore;

        protected override bool BlockClick => true;

        protected override void Build()
        {
            var backgroup = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch((RectTransform)backgroup.transform);
            backgroup.color = new Color(0f, 0f, 0f, 0.55f);

            var panel = StretchNode(Root, "Panel");

            BuildWin(panel);
            BuildFail(panel);
        }

        private void BuildWin(Transform parent)
        {
            var winPanel = Node(parent, "winPanel", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -15f), V(570f, 645f));
            _winPanel = winPanel.gameObject;

            // backGroupSkeleton1 → spine 占位（6 个 Renderer 全 inactive）
            SpinePlaceholder(winPanel, "backGroupSkeleton1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 100f), V(100f, 100f));

            Node(winPanel, "elvesPos", V(0.5f, 0.5f), V(0.5f, 0.5f), V(-187f, 342f), V(10f, 10f));

            var elvesLock = UiImg(winPanel, "elvesLock", "unlock ldd", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-185.7f, 433f), V(292f, 271f));
            elvesLock.rectTransform.localScale = new Vector3(0.6f, 0.6f, 1f);
            elvesLock.gameObject.SetActive(false);

            // backGroupSkeleton2 → spine 占位（5 个 Renderer）
            SpinePlaceholder(winPanel, "backGroupSkeleton2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 100f), V(100f, 100f));

            // view1（默认显示）
            var view1 = Node(winPanel, "view1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 15f), V(570f, 670f));

            var imgMultiple = UiImg(view1, "imgMultiple", "mp board", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 159f), V(405f, 72f));
            imgMultiple.gameObject.AddComponent<CanvasGroup>();
            var nb = UiImg(imgMultiple.transform, "Image", "number base", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-137.3f, 0f), V(100f, 35f));
            UiImg(nb.transform, "Image", "output_icon_1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-24f, 0f), V(35f, 35f));
            Txt(nb.transform, "txtCurStarMul", "x1.0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(16.5f, 1.4f), V(70f, 40f));
            Txt(nb.transform, "txtNextStarMul", "1.0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(107.1f, 1.4f), V(70f, 40f));
            var nb1 = UiImg(imgMultiple.transform, "Image (1)", "number base", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(22.9f, 0f), V(100f, 35f), V(0f, 0.5f));
            UiImg(nb1.transform, "Image (5)", "picIcon", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-24f, 0f), V(26f, 26f));
            Txt(nb1.transform, "txtCurFragMul", "x1.0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(16.5f, 1.4f), V(70f, 40f));
            Txt(nb1.transform, "txtNextFragMul", "1.0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(107.1f, 1.4f), V(70f, 40f));
            UiImg(imgMultiple.transform, "imgUP1", "green indicator", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-77.1f, 0f), V(17f, 19f));
            UiImg(imgMultiple.transform, "imgUP2", "green indicator", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(135.1f, 0f), V(17f, 19f));
            UiImg(imgMultiple.transform, "Image", "line", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(90f, 60f), V(69f, 2f));
            UiImg(imgMultiple.transform, "Image (2)", "line", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-90f, 60f), V(69f, 2f));
            Txt(imgMultiple.transform, "Text (2)", "再胜一局", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 62f), V(240f, 50f));

            var lightBg = Node(view1, "lightBg", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(76f, 446f), V(476f, 119f));
            lightBg.gameObject.AddComponent<CanvasGroup>();
            var light = Node(lightBg, "light", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(476f, 119f));
            light.gameObject.AddComponent<Image>();          // dump: Image 无 sprite
            light.gameObject.AddComponent<CanvasGroup>();
            light.gameObject.SetActive(false);

            var winNum = Node(lightBg, "winNum", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-20.3f, 14f), V(0f, 125f));
            var wlg = winNum.gameObject.AddComponent<HorizontalLayoutGroup>();
            wlg.spacing = 24f;
            wlg.childAlignment = TextAnchor.MiddleCenter;
            wlg.childForceExpandWidth = false;
            wlg.childForceExpandHeight = false;
            var wcsf = winNum.gameObject.AddComponent<ContentSizeFitter>();
            wcsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var win1 = UiImg(winNum, "win1", "number0-9_0", V(0f, 1f), V(0f, 1f),
                V(44f, -62.5f), V(88f, 95f));
            win1.gameObject.SetActive(false);
            UiImg(winNum, "win2", "number0-9_0", V(0f, 0f), V(0f, 0f), V(0f, 0f), V(88f, 95f));
            UiImg(winNum, "win3", "number0-9_0", V(0f, 0f), V(0f, 0f), V(0f, 0f), V(88f, 95f));

            UiImg(lightBg, "winfont", "liansheng", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(63.4f, 6.7f), V(139f, 75f), V(0f, 0.5f));

            var imgNewRecord = UiImg(lightBg, "imgNewRecord", "record", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(121f, 109f), V(188f, 43f));
            imgNewRecord.gameObject.SetActive(false);

            // view2（dump 默认 inactive）
            var view2 = Node(winPanel, "view2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 15f), V(570f, 670f));
            view2.gameObject.SetActive(false);
            UiImg(view2, "imgWinFont", "chenggong", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(52f, 447f), V(281f, 105f));
            var imgTip = UiImg(view2, "imgTip", "dialogue board", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-151.3f, 245.1f), V(393f, 125f), V(0.1f, 0.9f));
            var txtWinGameOpen = Txt(imgTip.transform, "txtWinGameOpen", "7", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -19.1f), V(400f, 60f));
            txtWinGameOpen.gameObject.SetActive(false);
            var txtElvesUnlock = Txt(imgTip.transform, "txtElvesUnlock", "7", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-11.9f, -19f), V(400f, 60f), true);
            txtElvesUnlock.gameObject.SetActive(false);
            UiImg(txtElvesUnlock.transform, "Image", "output_icon_2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(117.7f, 0f), V(50f, 50f));
            var txtFragNum = Txt(imgTip.transform, "txtFragNum", "7", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -19.1f), V(400f, 50f), true);
            txtFragNum.gameObject.AddComponent<Button>();    // dump: Button
            txtFragNum.gameObject.SetActive(false);
            UiImg(txtFragNum.transform, "Image", "output_icon_2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(6.4f, 0f), V(50f, 50f));

            // imgFlower：7 个 Flower spine 占位
            var imgFlower = Node(winPanel, "imgFlower", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(570f, 700f));
            SpinePlaceholder(imgFlower, "Flower1", V(0.5f, 0.5f), V(0.5f, 0.5f), V(-243f, 318f), V(100f, 100f));
            SpinePlaceholder(imgFlower, "Flower1 (1)", V(0.5f, 0.5f), V(0.5f, 0.5f), V(-165.7f, 298.4f), V(100f, 100f));
            SpinePlaceholder(imgFlower, "Flower1 (4)", V(0.5f, 0.5f), V(0.5f, 0.5f), V(-80.1f, 400.3f), V(100f, 100f),
                new Vector3(1f, -1f, 1f));
            SpinePlaceholder(imgFlower, "Flower2 (1)", V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.7f, 360.9f), V(100f, 100f));
            SpinePlaceholder(imgFlower, "Flower1 (2)", V(0.5f, 0.5f), V(0.5f, 0.5f), V(102.1f, 326.5f), V(100f, 100f));
            SpinePlaceholder(imgFlower, "Flower1 (3)", V(0.5f, 0.5f), V(0.5f, 0.5f), V(216f, 347f), V(100f, 100f));
            SpinePlaceholder(imgFlower, "Flower2 (2)", V(0.5f, 0.5f), V(0.5f, 0.5f), V(293.1f, 301.8f), V(100f, 100f),
                new Vector3(-1f, 1f, 1f));

            // imgStarNum（dump 默认 inactive）
            var imgStarNum = UiImg(winPanel, "imgStarNum", "level_1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 220f), V(443f, 46f));
            imgStarNum.gameObject.SetActive(false);
            Txt(imgStarNum.transform, "Text(1)", "累计收集", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-87f, 0.8f), V(110f, 50f), true);
            UiImg(imgStarNum.transform, "Image", "output_icon_1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(50f, 50f));

            // imgReward
            var imgReward = UiImg(winPanel, "imgReward", "reword_1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -31f), V(471f, 299f));
            imgReward.gameObject.AddComponent<CanvasGroup>();
            UiImg(imgReward.transform, "Image", "reword_2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 125.9f), V(218f, 27f));
            var fragParent = Node(imgReward.transform, "fragParent", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 31.1f), V(400f, 100f));
            var flg = fragParent.gameObject.AddComponent<HorizontalLayoutGroup>();
            flg.spacing = 24f;
            flg.childAlignment = TextAnchor.MiddleCenter;
            flg.childForceExpandWidth = false;
            flg.childForceExpandHeight = false;
            var reward1 = UiImg(fragParent, "reward1", "output_icon_1", V(0f, 0f), V(0f, 0f),
                V(0f, 0f), V(75f, 75f));
            Txt(reward1.transform, "txtStarNum", "+0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -47f), V(120f, 60f));
            var txtAddStar = Txt(reward1.transform, "txtAddStar", "+10", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(48f, 35.8f), V(80f, 40f), true);
            txtAddStar.gameObject.SetActive(false);
            var reward3 = UiImg(fragParent, "reward3", "output_icon_6", V(0f, 1f), V(0f, 1f),
                V(300f, -37.5f), V(75f, 75f));
            reward3.gameObject.SetActive(false);
            Txt(reward3.transform, "txtSkinNum", "+0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -47f), V(120f, 60f));
            var txtAddSkin = Txt(reward3.transform, "txtAddSkin", "+10", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(48f, 35.8f), V(80f, 40f), true);
            txtAddSkin.gameObject.SetActive(false);
            var fragReward = UiImg(imgReward.transform, "fragReward", "output_icon_2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(96.1f, 42f), V(75f, 75f));
            fragReward.gameObject.SetActive(false);
            Txt(fragReward.transform, "txtFragNum", "+0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -47f), V(120f, 60f));
            var txtAddFrag = Txt(fragReward.transform, "txtAddFrag", "+10", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(48f, 35.8f), V(80f, 40f), true);
            txtAddFrag.gameObject.SetActive(false);

            // imgBoxPro（宝箱进度条）
            var imgBoxPro = UiImg(winPanel, "imgBoxPro", "collection_bar01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-19f, -227.5f), V(312f, 34f));
            UiImg(imgBoxPro.transform, "imgProGray", "collection_bar", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(312f, 34f));
            UiImg(imgBoxPro.transform, "imgProgress", "collection_bar", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(312f, 34f));
            var imgBox = UiImg(imgBoxPro.transform, "imgBox", "baoxiang purple", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(178.2f, 5.5f), V(74.2f, 60.2f));
            imgBox.gameObject.AddComponent<Button>();        // dump: Button（无 handler）
            Txt(imgBoxPro.transform, "txtProgress", "0/100", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(6f, 2f), V(240f, 50f), true);
            var txtTipOpenBox = Txt(imgBoxPro.transform, "txtTipOpenBox", "宝箱可开启", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(19f, 34.9f), V(300f, 40f));
            txtTipOpenBox.gameObject.SetActive(false);

            // imgElves（dump 默认 inactive，Image 均无 sprite）
            var imgElves = Node(winPanel, "imgElves", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-260f, -131f), V(166f, 117f));
            imgElves.gameObject.AddComponent<Image>();
            imgElves.gameObject.SetActive(false);
            var elvesGray = Node(imgElves, "elvesGray", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -37.1f), V(123f, 34f));
            elvesGray.gameObject.AddComponent<Image>();
            var elvesPro = Node(imgElves, "elvesPro", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -37.1f), V(123f, 34f));
            elvesPro.gameObject.AddComponent<Image>();

            // imgUnlockTip（dump 默认 inactive，pivot=(0.1,1)）
            var imgUnlockTip = Node(winPanel, "imgUnlockTip", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-156f, -445f), V(360f, 136f), V(0.1f, 1f));
            imgUnlockTip.gameObject.SetActive(false);
            var tipBg = UiImg(imgUnlockTip, "Image (1)", "tip11", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 0f), V(360f, 136f));
            tipBg.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
            Txt(imgUnlockTip, "txtUnlockTip", "排行榜已解锁，和他人比拼实力", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -5.7f), V(296.9f, 88.7f), true);

            // btnReward3Video
            var btnReward3Video = ImgBtn(winPanel, "btnReward3Video", "Btn_01", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -112.9f), V(240f, 80f),
                () => { OnNext?.Invoke(); PanelManager.Instance.Pop(); });
            var videoImg = UiImg(btnReward3Video.transform, "Image", "video", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-60.7f, -1.4f), V(47f, 40f));
            videoImg.rectTransform.localScale = new Vector3(0.8f, 0.8f, 1f);
            Txt(btnReward3Video.transform, "Text (2)", "领取   倍", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(22f, 1f), V(140f, 45f), true);
            Txt(btnReward3Video.transform, "Text (3)", "3", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(36.4f, 1f), V(40f, 60f), true);

            // btnBackWin（dump 默认 inactive）
            var btnBackWin = ImgBtn(winPanel, "btnBackWin", "bule", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-158.6f, -377f), V(198f, 95f),
                () => { OnHome?.Invoke(); PanelManager.Instance.Pop(); });
            UiImg(btnBackWin.transform, "Image", "tanchuang_0003_zhuye", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -1f), V(87f, 48f));
            btnBackWin.gameObject.SetActive(false);

            // ContinueWin（dump 默认 inactive）
            var continueWin = ImgBtn(winPanel, "ContinueWin", "Btn_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(114.4f, -377f), V(300f, 95f),
                () => { OnNext?.Invoke(); PanelManager.Instance.Pop(); });
            UiImg(continueWin.transform, "Image", "tanchuang_0005_xiayiguan", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -1f), V(129f, 48f));
            continueWin.gameObject.SetActive(false);

            _txtWinTip = Txt(winPanel, "txtWinTip", "关卡1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 301f), V(200f, 60f), true);

            var scoreItem = UiImg(winPanel, "scoreItem", "buttonback", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-200f, 595.1f), V(186f, 87.4f));
            UiImg(scoreItem.transform, "aixin", "output_icon_1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-52.4f, 1.4f), V(50f, 50f));
            _txtScore = Txt(scoreItem.transform, "txtScore", "0", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(26f, 1f), V(120f, 50f), true);
        }

        private void BuildFail(Transform parent)
        {
            var failPanel = UiImg(parent, "failPanel", "Bg_02di", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 8f), V(575f, 586f));
            failPanel.gameObject.SetActive(false);
            _failPanel = failPanel.gameObject;

            // BlueSkeletonGraphic → spine 占位
            SpinePlaceholder(failPanel.transform, "BlueSkeletonGraphic", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 288f), V(100f, 100f));

            UiImg(failPanel.transform, "Image (1)", "Bg_nei2", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 18f), V(510f, 302.6f));
            Txt(failPanel.transform, "txtTitle", "失败", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 286f), V(300f, 80f), true);
            UiImg(failPanel.transform, "imgPatternFail", "one-more-chance-star", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-9f, -4f), V(191f, 229f));

            var btnBackFail = ImgBtn(failPanel.transform, "btnBackFail", "bule", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-161.1f, -206f), V(190f, 95f),
                () => { OnHome?.Invoke(); PanelManager.Instance.Pop(); });
            UiImg(btnBackFail.transform, "Image", "tanchuang_0003_zhuye", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, -1f), V(87f, 48f));

            var continueLose = ImgBtn(failPanel.transform, "ContinueLose", "Btn_02", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(109f, -206f), V(300f, 95f),
                () => { OnRetry?.Invoke(); PanelManager.Instance.Pop(); });
            UiImg(continueLose.transform, "Image", "tanchuang_0001_chongwan", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(34f, -1f), V(88f, 49f));
            UiImg(continueLose.transform, "Image", "output_icon_5", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-66.6f, -1.1f), V(90f, 90f));
            Txt(continueLose.transform, "txtHp", "-1", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-71.7f, 1.8f), V(60f, 70f), true);

            Txt(failPanel.transform, "txtFailTip", "还差一点点!", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(0f, 138f), V(340f, 65f));

            var spiritItem = UiImg(failPanel.transform, "spiritItem", "buttonback", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-200f, 502f), V(186f, 87.4f));
            UiImg(spiritItem.transform, "aixin", "output_icon_5", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(-52.4f, 1.4f), V(50f, 50f));
            ImgBtn(spiritItem.transform, "btnSpirit", "jmtili", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(60.4f, 1.4f), V(46f, 47f), null);
            Txt(spiritItem.transform, "txtSpirit", "5", V(0.5f, 0.5f), V(0.5f, 0.5f),
                V(3.6f, 3.6f), V(80f, 60f));
        }

        public override void Refresh()
        {
            if (_winPanel != null) _winPanel.SetActive(Win);
            if (_failPanel != null) _failPanel.SetActive(!Win);
            if (_txtWinTip != null) _txtWinTip.text = "关卡" + LevelNo;
            if (_txtScore != null) _txtScore.text = Stars.ToString();
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

        private static RectTransform SpinePlaceholder(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Vector3? scale = null)
        {
            var rt = Node(parent, name, aMin, aMax, pos, size);   // TODO spine: SkeletonGraphic 占位
            if (scale.HasValue) rt.localScale = scale.Value;
            UIHelper.NewRect(rt, "Renderer0");                    // TODO spine: 渲染占位
            return rt;
        }
    }
}
