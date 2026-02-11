using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class HandleSpectatingMobileUI : NetworkBehaviour
{
    public static HandleSpectatingMobileUI instance;

    [Header("Main References")]
    [SerializeField] private GameObject touchPad;
    [SerializeField] private HandleLeaderboardUI leaderboardUI;

    [Header("Chat UI References")]
    [SerializeField] private TMP_InputField chatInputField;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
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

    public void CloseAllHelperButtonsUI()
    {
        leaderboardUI.DisableLeaderboard();
        HandleEnablingChatInputFieldUI(); // if chat input field is open then this will close it
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

    public void HandleEnablingChatInputFieldUI()
    {
        // if active, disable
        if (chatInputField.gameObject.activeInHierarchy)
        {
            chatInputField.ActivateInputField();
            chatInputField.gameObject.SetActive(false);

            EnableTouchPadObj(true);
        }

        else
        {
            chatInputField.text = "";
            chatInputField.gameObject.SetActive(true);
            chatInputField.ActivateInputField();

            EnableTouchPadObj(false);
        }
    }
}