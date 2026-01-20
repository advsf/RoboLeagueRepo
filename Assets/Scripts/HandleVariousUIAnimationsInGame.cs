using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class HandleVariousUIAnimationsInGame : MonoBehaviour
{
    public static HandleVariousUIAnimationsInGame instance;

    [Header("Goal Score Animation UI")]
    [SerializeField] private GameObject goalScoreUIObj;
    [SerializeField] private TextMeshProUGUI scoredByText;
    [SerializeField] private TextMeshProUGUI assistedByText;
    [SerializeField] private float goalScoreAnimationDuration = 5f;

    [Header("Localization References")]
    [SerializeField] private LocalizedString scoredByLoc = new("Table1", "SCORED BY:");
    [SerializeField] private LocalizedString assistedByLoc = new("Table1", "ASSISTED BY:");

    [Header("Ball References")]
    [SerializeField] private BallSync ballSync;

    private void Start()
    {
        instance = this;
        goalScoreUIObj.SetActive(false);
    }

    private void OnDisable()
    {
        instance = null;
    }

    public void PlayGoalScoreUIAnimation()
    {
        // reset the texts
        scoredByText.text = "";
        assistedByText.text = "";

        goalScoreUIObj.SetActive(true); // all that is needed to do since the base animation is the animation that we are looking to play

        scoredByLoc["username"] = new StringVariable { Value = ballSync.scorerUsername.Value.ToString() };

        scoredByText.text = scoredByLoc.GetLocalizedString();

        if (!ballSync.assisterUsername.Value.IsEmpty)
        {
            assistedByLoc["username"] = new StringVariable { Value = ballSync.assisterUsername.Value.ToString() };
            assistedByText.text = assistedByLoc.GetLocalizedString();
        }

        else
        {
            assistedByText.text = new LocalizedString("Table1", "ASSISTED BY NONE").GetLocalizedString();
        }

        Invoke(nameof(DisableGoalScoreUIAnimation), goalScoreAnimationDuration);
    }

    private void DisableGoalScoreUIAnimation() => goalScoreUIObj.SetActive(false);
}
