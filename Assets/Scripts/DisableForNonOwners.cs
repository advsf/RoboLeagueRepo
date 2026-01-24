using UnityEngine;
using Unity.Netcode;

public class DisableForNonOwners : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
            gameObject.SetActive(false);
    }
}
