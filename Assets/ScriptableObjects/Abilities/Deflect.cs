using UnityEngine;

[CreateAssetMenu(fileName = "Deflect", menuName = "Scriptable Objects/Deflect")]
public class Deflect : Abilities
{
    [Header("Deflect References")]
    [SerializeField] private float deflectSphereRadius;
    [SerializeField] private float deflectSphereMaxDistance;
    [SerializeField] private float playerMoveToBallForce;
    [SerializeField] private float ballKickForce;
    [SerializeField] private float lowerCooldown;

    private int onActiviateHash;

    public override void Activate(GameObject user)
    {
        onActiviateHash = Animator.StringToHash("isActivated");

        // play UI animation
        animator.SetTrigger(onActiviateHash);

        ManageAbilityMoves abilityScript = user.GetComponentInChildren<ManageAbilityMoves>();

        if (abilityScript != null)
            abilityScript.Deflect(deflectSphereRadius, deflectSphereMaxDistance, playerMoveToBallForce, ballKickForce, lowerCooldown, this);
    }

    public override void StopAnimation()
    {
        animator.SetBool(onActiviateHash, false);
    }
}
