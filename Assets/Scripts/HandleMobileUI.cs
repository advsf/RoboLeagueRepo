using UnityEngine;
using Unity.Netcode;

public class HandleMobileUI : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
            Destroy(gameObject);
    }
}
