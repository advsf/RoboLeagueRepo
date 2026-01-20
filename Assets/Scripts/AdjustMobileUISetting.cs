using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AdjustMobileUISetting : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField] private float increaseScaleAmount = 0.3f;
    [SerializeField] private float decreaseScaleAmount = 0.3f;
    [SerializeField] private Image greenIndicatorUI;

    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        LoadLayout();

        EnableVisualHitboxUI(false);
    }

    private void OnDisable()
    {
        SaveLayout();
    }

    public void IncreaseScale()
    {
        float newSize = rectTransform.localScale.x + increaseScaleAmount;
        newSize = Mathf.Clamp(newSize, 0.9990874f, 5);

        rectTransform.localScale = new(newSize, newSize, newSize);
    }

    public void DecreaseScale()
    {
        float newSize = rectTransform.localScale.x - decreaseScaleAmount;
        newSize = Mathf.Clamp(newSize, 0.9990874f, 5);

        rectTransform.localScale = new(newSize, newSize, newSize);
    }

    private void LoadLayout()
    {
        if (!PlayerPrefs.HasKey(gameObject.name + "_X"))
            InitializeData();

        rectTransform.anchoredPosition = GetSavedPositionLayout(gameObject.name);

        float savedScale = GetSavedScaleXLayout(gameObject.name);
        rectTransform.localScale = new (savedScale, savedScale, savedScale);
    }

    private void InitializeData()
    {
        Vector2 originalPos = rectTransform.anchoredPosition;
        Vector2 originalScale = rectTransform.localScale;

        // store permanent data
        PlayerPrefs.SetFloat(gameObject.name + "OG_X", originalPos.x);
        PlayerPrefs.SetFloat(gameObject.name + "OG_Y", originalPos.y);
        PlayerPrefs.SetFloat(gameObject.name + "OG_Scale", originalScale.x);

        // normal data
        PlayerPrefs.SetFloat(gameObject.name + "_X", originalPos.x);
        PlayerPrefs.SetFloat(gameObject.name + "_Y", originalPos.y);
        PlayerPrefs.SetFloat(gameObject.name + "_Scale", originalScale.x);

        PlayerPrefs.Save();
    }

    public void OnDrag(PointerEventData eventData)
    {
        ManageAllMobileUIButtons.instance.AllowEditingOnThisMobileUIButton(this);

        EnableVisualHitboxUI(true);

        // move the UI
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ManageAllMobileUIButtons.instance.AllowEditingOnThisMobileUIButton(this);

        EnableVisualHitboxUI(true);
    }

    public void EnableVisualHitboxUI(bool condition)
    {
        greenIndicatorUI.enabled = condition;
    }

    public void SaveLayout()
    {
        Debug.Log("saving");

        PlayerPrefs.SetFloat(gameObject.name + "_X", rectTransform.anchoredPosition.x);
        PlayerPrefs.SetFloat(gameObject.name + "_Y", rectTransform.anchoredPosition.y);
        PlayerPrefs.SetFloat(gameObject.name + "_Scale", rectTransform.localScale.x);
        PlayerPrefs.Save();
    }

    public static Vector2 GetSavedPositionLayout(string name)
    {
        return new(PlayerPrefs.GetFloat(name + "_X"), PlayerPrefs.GetFloat(name + "_Y"));
    }

    public static float GetSavedScaleXLayout(string name)
    {
        return PlayerPrefs.GetFloat(name + "_Scale");
    }

    public void ResetUI()
    {
        float ogX = PlayerPrefs.GetFloat(gameObject.name + "OG_X");
        float ogY = PlayerPrefs.GetFloat(gameObject.name + "OG_Y");
        float ogScale = PlayerPrefs.GetFloat(gameObject.name + "OG_Scale");

        rectTransform.anchoredPosition = new Vector2(ogX, ogY);
        rectTransform.localScale = new(ogScale, ogScale, ogScale);

        // save data
        PlayerPrefs.SetFloat(gameObject.name + "_X", PlayerPrefs.GetFloat(gameObject.name + "OG_X"));
        PlayerPrefs.SetFloat(gameObject.name + "_Y", PlayerPrefs.GetFloat(gameObject.name + "OG_Y"));
        PlayerPrefs.SetFloat(gameObject.name + "_Scale", PlayerPrefs.GetFloat(gameObject.name + "OG_Scale"));
        PlayerPrefs.Save();
    }
}
