using UnityEngine;
using Unity.Netcode;
public class HandleNetworkStatusUI : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject serverDesyncObj;
    [SerializeField] private GameObject highLatencyObj;

    private void Update()
    {
        if (!IsOwner)
            return;

        int ping = PlayerInfo.instance.ping.Value;

        highLatencyObj.SetActive(ping > 300);
        serverDesyncObj.SetActive(ping > 1000);
    }
}
