using UnityEngine;

[CreateAssetMenu(fileName = "Powershot", menuName = "Scriptable Objects/Powershot")]
public class Powershot : Abilities
{
    [Header("Powershot Settings")]
    [SerializeField] private float powerShotIncreaseMultiplier;
    [SerializeField] private float durationBeforeAutoCancel;
    public float autoCancelCooldownDuration; // the adjusted cooldown when we auto-cancel the ability

    private int onActiviateHash;

    public override void Activate(GameObject user)
    {
        onActiviateHash = Animator.StringToHash("isActivated");

        HandleKicking kickingScript = user.GetComponentInChildren<HandleKicking>();

        if (kickingScript != null)
        {
            if (!kickingScript.didBicycleKick)
            {
                kickingScript.EnablePowerShot(powerShotIncreaseMultiplier, this, durationBeforeAutoCancel);
                animator.SetBool(onActiviateHash, true);
            }
        }
    }

    public override void StopAnimation()
    {
        animator.SetBool(onActiviateHash, false);
    }
}
