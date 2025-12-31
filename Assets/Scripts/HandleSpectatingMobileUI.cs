using UnityEngine;
using Unity.Netcode;

public class HandleSpectatingMobileUI : NetworkBehaviour
{
    public static HandleSpectatingMobileUI instance;

    [SerializeField] private GameObject touchPad;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner || !Application.isMobilePlatform)
        {
            gameObject.SetActive(false);
            return;
        }

        instance = this;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner)
            return;

        instance = null;
    }

    public void EnableTouchPadObj(bool condition)
    {
        touchPad.SetActive(condition);
    }

    public void HandleEnablingMenuUI()
    {
        // if active, then just disable the spawn UI and enable touchpad
        if (ServerManager.instance.IsSpawnSelectionCanvaObjActive())
        {
            ServerManager.instance.EnableSpawnSelectionCanvaObj(false);
            EnableTouchPadObj(true);
        }

        else
        {
            ServerManager.instance.EnableSpawnSelectionCanvaObj(true);
            EnableTouchPadObj(false);
        }
    }
}
