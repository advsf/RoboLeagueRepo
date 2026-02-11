using UnityEngine;
using Unity.Netcode;
using TMPro;

public class HandleMobileUI : NetworkBehaviour
{
    public static HandleMobileUI instance;

    [Header("References")]
    [SerializeField] private GameObject touchPadObj;
    [SerializeField] private HandleLeaderboardUI leaderboardUI;

    [Header("Ability Buttons References")]
    [SerializeField] private GameObject[] abilityButtons;
    [SerializeField] private GameObject[] gkAbilityButtons;

    [Header("Emote UI References")]
    [SerializeField] private GameObject emoteUI;

    [Header("Chat UI References")]
    [SerializeField] private TMP_InputField chatInputField;

    [Header("Spawn Ball References")]
    [SerializeField] private GameObject spawnBallSettingsObj;
    [SerializeField] private GameObject[] spawnBallButtons; // 0 - reset ball, 1 - spawn low ball, 2 - spawn high ball, 3 - spawn dummy, 4 - delete dummy, 5 - enable/disable goalkeeper

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
            return;

        instance = this;

        foreach (GameObject button in abilityButtons)
            button.SetActive(false);

        foreach (GameObject button in spawnBallButtons)
            button.SetActive(false);

        emoteUI.SetActive(false);
        chatInputField.gameObject.SetActive(false);
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

    public void HandleEmoteUI()
    {
        emoteUI.SetActive(!emoteUI.activeInHierarchy);
    }

    #region Ability and GK Buttons

    public void HandleAbilityButtons(bool isGK)
    {
        if (isGK)
        {
            foreach (GameObject button in abilityButtons)
                button.SetActive(false);

            foreach (GameObject button in gkAbilityButtons)
                button.SetActive(true);
        }

        else
        {
            foreach (GameObject button in abilityButtons)
                button.SetActive(true);

            foreach (GameObject button in gkAbilityButtons)
                button.SetActive(false);
        }
    }

    #endregion

    #region UI Button Functions

    public void HandleUpdatingAllUICustomization()
    {
        foreach (Transform obj in transform)
        {
            HandleUpdatingCustomMobileUI script = obj.GetComponent<HandleUpdatingCustomMobileUI>();

            if (obj.GetComponent<HandleUpdatingCustomMobileUI>() != null)
                script.UpdateUI();

        }
    }

    private void EnableAllMobileUI(bool condition)
    {
        foreach (Transform obj in transform)
            obj.gameObject.SetActive(condition);
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

    public void HandleEnablingSpawnBallSettingsUI()
    {
        if (spawnBallSettingsObj.activeInHierarchy || ServerManager.instance.didStartGame.Value)
        {
            foreach (GameObject button in spawnBallButtons)
                button.SetActive(false);

            spawnBallSettingsObj.SetActive(false);
        }

        else
        {
            // if practice server
            // enable all the buttons
            if (ServerManager.instance.isPracticeServer)
            {
                foreach (GameObject button in spawnBallButtons)
                    button.SetActive(true);
            }

            // only enable spawn ball
            else if (ServerManager.instance.isTutorialServer)
            {
                spawnBallButtons[0].SetActive(true);
            }

            // enable spawn ball, high ball, and low ball
            else
            {
                for (int i = 0; i < 3; i++)
                    spawnBallButtons[i].SetActive(true);
            }

            spawnBallSettingsObj.SetActive(true);
        }
    }

    #endregion

    #region Spawn Ball Functions

    public void SpawnBall()
    {
        if (ServerManager.instance.isTutorialServer)
        {
            HandleTutorialPlayerDetectors.instance.GetCurrentTutorialDetector().RespawnTutorialBall();
        }

        else
        {
            HandleKicking.instance.SpawnBallViaButton();

            // disable the buttons
            HandleEnablingSpawnBallSettingsUI();
        }
    }

    public void SpawnLowBall()
    {
        ServerManager.instance.SpawnLowBallServerRpc(PlayerMovement.instance.transform.position, PlayerMovement.instance.transform.forward);

        // disable the buttons
        HandleEnablingSpawnBallSettingsUI();
    }

    public void SpawnHighBall()
    {
        ServerManager.instance.SpawnHighBallServerRpc(PlayerMovement.instance.transform.position, PlayerMovement.instance.transform.forward);

        // disable the buttons
        HandleEnablingSpawnBallSettingsUI();
    }

    public void SpawnDummy()
    {
        HandleTrainingDummies.instance.CreateDummy();

        // disable the buttons
        HandleEnablingSpawnBallSettingsUI();
    }

    public void DeleteDummy()
    {
        HandleTrainingDummies.instance.DestroyAllDummy(false);

        // disable the buttons
        HandleEnablingSpawnBallSettingsUI();
    }

    public void HandleEnablingAIGK()
    {
        if (!BallManager.instance.mainBallSync.GetRigidbody().isKinematic && ServerManager.instance.CanGoalkeepersBeDisabled())
            ServerManager.instance.EnableDisableAIGK();

        // disable the buttons
        HandleEnablingSpawnBallSettingsUI();
    }

    #endregion

    #region Closing Other UI Functions

    public void CloseAllHelperButtonsUI()
    {
        if (leaderboardUI.IsLeaderboardActive())
            leaderboardUI.DisableLeaderboard();

        if (emoteUI.activeInHierarchy)
            emoteUI.SetActive(false);

        if (spawnBallSettingsObj.activeInHierarchy)
            spawnBallSettingsObj.SetActive(false);

        if (chatInputField.gameObject.activeInHierarchy)
        {
            if (PlayerInfo.instance.spectatingObj.activeInHierarchy)
                HandleChatbox.instance.SetMobileChatToggleInputToAll();

            chatInputField.ActivateInputField();
            chatInputField.gameObject.SetActive(false);
        }
    }

    public void CloseAllHelperButtonsUIExceptLeaderboard()
    {
        if (emoteUI.activeInHierarchy)
            emoteUI.SetActive(false);

        if (spawnBallSettingsObj.activeInHierarchy)
            spawnBallSettingsObj.SetActive(false);

        if (chatInputField.gameObject.activeInHierarchy)
        {
            chatInputField.ActivateInputField();
            chatInputField.gameObject.SetActive(false);
        }
    }

    public void CloseAllHelperButtonsUIExceptEmote()
    {
        if (leaderboardUI.IsLeaderboardActive())
            leaderboardUI.DisableLeaderboard();

        if (spawnBallSettingsObj.activeInHierarchy)
            spawnBallSettingsObj.SetActive(false);

        if (chatInputField.gameObject.activeInHierarchy)
        {
            chatInputField.ActivateInputField();
            chatInputField.gameObject.SetActive(false);
        }
    }

    public void CloseAllHelperButtonsUIExceptChat()
    {
        if (leaderboardUI.IsLeaderboardActive())
            leaderboardUI.DisableLeaderboard();

        if (emoteUI.activeInHierarchy)
            emoteUI.SetActive(false);

        if (spawnBallSettingsObj.activeInHierarchy)
            spawnBallSettingsObj.SetActive(false);
    }

    public void CloseAllHelperButtonsUIExceptSpawnSettings()
    {
        if (leaderboardUI.IsLeaderboardActive())
            leaderboardUI.DisableLeaderboard();

        if (emoteUI.activeInHierarchy)
            emoteUI.SetActive(false);

        if (chatInputField.gameObject.activeInHierarchy)
        {
            chatInputField.ActivateInputField();
            chatInputField.gameObject.SetActive(false);
        }
    }

    #endregion
}
