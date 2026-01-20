using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Settings;

public class HandleServerCusmizationInfo : MonoBehaviour
{
    public static HandleServerCusmizationInfo instance;

    [Header("References")]
    [SerializeField] private GameObject serverCustomizationInfoUI;
    [SerializeField] private TextMeshProUGUI eachHalfDurationText;
    [SerializeField] private TextMeshProUGUI halftimeDurationText;
    [SerializeField] private TextMeshProUGUI ballKickMultiplierText;
    [SerializeField] private TextMeshProUGUI ballCurveMultiplierText;
    [SerializeField] private TextMeshProUGUI playerSpeedMultiplierText;
    [SerializeField] private TextMeshProUGUI abilityEnabledText;
    [SerializeField] private TextMeshProUGUI doAbilityCDText;

    [Header("Localization References")]
    [SerializeField] private LocalizedString eachHalfDurationLoc = new("Table1", "EACH HALF DURATION");
    [SerializeField] private LocalizedString halftimeDurationLoc = new("Table1", "HALFTIME DURATION");
    [SerializeField] private LocalizedString ballKickMultiplierLoc = new("Table1", "BALL KICK MULTIPLIER");
    [SerializeField] private LocalizedString ballCurveMultiplierLoc = new("Table1", "BALL CURVE MULTIPLIER");
    [SerializeField] private LocalizedString playerSpeedMultiplierLoc = new("Table1", "PLAYER SPEED MULTIPLIER");
    [SerializeField] private LocalizedString isAbilityEnabledLoc = new("Table1", "IS ABILITY ENABLED");
    [SerializeField] private LocalizedString doAbilityCooldown = new("Table1", "DO ABILITY COOLDOWN");


    private ISessionInfo sessionInfo;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void Start()
    {
        instance = this;

        serverCustomizationInfoUI.SetActive(false);
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (sessionInfo == null)
            return;

        TurnOnServerCustomizationInfoUI(sessionInfo);
    }

    public void TurnOnServerCustomizationInfoUI(ISessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;

        serverCustomizationInfoUI.SetActive(true);

        eachHalfDurationLoc["duration"] = new StringVariable { Value = sessionInfo.Properties["EachHalfDuration"].Value + "s" };
        eachHalfDurationText.text = eachHalfDurationLoc.GetLocalizedString();

        halftimeDurationLoc["duration"] = new StringVariable { Value = sessionInfo.Properties["HalftimeDuration"].Value + "s" };
        halftimeDurationText.text = eachHalfDurationLoc.GetLocalizedString();

        ballKickMultiplierLoc["multiplier"] = new StringVariable { Value = sessionInfo.Properties["KickMultiplier"].Value + "x" };
        ballKickMultiplierText.text = ballKickMultiplierLoc.GetLocalizedString();

        ballCurveMultiplierLoc["multiplier"] = new StringVariable { Value = sessionInfo.Properties["CurveMultiplier"].Value + "x" };
        ballCurveMultiplierText.text = ballCurveMultiplierLoc.GetLocalizedString();

        playerSpeedMultiplierLoc["multiplier"] = new StringVariable { Value = sessionInfo.Properties["PlayerSpeedMultiplier"].Value + "x" };
        playerSpeedMultiplierText.text = playerSpeedMultiplierLoc.GetLocalizedString();

        isAbilityEnabledLoc["enabled"] = new StringVariable { Value = sessionInfo.Properties["IsAbilityEnabled"].Value };
        abilityEnabledText.text = isAbilityEnabledLoc.GetLocalizedString();

        doAbilityCooldown["enabled"] = new StringVariable { Value = sessionInfo.Properties["DoAbilityCD"].Value };
        doAbilityCDText.text = doAbilityCooldown.GetLocalizedString();
    }

    public void CloseServerCustomizationInfoUI()
    {
        sessionInfo = null;

        serverCustomizationInfoUI.SetActive(false);
    }
}
