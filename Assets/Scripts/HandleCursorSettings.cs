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

        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
#endif
    }

    public void EnableCursor(bool condition, bool canCamMove = true)
    {
        // if we should enable the cursor
        if (condition)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // for mobile
            HandleMobileUI.instance.EnableTouchPadObj(false);

            isUIOn = true;
        }

        // disable cursor
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // for mobile
            HandleMobileUI.instance.EnableTouchPadObj(true);

            isUIOn = false;
        }

        CameraLook.instance.EnableCamera(canCamMove);
    }

    public void SetUIOnMode(bool condition) => isUIOn = condition;

    public bool IsUIOn() => isUIOn;
}
