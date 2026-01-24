using UnityEngine;

public class HandlePlayerInventoryCosmeticsInLobby : MonoBehaviour
{
    public static HandlePlayerInventoryCosmeticsInLobby instance;

    public CosmeticInventory cosmeticInventory;

    [Header("Hair References")]
    [SerializeField] private Transform hairParent;
    [SerializeField] private Transform accessoryParent;
    [SerializeField] private Transform emotesParent;
    [SerializeField] private Transform ballParent;
    [SerializeField] private Transform trailParent;

    private GameObject currentHair;
    private GameObject currentAccessory;
    private GameObject currentEmote;
    private GameObject currentBall;
    private GameObject currentTrail;

    private void Start()
    {
        instance = this;

        UpdateCosmetics();
    }

    public void UpdateCosmetics()
    {
        Destroy(currentHair);
        Destroy(currentAccessory);
        Destroy(currentBall);
        Destroy(currentTrail);

        currentHair = Instantiate(cosmeticInventory.GetSelectedHair(PlayerPrefs.GetString("HairName", "None")), hairParent);
        ChangeTransform(currentHair);

        currentAccessory = Instantiate(cosmeticInventory.GetSelectedAccessory(PlayerPrefs.GetString("AccessoryName", "None")), accessoryParent);
        ChangeTransform(currentAccessory);

        currentBall = Instantiate(cosmeticInventory.GetSelectedBalls(PlayerPrefs.GetString("BallName", "DefaultBall")), ballParent);
        currentBall.SetActive(true);

        currentTrail = Instantiate(cosmeticInventory.GetSelectedTrails(PlayerPrefs.GetString("TrailName", "None")), trailParent);
        currentTrail.SetActive(true);
    }

    private void ChangeTransform(GameObject obj)
    {
        obj.SetActive(true);

        if (obj.GetComponent<AccessoryTranformProperty>()  == null)
            return;

        AccessoryTranformProperty transformProperty = obj.GetComponent<AccessoryTranformProperty>();

        obj.transform.localPosition = transformProperty.GetLocalPosition();
        obj.transform.localRotation = Quaternion.Euler(transformProperty.GetLocalRotation());
        obj.transform.localScale = transformProperty.GetLocalScale();
    }
}
