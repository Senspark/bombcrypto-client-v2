using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// <summary>
/// Nạp ảnh hướng dẫn của OnBoarding theo yêu cầu thay vì gắn cứng vào prefab.
///
/// 13 texture trong Textures/Pack/OnBoarding chỉ nặng 2.2 MB trên đĩa nhưng giải nén ra ~25 MB RAM,
/// và vì được tham chiếu thẳng từ FarmingScene.prefab nên chúng thường trú suốt phiên chơi — kể cả
/// với người chơi cũ không bao giờ mở tutorial (OnBoardingController chỉ chạy khi IStorageManager.NewUser).
///
/// Cách dùng: xoá sprite khỏi Image trong prefab, gắn component này lên cùng GameObject, kéo texture
/// vào spriteRef. Unity tự đánh dấu asset là Addressable khi thả vào AssetReference.
///
/// Ảnh được giữ lại tới khi GameObject bị huỷ (không nhả lúc tắt) để lật trang tutorial không chớp.
/// Đổi lại, người chơi mới đã mở tutorial thì vẫn tốn RAM tới hết scene — chấp nhận được, vì mục tiêu
/// là người chơi cũ không phải trả 25 MB cho thứ họ không xem.
/// </summary>
[RequireComponent(typeof(Image))]
public class OnBoardingImageLoader : MonoBehaviour {
    [SerializeField]
    private AssetReferenceSprite spriteRef;

    private Image _image;
    private bool _requested;

    private void OnEnable() {
        if (_requested) {
            return;
        }
        _requested = true;
        if (!_image) {
            _image = GetComponent<Image>();
        }
        _image.enabled = false;
        Load().Forget();
    }

    private async UniTaskVoid Load() {

        if (spriteRef == null || !spriteRef.RuntimeKeyIsValid()) {
            Debug.LogError($"[OnBoardingImageLoader] Chưa gán spriteRef cho '{PathOf()}'.");
            return;
        }

        var sprite = await AddressableLoader.LoadAsset<Sprite>(spriteRef);
        if (!sprite) {
            Debug.LogError($"[OnBoardingImageLoader] Nạp thất bại '{spriteRef.RuntimeKey}' cho '{PathOf()}'.");
            return;
        }

        if (_image) {
            _image.sprite = sprite;
            _image.enabled = true;
        }
    }

    private void OnDestroy() {
        if (!_requested || spriteRef == null || !spriteRef.RuntimeKeyIsValid()) {
            return;
        }
        if (_image) {
            _image.sprite = null;
        }
        if (spriteRef.IsValid()) {
            spriteRef.ReleaseAsset();
        }
    }

    private string PathOf() {
        var path = name;
        for (var p = transform.parent; p; p = p.parent) {
            path = p.name + "/" + path;
        }
        return path;
    }
}
