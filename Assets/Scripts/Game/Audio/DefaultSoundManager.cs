using System.Collections.Generic;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Utils;

using UnityEngine;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace App {
    public class DefaultSoundManager : ISoundManager {
        private Audio CurrentMusic { set; get; } = Audio.None;
        public float MusicVolume { set; get; } = 0.4f;
        public float SoundVolume { set; get; } = 0.6f;

        private readonly IDataManager _dataManager;
        private readonly Dictionary<Audio, List<(AudioClip Clip, float Volume)>> _clipsInfo = new();
        private readonly List<Audio> _audios = new();

        public DefaultSoundManager(IDataManager dataManager) {
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
            
            UniTask.WhenAll(
                AddClipAsync(Audio.TreasureMusic /*    */, ("treasure_music", MusicVolume)),
                AddClipAsync(Audio.CollectBCoin /*      */, ("Collect_BCoin", SoundVolume)),
                AddClipAsync(Audio.BombExplode /*       */, ("Bomb_Explosion", 0.8f)),
                AddClipAsync(Audio.Error /*             */, ("Error", SoundVolume)),
                AddClipAsync(Audio.Tap /*               */, ("Button_Click", 0.8f)),
                AddClipAsync(Audio.TapPlay /*           */, ("Button_Tap_Play", SoundVolume)),
                AddClipAsync(Audio.BuyBooster /*        */, ("Buy_Boosters", SoundVolume)),
                AddClipAsync(Audio.PlayerDestroy /*     */, ("Bomber_Death", SoundVolume)),
                AddClipAsync(Audio.BossDestroy /*       */, ("Boss_Death", SoundVolume)),
                AddClipAsync(Audio.GetItem /*           */, ("Item_Gain", SoundVolume)),
                AddClipAsync(Audio.DoorExit /*          */, ("sfx_teleport", SoundVolume)),
                AddClipAsync(Audio.InJail /*            */, ("sfx_prison", SoundVolume)),
                AddClipAsync(Audio.UseShield /*         */, ("sfx_shield", 0.7f)),
                AddClipAsync(Audio.UseKey /*            */, ("sfx_key", 0.7f)),
                AddClipAsync(Audio.PopupWin /*          */, ("popup_win", 1f)),
                AddClipAsync(Audio.PopupContinue /*     */, ("Dead", SoundVolume)),
                AddClipAsync(Audio.PopupDefeated /*     */, ("popup_defeated", 0.8f)),
                AddClipAsync(Audio.PopupDraw /*         */, ("popup_draw", SoundVolume)),
                AddClipAsync(Audio.GetCoins /*          */, ("Coin_Gain_Greater", SoundVolume)),
                AddClipAsync(Audio.TapNextLevel /*      */, ("Coin_Gain_Lesser", SoundVolume)),
                AddClipAsync(Audio.TicTock /*           */, ("Timer_Countdown", SoundVolume)),
                AddClipAsync(Audio.Spin /*              */, ("Spin_Wheel", SoundVolume)),
                AddClipAsync(Audio.Bonus /*             */, ("Bonus", SoundVolume)),
                AddClipAsync(Audio.UpgradeSuccess /*    */, ("Upgrade_Success", SoundVolume)),
                AddClipAsync(Audio.TankShoot /*         */, ("tank_shoot", SoundVolume)),
                AddClipAsync(Audio.TankMove /*          */, ("tank_move", SoundVolume)),
                AddClipAsync(Audio.KingShoot /*         */, ("king_shoot", SoundVolume)),
                AddClipAsync(Audio.KingSpawn /*         */, ("king_shoot", SoundVolume)),
                AddClipAsync(Audio.KingMove /*          */, ("king_move", SoundVolume)),
                AddClipAsync(Audio.MonsterShoot /*      */, ("monster_shoot", SoundVolume)),
                AddClipAsync(Audio.MonsterTakeDamage /* */, ("monster_take_damage", SoundVolume)),
                AddClipAsync(Audio.MonsterMove /*       */, ("monster_move", SoundVolume)),
                AddClipAsync(Audio.MosquitoShoot /*     */, ("mosquito_shoot", SoundVolume)),
                AddClipAsync(Audio.MosquitoQuickRun /*  */, ("mosquito_quick_run", SoundVolume)),
                AddClipAsync(Audio.MosquitoMove /*      */, ("mosquito_move", SoundVolume)),
                AddClipAsync(Audio.RobotShoot /*        */, ("robot_shoot", SoundVolume)),
                AddClipAsync(Audio.RobotSpawn /*        */, ("robot_spawn", SoundVolume)),
                AddClipAsync(Audio.RobotTakeDamage /*   */, ("robot_take_damage", SoundVolume)),
                AddClipAsync(Audio.RobotMove /*         */, ("robot_move", SoundVolume)),
                AddClipAsync(Audio.PirateShoot /*        */, ("pirate_shoot", SoundVolume)),
                AddClipAsync(Audio.PirateSpawn /*        */, ("pirate_spawn", SoundVolume)),
                AddClipAsync(Audio.PirateTakeDamage /*   */, ("pirate_take_damage", SoundVolume)),
                AddClipAsync(Audio.ChefShoot /*          */, ("chef_shoot", SoundVolume)),
                AddClipAsync(Audio.ChefSpawn /*          */, ("chef_spawn", SoundVolume)),
                AddClipAsync(Audio.ChefTakeDamage /*     */, ("chef_take_damage", SoundVolume)),
                AddClipAsync(Audio.PumpkinSpawn /*       */, ("pumpkin_spawn", SoundVolume)),
                AddClipAsync(Audio.PumpkinMove /*        */, ("pumpkin_move", SoundVolume)),
                AddClipAsync(Audio.HeroTakeDamage /*     */, ("hero_take_damage", SoundVolume)),
                AddClipAsync(Audio.CoinCounter /*        */, ("coin_counter", SoundVolume)),
                AddClipAsync(Audio.StoryMenuMusic /*    */, ("story_menu_music", 0.5f)),
                AddClipAsync(Audio.StoryMusic1 /*       */, ("Story_Music_1", 0.7f)),
                AddClipAsync(Audio.StoryMusic2 /*       */, ("Story_Music_2", 0.8f)),
                AddClipAsync(Audio.StoryMusic3 /*       */, ("Story_Music_3", 0.8f)),
                AddClipAsync(Audio.StoryMusic4 /*       */, ("Story_Music_4", 0.8f)),
                AddClipAsync(Audio.StoryMusic5 /*       */, ("Story_Music_5", 0.7f)),
                AddClipAsync(Audio.StoryMusic6 /*       */, ("Story_Music_6", MusicVolume)),
                AddClipAsync(Audio.StoryMusic7 /*       */, ("Story_Music_7", MusicVolume)),
                AddClipAsync(Audio.StoryMusic8 /*       */, ("Story_Music_8", MusicVolume)),
                AddClipAsync(Audio.StoryMusic9 /*       */, ("Story_Music_9", MusicVolume)),
                AddClipAsync(Audio.ArmorBreak /*        */, ("ArmorBreak", SoundVolume)),
                AddClipAsync(Audio.BlockDropDown /*     */, ("BlockDropDown", SoundVolume)),
                AddClipAsync(Audio.KickBomb /*          */, ("KickBomb", SoundVolume)),
                AddClipAsync(Audio.PickUpItem /*        */, ("PickUpItem", SoundVolume)),
                AddClipAsync(Audio.HurryUp /*           */, ("hurry_up", SoundVolume)),
                AddClipAsync(Audio.PvpMusic /*          */, ("pvp_music", 0.6f)),
                AddClipAsync(Audio.PvpBossMusic /*      */, ("pvp_boss_music", 0.7f)),
                AddClipAsync(Audio.MainMenuMusic /*     */, ("main_menu_music", 0.5f)),
                AddClipAsync(Audio.BirthdayMusic /*     */, ("birthday_music", MusicVolume)),
                AddClipAsync(Audio.LuckyWheelRotate /*  */, ("lucky_wheel_rotate", SoundVolume)),
                AddClipAsync(Audio.GetLuckyReward /*    */, ("get_lucky_reward", SoundVolume)),
                AddClipAsync(Audio.GetLuckyBnb /*       */, ("get_lucky_bnb", SoundVolume)),
                AddClipAsync(Audio.WheelSpin /*         */, ("sfx_wheelspin", SoundVolume)),
                AddClipAsync(Audio.WheelStop /*         */, ("sfx_wheelstop", SoundVolume)),
                AddClipAsync(Audio.TutorialReward /*    */, ("sfx_tutorial_reward", SoundVolume)),
                AddClipAsync(Audio.TutorialMoveToHand /**/, ("sfx_tutorial_move_to_hand", SoundVolume)),
                AddClipAsync(Audio.Welcome /*           */, ("sfx_welcome", SoundVolume)),
                AddClipAsync(Audio.RankUp /*            */, ("rank_up", SoundVolume)),
                AddClipAsync(Audio.RankDown /*          */, ("rank_down", SoundVolume)),
                AddClipAsync(Audio.ShowNoti /*          */, ("positive_feedback", SoundVolume))
            ).Forget();

            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        private async UniTask AddClipAsync(Audio audio, params (string Path, float Volume)[] paths) {
            var items = await UniTask.WhenAll(paths.Select(async key => {
                var path = $"Audio/{key.Path}";
                var clip = await Resources.LoadAsync<AudioClip>(path) as AudioClip;
                return (clip, key.Volume);
            }));
            var infos = GetClipsInfo(audio);
            infos.AddRange(items);
        }

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
            CurrentMusic = audio;

            var (clip, volume) = GetRandomizedClipInfo(audio);
            if (!clip) {
                return;
            }
            AudioPlayer.Instance.PlayMusic(clip, MusicVolume);
        }
        
        public void PlayOrResume(Audio audio) {
            CurrentMusic = audio;

            var (clip, volume) = GetRandomizedClipInfo(audio);
            if (!clip) {
                return;
            }
            AudioPlayer.Instance.PlayMusic(clip, MusicVolume);
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
            CurrentMusic = audio;

            var (clip, volume) = GetRandomizedClipInfo(audio);
            if (!clip) {
                return;
            }
            AudioPlayer.Instance.ChangeMusic(clip, MusicVolume);
        }

        public void ChangeMusicImmediate(Audio audio, bool loop = true) {
            if (audio == CurrentMusic) {
                return;
            }
            CurrentMusic = audio;

            var (clip, _) = GetRandomizedClipInfo(audio);
            if (!clip) {
                return;
            }
            AudioPlayer.Instance.ChangeMusicImmediate(clip, MusicVolume, loop);
        }

        public void PauseMusic() {
            AudioPlayer.Instance.PauseMusic();
        }

        public void ResumeMusic() {
            AudioPlayer.Instance.ResumeMusic();
        }

        public void PlaySound(Audio audio, Transform trans = null, bool allowMulti = false) {
            var (clip, _) = GetRandomizedClipInfo(audio);
            if (!clip) {
                return;
            }

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
        }

        public void PlaySoundMoving(Audio audio, Transform trans = null, bool allowMulti = false) {
            if (_audios.Count == 0) {
                PlaySound(audio, trans, allowMulti);
            }
        }

        public int PlayLoopSound(Audio audio, Transform trans = null) {
            var (clip, _) = GetRandomizedClipInfo(audio);
            if (clip == null) {
                return -1;
            }
            return AudioPlayer.Instance.PlayLoopSound(clip, SoundVolume, trans);
        }

        public void StopLoopSound(int id) {
            AudioPlayer.Instance.StopLoopSound(id);
        }

        private List<(AudioClip Clip, float Volume)> GetClipsInfo(Audio audio) {
            if (_clipsInfo.TryGetValue(audio, out var result)) {
                return result;
            }
            var infos = new List<(AudioClip, float)>();
            _clipsInfo.Add(audio, infos);
            return infos;
        }

        private (AudioClip Clip, float Volume) GetRandomizedClipInfo(Audio audio) {
            var infos = GetClipsInfo(audio);
            if (infos.Count == 0) {
                return (null, 0);
            }
            var index = Random.Range(0, infos.Count);
            var info = infos[index];
            return info;
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