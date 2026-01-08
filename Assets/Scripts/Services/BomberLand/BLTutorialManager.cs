using System.Threading.Tasks;

using UnityEngine;

namespace Services {
    public class BLTutorialManager : IBLTutorialManager {
        private const string PlayerPrefTimePlayPvp = "BlTimePlayPVP";

        private int _timePlayPvp = PlayerPrefs.GetInt(PlayerPrefTimePlayPvp, 0);
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() { }

        public void IncreaseTimePlayPvp() {
            _timePlayPvp++;
            PlayerPrefs.SetInt(PlayerPrefTimePlayPvp, _timePlayPvp);
        }

        public int TimePlayPvp() {
            return _timePlayPvp;
        }
    }
}