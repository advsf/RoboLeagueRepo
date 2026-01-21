using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Multiplayer;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class SessionItemData : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI sessionPlayersCount;
    [SerializeField] private TextMeshProUGUI sessionRegionAndUsernameText;
    [SerializeField] private TextMeshProUGUI serverCustomizationStatusText;
    [SerializeField] private Image serverCustomizationButtonImage;
    [SerializeField] private Image rankImage;

    [Header("Color Settings")]
    [SerializeField] private Color greenColor;
    [SerializeField] private Color purpleColor;

    [Header("Localization References")]
    [SerializeField] private LocalizedString normalTextLoc = new("Table1", "NORMAL");
    [SerializeField] private LocalizedString modifiedTextLoc = new("Table1", "MODIFIED");

    public UnityEvent<ISessionInfo> OnSessionSelected;
    public UnityEvent OnSessionDeselected;

    private ISessionInfo sessionInfo;

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
        SetServerCustomizationStatus();
    }


    public void SetSession(ISessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;
        SetSessionNameText(sessionInfo.Name);
        SetSessionPlayerCountText(sessionInfo.MaxPlayers - sessionInfo.AvailableSlots, sessionInfo.MaxPlayers);
        SetSessionRegionText();
        SetSessionHostRankImage();
        SetServerCustomizationStatus();
    }

    public void SetSessionNameText(string sessionName) => sessionNameText.text = sessionName;

    public void SetSessionPlayerCountText(int currentPlayers, int maxPlayers) => sessionPlayersCount.text = $"{currentPlayers} / {maxPlayers}";

    public void SetSessionRegionText() => sessionRegionAndUsernameText.text = $"{sessionInfo.Properties["Region"].Value} - {sessionInfo.Properties["HostUsername"].Value}";

    public void SetSessionHostRankImage()
    {
        rankImage.sprite = HandlePlayerData.instance.GetRankSprite(int.Parse(sessionInfo.Properties["HostRankIndex"].Value));
        Debug.Log(int.Parse(sessionInfo.Properties["HostRankIndex"].Value));
    }

    public void SetServerCustomizationStatus()
    {
        // if not modified
        if (sessionInfo.Properties["IsServerModified"].Value.Equals("F"))
        {
            serverCustomizationStatusText.text = normalTextLoc.GetLocalizedString();
            serverCustomizationButtonImage.color = greenColor;
        }

        // if modified
        else
        {
            serverCustomizationStatusText.text = modifiedTextLoc.GetLocalizedString();
            serverCustomizationButtonImage.color = purpleColor;
        }
    }

    public void OpenServerCustomizationInfo()
    {
        HandleServerCusmizationInfo.instance.TurnOnServerCustomizationInfoUI(sessionInfo);
    }

    public void OnSelect(BaseEventData eventData)
    {
        OnSessionSelected?.Invoke(sessionInfo);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        OnSessionDeselected?.Invoke();
    }
}
