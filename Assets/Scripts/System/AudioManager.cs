using UnityEngine;

namespace Xio.Game
{
    /// <summary>原版游戏音效管理：从 sound bundle 直接加载播放（名字与原版一致）。
    /// BGM 用原版 puzzle_bgm.mp3。</summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _inst;
        public static AudioManager Inst
        {
            get
            {
                if (_inst == null)
                {
                    var go = new GameObject("AudioManager");
                    _inst = go.AddComponent<AudioManager>();
                    if (Application.isPlaying) DontDestroyOnLoad(go);
                }
                return _inst;
            }
        }

        private AudioSource _bgm;
        private AudioSource Bgm
        {
            get
            {
                if (_bgm == null)
                {
                    _bgm = gameObject.AddComponent<AudioSource>();
                    _bgm.loop = true;
                    _bgm.volume = 0.55f;
                }
                return _bgm;
            }
        }

        /// <summary>播放原版音效（WAV，名字优先链）。批处理无音频设备时只验加载不播放。</summary>
        private void Play(float vol, params string[] names)
        {
            var clip = SoundAssets.Get(names);
            if (clip == null)
            {
                Debug.LogWarning("[音效] 缺失: " + string.Join("/", names));
                return;
            }
            PlayClip(clip, vol);
        }

        private void PlayClip(AudioClip clip, float vol)
        {
            if (clip == null) return;
            if (!Application.isPlaying) return; // 编辑器批处理无音频设备，FMOD 会崩
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.clip = clip;
            src.volume = vol;
            src.Play();
            Destroy(src, clip.length + 0.1f);
        }

        public void PlayBgm()
        {
            var clip = Resources.Load<AudioClip>("Audio/puzzle_bgm");
            if (clip == null) return;
            var bgm = Bgm;
            if (bgm.clip == clip && bgm.isPlaying) return;
            bgm.clip = clip;
            if (Application.isPlaying) bgm.Play();
        }

        // ===== 游戏动作（原版音效名）=====
        public void PlayPick() => Play(0.9f, "puzzle_pickUp", "pick");   // 捡牌进槽
        public void PlayPutDown() => Play(0.8f, "puzzle_putDown");        // 放回
        public void PlayLight() => Play(1f, "getsuipian");                // 点亮碎片
        public void PlayWin() => Play(1f, "puzzle_complete", "win");      // 拼图完成
        public void PlayLose() => Play(1f, "lose");                       // 失败
        public void PlayTurnUp() => Play(0.8f, "puzzle_turnUp");          // 翻牌

        /// <summary>三消：优先花牌专属音效（原版 ItemSoundv2/花牌同名，如 MG_001），否则通用消除。</summary>
        public void PlayMatch(string flowerTex = null)
        {
            if (!string.IsNullOrEmpty(flowerTex))
            {
                var own = Resources.Load<AudioClip>("Audio/Item/" + flowerTex);
                if (own != null) { PlayClip(own, 1f); return; }
            }
            Play(1f, "puzzle_merge", "merge"); // 通用消除（拼图模式音优先）
        }

        // ===== 道具（原版名）=====
        public void PlayToolClean() => Play(0.9f, "clean");       // 清扫
        public void PlayToolRedraw() => Play(0.9f, "redraw");     // 洗牌
        public void PlayToolCombine() => Play(0.9f, "combine");   // 合成
        public void PlayToolFreeze() => Play(0.9f, "bingdong");   // 冰冻

        public void PlayTool(ToolType t)
        {
            switch (t)
            {
                case ToolType.Clear: PlayToolClean(); break;
                case ToolType.Shuffle: PlayToolRedraw(); break;
                case ToolType.Compose: PlayToolCombine(); break;
                case ToolType.Freeze: PlayToolFreeze(); break;
            }
        }
    }
}
