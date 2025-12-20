using UnityEngine;
using Unity.Netcode;

public class HandleHitboxSize : NetworkBehaviour
{
    [SerializeField] private float mobileHitboxMultiplier = 1.5f;

    private void OnEnable()
    {
        // mobile assist
        if (Application.isMobilePlatform)
        {
            transform.localScale = new(transform.localScale.x * mobileHitboxMultiplier, transform.localScale.y, transform.localScale.z * mobileHitboxMultiplier);
        }    
    }
}
