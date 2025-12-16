using UnityEngine;

public class LobbyCameraAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    private int goToInventoryUIHash;
    private int goBackToMainLobbyUIHash;

    private void Start()
    {
        goToInventoryUIHash = Animator.StringToHash("GoToInventory");
        goBackToMainLobbyUIHash = Animator.StringToHash("GoToMainLobbyFromInventory");
    }

    public void PlayCamAnimationToInventoryUI() => animator.SetTrigger(goToInventoryUIHash);

    public void PlayCamAnimationFromInventoryToMainLobbyUI() => animator.SetTrigger(goBackToMainLobbyUIHash);
}
