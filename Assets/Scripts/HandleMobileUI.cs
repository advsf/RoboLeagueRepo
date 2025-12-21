using UnityEngine;
using Unity.Netcode;

public class HandleMobileUI : NetworkBehaviour
{
    public static HandleMobileUI instance;

    [SerializeField] private GameObject touchPadObj;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
            Destroy(gameObject);

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
        touchPadObj.SetActive(condition);
    }

    #region UI Button Functions

    public void GoBackToMenuViaUI()
    {
        ServerManager.instance.EnableSpawnSelectionCanvaObj(true);
    }

    #endregion
}
