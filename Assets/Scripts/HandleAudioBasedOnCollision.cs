using UnityEngine;

public class HandleAudioBasedOnCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;

    [Header("Settings")]
    [SerializeField] private float minBallVelToTriggerNetSound = 35f;

    private void OnCollisionEnter(Collision collision)
    {
        bool isBallHit = collision.gameObject.layer.Equals(LayerMask.NameToLayer("Ball"));

        if (!isBallHit)
            return;

        Rigidbody ballRb = collision.gameObject.GetComponentInChildren<Rigidbody>();

        if (ballRb.linearVelocity.magnitude > minBallVelToTriggerNetSound)
        {
            audioSource.Play();
        }
    }
}
