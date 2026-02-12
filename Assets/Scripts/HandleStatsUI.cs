using UnityEngine;
using Unity.Netcode;
using UnityEngine.Profiling;
using TMPro;

public class HandleStatsUI : NetworkBehaviour
{
    public static HandleStatsUI instance;
    [SerializeField] private GameObject statsUI;
    [SerializeField] private TextMeshProUGUI statsText;

    private float _deltaTime = 0.0f;


    private float _updateInterval = 0.5f; 
    private float _timer;
    private FrameTiming[] timings = new FrameTiming[1];

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            statsUI.SetActive(false);
            return;
        }

        statsUI.SetActive(FBPP.GetInt("ShowStatsUI") == 1);

        instance = this;
    }

    private void Update()
    {
        if (!statsUI.activeInHierarchy || !IsOwner)
            return;

        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

        _timer += Time.unscaledDeltaTime;
        if (_timer >= _updateInterval)
        {
            UpdateDebugDisplay();
            _timer = 0;
        }
    }

    public void EnableStatsUI(bool condition)
    {
        statsUI.SetActive(condition);
    }

    private void UpdateDebugDisplay()
    {
        FrameTimingManager.CaptureFrameTimings();
        FrameTimingManager.GetLatestTimings(1, timings);

        float fps = 1.0f / _deltaTime;

        long heapMem = Profiler.GetMonoUsedSizeLong() / 1048576;
        long totalMem = Profiler.GetTotalAllocatedMemoryLong() / 1048576;

        string connectionType = IsServer ? "Server" : "Client";
        ulong clientId = NetworkManager.Singleton.LocalClientId;

        statsText.text =
        $"<b>PERFORMANCE</b>\n" +
        $"FPS: {fps:0.}\n" +
        $"MEM (Heap/Total): {heapMem}MB / {totalMem}MB\n" +
        $"-------------------\n" +
        $"<b>NETWORK</b>\n" +
        $"ROLE: {connectionType}\n" +
        $"ID: {clientId}\n" +
        $"PING: {GetPing()} ms";
    }

    private int GetPing()
    {
        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport != null)
            return (int)NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId);

        return 0;
    }
}
