using UnityEngine;
using UnityEngine.InputSystem;

public class SpinLobbyPlayer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float spinSensitivity = 0.3f;
    [SerializeField] private float inertiaDamping = 6f;

    private float angularVelocity;
    private bool isHolding;

    private void Update()
    {
        float mouseX = Mouse.current.delta.ReadValue().x;
        isHolding = Input.GetMouseButton(0);

        if (isHolding)
            angularVelocity = -mouseX * spinSensitivity;

        else
            // slow down gradually when released
            angularVelocity = Mathf.Lerp(angularVelocity, 0f,inertiaDamping * Time.deltaTime);

        transform.Rotate(Vector3.up, angularVelocity, Space.World);
    }
}
