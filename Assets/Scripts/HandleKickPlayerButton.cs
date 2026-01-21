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
        StartCoroutine(DisconnectDelay());
    }

    private IEnumerator DisconnectDelay()
    {
        yield return new WaitForSeconds(0.5f);

        NetworkManager.Singleton.DisconnectClient(playerInfo.OwnerClientId);

        // send a message to everyone that user has been kicked
        HandleChatbox.instance.SendLocalizedTextClientRpc(
                username: playerInfo.username.Value.ToString(),
                team: "",
                position: "",
                goals: 0, // just a placeholder value
                assists: 0,
                saves: 0,
                xp: 0,
                localizationKey: "USER_KICKED_MESSAGE");
    }
}
