using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class HandleKickPlayerButton : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private SessionHolder sessionHolder;
    private PlayerInfo playerInfo;

    public void InitializeButtonInformation(PlayerInfo info)
    {
        playerInfo = info;
        usernameText.text = info.username.Value.ToString();
    }

    public void KickPlayer()
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { playerInfo.OwnerClientId }
            }
        };

        BanPlayerFromServerClientRpc(clientRpcParams);

        StartCoroutine(DisconnectDelay());
    }

    [ClientRpc]
    private void BanPlayerFromServerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        HandleSettings.instance.BanSession();
    }

    private IEnumerator DisconnectDelay()
    {
        yield return new WaitForSeconds(0.5f);

        NetworkManager.Singleton.DisconnectClient(playerInfo.OwnerClientId);

        // send a message to everyone that user has been kicked
        HandleChatbox.instance.SendTextClientRpc(true, -1, "", "", "", $"{playerInfo.username.Value} has been kicked", true);
    }
}
