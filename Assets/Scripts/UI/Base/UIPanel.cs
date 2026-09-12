using UnityEngine;

namespace Xio.UI
{
    /// <summary>面板基类：代码动态搭 UI 的统一生命周期。
    /// PanelManager.Push 时会 Build() 一次并全屏铺开，Pop 时销毁 Root。</summary>
    public abstract class UIPanel
    {
        /// <summary>面板根节点（全屏，anchor 铺满）。</summary>
        public RectTransform Root { get; protected set; }

        /// <summary>面板是否正在显示。</summary>
        public bool IsVisible { get; protected set; }

        /// <summary>是否遮罩下层（默认 false：下层可见，主城下层）。</summary>
        protected virtual bool BlockClick => false;

        /// <summary>进本面板时隐藏下层全部面板（玩法场景用：Canvas 是 ScreenSpaceOverlay
        /// 永远盖在 3D 相机之上，下层全屏 BG 不隐藏会盖死 3D 牌堆；原版进关卡=切场景同语义）。</summary>
        public virtual bool HideBelow => false;

        /// <summary>创建根节点并构建内容。</summary>
        public void Open(Transform canvasRoot)
        {
            var go = new GameObject(GetType().Name, typeof(RectTransform));
            Root = (RectTransform)go.transform;
            Root.SetParent(canvasRoot, false);
            Stretch(Root);
            // 需要拦截点击时加全屏透明 Image + GraphickRaycaster 由 Canvas 提供
            Build();
            Refresh();   // 首开即填充数据（金币/进度等；子类可覆写）
            IsVisible = true;
        }

        /// <summary>构建面板元素。</summary>
        protected abstract void Build();

        /// <summary>从数据层刷新（如金币/进度变化后调用）。</summary>
        public virtual void Refresh() { }

        /// <summary>关闭：销毁根节点。</summary>
        public virtual void Close()
        {
            IsVisible = false;
            if (Root != null)
            {
                var go = Root.gameObject;
                if (Application.isPlaying) Object.Destroy(go);
                else Object.DestroyImmediate(go);
            }
            Root = null;
        }

        protected static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}