using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandlePlayerJerseyCosmeticsInLobby : MonoBehaviour
{
    [Header("Jersey References")]
    [SerializeField] private GameObject jerseyObj;
    [SerializeField] private TextMeshProUGUI[] jerseyName;
    [SerializeField] private TextMeshProUGUI[] jerseyNumber;

    [Header("Rank Block UI References")]
    [SerializeField] private GameObject rankBlockUI;
    [SerializeField] private Image rankImage;
    [SerializeField] private int minimumRankIndexToUse;

    private void Start()
    {
        rankImage.sprite = HandlePlayerData.instance.GetRankSprite(minimumRankIndexToUse);

        if (FBPP.GetInt("RankIndex") >= minimumRankIndexToUse)
            rankBlockUI.SetActive(false);
        else
        {
            rankBlockUI.SetActive(true);

            // reset data since we can no longer use jerseys
            FBPP.SetString("JerseyName", "");
            FBPP.SetString("JerseyNumber", "");
        }

        UpdateJersey();
    }

    public void UpdateJersey()
    {
        foreach (TextMeshProUGUI nameText in jerseyName)
            nameText.text = FBPP.GetString("JerseyName", "");

        foreach (TextMeshProUGUI numberText in jerseyNumber)
            numberText.text = FBPP.GetString("JerseyNumber", "");

        jerseyObj.SetActive(FBPP.GetBool("IsJerseyEnabled"));
    }

    #region UI Functions

    public void SetJerseyNameText(string name)
    {
        if (name.Length > 6)
            return;

        FBPP.SetString("JerseyName", name);
        FBPP.Save();

        UpdateJersey();
    }

    public void SetJerseyNumberText(string number)
    {
        if (number.Length > 2 || number.Contains("-"))
            return;

        if (int.TryParse(number, out int num))
        {
            if (num < 0 || num > 99)
                return;

            FBPP.SetString("JerseyNumber", number);
            FBPP.Save();

            UpdateJersey();
        }
    }

    public void EnableJersey()
    {
        FBPP.SetBool("IsJerseyEnabled", true);
        FBPP.Save();

        UpdateJersey();
    }

    public void DisableJersey()
    {
        FBPP.SetBool("IsJerseyEnabled", false);
        FBPP.Save();

        UpdateJersey();
    }

    #endregion
}
