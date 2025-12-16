using UnityEngine;
using TMPro;

public class HandleInventoryEquip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Settings")]
    [SerializeField] private bool isHair;
    [SerializeField] private bool isAccessory;
    [SerializeField] private bool isEmote;
    [SerializeField] private bool isBall;
    [SerializeField] private bool isTrail;

    private void OnEnable()
    {
        // remove the (Clone) text from the g
        if (gameObject.name.Contains("(Clone)"))
            gameObject.name = gameObject.name.Substring(0, gameObject.name.Length - 7);
    }

    public void InventoryEquip()
    {
        if (isHair)
        {
            PlayerPrefs.SetString("HairName", gameObject.name);
        }

        else if (isAccessory)
        {
            PlayerPrefs.SetString("AccessoryName", gameObject.name);
        }

        else if (isEmote)
        {
            // this shouldn't work
            // fix later
            PlayerPrefs.SetString("Emote", gameObject.name);
        }

        else if (isBall)
        {
            PlayerPrefs.SetString("BallName", gameObject.name);
        }

        else if (isTrail)
        {
            PlayerPrefs.SetString("TrailName", gameObject.name);
        }

        // update
        HandlePlayerInventoryCosmeticsInLobby.instance.UpdateCosmetics();
    }
}
