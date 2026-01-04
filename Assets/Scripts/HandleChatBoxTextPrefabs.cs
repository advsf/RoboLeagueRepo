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
    [SerializeField] private float yIncreasePos;
    [SerializeField] private int eachCharacterPerLine; // used to determine how many characters can be in a line before the text wraps

    public void SetChatBoxText(int rankIndex, string chatText, bool isChattingAll)
    {
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

        // set the position of the rank imgs
        RectTransform rankImgRect = rankImage.rectTransform;
        RectTransform rankShadowImgRect = rankImageShadow.rectTransform;

        // get the y Pos
        float yIncreaseAmount = (int) (text.text.Length / eachCharacterPerLine) * yIncreasePos;

        rankImgRect.anchoredPosition = new(isChattingAll ? allChatPos : teamChatPos, rankImgRect.anchoredPosition.y + yIncreaseAmount);
        rankShadowImgRect.anchoredPosition = new(isChattingAll ? allChatPos : teamChatPos, rankShadowImgRect.anchoredPosition.y + yIncreaseAmount);

        text.text = chatText;
    }
}
