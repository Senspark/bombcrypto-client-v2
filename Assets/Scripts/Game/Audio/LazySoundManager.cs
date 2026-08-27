using System.Collections.Generic;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Utils;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace App {
    /// <summary>
    /// Load audio theo yêu cầu qua Addressables thay vì preload toàn bộ lúc khởi động.
    /// Nhạc nền chỉ giữ tối đa 1 clip trong bộ nhớ, clip cũ được release sau khi crossfade xong.
    /// </summary>
    public class LazySoundManager : ISoundManager {
        private const int MusicReleaseDelayMs = 3000;

        private static readonly Dictionary<Audio, string[]> Addresses = new() {
            [Audio.TreasureMusic] = new[] { "treasure_music" },
            [Audio.CollectBCoin] = new[] { "Collect_BCoin" },
            [Audio.BombExplode] = new[] { "Bomb_Explosion" },
            [Audio.Error] = new[] { "Error" },
            [Audio.Tap] = new[] { "Button_Click" },
            [Audio.TapPlay] = new[] { "Button_Tap_Play" },
            [Audio.BuyBooster] = new[] { "Buy_Boosters" },
            [Audio.PlayerDestroy] = new[] { "Bomber_Death" },
            [Audio.BossDestroy] = new[] { "Boss_Death" },
            [Audio.GetItem] = new[] { "Item_Gain" },
            [Audio.DoorExit] = new[] { "sfx_teleport" },
            [Audio.InJail] = new[] { "sfx_prison" },
            [Audio.UseShield] = new[] { "sfx_shield" },
            [Audio.UseKey] = new[] { "sfx_key" },
            [Audio.PopupWin] = new[] { "popup_win" },
            [Audio.PopupContinue] = new[] { "Dead" },
            [Audio.PopupDefeated] = new[] { "popup_defeated" },
            [Audio.PopupDraw] = new[] { "popup_draw" },
            [Audio.GetCoins] = new[] { "Coin_Gain_Greater" },
            [Audio.TapNextLevel] = new[] { "Coin_Gain_Lesser" },
            [Audio.TicTock] = new[] { "Timer_Countdown" },
            [Audio.Spin] = new[] { "Spin_Wheel" },
            [Audio.Bonus] = new[] { "Bonus" },
            [Audio.UpgradeSuccess] = new[] { "Upgrade_Success" },
            [Audio.TankShoot] = new[] { "tank_shoot" },
            [Audio.TankMove] = new[] { "tank_move" },
            [Audio.KingShoot] = new[] { "king_shoot" },
            [Audio.KingSpawn] = new[] { "king_spawn" },
            [Audio.KingMove] = new[] { "king_move" },
            [Audio.MonsterShoot] = new[] { "monster_shoot" },
            [Audio.MonsterTakeDamage] = new[] { "monster_take_damage" },
            [Audio.MonsterMove] = new[] { "monster_move" },
            [Audio.MosquitoShoot] = new[] { "mosquito_shoot" },
            [Audio.MosquitoQuickRun] = new[] { "mosquito_quick_run" },
            [Audio.MosquitoMove] = new[] { "mosquito_move" },
            [Audio.RobotShoot] = new[] { "robot_shoot" },
            [Audio.RobotSpawn] = new[] { "robot_spawn" },
            [Audio.RobotTakeDamage] = new[] { "robot_take_damage" },
            [Audio.RobotMove] = new[] { "robot_move" },
            [Audio.PirateShoot] = new[] { "pirate_shoot" },
            [Audio.PirateSpawn] = new[] { "pirate_spawn" },
            [Audio.PirateTakeDamage] = new[] { "pirate_take_damage" },
            [Audio.ChefShoot] = new[] { "chef_shoot" },
            [Audio.ChefSpawn] = new[] { "chef_spawn" },
            [Audio.ChefTakeDamage] = new[] { "chef_take_damage" },
            [Audio.PumpkinSpawn] = new[] { "pumpkin_spawn" },
            [Audio.PumpkinMove] = new[] { "pumpkin_move" },
            [Audio.HeroTakeDamage] = new[] { "hero_take_damage" },
            [Audio.CoinCounter] = new[] { "coin_counter" },
            [Audio.StoryMenuMusic] = new[] { "story_menu_music" },
            [Audio.StoryMusic1] = new[] { "story_music_1" },
            [Audio.StoryMusic2] = new[] { "story_music_2" },
            [Audio.StoryMusic3] = new[] { "story_music_3" },
            [Audio.StoryMusic4] = new[] { "story_music_4" },
            [Audio.StoryMusic5] = new[] { "story_music_5" },
            [Audio.StoryMusic6] = new[] { "story_music_6" },
            [Audio.StoryMusic7] = new[] { "story_music_7" },
            [Audio.StoryMusic8] = new[] { "story_music_8" },
            [Audio.StoryMusic9] = new[] { "story_music_9" },
            [Audio.ArmorBreak] = new[] { "ArmorBreak" },
            [Audio.BlockDropDown] = new[] { "BlockDropdown" },
            [Audio.KickBomb] = new[] { "KickBomb" },
            [Audio.PickUpItem] = new[] { "PickUpItem" },
            [Audio.HurryUp] = new[] { "hurry_up" },
            [Audio.PvpMusic] = new[] { "pvp_music" },
            [Audio.PvpBossMusic] = new[] { "pvp_boss_music" },
            [Audio.MainMenuMusic] = new[] { "main_menu_music" },
            [Audio.WheelSpin] = new[] { "sfx_wheelspin" },
            [Audio.WheelStop] = new[] { "sfx_wheelstop" },
            [Audio.TutorialReward] = new[] { "sfx_tutorial_reward" },
            [Audio.TutorialMoveToHand] = new[] { "sfx_tutorial_move_to_hand" },
            [Audio.Welcome] = new[] { "sfx_welcome" },
            [Audio.RankUp] = new[] { "rank_up" },
            [Audio.RankDown] = new[] { "rank_down" },
            [Audio.ShowNoti] = new[] { "positive_feedback" },
        };

        private Audio CurrentMusic { set; get; } = Audio.None;
        public float MusicVolume { set; get; } = 0.4f;
        public float SoundVolume { set; get; } = 0.6f;

        private readonly IDataManager _dataManager;
        private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> _handles = new();
        private readonly HashSet<string> _reportedFailures = new();
        private readonly HashSet<string> _musicAddresses = new();
        private readonly List<Audio> _audios = new();
        private readonly Dictionary<int, int> _loopChannels = new();
        private readonly HashSet<int> _cancelledLoops = new();

        private string _currentMusicAddress;
        private int _nextLoopId = 1;

        public LazySoundManager(IDataManager dataManager) {
            _dataManager = dataManager;
            if (AppConfig.IsTon()) {
                var soundManagerFocus = new GameObject("SoundManagerFocus").AddComponent<SoundManagerFocus>();
                Object.DontDestroyOnLoad(soundManagerFocus.gameObject);
            }
        }

        public Task<bool> Initialize() {
            AudioPlayer.Instance.MusicEnabled = MusicEnabled;
            AudioPlayer.Instance.SoundEnabled = SoundEnabled;

            MusicVolume = _dataManager.GetFloat("music_volume", 0.4f);
            SoundVolume = _dataManager.GetFloat("sound_volume", 0.6f);

            return Task.FromResult(true);
        }

        public void Destroy() {
            foreach (var handle in _handles.Values) {
                if (handle.IsValid()) {
                    Addressables.Release(handle);
                }
            }
            _handles.Clear();
            _musicAddresses.Clear();
            _currentMusicAddress = null;
        }

        #region Loading

        private static string PickAddress(Audio audio) {
            if (!Addresses.TryGetValue(audio, out var addresses) || addresses.Length == 0) {
                return null;
            }
            return addresses[Random.Range(0, addresses.Length)];
        }

        private AsyncOperationHandle<AudioClip> GetOrStartLoad(string address) {
            if (_handles.TryGetValue(address, out var handle)) {
                return handle;
            }
            handle = Addressables.LoadAssetAsync<AudioClip>(address);
            _handles[address] = handle;
            return handle;
        }

        private AudioClip ResolveClip(string address, AsyncOperationHandle<AudioClip> handle) {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result) {
                return handle.Result;
            }
            if (_reportedFailures.Add(address)) {
                Debug.LogError($"[LazySoundManager] Missing audio address '{address}'");
            }
            return null;
        }

        private void WhenLoaded(Audio audio, System.Action<AudioClip> onLoaded) {
            var address = PickAddress(audio);
            if (address == null) {
                if (_reportedFailures.Add(audio.ToString())) {
                    Debug.LogError($"[LazySoundManager] No audio address mapped for '{audio}'");
                }
                return;
            }
            var handle = GetOrStartLoad(address);
            if (handle.IsDone) {
                var clip = ResolveClip(address, handle);
                if (clip) {
                    onLoaded(clip);
                }
                return;
            }
            AwaitThenInvoke(address, handle, onLoaded).Forget();
        }

        private async UniTaskVoid AwaitThenInvoke(
            string address,
            AsyncOperationHandle<AudioClip> handle,
            System.Action<AudioClip> onLoaded) {
            try {
                await handle.Task;
            } catch (System.Exception e) {
                if (_reportedFailures.Add(address)) {
                    Debug.LogError($"[LazySoundManager] Failed to load audio address '{address}': {e.Message}");
                }
                return;
            }
            var clip = ResolveClip(address, handle);
            if (clip) {
                onLoaded(clip);
            }
        }

        private void SwitchMusicAddress(Audio audio) {
            var address = PickAddress(audio);
            if (address == _currentMusicAddress) {
                return;
            }
            _currentMusicAddress = address;
            if (!string.IsNullOrEmpty(address)) {
                _musicAddresses.Add(address);
            }
            WebGLTaskDelay.Instance.Delay(MusicReleaseDelayMs).Then(SweepUnusedMusic).Forget();
        }

        /// <summary>
        /// Chạy sau khi crossfade xong. Chỉ release clip nhạc mà AudioSource không còn giữ —
        /// AudioPlayer.PlayMusic bỏ qua clip mới nếu nhạc cũ đang phát, release mù sẽ destroy clip đang chạy.
        /// </summary>
        private void SweepUnusedMusic() {
            var playing = AudioPlayer.Instance ? AudioPlayer.Instance.CurrentMusicClip : null;
            var releasable = new List<string>();

            foreach (var address in _musicAddresses) {
                if (address == _currentMusicAddress) {
                    continue;
                }
                if (!_handles.TryGetValue(address, out var handle)) {
                    continue;
                }
                if (handle.IsValid() && handle.Result == playing) {
                    continue;
                }
                releasable.Add(address);
            }

            foreach (var address in releasable) {
                var handle = _handles[address];
                _handles.Remove(address);
                _musicAddresses.Remove(address);
                if (handle.IsValid()) {
                    Addressables.Release(handle);
                }
            }
        }

        #endregion

        public bool MusicEnabled {
            get =>
                _dataManager.GetInt("music_enabled", 1) == 1;
            set {
                _dataManager.SetInt("music_enabled", value ? 1 : 0);
                AudioPlayer.Instance.MusicEnabled = value;
            }
        }

        public bool SoundEnabled {
            get =>
                _dataManager.GetInt("sound_enabled", 1) == 1;
            set {
                _dataManager.SetInt("sound_enabled", value ? 1 : 0);
                AudioPlayer.Instance.SoundEnabled = value;
            }
        }

        public void SetVolumeSound(float volume) {
            SoundVolume = volume;
            _dataManager.SetFloat("sound_volume", volume);
        }

        public void SetVolumeMusic(float volume) {
            MusicVolume = volume;
            AudioPlayer.Instance.SetMusicVolume(volume);
            _dataManager.SetFloat("music_volume", volume);
        }

        public void PlayMusic(Audio audio) {
            if (audio == CurrentMusic) {
                return;
            }
            SwitchMusicAddress(audio);
            CurrentMusic = audio;

            WhenLoaded(audio, clip => AudioPlayer.Instance.PlayMusic(clip, MusicVolume));
        }

        public void PlayOrResume(Audio audio) {
            SwitchMusicAddress(audio);
            CurrentMusic = audio;

            WhenLoaded(audio, clip => AudioPlayer.Instance.PlayMusic(clip, MusicVolume));
        }

        public void StopMusic() {
            CurrentMusic = Audio.None;

            if (AudioPlayer.Instance) {
                AudioPlayer.Instance.StopMusic();
            }
        }

        public void StopImmediateMusic() {
            CurrentMusic = Audio.None;

            if (AudioPlayer.Instance) {
                AudioPlayer.Instance.StopImmediateMusic();
            }
        }

        public void ChangeMusic(Audio audio) {
            if (audio == CurrentMusic) {
                return;
            }
            SwitchMusicAddress(audio);
            CurrentMusic = audio;

            WhenLoaded(audio, clip => AudioPlayer.Instance.ChangeMusic(clip, MusicVolume));
        }

        public void ChangeMusicImmediate(Audio audio, bool loop = true) {
            if (audio == CurrentMusic) {
                return;
            }
            SwitchMusicAddress(audio);
            CurrentMusic = audio;

            WhenLoaded(audio, clip => AudioPlayer.Instance.ChangeMusicImmediate(clip, MusicVolume, loop));
        }

        public void PauseMusic() {
            AudioPlayer.Instance.PauseMusic();
        }

        public void ResumeMusic() {
            AudioPlayer.Instance.ResumeMusic();
        }

        public void PlaySound(Audio audio, Transform trans = null, bool allowMulti = false) {
            WhenLoaded(audio, clip => {
                //FIX ME: sound other cause sound win/lose interrupted...
                if (_audios.Contains(Audio.PopupDefeated) || _audios.Contains(Audio.DoorExit)) {
                    // ngoại trừ interrupted bởi RankUp và RankDown
                    if (audio is not Audio.RankUp and Audio.RankDown) {
                        return;
                    }
                }

                if (!allowMulti && _audios.Contains(audio)) {
                    return;
                }

                AudioPlayer.Instance.PlaySound(clip, SoundVolume, trans);

                if (allowMulti) {
                    return;
                }
                _audios.Add(audio);
                WebGLTaskDelay.Instance.Delay(Mathf.CeilToInt(clip.length * 1000))
                    .Then(() => { _audios.Remove(audio); }).Forget();
            });
        }

        public void PlaySoundMoving(Audio audio, Transform trans = null, bool allowMulti = false) {
            if (_audios.Count == 0) {
                PlaySound(audio, trans, allowMulti);
            }
        }

        public int PlayLoopSound(Audio audio, Transform trans = null) {
            var id = _nextLoopId++;
            WhenLoaded(audio, clip => {
                if (_cancelledLoops.Remove(id)) {
                    return;
                }
                _loopChannels[id] = AudioPlayer.Instance.PlayLoopSound(clip, SoundVolume, trans);
            });
            return id;
        }

        public void StopLoopSound(int id) {
            if (id < 0) {
                return;
            }
            if (_loopChannels.Remove(id, out var channel)) {
                AudioPlayer.Instance.StopLoopSound(channel);
                return;
            }
            _cancelledLoops.Add(id);
        }

        //Class này dùng để cho js gọi để tắt/bật âm thanh trên game trên telegram mobile khi ko focus nữa
        private class SoundManagerFocus : MonoBehaviour {
            public void DisableSound() {
                AudioPlayer.Instance.PauseMusic();
            }

            public void EnableSound() {
                AudioPlayer.Instance.ResumeMusic();
            }
        }
    }
}
