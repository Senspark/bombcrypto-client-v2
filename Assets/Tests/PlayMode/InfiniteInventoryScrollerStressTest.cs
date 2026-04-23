using System.Collections;
using System.Collections.Generic;
using App;
using DynamicScrollRect;
using Game.Dialog;
using NUnit.Framework;
using Scenes.FarmingScene.Scripts;
using Scripts.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using System.IO;
using Cysharp.Threading.Tasks;

public class InfiniteInventoryScrollerStressTest {
    private GameObject _testObj;
    private InfiniteInventoryScroller _scroller;
    private ScrollRect _scrollRect;
    private RectTransform _content;
    private RectTransform _viewport;
    private GridLayoutGroup _gridLayout;
    private DynamicInventoryItem _itemPrefab;

    private const int TOTAL_ITEMS = 10000;
    private const int POOL_THRESHOLD = 40;
    private const float SCROLL_SPEED = 5f; // scroll normal per frame

    private List<PlayerData> _mockData;
    private List<float> _frameDeltas = new List<float>();

    [SetUp]
    public void Setup() {
        // Create Mock UI hierarchy
        _testObj = new GameObject("TestScroller");
        var canvas = _testObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scrollObj = new GameObject("ScrollRect");
        scrollObj.transform.SetParent(_testObj.transform, false);
        _scrollRect = scrollObj.AddComponent<ScrollRect>();
        _scroller = scrollObj.AddComponent<InfiniteInventoryScroller>();

        var viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollObj.transform, false);
        _viewport = viewportObj.AddComponent<RectTransform>();
        _viewport.sizeDelta = new Vector2(800, 600); // 800x600 viewport
        _scrollRect.viewport = _viewport;

        var contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        _content = contentObj.AddComponent<RectTransform>();
        _content.anchorMin = new Vector2(0, 1);
        _content.anchorMax = new Vector2(1, 1);
        _content.pivot = new Vector2(0, 1);
        _content.sizeDelta = new Vector2(800, 0); // Need width to calculate columns

        _scrollRect.content = _content;

        _gridLayout = contentObj.AddComponent<GridLayoutGroup>();
        _gridLayout.cellSize = new Vector2(100, 100);
        _gridLayout.spacing = new Vector2(10, 10);

        // Create Item Prefab
        var prefabObj = new GameObject("ItemPrefab");
        var prefabRect = prefabObj.AddComponent<RectTransform>();
        prefabRect.sizeDelta = new Vector2(100, 100);
        _itemPrefab = prefabObj.AddComponent<DynamicInventoryItem>();

        // Setup missing mock dependency
        var invItem = prefabObj.AddComponent<InventoryItem>();
        var invItemField = typeof(DynamicInventoryItem).GetField("item", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        invItemField.SetValue(_itemPrefab, invItem);

        prefabObj.SetActive(false); // keep as prefab

        // Generate 10,000 Mock PlayerData
        _mockData = new List<PlayerData>();
        for (int i = 0; i < TOTAL_ITEMS; i++) {
            _mockData.Add(new PlayerData {
                genId = $"Hero_{i}",
                rare = (i % 5) + 1, // Random rarities 1-5
                level = 1,
                heroId = new Engine.Entities.HeroId(i, Engine.Entities.HeroAccountType.Nft)
            });
        }

        _frameDeltas.Clear();
    }

    [UnityTest]
    public IEnumerator StressTestInfiniteScroll() {
        bool testFailed = false;
        string failureReason = "";

        try {
            // 1. Setup Scroller
            _scroller.Init(
                _scrollRect,
                _content,
                _itemPrefab,
                _gridLayout,
                _viewport,
                null, // onScrollToBottom
                DialogInventory.ChooseMode.InventoryMode,
                new List<PlayerData>(), // heroesIdBurn
                new List<int>(), // heroesBurnIds
                null, // inventoryCallback
                null, // dialogCanvas
                null, // heroDescriptionPanel
                false, // isShowLockHero
                DialogInventory.ActiveFilter.All
            );

            // 2. Inject 10,000 items
            _scroller.SetData(_mockData);
            yield return null; // wait one frame for pool initialization
            yield return null;

            // 3. Validate Pool Size
            var pool = _scroller.GetActiveItems();
            int initialPoolSize = pool.Count;
            Assert.IsTrue(initialPoolSize <= POOL_THRESHOLD, $"Pool size exceeded threshold! Expected <= {POOL_THRESHOLD}, got {initialPoolSize}");

            // 4. Simulate Fast Scroll
            _scrollRect.verticalNormalizedPosition = 1f; // Top
            while (_scrollRect.verticalNormalizedPosition > 0f) {
                // Adjust position
                _scrollRect.verticalNormalizedPosition -= SCROLL_SPEED * Time.deltaTime;
                if (_scrollRect.verticalNormalizedPosition < 0f) {
                     _scrollRect.verticalNormalizedPosition = 0f;
                }

                // Force layout update to process scroll event
                _frameDeltas.Add(Time.deltaTime);
                yield return null;
            }

            // 5. Re-validate Pool Size after scroll
            pool = _scroller.GetActiveItems();
            int endPoolSize = pool.Count;
            Assert.IsTrue(endPoolSize <= POOL_THRESHOLD, $"Pool size exceeded threshold after scroll! Expected <= {POOL_THRESHOLD}, got {endPoolSize}");

            // 6. Test Filter by Rarity (e.g. Rarity 5)
            var filteredData = _mockData.FindAll(p => p.rare == 5);
            _scroller.SetData(filteredData);
            yield return null;

            var filterPool = _scroller.GetActiveItems();
            int filterPoolSize = filterPool.Count;
            Assert.IsTrue(filterPoolSize <= POOL_THRESHOLD, $"Pool size exceeded after filter! Expected <= {POOL_THRESHOLD}, got {filterPoolSize}");

            // 7. Calculate Spikes
            int spikes = 0;
            float maxDelta = 0;
            foreach (var delta in _frameDeltas) {
                if (delta > maxDelta) maxDelta = delta;
                if (delta > 0.05f) { // 50ms spike
                    spikes++;
                }
            }

            // 8. Generate Report
            GenerateReport(TOTAL_ITEMS, endPoolSize, spikes, maxDelta, true, "SUCCESS");

        } catch (System.Exception e) {
            testFailed = true;
            failureReason = e.Message;
            GenerateReport(TOTAL_ITEMS, -1, -1, -1, false, $"FAILED - {failureReason}");
            throw; // Re-throw to fail the NUnit test
        }
    }

    private void GenerateReport(int totalItems, int poolSize, int spikes, float maxDelta, bool filterConfirmed, string status) {
        string path = Application.dataPath + "/../client-stress-test-report.md";
        string content = $"# Client Inventory Stress Test Report\n\n" +
                         $"- **Total Items Injected**: {totalItems}\n" +
                         $"- **Real GameObjects Instantiated (Pool Size)**: {poolSize}\n" +
                         $"- **Frame Spikes (>50ms)**: {spikes} (Max frame time: {maxDelta * 1000:F2}ms)\n" +
                         $"- **Filter Optimization Confirmed**: {(filterConfirmed ? "Yes" : "No")}\n" +
                         $"- **Final Status**: {status}\n";

        File.WriteAllText(path, content);
        Debug.Log($"[StressTest] Report generated at {path}");
    }

    [TearDown]
    public void TearDown() {
        if (_testObj != null) {
            Object.DestroyImmediate(_testObj);
        }
    }
}
