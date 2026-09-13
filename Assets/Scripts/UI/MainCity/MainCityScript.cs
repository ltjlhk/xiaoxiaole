using UnityEngine;

namespace Xio.UI
{
    /// <summary>主城秒级刷新驱动器：驱动精力恢复倒计时文本更新（挂在 UIPanel 根上的轻量 MonoBehaviour）。</summary>
    public class MainCityScript : MonoBehaviour
    {
        public MainCityPanel panel;

        private void Update()
        {
            if (panel != null) panel.Tick(Time.deltaTime);
        }
    }
}