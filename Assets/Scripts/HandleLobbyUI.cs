using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandleLobbyUI : MonoBehaviour
{
    public static HandleLobbyUI instance;

    [Header("References")]
    [SerializeField] private GameObject mainLobbyUI; // brings the user to the first (main) screen
    [SerializeField] private GameObject playLobbyUI; // brings the user to the play screen
    [SerializeField] private GameObject settingUI;
    [SerializeField] private GameObject inventoryUI;

    [Header("Create Session UI References")]
    [SerializeField] private GameObject createSessionObj;
    [SerializeField] private GameObject sessionListObj;

    [Header("Joining Server UI Reference")]
    [SerializeField] private GameObject joiningServerUI;
    [SerializeField] private Button joiningServerExitButton; 

    [Header("Practice Session References")]
    [SerializeField] private GameObject practiceSessionCreationUI;

    [Header("Player Stats UI")]
    [SerializeField] private GameObject playerStatsUI;

    [Header("Change Username References")]
    [SerializeField] private GameObject changeUsernameUI;

    [Header("Change Username Error References")]
    [SerializeField] private TextMeshProUGUI usernameErrorText;

    [Header("Rank List UI")]
    [SerializeField] private GameObject rankListUI;

    [Header("Update Logs UI")]
    [SerializeField] private GameObject updateLogObj;

    [Header("Credits UI")]
    [SerializeField] private GameObject creditsObj;

    [Header("Controls UI")]
    [SerializeField] private GameObject controlsUI;

    [Header("Camera Animation")]
    [SerializeField] private LobbyCameraAnimation lobbyCamAnimation;

    private void Awake()
    {
        // for singleton
        instance = this;
    }

    private void Start()
    {
        SetDefaultLobbyUI();
    }

    private void OnDisable()
    {
        SetDefaultLobbyUI();
    }

    public void SetDefaultLobbyUI()
    {
        mainLobbyUI.SetActive(true);
        playLobbyUI.SetActive(false);

        createSessionObj.SetActive(false);
        sessionListObj.SetActive(false);

        joiningServerUI.SetActive(false);
        practiceSessionCreationUI.SetActive(false);
        changeUsernameUI.SetActive(false);

        rankListUI.SetActive(false);

        updateLogObj.SetActive(false);
        creditsObj.SetActive(false);
        controlsUI.SetActive(false);

        playerStatsUI.SetActive(true);

        inventoryUI.SetActive(false);
    }


    #region Main Lobby UIs
    public void GoToMainLobbyUI()
    {
        mainLobbyUI.SetActive(true);

        playLobbyUI.SetActive(false);
        settingUI.SetActive(false);
    }

    public void GoToPlayLobbyUI()
    {
        playLobbyUI.SetActive(true);

        mainLobbyUI.SetActive(false);
        settingUI.SetActive(false);
    }

    public void GoToSettingUI()
    {
        HandleSettings.instance.OpenSettingsUI();
    }

    #endregion

    #region Create Session UI
    public void OpenCreateSessionUI()
    {
        createSessionObj.SetActive(true);
        sessionListObj.SetActive(false);
    }

    public void CloseCreateSessionUI()
    {
        createSessionObj.SetActive(false);
        sessionListObj.SetActive(false);
    }

    #endregion

    #region Open Session List UI
    public void OpenSessionListUI()
    {
        sessionListObj.SetActive(true);
        createSessionObj.SetActive(false);

        HandleLobby.instance.RefreshSessionList();
    }

    public void CloseSessionListUI()
    {
        sessionListObj.SetActive(false);
        createSessionObj.SetActive(false);
    }

    #endregion

    #region Joining Server UI

    public void OpenJoiningServerUI()
    {
        joiningServerUI.SetActive(true);
        practiceSessionCreationUI.SetActive(false);
        createSessionObj.SetActive(false);
    }

    public void CloseJoiningServerUI()
    {
        joiningServerUI.SetActive(false);
        practiceSessionCreationUI.SetActive(false);
    }

    public void SetJoiningServerExitButtonActiveness(bool condition)
    {
        joiningServerExitButton.interactable = condition;
    }

    #endregion

    #region Inventory UI

    public void OpenInventoryUI()
    {
        // play the camera animation
        lobbyCamAnimation.PlayCamAnimationToInventoryUI();
        playerStatsUI.SetActive(false);
        mainLobbyUI.SetActive(false);

        inventoryUI.SetActive(true);
    }

    public void CloseInventoryUI()
    {
        lobbyCamAnimation.PlayCamAnimationFromInventoryToMainLobbyUI();
        playerStatsUI.SetActive(true);
        mainLobbyUI.SetActive(true);

        inventoryUI.SetActive(false);
    }

    #endregion

    #region Practice Session UI

    public void OpenPracticeSessionUI()
    {
        practiceSessionCreationUI.SetActive(true);
    }

    public void ClosePracticeSessionUI()
    {
        practiceSessionCreationUI.SetActive(false);
    }

    #endregion

    #region Change Username UI

    public void OpenChangeUsernameUI()
    {
        changeUsernameUI.SetActive(true);
        playerStatsUI.SetActive(false);

        HandlePlayerStatsUI.instance.SetUsernameInputFieldToCurrentUsername();
    }

    public void CloseChangeUsernameUI()
    {
        changeUsernameUI.SetActive(false);
        playerStatsUI.SetActive(true);

        usernameErrorText.text = "";
    }

    public void SetUsernameErrorTextUI(string text)
    {
        usernameErrorText.text = text;
    }

    #endregion

    #region Rank List UI

    public void OpenRankListUI()
    {
        rankListUI.SetActive(true);
        playerStatsUI.SetActive(false);
    }

    public void CloseRankListUI()
    {
        rankListUI.SetActive(false);
        playerStatsUI.SetActive(true);
    }

    #endregion

    #region Update Log UI

    public void OpenUpdateLogUI()
    {
        updateLogObj.SetActive(true);
        playerStatsUI.SetActive(false);
    }

    public void CloseUpdateLogUI()
    {
        updateLogObj.SetActive(false);
        playerStatsUI.SetActive(true);
    }

    #endregion

    #region Credits
    public void OpenCreditsUI()
    {
        creditsObj.SetActive(true);
        playerStatsUI.SetActive(false);
    }

    public void CloseCreditsUI()
    {
        creditsObj.SetActive(false);
        playerStatsUI.SetActive(true);
    }

    #endregion

    #region Controls

    public void OpenControlsUI()
    {
        controlsUI.SetActive(true);
        playerStatsUI.SetActive(false);
    }

    public void CloseControlsUI()
    {
        controlsUI.SetActive(false);
        playerStatsUI.SetActive(true);
    }

    #endregion

    public void QuitGame() => Application.Quit();
}
