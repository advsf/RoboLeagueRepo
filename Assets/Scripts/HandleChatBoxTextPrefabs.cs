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
    [SerializeField] private bool isOpenChatText;

    private int rankIndex;
    private string chatText;
    private bool isChattingAll;

    private bool isUpdated = false;

    // the reason why we have the OnEnable method
    // is becuase the closed text is disabled since it's being spawned into an already disabled parent
    // meaning that we aren't able to correctly position the rank sprite y position

    private void OnEnable()
    {
        if (!isUpdated && !isOpenChatText)
        {
            isUpdated = true;

            if (rankIndex >= 0)
            {
                rankImage.enabled = true;
                rankImageShadow.enabled = true;

                rankImage.sprite = HandlePlayerData.instance.GetRankSprite(rankIndex);
                rankImageShadow.sprite = HandlePlayerData.instance.GetRankSprite(rankIndex);
            }

            // set the position of the rank imgs
            RectTransform rankImgRect = rankImage.rectTransform;
            RectTransform rankShadowImgRect = rankImageShadow.rectTransform;

            text.text = chatText;

            text.ForceMeshUpdate();

            // get the y pos
            float verticalRankIncreaseAmount = (text.textInfo.lineCount - 1) * yPosIncreaseAmount;

            rankImgRect.anchoredPosition = new(isChattingAll ? allChatPos : teamChatPos, rankImgRect.anchoredPosition.y + verticalRankIncreaseAmount);
            rankShadowImgRect.anchoredPosition = new(isChattingAll ? allChatPos : teamChatPos, rankShadowImgRect.anchoredPosition.y + verticalRankIncreaseAmount);
        }
    }

    public void SetChatBoxText(int rankIndex, string chatText, bool isChattingAll)
    {
        this.rankIndex = rankIndex;
        this.chatText = chatText;
        this.isChattingAll = isChattingAll;

        if (rankIndex >= 0 && isOpenChatText)
        {
            rankImage.sprite = HandlePlayerData.instance.GetRankSprite(rankIndex);
            rankImageShadow.sprite = HandlePlayerData.instance.GetRankSprite(rankIndex);
        }

        else
        {
            rankImage.enabled = false;
            rankImageShadow.enabled = false;
        }

        text.text = chatText;

        text.ForceMeshUpdate();

        if (isOpenChatText)
        {
            // set the position of the rank imgs
            RectTransform rankImgRect = rankImage.rectTransform;
            RectTransform rankShadowImgRect = rankImageShadow.rectTransform;

            // get the y pos
            float verticalRankIncreaseAmount = (text.textInfo.lineCount - 1) * yPosIncreaseAmount;

            rankImgRect.anchoredPosition = new(isChattingAll ? allChatPos : teamChatPos, rankImgRect.anchoredPosition.y + verticalRankIncreaseAmount);
            rankShadowImgRect.anchoredPosition = new(isChattingAll ? allChatPos : teamChatPos, rankShadowImgRect.anchoredPosition.y + verticalRankIncreaseAmount);
        }
    }
}
