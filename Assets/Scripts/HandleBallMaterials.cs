using UnityEngine;

public class HandleBallMaterials : MonoBehaviour
{
    [Header("Mat References")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Rigidbody ballRb;

    [Header("Toon Shader Settings")]
    [SerializeField] private float ballVelocityThreshold; // how high the ball should be

    private void Start()
    {
        _renderer.enabled = false;
    }

    private void Update()
    {
        // if the ball is high or moving fast
        if (ballRb.linearVelocity.magnitude >= ballVelocityThreshold)
            _renderer.enabled = true;

        // if it isn't
        // remove the outline (toon shader)
        if (ballRb.linearVelocity.magnitude < ballVelocityThreshold)
            _renderer.enabled = false;

    }
}
