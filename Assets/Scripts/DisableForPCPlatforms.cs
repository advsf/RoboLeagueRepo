using UnityEngine;

public class DisableForPCPlatforms : MonoBehaviour
{
    [SerializeField] private bool isTesting;

    private void Start()
    {
        if (isTesting)
            return;

        if (!Application.isMobilePlatform && !Application.isConsolePlatform)
            gameObject.SetActive(false);
    }
}
