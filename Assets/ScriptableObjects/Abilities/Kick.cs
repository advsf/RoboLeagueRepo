using UnityEngine;

[CreateAssetMenu(fileName = "Kick", menuName = "Scriptable Objects/Kick")]
public class Kick : Abilities
{
    [Header("Kick Ability References")]
    [SerializeField] private float kickSphereRadius;
    [SerializeField] private float kickSphereMaxDistance;
    [SerializeField] private float kickForce;
    [SerializeField] private float timeToReEnableMovement;
    [SerializeField] private float cooldownDelay;

    private int onActiviateHash;

    public override void Activate(GameObject user)
    {
        onActiviateHash = Animator.StringToHash("isActivated");

        // play UI animation
        animator.SetTrigger(onActiviateHash);

        ManageAbilityMoves abilityScript = user.GetComponentInChildren<ManageAbilityMoves>();

        if (abilityScript != null)
            abilityScript.Kick(kickSphereRadius, kickSphereMaxDistance, kickForce, user.transform.forward, cooldownDelay, timeToReEnableMovement, this);
    }

    public override void StopAnimation()
    {
        animator.SetBool(onActiviateHash, false);
    }
}
