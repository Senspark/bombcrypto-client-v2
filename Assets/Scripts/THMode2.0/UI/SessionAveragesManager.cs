using System;
using System.Runtime.InteropServices;
using App;
using Engine.Entities;
using Senspark;
using UnityEngine;

/// <summary>
/// Persistent per-wallet tracker for the "Farm Averages" panel.
///
/// Tokens (BCOIN/SEN): per-minute TH payout (THModeV2Manager.OnReward) + the per-block
/// rewards that come inside the START_EXPLODE_V5 response for broken blocks. The server
/// credits those block rewards directly (BlockRewardManager), separate from the TH pool,
/// so both sources are counted.
///
/// Chests + bombs: counted from ServerObserver.OnPveExploded. The explode response only
/// carries i/j/hp per block (no type), so the chest type is resolved from the client-side
/// farm map (IBHeroManager.MapDatas[i,j]). One bomb per explosion.
///
/// Persistence: PlayerPrefs keyed by wallet (WebGL-backed by IndexedDB, survives reconnect).
/// The timer starts on the first farm activity and runs continuously (can reach days). Only
/// Reset() zeroes the counters and restarts the timer. Lives in DontDestroyOnLoad so it keeps
/// accumulating across map reloads; the panel UI reads from it.
/// </summary>
public class SessionAveragesManager : MonoBehaviour
{
    public static SessionAveragesManager Instance { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void MediasLS_Set(string key, string value);
    [DllImport("__Internal")] private static extern string MediasLS_Get(string key);
    [DllImport("__Internal")] private static extern void MediasLS_Remove(string key);
#endif

    [Serializable]
    public class Data
    {
        public long startTimeMs;   // when counting started (0 = not started yet)
        public double bcoin;
        public double sen;
        public long chestWood;
        public long chestSilver;
        public long chestGold;
        public long chestDiamond;
        public long chestTotal;    // every chest type (incl. legend/key/bcoin-diamond)
        public long bombs;
    }

    private Data _data = new Data();
    private string _wallet;
    private ObserverHandle _handle;
    private bool _ready;
    private float _initTimer;
    private float _saveTimer;
    private bool _dirty;
    private bool _loggedReward;
    private int _pveLogs;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("SessionAveragesManager");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<SessionAveragesManager>();
    }

    private void Update()
    {
        if (!_ready)
        {
            _initTimer += Time.unscaledDeltaTime;
            if (_initTimer >= 1f) { _initTimer = 0f; TryInit(); }
            return;
        }
        if (_dirty)
        {
            _saveTimer += Time.unscaledDeltaTime;
            if (_saveTimer >= 5f) Save();
        }
    }

    private void TryInit()
    {
        var server = TryResolve<IServerManager>();
        if (server == null) return;
        var thMode = server.ThModeV2Manager;
        if (thMode == null) return;

        var accMgr = TryResolve<IUserAccountManager>();
        string wallet = null;
        if (accMgr != null)
        {
            var acc = accMgr.GetRememberedAccount();
            if (acc != null)
                wallet = string.IsNullOrEmpty(acc.walletAddress) ? acc.userName : acc.walletAddress;
        }
        if (string.IsNullOrEmpty(wallet)) return; // wait until logged in

        _wallet = wallet;
        Load();

        _handle = new ObserverHandle();
        _handle.AddObserver(thMode, new THModeObserver { OnReward = OnThReward });
        _handle.AddObserver(server, new ServerObserver { OnPveExploded = OnPveExploded });
        _ready = true;
        Debug.Log("[Medias] SessionAveragesManager ready for wallet " + _wallet);
    }

    private static T TryResolve<T>() where T : class
    {
        try { return ServiceLocator.Instance.Resolve<T>(); }
        catch { return null; }
    }

    private void OnThReward(THModeV2RewardData d)
    {
        if (d == null) return;
        if (!_loggedReward) { _loggedReward = true; Debug.Log($"[Medias] first TH reward: bcoin={d.Bcoin} sen={d.Sen} coin={d.Coin}"); }
        EnsureStarted();
        if (d.Bcoin > 0f) _data.bcoin += d.Bcoin;
        if (d.Sen > 0f) _data.sen += d.Sen;
        _dirty = true;
    }

    private void OnPveExploded(IPveExplodeResponse r)
    {
        if (r == null) return;
        EnsureStarted();
        _data.bombs += 1;
        var blocks = r.DestroyedBlocks;
        var map = TryResolve<IBHeroManager>()?.MapDatas;
        if (_pveLogs < 6)
        {
            _pveLogs++;
            string detail = "";
            if (blocks != null)
                foreach (var bl in blocks)
                    if (bl != null)
                        detail += $" (i={bl.Coord.x} j={bl.Coord.y} hp={bl.Hp} rw={(bl.Rewards != null ? bl.Rewards.Count : 0)} et={ResolveEntityType(bl, map)})";
            Debug.Log($"[Medias] PVE explode #{_pveLogs}: {(blocks != null ? blocks.Count : 0)} blocks{detail}");
        }
        if (blocks != null)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                var b = blocks[i];
                if (b == null || b.Hp > 0) continue; // only fully broken blocks
                switch (ResolveEntityType(b, map))
                {
                    case EntityType.woodenChest: _data.chestWood++; _data.chestTotal++; break;
                    case EntityType.silverChest: _data.chestSilver++; _data.chestTotal++; break;
                    case EntityType.goldenChest: _data.chestGold++; _data.chestTotal++; break;
                    case EntityType.diamondChest: _data.chestDiamond++; _data.chestTotal++; break;
                    case EntityType.legendChest:
                    case EntityType.keyChest:
                    case EntityType.BcoinDiamondChest:
                        _data.chestTotal++; break;
                }
                AddBlockRewards(b);
            }
        }
        _dirty = true;
    }

    // The explode response only carries i/j/hp; look the type up in the client-side farm map.
    // Falls back to the block's own Type (+5 offset) when the map has no entry (e.g. story mode).
    private static EntityType ResolveEntityType(IPveBlockData b, MapData[,] map)
    {
        if (map != null)
        {
            int i = b.Coord.x, j = b.Coord.y;
            if (i >= 0 && i < map.GetLength(0) && j >= 0 && j < map.GetLength(1) && map[i, j] != null)
                return map[i, j].entityType;
        }
        return (EntityType)(b.Type + 5);
    }

    private void AddBlockRewards(IPveBlockData b)
    {
        if (b.Rewards == null) return;
        for (int i = 0; i < b.Rewards.Count; i++)
        {
            var rw = b.Rewards[i];
            if (rw == null || rw.Value <= 0f || rw.Type == null) continue;
            switch (rw.Type.Type)
            {
                case BlockRewardType.BCoin:
                case BlockRewardType.BCoinDeposited:
                case BlockRewardType.BcoinBridge:
                    _data.bcoin += rw.Value; break;
                case BlockRewardType.Senspark:
                case BlockRewardType.SensparkDeposited:
                case BlockRewardType.SenBridge:
                    _data.sen += rw.Value; break;
            }
        }
    }

    private void EnsureStarted()
    {
        if (_data.startTimeMs == 0)
        {
            _data.startTimeMs = NowMs();
            _dirty = true;
        }
    }

    public void Reset()
    {
        _data = new Data { startTimeMs = NowMs() };
        Save();
    }

    // ------------------------------------------------------- getters (UI)

    public Data Snapshot() => _data;

    public double ElapsedHours()
    {
        if (_data.startTimeMs == 0) return 0;
        var ms = NowMs() - _data.startTimeMs;
        return ms <= 0 ? 0 : ms / 3600000.0;
    }

    public TimeSpan Elapsed()
    {
        if (_data.startTimeMs == 0) return TimeSpan.Zero;
        var ms = NowMs() - _data.startTimeMs;
        return ms <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(ms);
    }

    // ------------------------------------------------------- persistence

    private static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private string Key() => "FarmAverages-" + _wallet;

    private void Load()
    {
        var json = LoadRaw(Key());
        if (!string.IsNullOrEmpty(json))
        {
            try { _data = JsonUtility.FromJson<Data>(json) ?? new Data(); }
            catch { _data = new Data(); }
        }
        else _data = new Data();
    }

    private void Save()
    {
        _saveTimer = 0f;
        _dirty = false;
        if (string.IsNullOrEmpty(_wallet)) return;
        SaveRaw(Key(), JsonUtility.ToJson(_data));
    }

    // Real browser localStorage in WebGL (visible in DevTools -> Application -> Local Storage);
    // PlayerPrefs fallback in the editor / other platforms.
    private static string LoadRaw(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return MediasLS_Get(key);
#else
        return PlayerPrefs.GetString(key, "");
#endif
    }

    private static void SaveRaw(string key, string value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        MediasLS_Set(key, value);
#else
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
#endif
    }

    private void OnApplicationPause(bool paused) { if (paused && _dirty) Save(); }
    private void OnApplicationQuit() { if (_dirty) Save(); }
    private void OnDestroy() { _handle?.Dispose(); }
}
