using UnityEngine;
using Unity.Netcode;

public class EnableUIHUD : NetworkBehaviour
{
    [SerializeField] private GameObject uiHolder;

    private void Start()
    {
        if (!IsOwner)
            return;

        ToggleHUD(FBPP.GetInt("EnableHud") == 1);

        // subscribe to the event where we change the value of toggle HUD
        HandleSettings.OnHUDToggled += ToggleHUD;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        base.OnNetworkDespawn();

        HandleSettings.OnHUDToggled -= ToggleHUD;
    }

    private void ToggleHUD(bool enable)
    {
        Debug.Log(enable);

        uiHolder.SetActive(enable);
    }
}
