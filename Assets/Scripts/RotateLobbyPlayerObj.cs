using UnityEngine;

public class RotateLobbyPlayerObj : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float spinStrength = 6.65f;
    [SerializeField] private float inertiaDamping = 6f;
    [SerializeField] private float spinCooldown = 0.5f;

    private float angularVelocity;

    private bool canSpin = true;

    private void Update()
    {
        angularVelocity = Mathf.Lerp(angularVelocity, 0f, inertiaDamping * Time.deltaTime);

        transform.Rotate(Vector3.up, angularVelocity, Space.World);
    }

    public void SpinPlayerLeft()
    {
        if (!canSpin)
            return;

        canSpin = false;

        angularVelocity = spinStrength;

        Invoke(nameof(AllowPlayerToSpinAgain), spinCooldown);
    }

    public void SpinPlayerRight()
    {
        if (!canSpin)
            return;

        canSpin = false;

        angularVelocity = -spinStrength;

        Invoke(nameof(AllowPlayerToSpinAgain), spinCooldown);
    }

    private void AllowPlayerToSpinAgain() => canSpin = true;
}
