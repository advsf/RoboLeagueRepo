using UnityEngine;
using UnityEngine.UI;

public class BallLandingPredictor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody ballRb;
    [SerializeField] private GameObject landingIndicatorPrefab;

    [Header("Settings")]
    [SerializeField] private float groundDetectionHeight;
    [SerializeField] private float yPos = 1.5f;
    [SerializeField] private float scaleDivider;
    [SerializeField] private float timeBeforeDisable = 0.5f;

    private GameObject landingIndicatorInstance;

    private bool isOnGround;
    private bool isTurningOff; // used to turn off the landing indicator smoothly

    private float originalScale;

    private void Start()
    {
        landingIndicatorInstance = Instantiate(landingIndicatorPrefab);
        landingIndicatorInstance.SetActive(false);
        originalScale = landingIndicatorInstance.transform.localScale.x;
    }

    private void OnDestroy()
    {
        Destroy(landingIndicatorInstance);
    }

    private void Update()
    {
        isOnGround = ballRb.position.y < groundDetectionHeight;

        if (!isOnGround)
        {
            landingIndicatorInstance.SetActive(true);

            // simpe prediction - only track the x and z position
            landingIndicatorInstance.transform.position = new(ballRb.position.x, yPos, ballRb.position.z);

            HandleLandingPointScale();
        }

        else if (isOnGround && landingIndicatorInstance.activeInHierarchy)
        {
            Invoke(nameof(DisableLandingIndictator), timeBeforeDisable);
        }
    }

    private void HandleLandingPointScale()
    {
        float heightDiff = ballRb.position.y - yPos;
        float scaleMultiplier = Mathf.Max(heightDiff / scaleDivider, 1.5f);
        float calculatedScale = originalScale * scaleMultiplier;

        landingIndicatorInstance.transform.localScale = new Vector3(calculatedScale, landingIndicatorInstance.transform.localScale.y, calculatedScale);
    }

    private void DisableLandingIndictator()
    {
        landingIndicatorInstance.SetActive(false);
        isTurningOff = false;
    }
}
