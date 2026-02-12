using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.Rendering.Universal;

public class CameraLook : NetworkBehaviour
{
    public static CameraLook instance;

    [Header("Settings")]
    [SerializeField] private float sensMultiplier = 0.001f;
    [SerializeField] private float minYRot = -25f;
    [SerializeField] private float maxYRot = 80f;
    [SerializeField] private bool isPlayingCamera;

    [Header("Orbit & Collision")]
    [SerializeField] private LayerMask collisionLayers; 
    [SerializeField] private float collisionPadding = 0.2f; 
    private float cameraDistance;
    private float cameraYOffset;

    [Header("Mobile Support")]
    [SerializeField] private TouchPanel touchPanel;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;
    [SerializeField] private UniversalAdditionalCameraData camData;

    private float mouseX;
    private float mouseY;
    private float yRot;

    private bool canCamMove;

    public float GetCurrentUserSensitivity { get => sensMultiplier * FBPP.GetFloat("Sensitivity"); }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            canCamMove = true;
        }

        else
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        if (instance == this)
            instance = null;

        base.OnNetworkDespawn();
    }

    private void OnEnable()
    {
        if (!IsOwner)
            return;

        // since we are using two camera system
        // everytime we switch, change the singleton pattern to reference this specific camera
        instance = this;

        HandleCursorSettings.instance.EnableCursor(false, true);

        UpdateCamFOV();

        UpdatePostProcessing();
    }

    private void LateUpdate()
    {
        if (!IsOwner)
            return;

        HandleLook();
    }

    private void HandleLook()
    {
        // get user input if camera can be moved
        if (canCamMove)
        {
            Vector2 mouseDelta = Vector2.zero;
            Vector2 touchDelta = Vector2.zero;

            // pc input
            if (!Application.isMobilePlatform || HandleKBMSupport.instance.IsUsingKBM)
                mouseDelta = Mouse.current.delta.ReadValue();
            
            // mobile input
            else
                touchDelta = touchPanel.GetTouchDelta;

            mouseX = mouseDelta.x * FBPP.GetInt("InvertHorizontalMouse") + touchDelta.x;
            mouseY = mouseDelta.y * FBPP.GetInt("InvertVerticalMouse") + touchDelta.y;

            if (isPlayingCamera)
            {
                if (mouseX != 0)
                    player.Rotate(Vector3.up * mouseX * GetCurrentUserSensitivity);

                yRot += -mouseY * GetCurrentUserSensitivity;
                yRot = Mathf.Clamp(yRot, minYRot, maxYRot);
            }
            else
            {
                player.Rotate(Vector3.up * mouseX * GetCurrentUserSensitivity);

                yRot += -mouseY * GetCurrentUserSensitivity;
                yRot = Mathf.Clamp(yRot, minYRot, maxYRot);
            }
        }

        // update camera position always
        if (isPlayingCamera)
        {
            cameraDistance = FBPP.GetFloat("CameraDistance", 12);
            cameraYOffset = FBPP.GetFloat("CameraYOffset", 3);

            Vector3 orbitCenter = player.position + new Vector3(0f, cameraYOffset, 0f);
            Quaternion rotation = Quaternion.Euler(yRot, player.eulerAngles.y, 0f);
            Vector3 desiredPosition = orbitCenter + (rotation * Vector3.back * cameraDistance);

            // collision detection
            Vector3 finalPosition = desiredPosition;
            if (Physics.Linecast(orbitCenter, desiredPosition, out RaycastHit hit, collisionLayers))
                finalPosition = hit.point + (hit.normal * collisionPadding);

            transform.position = finalPosition;
            transform.LookAt(orbitCenter);
        }

        // spectator camera
        else
            transform.localRotation = Quaternion.Euler(yRot, 0f, 0f);
    }

    public void EnableCamera(bool condition)
    {
        canCamMove = condition;
    }

    public void UpdateCamFOV()
    {
        cam.fieldOfView = FBPP.GetFloat("CameraFOV");
    }

    private void UpdatePostProcessing()
    {
        bool isEnabled = FBPP.GetInt("PostProcessing", 1) == 1;

        camData.renderPostProcessing = isEnabled;
    }
}