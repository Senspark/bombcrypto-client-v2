using System.Collections;
using System.Threading.Tasks;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Engine.Input;
using Engine.Manager;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputType {
    Keyboard,
    Controller,
}

public enum ControllerType {
    Unknown,
    Xbox,
    PlayStation,
}
[Service(nameof(IInputManager))]
public interface IInputManager: IService {
    InputConfigData InputConfig { get; set; }
    ControllerType ControllerType { get; }
    bool ReadButton(string buttonName);
    bool ReadJoystick(string buttonName);
    float ReadAxis(string buttonName);
    UniTask<Sprite> GetImage(string name);
    InputType InputType { get; }
    void SaveConfigData();
    void SetVibration(float time);
    void SetVibration();
    void SetVibration(float min, float max, float time = 0.5f);
}
public class InputManager : MonoBehaviour, IInputManager
{
    private static InputManager _instance;
    private InputCheckChange _inputCheckChange;
    private InputDetection _inputDetection;
    private KeyboardDetection _keyboardDetection;
    private InputCacheImage _inputCacheImage;
    private Coroutine _vibrationCoroutine;
    private readonly Gamepad _pad = Gamepad.current;
    
    private InputConfigData _inputConfig;
    
    public ControllerType ControllerType => _inputCheckChange.ControllerType;
    
    
    public InputConfigData InputConfig {
        get => _inputConfig;
        set => _inputConfig = value;
    }

    public UniTask<Sprite> GetImage(string image) {
        return _inputCacheImage.GetImage(image);
    }

    public InputType InputType => _inputCheckChange.InputType;
    private void Awake() {
        LoadConfigData();
        if (AppConfig.IsMobile()) {
            Destroy(gameObject);
            return;
        }
        if (_instance == null) {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        } 
        else if(_instance != this) {
            Destroy(this);
            return;
        }
        EventManager.Add(PlayerEvent.OnDamage, SetVibration);
        
        //Kiểm tra xem user đang dùng keyboard hay loại controller nào
        _inputCheckChange = new InputCheckChange();
        //Kiểm tra xem user đang sử dụng nút nào của controller
        _inputDetection = new InputDetection();
        //Kiểm tra xem user đang sử dụng nút nào của bàn phím
        _keyboardDetection = new KeyboardDetection(_inputConfig);
        //Lưu lại hình ảnh của controller
        _inputCacheImage = new InputCacheImage();
        
        //For testing
        // if(!AppConfig.IsProduction) {
        //     new TestInput();
        // }
        
    }
    
    //Kiểm tra đồng thời cả keyboard và controller
    public bool ReadButton(string buttonName) {
        if (AppConfig.IsMobile()) {
            return false;
        }
        
        //Kiểm tra xem phím này có map qua bàn phím đc ko
        if (_keyboardDetection.ReadButton(buttonName))
            return true;
        //Kiểm tra trên controller
        return _inputDetection.ReadButton(buttonName);
    }

    //Chỉ kiểm tra controller
    public bool ReadJoystick(string buttonName) {
        if (AppConfig.IsMobile()) {
            return false;
        }
        
        return _inputDetection.ReadButton(buttonName);
    }

    public float ReadAxis(string buttonName) {
        if (AppConfig.IsMobile()) {
            return 0f;
        }
        
        return Input.GetAxis(buttonName);
    }
    
    private void LoadConfigData() {
        _inputConfig = SaveLoadManager.Load<InputConfigData>("inputConfig");
    }
    
    public void SaveConfigData() {
        SaveLoadManager.Save(_inputConfig, "inputConfig");
    }

    public Task<bool> Initialize() {
        return Task.FromResult(true);
    }

    public void Destroy() {
    }

    private void Update() {
        _inputCheckChange.Process();
        _inputDetection.Process();
    }

    #region Vibration

    public void SetVibration(float time) {
        SetVibration(0.5f, 0.5f, time);
    }
    public void SetVibration() {
        SetVibration(0.5f, 0.5f, 0.5f);
    }
    public void SetVibration(float min , float max , float time = 0.5f) {
        if(_pad == null)
            return;
        if (_vibrationCoroutine != null) {
            StopCoroutine(_vibrationCoroutine);
            _pad.SetMotorSpeeds(0f, 0f);
        }
        _vibrationCoroutine = StartCoroutine(StartVibration(min, max, time));
    }
    IEnumerator StartVibration(float min, float max, float time)
    {
        _pad.SetMotorSpeeds(min, max);
        yield return new WaitForSeconds(time);
        // Stop vibration
        _pad.SetMotorSpeeds(0f, 0f);
        _vibrationCoroutine = null;

    }

    #endregion    
    
}
