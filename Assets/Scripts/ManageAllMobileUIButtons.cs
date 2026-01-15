using UnityEngine;

public class ManageAllMobileUIButtons : MonoBehaviour
{
    public static ManageAllMobileUIButtons instance;

    private AdjustMobileUISetting selectedUIButton;

    private void Start()
    {
        instance = this;
    }
    
    public void AllowEditingOnThisMobileUIButton(AdjustMobileUISetting uiButton)
    {
        // disable the green hitbox
        if (selectedUIButton != null)
            selectedUIButton.EnableVisualHitboxUI(false);

        selectedUIButton = uiButton;
    }

    public void ResetSelectedUIButton()
    {
        selectedUIButton = null;
    }

    public void IncreaseScale()
    {
        selectedUIButton.IncreaseScale();
    }

    public void DecreaseScale()
    {
        selectedUIButton.DecreaseScale();
    }

    public void ResetAllMobileUI()
    {
        foreach (Transform obj in transform)
        {
            if (obj.GetComponent<AdjustMobileUISetting>() != null)
                obj.GetComponent<AdjustMobileUISetting>().ResetUI();
        }
    }
}
