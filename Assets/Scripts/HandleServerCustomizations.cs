using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HandleServerCustomizations : MonoBehaviour
{
    public static HandleServerCustomizations instance;

    [Header("Current Settings")]
    public int serverEachHalfDuration = 300; // in seconds
    public int serverHalftimeDuration = 10;
    public float serverBallKickMultiplier = 1;
    public float serverBallCurveMultiplier = 1;
    public float serverPlayerSpeedMultiplier = 1;
    public string serverAbilityEnabled = "True";
    public string serverDoAbilityHaveCooldown = "True";

    public int serverMapHash; // add different maps later

    [Header("UI References")]
    [SerializeField] private GameObject canvaObj;
    [SerializeField] private TMP_InputField eachHalfDurationInputField;
    [SerializeField] private TMP_InputField halftimeDurationInputField;
    [SerializeField] private TMP_InputField ballKickInputField;
    [SerializeField] private TMP_InputField curveInputField;
    [SerializeField] private TMP_InputField speedMultiplierInputField;
    [SerializeField] private Toggle isAbilityEnabledToggle;
    [SerializeField] private Toggle doAbilityHaveCooldownToggle;

    public bool areServerSettingsChanged = false; // determines if we give xp in the server or not

    private void Start()
    {
        if (instance == null)
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
            return;
        }

        ResetToDefaultCustomizations();
        DisableServerCustoimzationUI();
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (!arg0.name.Equals("Lobby"))
            return;

        EnableServerCustomizationUI();
        ResetToDefaultCustomizations();
        DisableServerCustoimzationUI();
    }

    public void ResetToDefaultCustomizations()
    {
        serverEachHalfDuration = 300;
        serverHalftimeDuration = 10;
        serverBallKickMultiplier = 1;
        serverBallCurveMultiplier = 1;
        serverPlayerSpeedMultiplier = 1;

        eachHalfDurationInputField.text = serverEachHalfDuration.ToString() + "s";
        halftimeDurationInputField.text = serverHalftimeDuration.ToString() + "s";
        ballKickInputField.text = serverBallKickMultiplier.ToString() + "x";
        curveInputField.text = serverBallCurveMultiplier.ToString() + "x";
        speedMultiplierInputField.text = serverPlayerSpeedMultiplier.ToString() + "x";
        isAbilityEnabledToggle.isOn = true;
        doAbilityHaveCooldownToggle.isOn = true;

        areServerSettingsChanged = false;
    }

    public void EnableServerCustomizationUI()
    {
        canvaObj.SetActive(true);
    }

    public void DisableServerCustoimzationUI()
    {
        canvaObj.SetActive(false);
    }

    public void ChangeServerSettings()
    {
        // each half duration
        if (int.TryParse(eachHalfDurationInputField.text, out int eachHalfDuration))
        {
            // if it's less than 60 seconds
            if (eachHalfDuration < 60)
                eachHalfDuration = 60;

            eachHalfDurationInputField.text = eachHalfDuration.ToString() + "s";

            serverEachHalfDuration = eachHalfDuration;
            areServerSettingsChanged = true;
        }

        else
        {
            eachHalfDurationInputField.text = serverEachHalfDuration.ToString() + "s";
        }

        // halftime duration
        if (int.TryParse(halftimeDurationInputField.text, out int halfTimeDuration))
        {
            // if it's less than 5 seconds
            if (halfTimeDuration < 5)
                halfTimeDuration = 5;

            halftimeDurationInputField.text = halfTimeDuration.ToString() + "s";

            serverHalftimeDuration = halfTimeDuration;
            areServerSettingsChanged = true;
        }

        else
        {
            halftimeDurationInputField.text = serverHalftimeDuration.ToString() + "s";
        }

        // kick multiplier
        if (float.TryParse(ballKickInputField.text, out float kickMultiplier))
        {
            // if it's less than 0.1x
            if (kickMultiplier < 0.1)
                kickMultiplier = 0.1f;

            else if (kickMultiplier > 2.5)
                kickMultiplier = 2.5f;

            ballKickInputField.text = kickMultiplier.ToString() + "x";

            serverBallKickMultiplier = kickMultiplier;
            areServerSettingsChanged = true;
        }

        // curve multiplier
        if (float.TryParse(curveInputField.text, out float curveMultiplier))
        {
            // if it's less than 0.1x or greater than 2
            if (curveMultiplier < 0.1)
                curveMultiplier = 0.1f;
            
            else if (curveMultiplier > 2)
                curveMultiplier = 2;

            curveInputField.text = curveMultiplier.ToString() + "x";

            serverBallCurveMultiplier = curveMultiplier;
            areServerSettingsChanged = true;
        }

        // speed multiplier
        if (float.TryParse(speedMultiplierInputField.text, out float speedMultiplier))
        {
            // if it's less than 0.1x or greater than 3
            if (speedMultiplier < 0.1)
                speedMultiplier = 0.1f;

            else if (speedMultiplier > 3)
                speedMultiplier = 3f;

            speedMultiplierInputField.text = speedMultiplier.ToString() + "x";

            serverPlayerSpeedMultiplier = speedMultiplier;
            areServerSettingsChanged = true;
        }

        // is ability enabled
        if (!isAbilityEnabledToggle.isOn)
        {
            serverAbilityEnabled = "False";
            areServerSettingsChanged = true;
        }

        // do ability have cooldown
        if (!doAbilityHaveCooldownToggle.isOn)
        {
            serverDoAbilityHaveCooldown = "False";
            areServerSettingsChanged = true;
        }

        if (areServerSettingsChanged)
            HandleLobbyUI.instance.UpdateServerCustomizationButtonColor();
    }
}
