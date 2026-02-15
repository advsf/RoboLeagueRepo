using Unity.Netcode;
using UnityEngine;

public class HandleMobileQuickChat : NetworkBehaviour
{
    [SerializeField] private HandleChatbox chatMessageScript;

    public void SendGG()
    {
        SendMessageOnChat("GG!", true);
    }

    public void SendWellPlayed()
    {
        SendMessageOnChat("Well played!", true);
    }

    public void SendWow()
    {
        SendMessageOnChat("Wow!", true);
    }

    public void SendPass()
    {
        SendMessageOnChat("Pass!", false);
    }

    public void SendShoot()
    {
        SendMessageOnChat("Shoot!", false);
    }

    public void SendDefend()
    {
        SendMessageOnChat("Defend!", false);
    }

    private void SendMessageOnChat(string text, bool isChattingGlobally)
    {
        // disable the mobile quickchat UI
        if (PlayerInfo.instance.playingObj.activeInHierarchy)
            HandleMobileUI.instance.CloseQuickChatUI();

        else
            HandleSpectatingMobileUI.instance.CloseQuickChatUI();

        if (!chatMessageScript.canText)
            return;

        // server logic
        if (IsServer)
        {
            // if sending to everyone
            if (isChattingGlobally)
            {
                chatMessageScript.SendTextClientRpc(isChattingGlobally, PlayerInfo.instance.spectatingObj.activeInHierarchy, PlayerInfo.instance.rankIndex.Value, PlayerInfo.instance.currentTeam.Value.ToString(), PlayerInfo.instance.username.Value.ToString(), PlayerInfo.instance.currentPosition.Value.ToString(), text, NetworkManager.Singleton.LocalClientId);
            }

            // if sending only to team
            else
            {
                var targetClientIds = PlayerInfo.instance.currentTeam.Value.Equals("Blue") ? ServerManager.instance.blueTeamPlayerIds.ToArray() : ServerManager.instance.redTeamPlayerIds.ToArray();

                var rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = targetClientIds
                    }
                };


                chatMessageScript.SendTextClientRpc(isChattingGlobally, PlayerInfo.instance.spectatingObj.activeInHierarchy, PlayerInfo.instance.rankIndex.Value, PlayerInfo.instance.currentTeam.Value.ToString(), PlayerInfo.instance.username.Value.ToString(), PlayerInfo.instance.currentPosition.Value.ToString(), text, NetworkManager.Singleton.LocalClientId, false, rpcParams);
            }
        }

        // client logic
        else
            chatMessageScript.SendTextServerRpc(isChattingGlobally, PlayerInfo.instance.spectatingObj.activeInHierarchy, PlayerInfo.instance.rankIndex.Value, PlayerInfo.instance.currentTeam.Value.ToString(), HandlePlayerData.instance.GetUsername(), PlayerInfo.instance.currentPosition.Value.ToString(), text);

        // spam detection
        chatMessageScript.HandleTrackingAmountOfTextSent();
    }
}
