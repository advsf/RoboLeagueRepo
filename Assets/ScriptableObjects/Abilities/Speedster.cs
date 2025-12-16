using UnityEngine;

[CreateAssetMenu(fileName = "Speedster", menuName = "Scriptable Objects/Speedster")]
public class Speedster : Abilities
{
    [Header("Speedster Settings")]
    [SerializeField] private float speedIncreaseAmount;
    [SerializeField] private float speedIncreaseDuration;

    private int onActiviateHash;

    public override void Activate(GameObject user)
    {
        onActiviateHash = Animator.StringToHash("isActivated");

        ManageAbilityMoves abilityScript = user.GetComponentInChildren<ManageAbilityMoves>();

        if (abilityScript != null)
        {
            // apply the speed boost logic
            abilityScript.Speedster(speedIncreaseAmount, speedIncreaseDuration, this);
            animator.SetBool(onActiviateHash, true);
        }
    }

    public override void StopAnimation()
    {
        animator.SetBool(onActiviateHash, false);
    }
}
