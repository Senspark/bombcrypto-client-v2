using System;
using System.Collections;
using System.Collections.Generic;

using App;

using UnityEngine;

public class BLHeaderBarshop : MonoBehaviour {
    [SerializeField]
    private GameObject[] tokens;

    private void Awake() {
        var normal = AppConfig.IsTon() || !ScreenUtils.IsIPadScreen();
        tokens[0].SetActive(normal);
        tokens[1].SetActive(!normal);
    }
}