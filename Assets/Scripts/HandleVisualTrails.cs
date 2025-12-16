using UnityEngine;

public class HandleVisualTrails : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] trails;

    private void OnEnable()
    {
        foreach (GameObject trail in trails)
        {
            trail.SetActive(trail.name.Equals(PlayerPrefs.GetString("TrailName")));
        }
    }
}
