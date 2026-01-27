using UnityEngine;
using Unity.Netcode;

public class HandleHitboxSize : NetworkBehaviour
{
    [SerializeField] private float mobileHitboxMultiplier = 1.5f;

    private Vector3 originalScale;

    private void Start()
    {
        if (!IsOwner || !Application.isMobilePlatform)
            return;

        originalScale = transform.localScale;

        // mobile assist
        HandleKBMSupport.OnInputChanged += SetScale;
        SetScale();
    }

    private void SetScale()
    {
        if (!HandleKBMSupport.instance.IsUsingKBM)
            transform.localScale = new(originalScale.x * mobileHitboxMultiplier, originalScale.y, originalScale.z * mobileHitboxMultiplier);

        else
            transform.localScale = originalScale;
    }
}
