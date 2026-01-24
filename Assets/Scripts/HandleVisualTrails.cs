using UnityEngine;

public class HandleVisualTrails : MonoBehaviour
{
    public CosmeticInventory cosmeticInventory;
    private GameObject trailObj;

    private void OnEnable()
    {
        trailObj = Instantiate(cosmeticInventory.GetSelectedTrails(PlayerPrefs.GetString("TrailName", "None")), transform);
        trailObj.SetActive(true);
    }
}
