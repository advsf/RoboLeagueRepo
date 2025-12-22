using System.Collections;
using UnityEngine;

public class DisableForMobilePlatforms : MonoBehaviour
{
    private void Start()
    {
        if (Application.isMobilePlatform)
            gameObject.SetActive(false);
    }
}
