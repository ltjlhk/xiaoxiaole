using UnityEngine;
using Spine.Unity;

namespace Xio.Assets
{
    /// <summary>运行时挂载 Spine 动画到指定节点：SkeletonGraphic + SkeletonAnimation（4.x 拆分架构）。
    /// 动画状态通过返回值的 skeletonAnimation（ISkeletonAnimation）访问：sg.skeletonAnimation.AnimationState.SetAnimation(0, "idle", true)。</summary>
    public static class SpineView
    {
        /// <summary>在 parent 下创建名字为 goName 的 Spine 节点并播放指定动画。</summary>
        public static SkeletonGraphic Play(Transform parent, string goName, string skeletonName, string anim, bool loop, Vector2 sizeDelta)
        {
            var sda = SpineAssets.Get(skeletonName);
            if (sda == null || sda.GetSkeletonData(false) == null)
            {
                Debug.LogWarning($"[SpineView] 骨架不可用: {skeletonName}");
                return null;
            }
            var go = new GameObject(goName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var sg = go.AddComponent<SkeletonGraphic>();
            sg.skeletonDataAsset = sda;
            var animCtl = go.AddComponent<SkeletonAnimation>();
            sg.Initialize(true);
            sg.rectTransform.sizeDelta = sizeDelta;
            if (!string.IsNullOrEmpty(anim) && animCtl.AnimationState != null)
                animCtl.AnimationState.SetAnimation(0, anim, loop);
            return sg;
        }
    }
}