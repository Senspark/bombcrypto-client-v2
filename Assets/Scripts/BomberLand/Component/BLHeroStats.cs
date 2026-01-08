using System.Collections.Generic;

using App;

using Constant;

using Data;

using Senspark;

using Game.Dialog;

using Services;

using UnityEngine;

namespace BomberLand.Component {
    public class BLHeroStats : MonoBehaviour {
        [SerializeField]
        private BLHeroStat range;

        [SerializeField]
        private BLHeroStat speed;

        [SerializeField]
        private BLHeroStat bomb;

        [SerializeField]
        private BLHeroStat health;

        [SerializeField]
        private BLHeroStat damage;

        private IHeroStatsManager _heroStatsManager;
        private ITRHeroManager _trHeroManager;

        public virtual void UpdateHero(UIHeroData data) {
            range.SetValue(data.BombRange.value, data.BombRange.max);
            speed.SetValue(data.Speed.value, data.Speed.max);
            bomb.SetValue(data.BombNum.value, data.BombNum.max);
            health.SetValue(data.Health.value, data.Health.max);
            damage.SetValue(data.Damage.value, data.Damage.max);
        }

        public virtual void UpdateStats(UIHeroData data) {
            if (data == null) {
                range.SetValue(0, 0);
                speed.SetValue(0, 0);
                bomb.SetValue(0, 0);
                health.SetValue(0, 0);
                damage.SetValue(0, 0);
                return;
            }

            range.SetValue(data.BombRange.value, data.BombRange.max);
            speed.SetValue((int)data.Speed.value, data.Speed.max);
            bomb.SetValue(data.BombNum.value, data.BombNum.max);
            health.SetValue(data.Health.value, data.Health.max);
            damage.SetValue(data.Damage.value, data.Damage.max);
        }

        protected void SetEmptyStats() {
            range.SetValue(0, 0);
            speed.SetValue(0, 0);
            bomb.SetValue(0, 0);
            health.SetValue(0, 0);
            damage.SetValue(0, 0);
        }

        public void UpdateStatsFromSkinStats(Dictionary<int, StatData[]> skinStats) {
            if (skinStats == null) {
                return;
            }
            range.ResetBars();
            speed.ResetBars();
            bomb.ResetBars();
            health.ResetBars();
            damage.ResetBars();
            foreach (var skin in skinStats.Keys) {
                foreach (var stat in skinStats[skin]) {
                    switch (stat.StatId) {
                        case (int)StatId.Range:
                            range.SetEquipValue(stat.Value);
                            break;
                        case (int)StatId.Speed:
                            speed.SetEquipValue(stat.Value);
                            break;
                        case (int)StatId.Count:
                            bomb.SetEquipValue(stat.Value);
                            break;
                        case (int)StatId.Health:
                            health.SetEquipValue(stat.Value);
                            break;
                        case (int)StatId.Damage:
                            damage.SetEquipValue(stat.Value);
                            break;
                    }
                }
            }
        }

        protected void UpdateHeroFromStatData(StatData[] stats) {
            foreach (var stat in stats) {
                var statId = (StatId) stat.StatId;
                switch (statId) {
                    case StatId.Range:
                        range.SetValue(stat.Value, stat.Max);
                        break;
                    case StatId.Speed:
                        speed.SetValue(stat.Value, stat.Max);
                        break;
                    case StatId.Count:
                        bomb.SetValue(stat.Value, stat.Max);
                        break;
                    case StatId.Health:
                        health.SetValue(stat.Value, stat.MaxUpgradeValue);
                        break;
                    case StatId.Damage:
                        damage.SetValue(stat.Value, stat.MaxUpgradeValue);
                        break;
                }
            }
        }

        protected void UpdateHeroFromStatDataWithUpgrade(StatData[] stats, Dictionary<StatId, int> upgradeStats) {
            foreach (var stat in stats) {
                var statId = (StatId) stat.StatId;
                switch (statId) {
                    case StatId.Range:
                        range.SetValue(stat.Value, stat.Max + upgradeStats[statId]);
                        break;
                    case StatId.Speed:
                        speed.SetValue(stat.Value, stat.Max + upgradeStats[statId]);
                        break;
                    case StatId.Count:
                        bomb.SetValue(stat.Value, stat.Max + upgradeStats[statId]);
                        break;
                    case StatId.Health:
                        health.SetValue(stat.Value + upgradeStats[statId], stat.MaxUpgradeValue);
                        break;
                    case StatId.Damage:
                        damage.SetValue(stat.Value + upgradeStats[statId], stat.MaxUpgradeValue);
                        break;
                }
            }
        }
        
        public void UpdateHeroFromItemId(int itemId) {
            _heroStatsManager ??= ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            _trHeroManager ??= ServiceLocator.Instance.Resolve<ITRHeroManager>();
            var stats = _heroStatsManager.GetStats(itemId);
            UpdateHeroFromStatData(stats);
        }
    }
}