using UnityEngine;

public class HandleVisualBallModels : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] balls;

    private void OnEnable()
    {
        foreach (GameObject ball in balls)
            ball.SetActive(ball.name.Equals(PlayerPrefs.GetString("BallName", "DefaultBall")));
    }
}
