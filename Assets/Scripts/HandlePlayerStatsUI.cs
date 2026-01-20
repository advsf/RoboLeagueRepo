using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Settings;

public class HandlePlayerStatsUI : MonoBehaviour
{
    public static HandlePlayerStatsUI instance;

    [Header("References")]
    [SerializeField] private Image rankImage;
    [SerializeField] private Image rankImageShadow;
    [SerializeField] private Slider rankBarSlider;
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TextMeshProUGUI rankXPText;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI gameStatsText;

    [Header("Localization References")]
    [SerializeField] private LocalizedString maxRankLoc = new("Table1", "XP_MAX_RANK");
    [SerializeField] private LocalizedString nextRankLoc = new("Table1", "XP_NEXT_RANK");
    [SerializeField] private LocalizedString usernameLoc = new("Table1", "PLAYER_USERNAME");
    [SerializeField] private LocalizedString playerStatsLoc = new("Table1", "PLAYER_STATS");

    [Header("UI Bar Settings")]
    [SerializeField] private float xpBarFillDuration = 1.5f;

    private static readonly Regex usernameRegex = new (@"^[\p{L}\p{N}._-]{1,10}$", RegexOptions.Compiled);

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        StartCoroutine(InitializeUIAndAnimate());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        UpdateStatsUI();
    }

    private IEnumerator InitializeUIAndAnimate()
    {
        yield return null;

        usernameInputField.text = HandlePlayerData.instance.GetUsername();
        UpdateStatsUI(); 

        rankBarSlider.value = 0;

        yield return new WaitForSeconds(3);

        StartCoroutine(FillUpXPBar());
    }

    #region Username

    public void SetUsername(string newUsername)
    {
        if (!IsUsernameValid())
            return;

        FBPP.SetString("Username", newUsername);
        FBPP.Save();

        UpdateStatsUI();

        usernameInputField.text = newUsername;
    }

    public void SetUsernameThroughUI()
    {
        if (!IsUsernameValid())
            return;

        FBPP.SetString("Username", usernameInputField.text);
        FBPP.Save();

        UpdateStatsUI();

        usernameInputField.text = usernameInputField.text;

        HandleLobbyUI.instance.CloseChangeUsernameUI();
    }

    private bool IsUsernameValid()
    {
        if (Application.isEditor)
            return true;

        if (usernameInputField.text.Length < 1)
        {
            HandleLobbyUI.instance.SetUsernameErrorTextUI("USERNAME CANNOT BE EMPTY!");
            return false;
        }

        if (usernameInputField.text.Length > 9)
        {
            HandleLobbyUI.instance.SetUsernameErrorTextUI("USERNAME CANNOT BE THIS LONG!");
            return false;
        }

        if (!usernameRegex.IsMatch(usernameInputField.text))
        {
            HandleLobbyUI.instance.SetUsernameErrorTextUI("USERNAME CANNOT CONTAIN INVALID CHARACTERS!");
            return false;
        }

        return true;
    }

    public void SetUsernameInputFieldToCurrentUsername()
    {
        usernameInputField.text = FBPP.GetString("Username");
    }

    #endregion

    #region Player Stats UI
    public void UpdateStatsUI()
    {
        float currentXP = FBPP.GetFloat("RankXP");

        // rank image
        rankImage.enabled = true;
        rankImage.sprite = HandlePlayerData.instance.GetRankSprite();

        // rank shadow image
        rankImageShadow.enabled = true;
        rankImageShadow.sprite = HandlePlayerData.instance.GetRankSprite();

        // rank information
        rankBarSlider.value = FBPP.GetFloat("RankXP") / 100;

        if (HandlePlayerData.instance.isPlayerMaxRank)
        {
            maxRankLoc["xp"] = new FloatVariable { Value = currentXP }; ; 
            rankXPText.text = maxRankLoc.GetLocalizedString();
        }

        else
        {
            string nextRank = HandlePlayerData.instance.GetRankSprite(FBPP.GetInt("RankIndex") + 1).name;
            nextRankLoc["xp"] = new FloatVariable { Value = currentXP }; ; 
            nextRankLoc["rankName"] = new StringVariable { Value = nextRank };
            rankXPText.text = nextRankLoc.GetLocalizedString();
        }

        // username & game stat
        usernameLoc["username"] = new StringVariable { Value = HandlePlayerData.instance.GetUsername() };
        usernameText.text = usernameLoc.GetLocalizedString();

        playerStatsLoc["goals"] = new IntVariable { Value = HandlePlayerData.instance.GetGoalsCount() };
        playerStatsLoc["assists"] = new IntVariable { Value = HandlePlayerData.instance.GetAssistsCount() };
        playerStatsLoc["saves"] = new IntVariable { Value = HandlePlayerData.instance.GetSavesCount() };
        gameStatsText.text = playerStatsLoc.GetLocalizedString();
    }

    private IEnumerator FillUpXPBar()
    {
        rankBarSlider.value = 0; 
        float initialVal = 0; 
        float targetVal = FBPP.GetFloat("RankXP") / 100f;
        float time = 0f;

        while (time < xpBarFillDuration)
        {
            time += Time.deltaTime;
            rankBarSlider.value = Mathf.Lerp(initialVal, targetVal, time / xpBarFillDuration);

            yield return null;
        }

        rankBarSlider.value = targetVal;
    }

    #endregion
}
