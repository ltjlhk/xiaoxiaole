using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xio.Assets;
using Xio.Game;

namespace Xio.UI
{
    /// <summary>
    /// 赛季任务面板（dump res_SeasonTask_12754 1:1）：赛季皮肤/精灵展示 + 活跃度 + 任务列表（进度/领取）。
    /// 任务数据取 SeasonTaskConfig（解析方式与 SeasonPanel 一致），领取走 SaveManager.ClaimSeasonTask。
    /// </summary>
    public sealed class SeasonTaskPanel : UIPanel
    {
        private RectTransform _taskList;
        private List<SeasonTaskInfo> _tasks = new List<SeasonTaskInfo>();

        protected override bool BlockClick => true;

        protected override void Build()
        {
            // backgroup / bg / Panel
            var back = UIHelper.Image(Root, "backgroup");
            UIHelper.Stretch(back.rectTransform);
            back.color = new Color(0f, 0f, 0f, 0.55f);
            var bgOuter = UIHelper.Image(Root, "bg");
            UIHelper.Stretch(bgOuter.rectTransform);
            var panel = UIHelper.NewRect(Root, "Panel");
            UIHelper.Stretch(panel);

            var bg = UIHelper.Image(panel, "bg");
            UIHelper.Stretch(bg.rectTransform);

            // ===== 顶部标题板 =====
            var titleBG = ImgX(bg.transform, "titleBG", "tittle base",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(716, 135.3f), new Vector2(-21.2f, -149));
            Txt(titleBG.transform, "txtDate", "XY月YZ日-XY月ZY日", Fs(45.1f), new Vector2(439.3f, 45.1f), new Vector2(19.9f, -85.5f), true);
            Txt(titleBG.transform, "txtStartEndTime", "", Fs(45.1f), new Vector2(439.3f, 45.1f), new Vector2(19.9f, -117), true);
            UIHelper.Image(titleBG.transform, "imgSeasonName");
            Rt((RectTransform)titleBG.transform.Find("imgSeasonName"),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(524.8f, 116.5f), new Vector2(25.7f, -9));

            // Title 组（默认隐藏：图 + 数字 + 尾图）
            var title = Box(bg.transform, "Title", new Vector2(360.6f, 86.1f), new Vector2(21.2f, -2));
            var hlg = title.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            title.gameObject.AddComponent<ContentSizeFitter>();
            var imgTitle = ImgX(title, "imgTitle", "title2",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                new Vector2(62, 60), new Vector2(31, -43.1f));
            var num = TxtX(title, "txtTitle", "1", Fs(86.1f),
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                new Vector2(29, 86.1f), new Vector2(83.3f, -43.1f), true);
            num.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            ImgX(title, "imgTitleStartEnd", "title",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                new Vector2(256, 60), new Vector2(232.6f, -43.1f));
            title.gameObject.SetActive(false);

            // 赛季名（默认隐藏）
            Txt(bg.transform, "txtSeasonName", "森林之息", Fs(77.1f), new Vector2(289.1f, 77.1f), new Vector2(-203, 475), true)
                .gameObject.SetActive(false);

            // ===== 皮肤展示（左） =====
            var imgSkin = Img(bg.transform, "imgSkin", "skin4", new Vector2(170.4f, 179.3f), new Vector2(-199.1f, 241));
            var skinNameBg = Img(imgSkin.transform, "NameBg", "name base", new Vector2(215, 72), new Vector2(0, -115));
            TxtX(skinNameBg.transform, "txtSkinName", "皮肤名字", 20,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(0, -12.9f), new Vector2(0, -8.5f), true);
            var skinTag = Box(skinNameBg.transform, "Image", new Vector2(37.8f, 37.8f), new Vector2(-1.8f, 73.1f));
            Txt(skinTag, "Text (Legacy)", "皮肤", Fs(30), new Vector2(160, 30), Vector2.zero);
            skinTag.gameObject.SetActive(false);
            MakeSlider(imgSkin.transform, "SkinPieceSlider",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(171.9f, 30.3f), new Vector2(6.1f, -171.3f),
                "collection progress bar", "collection progress bar green", 9990, 11111, true);
            imgSkin.transform.Find("SkinPieceSlider").gameObject.SetActive(false);
            MakeUnlock(imgSkin.transform, "Unlock", new Vector2(0, -182));

            // 背光占位
            UIHelper.Image(bg.transform, "backLight");
            Rt((RectTransform)bg.transform.Find("backLight"),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(219, 217), new Vector2(-200.1f, 245.9f));

            // ===== 精灵展示（右） =====
            var imgFairy = Img(bg.transform, "imgFairy", "Rabbit", new Vector2(179, 260), new Vector2(182.3f, 308));
            Box(imgFairy.transform, "FairyPos", new Vector2(100, 100), new Vector2(0, -77));
            var fairyNameBg = Img(imgFairy.transform, "NameBg", "name base", new Vector2(215, 72), new Vector2(0, -182));
            TxtX(fairyNameBg.transform, "txtFairyName", "精灵名", 20,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(0, -12.9f), new Vector2(0, -8.5f), true);
            var fairyTag = Box(fairyNameBg.transform, "Image", new Vector2(37.8f, 37.8f), new Vector2(-1.8f, 73.1f));
            Txt(fairyTag, "Text (Legacy)", "精灵", Fs(30), new Vector2(160, 30), Vector2.zero);
            fairyTag.gameObject.SetActive(false);
            MakeSlider(imgFairy.transform, "FairyPieceSlider",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(171.9f, 30.3f), new Vector2(6.1f, -238.3f),
                "collection progress bar", "collection progress bar green", 9990, 11111, true);
            imgFairy.transform.Find("FairyPieceSlider").gameObject.SetActive(false);
            MakeUnlock(imgFairy.transform, "Unlock", new Vector2(0, -249));

            // ===== 活跃奖励区 =====
            var rewardBG = ImgX(bg.transform, "rewardBG", "base",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0.5f),
                new Vector2(0, 622), new Vector2(0, 311));
            ImgX(rewardBG.transform, "Pattern", "pattern",
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(0, -160.8f), new Vector2(0, -80.8f));

            // 当前活跃
            var activityBG = ImgX(rewardBG.transform, "activityBG", "activity base",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                new Vector2(277, 61), new Vector2(138.5f, 19.8f));
            Txt(activityBG.transform, "txtActivity", SaveManager.Data.activity.ToString(),
                Fs(48.6f), new Vector2(63.9f, 48.6f), new Vector2(65.1f, -0.4f), true);
            TxtX(activityBG.transform, "Text (Legacy)", "当前活跃:", Fs(54.8f),
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(123.1f, 54.8f), new Vector2(62.1f, -1.8f), true);
            Img(activityBG.transform, "Image (1)", "activity icon", new Vector2(48.7f, 46.6f), new Vector2(8.8f, -1.4f));

            // 下一档大奖励横滑列表
            MakeScroll(rewardBG.transform, "Scroll View",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(0, 150.5f), new Vector2(0, -86), true);

            // 下一档大奖励卡
            var bigPrice = ImgX(rewardBG.transform, "BigPrice", "next  gift base",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(146, 180), new Vector2(-72.5f, -82.1f));
            Img(bigPrice.transform, "Effect", "Ⅱ gift light", new Vector2(131.2f, 131.2f), Vector2.zero);
            var itemIcon = Img(bigPrice.transform, "ItemIcon", "gift icon", new Vector2(101, 89), new Vector2(-2.7f, -4));
            TxtX(itemIcon.transform, "txtItemCount", "X99", 20,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(12.3f, -7.2f), new Vector2(13.8f, -7.4f), true);
            TxtX(bigPrice.transform, "txtActivity", "", 20,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f),
                new Vector2(125.8f, 44.9f), new Vector2(8.3f, -155.4f), true);
            var buyAct = Btn(bigPrice.transform, "btnBuyActivity", "Btn_01", new Vector2(134.2f, 40.6f), new Vector2(5.9f, 58),
                () => Debug.Log("[SeasonTask] 购买活跃"));
            TxtX(buyAct.transform, "Text (Legacy)", "购买活跃", 20,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, true);
            UIHelper.Image(bigPrice.transform, "Image (1)").gameObject.SetActive(false);

            // 温馨提示
            var tip = TxtX(rewardBG.transform, "Text (Legacy)", "温馨提示：赛季结束未完成上述进度将错失奖励！", Fs(45.9f),
                new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-102.5f, 45.9f), new Vector2(0, 121.4f), true);
            ImgX(tip.transform, "Image (1)", "de-line",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(50, 10), new Vector2(-5.7f, 2.4f)).transform.localScale = new Vector3(-1, 1, 1);
            ImgX(tip.transform, "Image (2)", "de-line",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(50, 10), new Vector2(3.8f, 2.4f));

            // ===== 任务列表 =====
            _taskList = BoxX(bg.transform, "TaskList",
                new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 143.3f), new Vector2(0, 25.1f));
            var tlg = _taskList.gameObject.AddComponent<VerticalLayoutGroup>();
            tlg.childControlWidth = true; tlg.childControlHeight = false;
            tlg.childForceExpandWidth = true; tlg.childForceExpandHeight = false;
            tlg.spacing = 8;

            // 重置区
            var resetPanel = BoxX(bg.transform, "ResetPanel",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f),
                new Vector2(729, 100), new Vector2(0, 49.6f));
            Txt(resetPanel, "txtReset", "今日剩余<color=red>3</color>次重置任务机会",
                Fs(57.6f), new Vector2(399, 57.6f), new Vector2(-147.1f, 17.7f), true);
            Txt(resetPanel, "txtReset (1)", "(重置后可获得更多活跃度)",
                Fs(57.6f), new Vector2(399, 57.6f), new Vector2(-168, -13.9f), true);
            var resetBtn = Btn(resetPanel, "ResetBtn", "Btn_01", new Vector2(192.7f, 71.7f), new Vector2(262.8f, 3.3f),
                () => Debug.Log("[SeasonTask] 观看广告重置任务"));
            TxtX(resetBtn.transform, "Text (Legacy)", "重置", 20,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(-54.5f, 0), new Vector2(18.3f, 0), true);
            Img(resetBtn.transform, "Image", "video", new Vector2(32.1f, 26.6f), new Vector2(-37.4f, -2.1f));
            var freeReset = Btn(resetPanel, "FreeResetBtn", "Btn_01", new Vector2(192.7f, 71.7f), new Vector2(262.8f, 3.3f),
                () => Debug.Log("[SeasonTask] 免费重置任务"));
            TxtX(freeReset.transform, "Text (Legacy)", "重置", 20,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(-17, 0), new Vector2(-0.5f, 0), true);
            Img(freeReset.transform, "Image", "video", new Vector2(32.1f, 26.6f), new Vector2(-37.4f, -2.1f)).gameObject.SetActive(false);
            freeReset.gameObject.SetActive(false);

            // 关闭
            BtnX(panel, "btnClose", "collection_icon_01",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                new Vector2(73, 73), new Vector2(56.5f, -149), () => PanelManager.Instance.Pop());

            // ===== 购买活跃子弹窗（默认隐藏，结构 1:1） =====
            MakeBuyActivity(Root);

            // 引导遮罩与占位（默认隐藏）
            var guide = UIHelper.NewRect(Root, "GuideMask");
            UIHelper.Stretch(guide);
            guide.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            guide.gameObject.SetActive(false);
            var hold = UIHelper.NewRect(Root, "Image");
            UIHelper.Stretch(hold);
            hold.gameObject.SetActive(false);

            // 任务数据与条目
            _tasks = LoadTasks();
            RebuildList();
        }

        /// <summary>任务条目（dump res_SeasonTaskItem_12127 1:1）：名称/进度条/去完成·领取/已完成。</summary>
        private void MakeTaskItem(Transform parent, SeasonTaskInfo tk)
        {
            var item = Img(parent, "SeasonTaskItem_" + tk.Id, "task base", new Vector2(721, 92), Vector2.zero);
            int prog = TaskTracker.ProgressOf(tk.Id);
            bool claimed = SaveManager.IsTaskClaimed(tk.Id);
            bool done = prog >= tk.TaskTarget;

            // StatusBg（done 角标）
            Img(item.transform, "StatusBg", "done", new Vector2(135, 86.4f), new Vector2(-290.9f, 0)).gameObject.SetActive(claimed);

            // Activity（+奖励）
            var act = ImgX(item.transform, "Activity", "activity icon",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(41.2f, 39.4f), new Vector2(44.2f, -4));
            Txt(act.transform, "txtGetActivity", "+" + tk.TaskReward, Fs(65), new Vector2(49.9f, 65), new Vector2(39.6f, -6.2f), true);

            // TaskInfo（名称 + 进度 + 进度条）
            var info = Box(item.transform, "TaskInfo", new Vector2(371.7f, 90), new Vector2(-31.6f, 0));
            TxtX(info, "txtTaskName", string.IsNullOrEmpty(tk.TaskTxt) ? "赛季任务" : tk.TaskTxt, Fs(47.6f),
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                new Vector2(420.8f, 47.6f), new Vector2(219.4f, -31.6f), true);
            TxtX(info, "txtProcess", prog + "/" + tk.TaskTarget, Fs(50.5f),
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(179, 50.5f), new Vector2(-89, -31.6f), true);
            MakeSlider(info, "Slider",
                new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.9f, 44.6f), new Vector2(0, -19.1f),
                "progress bar 0", "progress bar 1", prog, tk.TaskTarget, true);

            // CompleteBtn（去完成 / 领取）
            System.Action click;
            string label;
            if (claimed) { label = "去完成"; click = () => Debug.Log("[SeasonTask] 任务已完成并领取"); }
            else if (done)
            {
                int id = tk.Id, coins = tk.TaskReward;
                label = "领取";
                click = () => { if (SaveManager.ClaimSeasonTask(id, coins)) RebuildList(); };
            }
            else { label = "去完成"; click = () => Debug.Log("[SeasonTask] 前往完成任务 " + tk.Id); }
            var completeBtn = BtnX(item.transform, "CompleteBtn", "blackbg",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(131.1f, 49.4f), new Vector2(-72.6f, 0), click);
            Txt(completeBtn.transform, "txtComplete", label, Fs(36.1f), new Vector2(97.4f, 36.1f), Vector2.zero, true);
            ImgX(completeBtn.transform, "imgShare", "share",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                new Vector2(24.1f, 25.7f), new Vector2(29.1f, -24.7f)).gameObject.SetActive(false);
            if (claimed) completeBtn.gameObject.SetActive(false);

            // 遮罩与已完成角标
            var mask = Img(item.transform, "Mask", "task base", new Vector2(721, 92), Vector2.zero);
            Rt(mask.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            mask.gameObject.SetActive(claimed);
            var completed = Img(item.transform, "Completed", "grey", new Vector2(121, 49.9f), new Vector2(287.7f, -0.2f));
            Txt(completed.transform, "Text (Legacy)", "已完成", Fs(49.4f), new Vector2(115.9f, 49.4f), Vector2.zero, true);
            Img(completed.transform, "Image (1)", "pet_icon_Equipped", new Vector2(31.1f, 25.9f), new Vector2(-33.4f, -3.3f)).gameObject.SetActive(false);
            completed.gameObject.SetActive(claimed);
            Img(item.transform, "MoveActivity", "activity icon", new Vector2(68, 65), new Vector2(-281.4f, -1.8f)).gameObject.SetActive(false);
        }

        /// <summary>重建任务列表（领取后刷新状态）。</summary>
        private void RebuildList()
        {
            if (_taskList == null) return;
            for (int i = _taskList.childCount - 1; i >= 0; i--)
                SafeDestroy(_taskList.GetChild(i).gameObject);
            foreach (var tk in _tasks)
                MakeTaskItem(_taskList, tk);
        }

        /// <summary>“已拥有”角标（皮肤/精灵共用）。</summary>
        private static void MakeUnlock(Transform parent, string name, Vector2 pos)
        {
            var unlock = Txt(parent, name, "已拥有", Fs(62), new Vector2(90.8f, 62), pos, true);
            ImgX(unlock.transform, "Image (1)", "pet_icon_Equipped",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(35.9f, 29.9f), new Vector2(18, -4.8f));
        }

        /// <summary>购买活跃子弹窗（dump BuyActivity 子树，默认隐藏）。</summary>
        private void MakeBuyActivity(Transform parent)
        {
            var root = Box(parent, "BuyActivity", Vector2.zero, Vector2.zero);
            Rt(root, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var back = UIHelper.Image(root, "backgroup");
            UIHelper.Stretch(back.rectTransform);
            var buyPanel = UIHelper.NewRect(root, "Panel");
            UIHelper.Stretch(buyPanel);

            var board = Img(buyPanel, "Image", "Bg_01di", new Vector2(577.6f, 521.7f), new Vector2(0, -5));
            var rewardBg = ImgX(board.transform, "RewardBg", "Bg_nei2",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(511.1f, 291.6f), new Vector2(3, -143));
            Txt(rewardBg.transform, "txtReward", "购买活跃至<color=#DA1314>300</color>,可以领取以上奖励",
                Fs(64), new Vector2(473.9f, 64), new Vector2(0, -95), true).gameObject.SetActive(false);
            MakeScroll(rewardBg.transform, "Scroll View",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(473.9f, 171.1f), new Vector2(0, -29.2f), true);
            TxtX(rewardBg.transform, "txtNull", "无", 20,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(0, -64), new Vector2(0, -1), true);

            var spine = BoxX(board.transform, "BlueSkeletonGraphic",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(100, 100), new Vector2(3, 22.6f));
            Box(spine, "Renderer0", new Vector2(100, 100), Vector2.zero);
            Box(spine, "Renderer1", new Vector2(100, 100), Vector2.zero).gameObject.SetActive(false);
            Box(spine, "Renderer2", new Vector2(100, 100), Vector2.zero).gameObject.SetActive(false);
            BtnX(spine, "btnClose", "tanchuang_0017_guangbianniu",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(40, 39), new Vector2(229, -107.2f), () => root.gameObject.SetActive(false));

            var buyGold = BtnX(board.transform, "btnBuyGold", "Btn_02",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f),
                new Vector2(339.4f, 93.4f), new Vector2(0, 220.2f), () => Debug.Log("[SeasonTask] 金币购买活跃"));
            TxtX(buyGold.transform, "Text", "购买", 20,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                new Vector2(70, 52), new Vector2(106, -46.7f), true).gameObject.AddComponent<ContentSizeFitter>();
            ImgX(buyGold.transform, "Image (1)", "gold_con",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                new Vector2(45.1f, 46.8f), new Vector2(172.3f, -46.7f));
            TxtX(buyGold.transform, "Text (Legacy)", "999", 20,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                new Vector2(63, 52), new Vector2(235.1f, -46.7f), true);
            buyGold.gameObject.SetActive(false);

            var buyAd = BtnX(board.transform, "btnBuyAd", "Btn_01",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f),
                new Vector2(339.4f, 93.4f), new Vector2(0, 106), () => Debug.Log("[SeasonTask] 观看广告购买活跃"));
            Img(buyAd.transform, "imgSpirit", "video", new Vector2(47, 39), new Vector2(-97, 0));
            Txt(buyAd.transform, "txtState", "购买(0/3)", Fs(76), new Vector2(191.6f, 76), new Vector2(22.3f, 0), true);

            TxtX(board.transform, "txtExplain", "购买活跃30后，可领取以上奖励", Fs(80),
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f),
                new Vector2(457.1f, 80), new Vector2(0, -317.1f), true);
            TxtX(board.transform, "txtTitle", "购买活跃", Fs(100),
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f),
                new Vector2(240, 100), new Vector2(0, 16), true);
            Txt(board.transform, "Text (Legacy)", "满3次广告可获得活跃", Fs(40.2f), new Vector2(349.9f, 40.2f), new Vector2(0, -221.6f), true);

            root.gameObject.SetActive(false);
        }

        /// <summary>SeasonTaskConfig 解析（与 SeasonPanel 同款：LoadRaw + SplitTopLevel + FromJson）。</summary>
        private static List<SeasonTaskInfo> LoadTasks()
        {
            var list = new List<SeasonTaskInfo>();
            var raw = ConfigLoader.LoadRaw("SeasonTaskConfig");
            if (!string.IsNullOrEmpty(raw))
                foreach (var kv in ConfigLoader.SplitTopLevel(raw))
                {
                    var tk = ConfigLoader.FromJson<SeasonTaskInfo>(kv.Value);
                    if (tk != null) list.Add(tk);
                }
            list.Sort((a, b) => a.Id.CompareTo(b.Id));
            return list;
        }

        public override void Refresh()
        {
            _tasks = LoadTasks();
            RebuildList();
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }

        // ===== 搭建小工具（基于 UIHelper） =====

        private static int Fs(float h) => Mathf.Clamp(Mathf.RoundToInt(h / 2.2f), 20, 44);

        private static RectTransform Rt(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot,
            Vector2 size, Vector2 pos)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            return rt;
        }

        private static RectTransform Box(Transform parent, string name, Vector2 size, Vector2 pos)
            => Rt(UIHelper.NewRect(parent, name), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), size, pos);

        private static RectTransform BoxX(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos)
            => Rt(UIHelper.NewRect(parent, name), aMin, aMax, pivot, size, pos);

        private static Image Img(Transform parent, string name, string sp, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name, OriginalAssets.GetUi(sp));
            if (img.sprite == null) img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), size, pos);
            return img;
        }

        private static Image ImgX(Transform parent, string name, string sp,
            Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos)
        {
            var img = UIHelper.Image(parent, name, OriginalAssets.GetUi(sp));
            if (img.sprite == null) img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, aMin, aMax, pivot, size, pos);
            return img;
        }

        private static Text Txt(Transform parent, string name, string content, int fontSize,
            Vector2 size, Vector2 pos, bool outline = false, FontStyle style = FontStyle.Normal)
        {
            var t = UIHelper.Text(parent, name, content, fontSize, Color.white, style);
            Rt(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), size, pos);
            if (outline)
            {
                var o = t.gameObject.AddComponent<Outline>();
                o.effectColor = Color.black;
                o.effectDistance = new Vector2(2f, -2f);
            }
            return t;
        }

        private static Text TxtX(Transform parent, string name, string content, int fontSize,
            Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos, bool outline = false)
        {
            var t = UIHelper.Text(parent, name, content, fontSize, Color.white);
            Rt(t.rectTransform, aMin, aMax, pivot, size, pos);
            if (outline)
            {
                var o = t.gameObject.AddComponent<Outline>();
                o.effectColor = Color.black;
                o.effectDistance = new Vector2(2f, -2f);
            }
            return t;
        }

        private static Button Btn(Transform parent, string name, string sp, Vector2 size, Vector2 pos,
            System.Action onClick)
        {
            var b = UIHelper.Button(parent, name, onClick);
            var img = b.gameObject.GetComponent<Image>();
            var s = sp != null ? OriginalAssets.GetUi(sp) : null;
            if (s != null) { img.sprite = s; img.type = Image.Type.Sliced; }
            else img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), size, pos);
            return b;
        }

        private static Button BtnX(Transform parent, string name, string sp,
            Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos,
            System.Action onClick)
        {
            var b = UIHelper.Button(parent, name, onClick);
            var img = b.gameObject.GetComponent<Image>();
            var s = sp != null ? OriginalAssets.GetUi(sp) : null;
            if (s != null) { img.sprite = s; img.type = Image.Type.Sliced; }
            else img.color = new Color(1f, 1f, 1f, 0.28f);
            Rt(img.rectTransform, aMin, aMax, pivot, size, pos);
            return b;
        }

        /// <summary>进度条（Background/Fill Area/Fill + Slider，与 dump 结构一致）。</summary>
        private static Slider MakeSlider(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos,
            string bgSp, string fillSp, int cur, int max, bool showKnobIcon)
        {
            var root = Rt(UIHelper.NewRect(parent, name), aMin, aMax, pivot, size, pos);

            var bgImg = UIHelper.Image(root, "Background", OriginalAssets.GetUi(bgSp));
            if (bgImg.sprite == null) bgImg.color = new Color(1f, 1f, 1f, 0.3f);
            Rt(bgImg.rectTransform, new Vector2(0, 0.2f), new Vector2(1, 0.8f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var area = Rt(UIHelper.NewRect(root, "Fill Area"), new Vector2(0, 0.2f), new Vector2(1, 0.8f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var fill = Rt(UIHelper.NewRect(area, "Fill"), new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
            var fImg = fill.gameObject.AddComponent<Image>();
            var fSp = OriginalAssets.GetUi(fillSp);
            if (fSp != null) fImg.sprite = fSp; else fImg.color = new Color(0.35f, 0.8f, 0.4f, 0.9f);

            var sl = root.gameObject.AddComponent<Slider>();
            sl.fillRect = fill;
            sl.minValue = 0;
            sl.maxValue = Mathf.Max(1, max);
            sl.value = Mathf.Clamp(cur, 0, Mathf.Max(1, max));
            sl.interactable = false;

            if (showKnobIcon)
            {
                var icon = ImgX(root, "Image (1)", "picIcon",
                    new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(41.3f, 41.1f), new Vector2(4.3f, 0));
                var count = TxtX(root, "CountTxt", "9990/11111", 20,
                    new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                    new Vector2(-17.6f, 2.8f), new Vector2(8.8f, 1.4f), true);
                count.transform.SetAsLastSibling();
                icon.transform.SetAsLastSibling();
            }
            return sl;
        }

        /// <summary>滚动列表（Background/Viewport/Content/Scrollbar，与 dump 结构一致；horizontal 时内容横排）。</summary>
        private static ScrollRect MakeScroll(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos, bool horizontal)
        {
            var root = BoxX(parent, name, aMin, aMax, pivot, size, pos);
            var bgImg = root.gameObject.AddComponent<Image>();
            var bgSp = OriginalAssets.GetUi("Background");
            if (bgSp != null) { bgImg.sprite = bgSp; bgImg.type = Image.Type.Sliced; }
            else bgImg.color = new Color(0.1f, 0.12f, 0.18f, 0.6f);
            var sc = root.gameObject.AddComponent<ScrollRect>();

            var vp = Box(root, "Viewport", Vector2.zero, Vector2.zero);
            UIHelper.Stretch(vp);
            var vpImg = vp.gameObject.AddComponent<Image>();
            var mSp = OriginalAssets.GetUi("UIMask");
            if (mSp != null) vpImg.sprite = mSp; else vpImg.color = new Color(1f, 1f, 1f, 0f);
            vp.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = Box(vp, "Content", Vector2.zero, Vector2.zero);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0, 1);
            if (horizontal)
            {
                var hlg = content.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.childControlWidth = false; hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;
                hlg.spacing = 10;
                content.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.childControlWidth = true; vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
                vlg.spacing = 6;
                content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            var sbRoot = Box(root, "Scrollbar Vertical", new Vector2(12, 0), Vector2.zero);
            sbRoot.anchorMin = new Vector2(1, 0);
            sbRoot.anchorMax = new Vector2(1, 1);
            sbRoot.pivot = new Vector2(1, 1);
            var sbImg = sbRoot.gameObject.AddComponent<Image>();
            if (bgSp != null) { sbImg.sprite = bgSp; sbImg.type = Image.Type.Sliced; }
            else sbImg.color = new Color(1f, 1f, 1f, 0.1f);
            var area = UIHelper.Stretch(Box(sbRoot, "Sliding Area", Vector2.zero, Vector2.zero));
            var handle = UIHelper.Stretch(Box(area, "Handle", Vector2.zero, Vector2.zero));
            var hImg = handle.gameObject.AddComponent<Image>();
            var hSp = OriginalAssets.GetUi("UISprite");
            if (hSp != null) hImg.sprite = hSp; else hImg.color = new Color(1f, 1f, 1f, 0.35f);
            var sb = sbRoot.gameObject.AddComponent<Scrollbar>();
            sb.targetGraphic = hImg;
            sb.direction = Scrollbar.Direction.BottomToTop;

            sc.viewport = vp;
            sc.content = content;
            sc.verticalScrollbar = sb;
            sc.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            sc.horizontal = horizontal;
            sc.vertical = !horizontal;
            sc.movementType = ScrollRect.MovementType.Clamped;
            sc.scrollSensitivity = 24f;
            return sc;
        }
    }
}
