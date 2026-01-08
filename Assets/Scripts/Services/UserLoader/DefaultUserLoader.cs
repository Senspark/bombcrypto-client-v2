using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;

namespace Services.UserLoader {
    public interface IUserLoader {
        List<(string, Func<Task>)> GetLoads();
    }
    
    public class DefaultUserLoader : IUserLoader {

        private readonly IUserLoader _userLoaderBridge;

        public DefaultUserLoader() {
            switch (AppConfig.GamePlatform) {
                case GamePlatform.WEBGL or GamePlatform.MOBILE or GamePlatform.TOURNAMENT:
                    if (AppConfig.IsWebAirdrop()) {
                        _userLoaderBridge = new WebAirdropUserLoader();
                    } else {
                        _userLoaderBridge = new WebGlUserLoader();
                    }
                    break;
                case GamePlatform.TON:
                    _userLoaderBridge = new TonUserLoader();
                    break;
                case GamePlatform.SOL:
                    _userLoaderBridge = new SolanaUserLoader();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public List<(string, Func<Task>)> GetLoads() {
            return _userLoaderBridge.GetLoads();
        }
    }
}