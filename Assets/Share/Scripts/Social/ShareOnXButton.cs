using System;
using System.Collections;

using App;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Senspark;

using UnityEngine;
using UnityEngine.UI;

#if !UNITY_EDITOR
using Communicate;
using Share.Scripts.Dialog;
#endif

namespace Share.Scripts.Social {
    /// <summary>
    /// Component drop-in: designer gắn thẳng vào prefab dialog (DialogNewHero / summary / token reward),
    /// đặt + style tuỳ ý. Host gọi SetContext bơm sẵn SharePayload (host dựng qua SharePayload.ForHeroes /
    /// ForToken) — nút KHÔNG biết PlayerData/TokenData, chỉ đóng dấu ảnh + gửi.
    /// Click -> chụp TOÀN MÀN HÌNH (tự ẩn mình qua CanvasGroup) -> gửi tag SHARE_ON_X sang React.
    /// Editor: không gọi JS, save ảnh + dump JSON.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ShareOnXButton : MonoBehaviour {
        private const string ShareTag = "SHARE_ON_X";

        [Tooltip("Cạnh lớn nhất tối đa của ảnh (px). <=0 = giữ nguyên độ phân giải màn hình.")]
        [SerializeField] private int maxDim = 1080;

        [Range(50, 95)]
        [SerializeField] private int jpgQuality = 80;

        private Canvas _dialogCanvas;
        private SharePayload _payload;
        private bool _busy;
        private CanvasGroup _canvasGroup;
        private LayoutElement _layoutElement;

        private void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
            // Nếu nút nằm trong LayoutGroup -> cần LayoutElement để ignoreLayout lúc chụp (sibling
            // co giãn như nút OK sẽ giãn full lại). Tự thêm nếu chưa có.
            _layoutElement = GetComponent<LayoutElement>();
            if (!_layoutElement && transform.parent && transform.parent.GetComponent<LayoutGroup>()) {
                _layoutElement = gameObject.AddComponent<LayoutElement>();
            }
        }

        // Mặc định prefab để GameObject INACTIVE (ẩn). Host gọi SetContext mới hiện.
        // payload do host dựng sẵn (source + heroes hoặc token); nút chỉ stamp ảnh vào lúc chụp.
        public void SetContext(Canvas dialogCanvas, SharePayload payload) {
            _dialogCanvas = dialogCanvas;
            _payload = payload;
            gameObject.SetActive(true);
        }

        // Wire vào Button.onClick trong prefab.
        public void OnClickShare() {
            // _payload null = nút active nhưng host chưa bơm context -> bỏ qua, đừng NRE.
            if (_busy || _payload == null) {
                return;
            }
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            StartCoroutine(CaptureRoutine());
        }

        private IEnumerator CaptureRoutine() {
            _busy = true;

            // Ẩn chính nút (KHÔNG tắt GameObject để coroutine không bị dừng).
            // ignoreLayout -> LayoutGroup bỏ qua nút này -> sibling co giãn (vd nút OK) giãn full lại.
            var parent = transform.parent as RectTransform;
            if (_layoutElement) {
                _layoutElement.ignoreLayout = true;
            }
            if (_canvasGroup) {
                _canvasGroup.alpha = 0f;
            }
            if (_layoutElement && parent) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            }

            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();

            if (_layoutElement) {
                _layoutElement.ignoreLayout = false;
            }
            if (_canvasGroup) {
                _canvasGroup.alpha = 1f;
            }
            if (_layoutElement && parent) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            }

            if (maxDim > 0 && Mathf.Max(tex.width, tex.height) > maxDim) {
                tex = Downscale(tex, maxDim);
            }

            var jpg = ImageConversion.EncodeToJPG(tex, jpgQuality);
            Destroy(tex);

            _payload.image = Convert.ToBase64String(jpg);
            var json = JsonConvert.SerializeObject(_payload);
#if UNITY_EDITOR
            // Editor: KHÔNG gọi JS (JS_CallMethod chỉ sống trên WebGL). Save ảnh + dump JSON để xem.
            SaveEditorPreview(jpg, _payload, json);
            _busy = false;
#else
            SendShare(json).Forget();
#endif
        }

        private Texture2D Downscale(Texture2D src, int maxEdge) {
            var scale = (float)maxEdge / Mathf.Max(src.width, src.height);
            var nw = Mathf.Max(1, Mathf.RoundToInt(src.width * scale));
            var nh = Mathf.Max(1, Mathf.RoundToInt(src.height * scale));
            var rt = RenderTexture.GetTemporary(nw, nh, 0, RenderTextureFormat.ARGB32);
            var prev = RenderTexture.active;
            Graphics.Blit(src, rt);
            RenderTexture.active = rt;
            var dst = new Texture2D(nw, nh, TextureFormat.RGB24, false);
            dst.ReadPixels(new Rect(0, 0, nw, nh), 0, 0);
            dst.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            Destroy(src);
            return dst;
        }

#if !UNITY_EDITOR
        private async UniTaskVoid SendShare(string json) {
            try {
                var status = await NewJavascriptProcessor.Instance.CallReact(ShareTag, json);
                if (!string.IsNullOrEmpty(status) && status.StartsWith("ERROR", StringComparison.Ordinal)) {
                    DialogOK.ShowErrorMsgOnly(_dialogCanvas, status);
                }
            } catch (Exception e) {
                DialogOK.ShowErrorMsgOnly(_dialogCanvas, e.Message);
            } finally {
                _busy = false;
            }
        }
#endif

#if UNITY_EDITOR
        private void SaveEditorPreview(byte[] jpg, SharePayload payload, string fullJson) {
            try {
                var dir = System.IO.Path.Combine(Application.dataPath, "../Logs");
                System.IO.Directory.CreateDirectory(dir);
                var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                var imgPath = System.IO.Path.Combine(dir, $"share_preview_{stamp}.jpg");
                System.IO.File.WriteAllBytes(imgPath, jpg);

                // Full payload (kèm base64) -> file, để test React parse mà không spam console.
                var jsonPath = System.IO.Path.Combine(dir, $"share_payload_{stamp}.json");
                System.IO.File.WriteAllText(jsonPath, fullJson);

                // Console: bản gọn (source + heroes positional + size ảnh).
                var compact = JsonConvert.SerializeObject(new {
                    source = payload.source,
                    heroes = payload.heroes,
                    imageBytes = jpg.Length,
                });
                Debug.Log($"[ShareOnX] (Editor, KHÔNG gọi JS)\n  image: {imgPath}\n  json : {jsonPath}\n  payload: {compact}");
            } catch (Exception e) {
                Debug.LogWarning($"[ShareOnX] save preview failed: {e.Message}");
            }
        }
#endif
    }
}
