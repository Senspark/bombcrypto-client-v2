using System;

using App;

using Senspark;

using UnityEngine;

public class BorderBackground : MonoBehaviour {
    [SerializeField]
    private GameObject[] borderList;

    private IPlayerStorageManager _playerStoreManager;

    public void Awake() {
        _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
    }

    private void Start() {
        var tileIndex = _playerStoreManager.TileSet;
        for (var i = 0; i < borderList.Length; i++) {
            borderList[i].SetActive(i == GetBorderIndex(tileIndex));
        }
    }

    private int GetBorderIndex(int tileIndex) {
        return tileIndex switch {
            0 => 0,
            1 => 0,
            2 => 1,
            3 => 1,
            4 => 2,
            5 => 2,
            6 => 2,
            7 => 1,
            8 => 2,
            9 => 1,
            _ => 0
        };
    }
}
