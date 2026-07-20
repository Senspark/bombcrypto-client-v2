using System;
using System.Globalization;
using System.Net.Http;
using System.Numerics;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Editor {
    // Testnet only.
    public class BridgeDebugWindow : EditorWindow {
        private const string P = "BombBridgeDebug.";

        private static readonly string[] NetworkNames = { "bsctestnet", "amoy" };
        private static readonly string[,] Defaults = {
            { "0xb34e148821082C7c73f094Be1E194b3474209bba", "0x648a9cf8e95c73110d28e7e2329b2d0910bd36b8", "0x4B5828F31550aFe15C61D7a765D9597ad4282325" },
            { "0x97D80e2914bcBd6F957eE804c7B6fe844A0A1cd7", "0xcF693b54F86c49bbBa54Ff887488Bbf84C5D05BF", "0x93567522610828695F36178b180989996082404A" },
        };
        private static readonly string[] TokenLabels = { "BCOIN", "SEN" };

        private static readonly HttpClient Http = new HttpClient();

        // config
        private string _proxyUrl = "http://127.0.0.1:8555";
        private int _networkIndex;
        private string _privateKey = "";
        private string _bridgeAddress = "";
        private string _bcoinToken = "";
        private string _senToken = "";
        private string _userWallet = "";

        private int _txTokenIndex;
        private string _txAmount = "1";

        private string _log = "";
        private Vector2 _scroll;

        [MenuItem("Tools/Bridge Debug Panel")]
        public static void ShowWindow() {
            GetWindow<BridgeDebugWindow>("Bridge Debug");
        }

        private void OnEnable() {
            _proxyUrl = EditorPrefs.GetString(P + "proxyUrl", _proxyUrl);
            _networkIndex = EditorPrefs.GetInt(P + "networkIndex", 0);
            _privateKey = EditorPrefs.GetString(P + "privateKey", "");
            _bridgeAddress = EditorPrefs.GetString(P + "bridge", Defaults[_networkIndex, 0]);
            _bcoinToken = EditorPrefs.GetString(P + "bcoin", Defaults[_networkIndex, 1]);
            _senToken = EditorPrefs.GetString(P + "sen", Defaults[_networkIndex, 2]);
            _userWallet = EditorPrefs.GetString(P + "wallet", "");
        }

        private void OnDisable() => SavePrefs();

        private void SavePrefs() {
            EditorPrefs.SetString(P + "proxyUrl", _proxyUrl);
            EditorPrefs.SetInt(P + "networkIndex", _networkIndex);
            EditorPrefs.SetString(P + "privateKey", _privateKey);
            EditorPrefs.SetString(P + "bridge", _bridgeAddress);
            EditorPrefs.SetString(P + "bcoin", _bcoinToken);
            EditorPrefs.SetString(P + "sen", _senToken);
            EditorPrefs.SetString(P + "wallet", _userWallet);
        }

        private void OnGUI() {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Config (testnet only)", EditorStyles.boldLabel);
            _proxyUrl = Row("editor-chain-proxy URL", _proxyUrl);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Network", GUILayout.Width(180));
            var newNet = EditorGUILayout.Popup(_networkIndex, NetworkNames);
            EditorGUILayout.EndHorizontal();
            if (newNet != _networkIndex) {
                _networkIndex = newNet;
                LoadDefaultsForNetwork();
            }
            if (GUILayout.Button("Load testnet defaults for " + NetworkNames[_networkIndex])) {
                LoadDefaultsForNetwork();
            }
            _bridgeAddress = Row("Bridge address", _bridgeAddress);
            _bcoinToken = Row("BCOIN token", _bcoinToken);
            _senToken = Row("SEN token", _senToken);
            _userWallet = Row("User wallet", _userWallet);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Private key (throwaway)", GUILayout.Width(180));
            _privateKey = EditorGUILayout.PasswordField(_privateKey);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("On-chain via proxy (works without Play mode)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Token", GUILayout.Width(180));
            _txTokenIndex = EditorGUILayout.Popup(_txTokenIndex, TokenLabels);
            EditorGUILayout.EndHorizontal();
            _txAmount = Row("Amount (tokens)", _txAmount);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Approve")) Run(DoApprove);
            if (GUILayout.Button("Deposit")) Run(DoDeposit);
            if (GUILayout.Button("Read counters")) Run(DoReadCounters);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Log", EditorStyles.boldLabel);
            if (GUILayout.Button("Clear", GUILayout.Width(60))) _log = "";
            EditorGUILayout.EndHorizontal();
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(200));
            EditorGUILayout.TextArea(_log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private string Row(string label, string value) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(180));
            var result = EditorGUILayout.TextField(value);
            EditorGUILayout.EndHorizontal();
            return result;
        }

        private void LoadDefaultsForNetwork() {
            _bridgeAddress = Defaults[_networkIndex, 0];
            _bcoinToken = Defaults[_networkIndex, 1];
            _senToken = Defaults[_networkIndex, 2];
        }


        private async UniTask DoApprove() {
            var wei = ToWei(_txAmount);
            var token = _txTokenIndex == 0 ? _bcoinToken : _senToken;
            Log($"approve token={token} spender={_bridgeAddress} amount={wei}");
            var jo = await ProxySend(token, "approve(address,uint256)", new object[] { _bridgeAddress, wei });
            Log($"approve tx: {jo}");
        }

        private async UniTask DoDeposit() {
            var wei = ToWei(_txAmount);
            var token = _txTokenIndex == 0 ? _bcoinToken : _senToken;
            Log($"deposit token={token} amount={wei}");
            var jo = await ProxySend(_bridgeAddress, "deposit(address,uint256)", new object[] { token, wei });
            Log($"deposit tx: {jo}");
        }

        private async UniTask DoReadCounters() {
            var token = _txTokenIndex == 0 ? _bcoinToken : _senToken;
            var dep = await ProxyCall(_bridgeAddress, "deposited(address,address)", new object[] { _userWallet, token });
            var wd = await ProxyCall(_bridgeAddress, "withdrawn(address,address)", new object[] { _userWallet, token });
            Log($"on-chain {TokenLabels[_txTokenIndex]}  deposited={dep}  withdrawn={wd}");
        }


        private async UniTask<string> ProxyCall(string address, string method, object[] args) {
            var body = new { network = NetworkNames[_networkIndex], address, method, args };
            var jo = await PostJson(_proxyUrl.TrimEnd('/') + "/call", body);
            return jo["result"]?.ToString();
        }

        private async UniTask<JObject> ProxySend(string address, string method, object[] args) {
            if (string.IsNullOrEmpty(_privateKey)) {
                throw new Exception("Private key is empty (throwaway testnet key required for /send).");
            }
            var body = new { network = NetworkNames[_networkIndex], address, method, args, privateKey = _privateKey };
            return await PostJson(_proxyUrl.TrimEnd('/') + "/send", body);
        }

        private static async UniTask<JObject> PostJson(string url, object body) {
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await Http.PostAsync(url, content);
            var text = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) {
                throw new Exception($"HTTP {(int)resp.StatusCode}: {text}");
            }
            return JObject.Parse(text);
        }

        private static string ToWei(string tokens) {
            var d = decimal.Parse(tokens, CultureInfo.InvariantCulture);
            var wei = d * 1_000_000_000_000_000_000m;
            return new BigInteger(wei).ToString(CultureInfo.InvariantCulture);
        }

        private void Run(Func<UniTask> fn) {
            SavePrefs();
            UniTask.Void(async () => {
                try {
                    await fn();
                } catch (Exception e) {
                    Log($"ERROR: {e.Message}");
                }
                Repaint();
            });
        }

        private void Log(string message) {
            _log = $"[{DateTime.Now:HH:mm:ss}] {message}\n" + _log;
            Debug.Log($"[BridgeDebug] {message}");
        }
    }
}
