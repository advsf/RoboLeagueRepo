using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Multiplayer;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SessionItemData : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI sessionPlayersCount;
    [SerializeField] private TextMeshProUGUI sessionRegionAndUsernameText;
    [SerializeField] private Image rankImage;

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
    }

    public void SetSessionNameText(string sessionName) => sessionNameText.text = sessionName;

    public void SetSessionPlayerCountText(int currentPlayers, int maxPlayers) => sessionPlayersCount.text = $"{currentPlayers}/{maxPlayers}";

    public void SetSessionRegionText() => sessionRegionAndUsernameText.text = $"{sessionInfo.Properties["Region"].Value} - {sessionInfo.Properties["HostUsername"].Value}";

    public void SetSessionHostRankImage()
    {
        rankImage.sprite = HandlePlayerData.instance.GetRankSprite(int.Parse(sessionInfo.Properties["HostRankIndex"].Value));
        Debug.Log(int.Parse(sessionInfo.Properties["HostRankIndex"].Value));
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
