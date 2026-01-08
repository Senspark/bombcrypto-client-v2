using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using App;

using Constant;

using DG.Tweening;

using Senspark;

using UnityEngine;

[System.Serializable]
public class RewardObjectInfo {
    [NotNull] public Sprite iconBsc, iconPolygon;
    public Color32 color;
    
    public Sprite GetIcon(NetworkType type) {
        return type == NetworkType.Binance ? iconBsc : iconPolygon;
    }
}
public class RewardObjectThModeController : MonoBehaviour {
    [SerializeField]
    private RewardObjectThMode objectPrefab;
    
    [SerializeField]
    private RewardObjectInfo bcoin, sen, coin;
    
    private List<AnimationObject> _objectList = new();
    private List<List<AnimationObject>> _animationList = new();
    
    private INetworkConfig _networkConfig;
    private Transform _transform;
    
    public float _moveUpDuration = 1.2f;
    public float _fadeOutDuration = 1.2f;
    public float _staggerDelay = 0.2f;
    public float _moveUpOffset = 40f;
    
    private float[] _startPos = new[] { -130f, -90f, -60f, -20f, 20f, 60f };
    
    private void Start() {
        _networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
        _transform = transform;
    }
    
    public void ShowReward(RewardType type, List<float[]> amountArray) {
        //Clear list animtion cũ, đảm bảo các rarity có khoảng cách rỗng nếu ko có thưởng, nhằm mục đích phân biệt rarity
        Clear();
        if(amountArray == null)
            return;
        
        //show thưởng theo pool hiện tại (bcoin, sen, coin)
        for (var i = 0; i < amountArray.Count; i++) {
            //tách riêng từng rarity của 1 pool
            for (var j = 0; j < amountArray[i].Length; j++) {
                var obj = GetObject();
                UpdateInfo(type, amountArray[i][j], ref obj);
                obj.StartPos = _startPos[i];
                //stack to animation list for animation later
                _animationList[i].Add(obj);
            }
        }
        DoAnimation();
    }
    
    public AnimationObject GetObject()
    {
        AnimationObject ani = null;
        
        // Check if _objectList has available objects
        if (_objectList.Count > 0)
        {
            // Get object from the list
            ani = _objectList[0];
            _objectList.RemoveAt(0);
        } else {
            // Instantiate new object from prefab if _objectList is empty
            var obj = Instantiate(objectPrefab, _transform);
            obj.Init();
            ani = new AnimationObject();
            ani.Reward = obj;
            ani.Reward.Canvas.alpha = 0;
        }
        return ani;
    }
    
    public void ReturnObject(AnimationObject obj)
    {
        // Add object back to _objectList
        _objectList.Add(obj);
    }
    
    private void UpdateInfo(RewardType type, float amount, ref AnimationObject obj) {
        var amountString = amount.ToString("0.########");
        RewardObjectInfo currentToken;
        switch (type) {
            case RewardType.BCOIN:
                currentToken = bcoin;
                break;
            case RewardType.Senspark:
                currentToken = sen;
                break;
            case RewardType.COIN:
                currentToken = coin;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        obj.Reward.Amount.text = $"+{amountString}";
        obj.Reward.Amount.color = currentToken.color;
        obj.Reward.Icon.sprite = currentToken.GetIcon(_networkConfig.NetworkType);
    }
    
    private void DoAnimation() {
        Sequence mainSequence = DOTween.Sequence();
        foreach (var ani in _animationList) {
            float delay = 0f;
            foreach (var obj in ani) {
                Sequence sequence = DOTween.Sequence();
                obj.Reward.Rect.DOAnchorPosY(obj.StartPos, 0);
                obj.Reward.gameObject.SetActive(true);
                
                sequence.SetUpdate(true);
                
                // Set the initial delay for this sequence
                delay += _staggerDelay;
                sequence.PrependInterval(delay);
                
                sequence.Append(obj.Reward.Canvas.DOFade(1, 0));
                // Move up
                sequence.Join(obj.Reward.Rect.DOAnchorPosY(obj.StartPos + _moveUpOffset, _moveUpDuration).SetEase(Ease.OutQuad));
                // Fade out
                sequence.Join(obj.Reward.Canvas.DOFade(0, _fadeOutDuration).SetEase(Ease.InQuad));
                
                // Add a callback to deactivate the object when the sequence is complete
                sequence.OnComplete(() => {
                    obj.Reward.gameObject.SetActive(false);
                    ReturnObject(obj);
                });
                mainSequence.Join(sequence);
            }
            
        }
    }
    
    private void Clear() {
        _animationList.Clear();
        for (int i = 0; i < 6; i++) {
            _animationList.Add(new List<AnimationObject>());
        }
    }
    
    //Test
    
    public RewardType retype = RewardType.Senspark;
    public List<float[]> array = new List<float[]> {
        new []{1f,2f},
        Array.Empty<float>(),
        new []{1f,2f,3,5,4},
        Array.Empty<float>(),
        new []{1f,2f,3,5},
        new []{1f,2f,3f, 5f,6f},
    };
    [Button]
    public void Test() {
        ShowReward(retype, array);
    }
    public class AnimationObject {
        public RewardObjectThMode Reward;
        public float StartPos;
    }
    
}
