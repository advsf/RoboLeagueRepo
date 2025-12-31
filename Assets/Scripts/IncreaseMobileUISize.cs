using UnityEngine;

public class IncreaseMobileUISize : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Vector3 mobilePosition;
    [SerializeField] private Vector3 mobileScale;
    [SerializeField] private bool changeMobilePosition = false;
    [SerializeField] private bool changeMobileScale = false;

    private void Start()
    {
        if (!Application.isMobilePlatform)
            return;

        if (changeMobilePosition)
            GetComponent<RectTransform>().localPosition = mobilePosition;

        if (changeMobileScale)
            transform.localScale = mobileScale;
    }
}
