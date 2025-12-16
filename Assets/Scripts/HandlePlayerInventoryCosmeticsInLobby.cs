using UnityEngine;

public class HandlePlayerInventoryCosmeticsInLobby : MonoBehaviour
{
    public static HandlePlayerInventoryCosmeticsInLobby instance;

    [Header("Hair References")]
    [SerializeField] private GameObject[] hairs;

    [Header("Accessory References")]
    [SerializeField] private GameObject[] accessories;

    [Header("Emotes References")]
    [SerializeField] private GameObject[] emotes;

    [Header("Ball References")]
    [SerializeField] private GameObject[] balls;

    [Header("Trail References")]
    [SerializeField] private GameObject[] trails;

    private void Start()
    {
        instance = this;

        UpdateCosmetics();
    }

    public void UpdateCosmetics()
    {
        foreach (GameObject hair in hairs)
            hair.SetActive(hair.name.Equals(PlayerPrefs.GetString("HairName", "None")));

        foreach (GameObject accessory in accessories)
            accessory.SetActive(accessory.name.Equals(PlayerPrefs.GetString("AccessoryName", "None")));

        foreach (GameObject ball in balls)
            ball.SetActive(ball.name.Equals(PlayerPrefs.GetString("BallName", "DefaultBall")));

        foreach (GameObject trail in trails)
            trail.SetActive(trail.name.Equals(PlayerPrefs.GetString("TrailName", "DefaultBall")));
    }
}
