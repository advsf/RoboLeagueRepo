using UnityEngine;

public class HandleVisualBallModels : MonoBehaviour
{
    public CosmeticInventory cosmeticInventory;
    private GameObject ballObj;

    private void OnEnable()
    {
        ballObj = Instantiate(cosmeticInventory.GetSelectedBalls(PlayerPrefs.GetString("BallName", "DefaultBall")), transform);
        ballObj.SetActive(true);
    }
}
