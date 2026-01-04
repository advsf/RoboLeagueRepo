using UnityEngine;

public class ManageAllMobileUIButtons : MonoBehaviour
{
    public void ResetAllMobileUI()
    {
        foreach (Transform obj in transform)
        {
            if (obj.GetComponent<AdjustMobileUISetting>() != null)
                obj.GetComponent<AdjustMobileUISetting>().ResetUI();
        }
    }
}
