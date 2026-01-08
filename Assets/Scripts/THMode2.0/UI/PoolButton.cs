using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum PoolType {
    BCoin,
    Sen,
    Coin
}

public class PoolButton : MonoBehaviour {
    [SerializeField]
    private Vector3 buttonListPosition = new Vector3(27, 72, 117);
    
    [SerializeField]
    private Sprite buttonPress, buttonNormal;
    
    [SerializeField]
    private TokenButton[] tokenBtn;
    
    [SerializeField]
    private MinePoolController minePoolController;
    
    private List<float> _listPos;
    
    [HideInInspector]
    public bool isChanging;
    
    private void Start() {
        foreach (var btn in tokenBtn) {
            btn.Init(this);
        }
        tokenBtn[0].isChosen = true;
        _listPos = new List<float>() { buttonListPosition.x, buttonListPosition.y, buttonListPosition.z };
    }
    
    public void BtnTokenClick(TokenButton btn) {
        //Chọn lại nút đang đc chọn thì ko làm gì
        if (btn.isChosen)
            return;
        
        isChanging = true;
        
        //Update 6 pool của loại token này
        minePoolController.ChangePool(btn.PoolType, true);
        
        //Update màu nút nhất và cập nhật vị trí
        var listPos = _listPos;
        btn.ChangeBtnUi(buttonPress, true);
        btn.RectTransform.DOAnchorPosX(listPos[0], 0.3f).OnComplete(
            () => { isChanging = false; });
        var i = 0;
        foreach (var b in tokenBtn) {
            if (b == btn)
                continue;
            i++;
            b.ChangeBtnUi(buttonNormal, false);
            b.RectTransform.DOAnchorPosX(listPos[i], 0.3f);
        }
    }
}