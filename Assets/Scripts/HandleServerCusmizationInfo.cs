using UnityEngine;
using Unity.Services.Multiplayer;
using TMPro;

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

    private ISessionInfo sessionInfo;

    private void Start()
    {
        instance = this;

        serverCustomizationInfoUI.SetActive(false);
    }

    public void TurnOnServerCustomizationInfoUI(ISessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;

        serverCustomizationInfoUI.SetActive(true);

        eachHalfDurationText.text = "Each Half Duration: " + sessionInfo.Properties["EachHalfDuration"].Value + "s";
        halftimeDurationText.text = "Halftime Duration: " + sessionInfo.Properties["HalftimeDuration"].Value + "s";
        ballKickMultiplierText.text = "Ball Kick Multiplier: " + sessionInfo.Properties["KickMultiplier"].Value + "x";
        ballCurveMultiplierText.text = "Ball Curve Multiplier: " + sessionInfo.Properties["CurveMultiplier"].Value + "x";
        playerSpeedMultiplierText.text = "Player Speed Multiplier: " + sessionInfo.Properties["PlayerSpeedMultiplier"].Value + "x";
        abilityEnabledText.text = "Is Ability Enabled: " + sessionInfo.Properties["IsAbilityEnabled"].Value;
        doAbilityCDText.text = "Do Ability Cooldown: " + sessionInfo.Properties["DoAbilityCD"].Value;
    }

    public void CloseServerCustomizationInfoUI()
    {
        serverCustomizationInfoUI.SetActive(false);
    }
}
