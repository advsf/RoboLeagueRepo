using UnityEngine;

public class DisableForPCPlatforms : MonoBehaviour
{
    private void Start()
    {
        if (Application.isEditor)
            return;

        if (!Application.isMobilePlatform && !Application.isConsolePlatform)
            gameObject.SetActive(false);
    }
}
