using UnityEngine;

[CreateAssetMenu(fileName = "Roulette", menuName = "Scriptable Objects/Roulette")]
public class Roulette : Abilities
{
    [Header("Roulette Settings")]
    public float exitBallForce;
    public float playerMoveForce; // force moving the player to where the ball is
    public float rouletteAnimationDuration;
    public float speedBoostAmount;
    public float speedBoostDuration;

    [Header("SphereCheck References")]
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float sphereCastMaxDistance;

    [Header("Cooldown Settings")]
    public float cooldownDelay;
    [SerializeField] private float autoCancelCooldown;

    private int onActiviateHash;

    public override void Activate(GameObject user)
    {
        onActiviateHash = Animator.StringToHash("isActivated");
        animator.SetBool(onActiviateHash, true);

        ManageAbilityMoves abilityScript = user.GetComponentInChildren<ManageAbilityMoves>();

        if (abilityScript != null)
            abilityScript.Roulette(playerMoveForce, exitBallForce, rouletteAnimationDuration, speedBoostAmount, speedBoostDuration, autoCancelCooldown, sphereCastRadius, sphereCastMaxDistance, cooldownDelay, this);
    }

    public override void StopAnimation()
    {
        animator.SetBool(onActiviateHash, false);
    }
}
