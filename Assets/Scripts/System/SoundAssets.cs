using UnityEngine;

namespace Xio.Game
{
    /// <summary>原版游戏音效（96 个，从 sound bundle 抠出 AAC 解码为 WAV，放 Resources/Audio/Bundle）。
    /// 名单：puzzle_pickUp/pick(捡牌) merge/puzzle_merge(消除) getsuipian(获得碎片) puzzle_complete(拼图完成)
    /// win/lose clean(清扫) redraw(重洗) combine(合成) bingdong(冰冻) puzzle_turnUp(翻牌) puzzle_sendCard(发牌)…</summary>
    public static class SoundAssets
    {
        private const string Dir = "Audio/Bundle/";

        /// <summary>按原版名取音效；支持 fallback 链。</summary>
        public static AudioClip Get(params string[] names)
        {
            foreach (var n in names)
            {
                var c = Resources.Load<AudioClip>(Dir + n);
                if (c != null) return c;
            }
            return null;
        }
    }
}
