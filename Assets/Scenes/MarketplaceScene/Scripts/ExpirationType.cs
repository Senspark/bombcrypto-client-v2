using System;

using TMPro;

using UnityEngine;

public class ExpirationType : MonoBehaviour {
    [SerializeField] private GameObject img7D, img30D;
    [SerializeField] private TMP_Text txt7D, txt30D;
    public Action On7DayClick, On30DayClick;

    private void Awake() {
        img7D.SetActive(true);
        img30D.SetActive(false);
        txt7D.color = Color.yellow;
        txt30D.color = Color.white;
    }
    
    public void HideExpirationType() {
        gameObject.SetActive(false);
    }

    public void OnClick7Day() {
        img7D.SetActive(true);
        img30D.SetActive(false);
        txt7D.color = Color.yellow;
        txt30D.color = Color.white;
        On7DayClick?.Invoke();
    }
    
    public void OnClick30Day() {
        img7D.SetActive(false);
        img30D.SetActive(true);
        txt7D.color = Color.white;
        txt30D.color = Color.yellow;
        On30DayClick?.Invoke();
    }
}
