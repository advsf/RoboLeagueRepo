using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Multiplayer;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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

    public UnityEvent<ISessionInfo> OnSessionSelected;
    public UnityEvent OnSessionDeselected;

    private ISessionInfo sessionInfo;

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
            serverCustomizationStatusText.text = "Normal";
            serverCustomizationButtonImage.color = greenColor;
        }

        // if modified
        else
        {
            serverCustomizationStatusText.text = "Modified";
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
