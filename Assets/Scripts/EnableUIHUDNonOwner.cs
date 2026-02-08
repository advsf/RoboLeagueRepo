 using UnityEngine;

public class EnableUIHUDNonOwner : MonoBehaviour
{
    [SerializeField] private GameObject[] uiHolders;

    private void Start()
    {
        ToggleHUD(FBPP.GetInt("EnableHud") == 1);

        HandleSettings.OnHUDToggled += ToggleHUD;
    }

    private void OnDestroy()
    {
        HandleSettings.OnHUDToggled -= ToggleHUD;
    }

    private void ToggleHUD(bool enable)
    {
        foreach (GameObject ui in uiHolders)
            ui.SetActive(enable);
    }
}
