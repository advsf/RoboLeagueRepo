using Unity.Netcode;
using UnityEngine;

public class HandleSpectatingMobileUI : NetworkBehaviour
{
    public static HandleSpectatingMobileUI instance;

    [Header("Main References")]
    [SerializeField] private GameObject touchPad;
    [SerializeField] private HandleLeaderboardUI leaderboardUI;

    [Header("Quick Chat UI References")]
    [SerializeField] private GameObject quickChatObj;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        instance = this;

        quickChatObj.SetActive(false);
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
        if (leaderboardUI.IsLeaderboardActive())
            leaderboardUI.DisableLeaderboard();

        if (quickChatObj.activeInHierarchy)
            quickChatObj.SetActive(false);
    }

    public void CloseAllHelperButtonsUIExceptLeaderboard()
    {
        if (quickChatObj.activeInHierarchy)
            quickChatObj.SetActive(false);
    }

    public void CloseAllHelperButtonsUIExceptChat()
    {
        if (leaderboardUI.IsLeaderboardActive())
            leaderboardUI.DisableLeaderboard();
    }

    public void CloseQuickChatUI()
    {
        quickChatObj.SetActive(false);
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

    public void HandleEnablingQuickChat()
    {
        // if chat is disabled
        if (FBPP.GetInt("EnableChat") == 0)
            return;

        quickChatObj.SetActive(!quickChatObj.activeInHierarchy);
    }
}