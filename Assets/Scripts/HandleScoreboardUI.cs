using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class HandleScoreboardUI : NetworkBehaviour
{
    public static HandleScoreboardUI instance;

    [Header("References")]
    [SerializeField] private GameObject scoreboardUI;
    [SerializeField] private GameObject scoreInformationUI;
    [SerializeField] private TextMeshProUGUI startGameHelpText;
    [SerializeField] private TextMeshProUGUI blueTeamGoalAmount;
    [SerializeField] private TextMeshProUGUI redTeamGoalAmount;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private TextMeshProUGUI scoreInformationText;
    [SerializeField] private GameObject mobileStartButton;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        HandleMobileStartingGameText();
    }

    private void Start()
    {
        instance = this;

        blueTeamGoalAmount.text = ServerManager.instance.blueTeamGoalCount.Value.ToString();
        redTeamGoalAmount.text = ServerManager.instance.redTeamGoalCount.Value.ToString();

        timer.text = "00:00";

        ServerManager.instance.blueTeamGoalCount.OnValueChanged += ChangeBlueTeamGoalCount;
        ServerManager.instance.redTeamGoalCount.OnValueChanged += ChangeRedTeamGoalCount;
        ServerManager.instance.matchTime.OnValueChanged += UpdateTimer;

        // dont show the clients the helper text "Press T To Start Game"
        if (!IsServer)
            EnableStartGameHelperTextUI(false);

        EnableMobileStartGameBututon(false);

        HandleMobileStartingGameText();
    }

    private void HandleMobileStartingGameText()
    {
        if (!IsServer)
            return;

        var localizedString = new LocalizedString("Table1", !Application.isMobilePlatform ? "PRESS T TO START (MUST BE >1 PLAYERS)" : "PRESS THE BUTTON BELOW TO START (MUST BE >1 PLAYERS)");
        string translatedMessage = localizedString.GetLocalizedString();

        ChangeStartGameHelperTextUI(translatedMessage);
    }

    public override void OnNetworkDespawn()
    {
        instance = null;

        ServerManager.instance.blueTeamGoalCount.OnValueChanged -= ChangeBlueTeamGoalCount;
        ServerManager.instance.redTeamGoalCount.OnValueChanged -= ChangeRedTeamGoalCount;
        ServerManager.instance.matchTime.OnValueChanged -= UpdateTimer;

        base.OnNetworkDespawn();
    }

    public void ChangeScoreboardInformationText(string text) => scoreInformationText.text = text;

    public void EnableScoreboardInformationUI(bool condition) => scoreInformationUI.SetActive(condition);

    public void EnableTimerUI(bool condition) => scoreboardUI.SetActive(condition);

    public void EnableStartGameHelperTextUI(bool condition) => startGameHelpText.enabled = condition;

    public void EnableMobileStartGameBututon(bool condition) => mobileStartButton.SetActive(condition);

    public void ChangeStartGameHelperTextUI(string text) => startGameHelpText.text = text;

    private void UpdateTimer(float previous, float current)
    {
        int minutes = Mathf.FloorToInt(current / 60); 
        int seconds = Mathf.FloorToInt(current % 60);

        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void ChangeBlueTeamGoalCount(int previous, int current)
    {
        blueTeamGoalAmount.text = current.ToString();
    }

    private void ChangeRedTeamGoalCount(int previous, int current)
    {
        redTeamGoalAmount.text = current.ToString();
    }
}
