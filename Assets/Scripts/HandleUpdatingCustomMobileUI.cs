using UnityEngine;
using Unity.Netcode;

public class HandleUpdatingCustomMobileUI : NetworkBehaviour
{
    [Header("Setting")]
    [SerializeField] private bool useAnotherName;
    [SerializeField] private string newSaveDataName;
    [SerializeField] private RectTransform rectTransform;

    private void OnEnable()
    {
        if (!IsOwner)
            return;

        UpdateUI();
    }

    public void UpdateUI()
    {
        string keyName = useAnotherName ? newSaveDataName : gameObject.name;

        if (PlayerPrefs.HasKey(keyName + "_X"))
        {
            float x = PlayerPrefs.GetFloat(keyName + "_X");
            float y = PlayerPrefs.GetFloat(keyName + "_Y");
            float scale = PlayerPrefs.GetFloat(keyName + "_Scale");

            rectTransform.anchoredPosition = new Vector2(x, y);
            rectTransform.localScale = new(scale, scale, scale);
        }
    }
}
