using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandleChatBoxTextPrefabs : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image rankImage;
    [SerializeField] private Image rankImageShadow;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Rank Image Position Setting")]
    [SerializeField] private float allChatPos;
    [SerializeField] private float teamChatPos;
    [SerializeField] private float yPosIncreaseAmount;

    private float initialRankY;
    private float initialShadowY;
    private bool hasInitialized;

    private bool isChattingAll;

    private void Awake()
    {
        initialRankY = rankImage.rectTransform.anchoredPosition.y;
        initialShadowY = rankImageShadow.rectTransform.anchoredPosition.y;
        hasInitialized = true;
    }

    private void OnEnable()
    {
        if (hasInitialized)
            UpdateRankPosition();
    }

    public void SetChatBoxText(int rankIndex, string chatText, bool isChattingAll)
    {
        this.isChattingAll = isChattingAll;

        text.text = chatText;
        text.ForceMeshUpdate(true);

        if (rankIndex >= 0)
        {
            rankImage.sprite = HandlePlayerData.instance.GetRankSprite(rankIndex);
            rankImageShadow.sprite = HandlePlayerData.instance.GetRankSprite(rankIndex);
        }

        else
        {
            rankImage.enabled = false;
            rankImageShadow.enabled = false;
        }

        UpdateRankPosition();
    }

    private void UpdateRankPosition()
    {
        if (!gameObject.activeInHierarchy)
            return;

        text.ForceMeshUpdate();

        RectTransform rankImgRect = rankImage.rectTransform;
        RectTransform rankShadowImgRect = rankImageShadow.rectTransform;

        int lines = Mathf.Max(1, text.textInfo.lineCount) - 1;

        float newRankY = initialRankY + (lines * yPosIncreaseAmount);
        float newShadowY = initialShadowY + (lines * yPosIncreaseAmount);

        rankImgRect.anchoredPosition = new Vector2(isChattingAll ? allChatPos : teamChatPos, newRankY);
        rankShadowImgRect.anchoredPosition = new Vector2(isChattingAll ? allChatPos : teamChatPos, newShadowY);
    }
}
