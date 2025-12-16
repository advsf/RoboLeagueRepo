using UnityEngine;

[CreateAssetMenu(fileName = "Trap", menuName = "Scriptable Objects/Trap")]
public class Trap : Abilities
{
    [Header("Trap Ability Reference")]
    [SerializeField] private float trapBallSphereRadius;
    [SerializeField] private float trapBallSphereMaxDistance;
    [SerializeField] private float highTrapBallYThreshold;
    [SerializeField] private float cooldownDelay;
    [SerializeField] private float movementDelay; // how long before the user can move again after trapping

    private int onActiviateHash;

    public override void Activate(GameObject user)
    {
        onActiviateHash = Animator.StringToHash("isActivated");

        ManageAbilityMoves abilityScript = user.GetComponentInChildren<ManageAbilityMoves>();

        // play UI animation
        animator.SetTrigger(onActiviateHash);

        if (abilityScript != null)
            abilityScript.Trap(trapBallSphereRadius, trapBallSphereMaxDistance, highTrapBallYThreshold, movementDelay, cooldownDelay, this);
    }

    public override void StopAnimation()
    {
        animator.SetBool(onActiviateHash, false);
    }
}
