using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class HandlePlayerNameTag : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI playerTag;
    [SerializeField] private PlayerInfo playerInfo;

    public Transform localPlayerCam;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            playerTag.text = playerInfo.username.Value.ToString();

        // disable the nametag for the owner
        else
        {
            gameObject.SetActive(false);
            return;
        }

        playerInfo.username.OnValueChanged += HandleNameChange;

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsOwner && IsSpawned)
        {
            AssignCamera();

            if (localPlayerCam != null)
            {
                transform.rotation = localPlayerCam.rotation;
            }
        }
    }

    private void HandleNameChange(FixedString64Bytes oldName, FixedString64Bytes newName)
    {
        playerTag.text = newName.ToString();
    }

    private void AssignCamera()
    {
        Camera cam = NetworkManager.LocalClient.PlayerObject.GetComponentInChildren<PlayerInfo>().GetCurrentPlayerCamera();

        localPlayerCam = cam.transform;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        localPlayerCam = null; 
    }
}
