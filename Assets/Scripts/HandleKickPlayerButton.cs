using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class HandleKickPlayerButton : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private SessionHolder sessionHolder;
    [SerializeField] private LocalizedString userKickedLoc = new("Table1", "USER_KICKED_MESSAGE");
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
        userKickedLoc["username"] = new StringVariable { Value = playerInfo.username.Value.ToString() };
        HandleChatbox.instance.SendTextClientRpc(true, -1, "", "", "", userKickedLoc.GetLocalizedString(), true);
    }
}
