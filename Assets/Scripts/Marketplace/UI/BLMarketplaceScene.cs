using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Senspark;

using Game.Dialog;
using Game.UI;

using Newtonsoft.Json;

using Services;
using Services.Server;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

using Object = UnityEngine.Object;

namespace Marketplace.UI {

    public class BLMarketplaceScene : MonoBehaviour {

        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private Canvas canvasDialog642X363;
        
        private ILogManager _logManager;
        private IMarketplace _marketplace;
        private ISceneManager _sceneLoader;
        private IServerManager _serverManager;
        private ISoundManager _soundManager;

        private void Awake() {
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _marketplace = _serverManager.Marketplace;
            _sceneLoader = ServiceLocator.Instance.Resolve<ISceneManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }
        
    }
}