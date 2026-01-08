using UnityEngine;
using TMPro;
using System.Diagnostics;
using UnityEngine.Profiling;
using System.Collections;

using App;

public class PerformanceMonitor : MonoBehaviour
{
    public TMP_Text fpsText;
    public TMP_Text ramText;

    private float _deltaTime = 0.0f;
    private Process _currentProcess;

    void Start()
    {
        if(AppConfig.IsProduction)
        {
            gameObject.SetActive(false);
            return;
        }
        _currentProcess = Process.GetCurrentProcess();
        StartCoroutine(UpdatePerformanceMetrics());
    }

    void Update()
    {
        // Calculate FPS
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
    }

    private IEnumerator UpdatePerformanceMetrics()
    {
        while (true)
        {
            // Calculate FPS
            var fps = 1.0f / _deltaTime;
            fpsText.text = $"FPS: {Mathf.Ceil(fps)}";

            // Get RAM usage
            var ramUsage = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
            ramText.text = $"RAM: {ramUsage:F2} MB";

            // Wait for 0.2 seconds
            yield return new WaitForSeconds(0.2f);
        }
    }
}