using UnityEngine;

public class HandleCursorSettings : MonoBehaviour
{
    public static HandleCursorSettings instance;

    [SerializeField] private bool forceCursorOn;

    private bool isUIOn;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }

        EnableCursor(true);
    }

    private void Update()
    {

#if UNITY_EDITOR
        if (forceCursorOn)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
#endif
    }

    public void EnableCursor(bool condition, bool canCamMove = true)
    {
        // if we should disable the cursor
        if (condition)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // for mobile
            if (Application.isMobilePlatform)
                HandleMobileUI.instance.EnableTouchPadObj(false);

            isUIOn = true;
        }

        // enable cursor
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // for mobile
            if (Application.isMobilePlatform)   
                HandleMobileUI.instance.EnableTouchPadObj(true);

            isUIOn = false;
        }

        CameraLook.instance.EnableCamera(canCamMove);
    }

    public void SetUIOnMode(bool condition)
    {
        if (Application.isMobilePlatform)
            HandleMobileUI.instance.EnableTouchPadObj(condition);

        isUIOn = condition;
    }

    public bool IsUIOn() => isUIOn;
}
