using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Xio.UI
{
    /// <summary>单例面板管理器：栈模型。Push 压栈显示新面板，Pop 回收顶层。
    /// 关卡玩法面板也作为普通面板入栈，切场零开销。</summary>
    public class PanelManager
    {
        public static PanelManager Instance { get; } = new PanelManager();

        private readonly List<UIPanel> _stack = new List<UIPanel>();
        private RectTransform _canvasRoot;

        /// <summary>切场景后旧 Canvas/面板被销毁，重置缓存并在新场景重建。</summary>
        public void ResetForScene()
        {
            _canvasRoot = null;
            _stack.Clear();
        }

        /// <summary>统一 Canvas 根（首次使用时查找场景中已有 Canvas）。
        /// 切场景后缓存失效时自动重查，避免引用已销毁对象。</summary>
        public RectTransform CanvasRoot
        {
            get
            {
                if (_canvasRoot != null) return _canvasRoot;
                var cv = Object.FindObjectOfType<Canvas>();
                if (cv == null)
                    Debug.LogError("[PanelManager] 场景缺少 Canvas，请先创建 GameRoot");
                _canvasRoot = (RectTransform)cv.transform;
                return _canvasRoot;
            }
        }

        /// <summary>压栈显示新面板（顶层冻结下层，不销毁保留状态）。</summary>
        public T Push<T>() where T : UIPanel, new()
        {
            return Push<T>(null);
        }

        /// <summary>压栈显示新面板，并允许在 Open 前设置参数（弹出式面板/关卡切换）。</summary>
        public T Push<T>(System.Action<T> initialize) where T : UIPanel, new()
        {
            var panel = new T();
            if (initialize != null) initialize(panel);
            panel.Open(CanvasRoot);
            _stack.Add(panel);
            ApplyLayerVisibility();
            return panel;
        }

        /// <summary>返回栈顶。</summary>
        public UIPanel Top => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;

        /// <summary>栈内面板数。</summary>
        public int Count => _stack.Count;

        /// <summary>弹出并销毁栈顶面板，并刷新新的栈顶（金币/解锁态实时更新）。</summary>
        public void Pop()
        {
            if (_stack.Count == 0) return;
            var top = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            top.Close();
            ApplyLayerVisibility();
            if (_stack.Count > 0) _stack[_stack.Count - 1].Refresh();
        }

        /// <summary>按最顶 HideBelow 面板裁剪显隐：其下全部隐藏（Canvas overlay 盖 3D），其上正常显示。</summary>
        private void ApplyLayerVisibility()
        {
            int h = 0;
            for (int i = _stack.Count - 1; i >= 0; i--)
                if (_stack[i].HideBelow) { h = i; break; }
            for (int i = 0; i < _stack.Count; i++)
            {
                var p = _stack[i];
                if (p.Root == null) continue;
                bool active = i >= h;
                if (p.Root.gameObject.activeSelf != active)
                    p.Root.gameObject.SetActive(active);
            }
        }

        /// <summary>清空全部面板。</summary>
        public void Clear()
        {
            while (_stack.Count > 0) Pop();
        }

        /// <summary>从栈中移除指定类型的面板（无论位置），并刷新可见性。</summary>
        public void Remove<T>() where T : UIPanel
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_stack[i] is T)
                {
                    _stack[i].Close();
                    _stack.RemoveAt(i);
                }
            }
            RefreshStackVisibility();
            ApplyLayerVisibility();
        }

        /// <summary>把指定面板提到最顶层（如关卡内点开道具弹窗）。</summary>
        public void BringToTop<T>() where T : UIPanel
        {
            for (int i = 0; i < _stack.Count; i++)
                if (_stack[i] is T)
                {
                    var p = _stack[i];
                    _stack.RemoveAt(i);
                    _stack.Add(p);
                    break;
                }
            RefreshStackVisibility();
        }

        private void RefreshStackVisibility()
        {
            // 保持栈序作为 Canvas 子节点序（后面的在上层）
            for (int i = 0; i < _stack.Count; i++)
            {
                var p = _stack[i];
                if (p.Root != null) p.Root.SetSiblingIndex(i);
            }
        }
    }
}