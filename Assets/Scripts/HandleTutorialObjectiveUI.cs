using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Settings;

public class HandleTutorialObjectiveUI : MonoBehaviour
{
    public static HandleTutorialObjectiveUI instance;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI resetBallText;
    [SerializeField] private Animator animator;

    [Header("Localization References")]
    [SerializeField] private LocalizedString pressRToResetLoc = new("Table1", "PRESS R TO RESET THE BALL");
    [SerializeField] private LocalizedString pressTheBallToResetLoc = new("Table1", "PRESS THE BALL ICON TO RESET THE BALL");
    [SerializeField] private LocalizedString tutorialObjective0Loc = new("Table1", "TUTORIAL_OBJECTIVE_0");
    [SerializeField] private LocalizedString tutorialObjective1Loc = new("Table1", "TUTORIAL_OBJECTIVE_1");
    [SerializeField] private LocalizedString tutorialObjective2Loc = new("Table1", "TUTORIAL_OBJECTIVE_2");
    [SerializeField] private LocalizedString tutorialObjective3Loc = new("Table1", "TUTORIAL_OBJECTIVE_3");
    [SerializeField] private LocalizedString tutorialObjective4Loc = new("Table1", "TUTORIAL_OBJECTIVE_4");
    [SerializeField] private LocalizedString tutorialObjective5Loc = new("Table1", "TUTORIAL_OBJECTIVE_5");
    [SerializeField] private LocalizedString tutorialObjective6Loc = new("Table1", "TUTORIAL_OBJECTIVE_6");
    [SerializeField] private LocalizedString tutorialObjective7Loc = new("Table1", "TUTORIAL_OBJECTIVE_7");
    [SerializeField] private LocalizedString tutorialObjective8Loc = new("Table1", "TUTORIAL_OBJECTIVE_8");
    [SerializeField] private LocalizedString tutorialObjective9Loc = new("Table1", "TUTORIAL_OBJECTIVE_9");
    [SerializeField] private LocalizedString tutorialObjective10Loc = new("Table1", "TUTORIAL_OBJECTIVE_10");
    [SerializeField] private LocalizedString tutorialObjective11Loc = new("Table1", "TUTORIAL_OBJECTIVE_11");
    [SerializeField] private LocalizedString tutorialObjective12Loc = new("Table1", "TUTORIAL_OBJECTIVE_12");

    private int onEnableHash;
    private int onDisableHash;

    private int amountOfDetectorsPassed = -1;

    private void Start()
    {
        instance = this;

        onEnableHash = Animator.StringToHash("OnEnable");
        onDisableHash = Animator.StringToHash("OnDisable");

        UpdateTutorialHelpText();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        instance = null;
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        UpdateTutorialObjectiveUI(false);
        UpdateTutorialHelpText();
    }

    public void EnableTutorialObjectiveUI()
    {
        animator.SetTrigger(onEnableHash);

        amountOfDetectorsPassed = -1;
        objectiveText.color = Color.white;

        UpdateTutorialObjectiveUI();
    }

    public IEnumerator DisableTutorialObjectiveUI()
    {
        objectiveText.color = Color.green;

        yield return new WaitForSeconds(1);

        animator.SetTrigger(onDisableHash);
    }

    private void UpdateTutorialHelpText()
    {
        // pc
        if (!Application.isMobilePlatform)
            resetBallText.text = pressRToResetLoc.GetLocalizedString();

        // mobile
        else
            resetBallText.text = pressTheBallToResetLoc.GetLocalizedString();
    }

    public void UpdateTutorialObjectiveUI(bool increaseStageProgression = true)
    {
        if (increaseStageProgression)
            amountOfDetectorsPassed++;

        switch (TutorialManager.instance.currentTutorialStageNumber)
        {
            // movement
            case 0:
                tutorialObjective0Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                objectiveText.text = tutorialObjective0Loc.GetLocalizedString();

                if (amountOfDetectorsPassed == 5)
                    StartCoroutine(DisableTutorialObjectiveUI());
                break;

            // dribbling
            case 1:

                tutorialObjective1Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                objectiveText.text = tutorialObjective1Loc.GetLocalizedString();

                if (amountOfDetectorsPassed == 8)
                    StartCoroutine(DisableTutorialObjectiveUI());
                break;
            
            // shooting
            case 2:
                tutorialObjective2Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                objectiveText.text = tutorialObjective2Loc.GetLocalizedString();

                if (amountOfDetectorsPassed == 8)
                    StartCoroutine(DisableTutorialObjectiveUI());
                break;
            // power kick
            case 3:
                tutorialObjective3Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                objectiveText.text = tutorialObjective3Loc.GetLocalizedString();

                if (amountOfDetectorsPassed == 3)
                    StartCoroutine(DisableTutorialObjectiveUI());
                break;
            // speedster
            case 4:
                tutorialObjective4Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                objectiveText.text = tutorialObjective4Loc.GetLocalizedString();

                if (amountOfDetectorsPassed == 1)
                    StartCoroutine(DisableTutorialObjectiveUI());
                break;
            // trap
            case 5:
                tutorialObjective5Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                objectiveText.text = tutorialObjective5Loc.GetLocalizedString();

                if (amountOfDetectorsPassed == 3)
                    StartCoroutine(DisableTutorialObjectiveUI());
                break;
            // roulette
            case 6:
                tutorialObjective6Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                objectiveText.text = tutorialObjective6Loc.GetLocalizedString();

                if (amountOfDetectorsPassed == 1)
                    StartCoroutine(DisableTutorialObjectiveUI());
                break;
            // kick
            case 7:
                tutorialObjective7Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                objectiveText.text = tutorialObjective7Loc.GetLocalizedString();

                if (amountOfDetectorsPassed == 1)
                    StartCoroutine(DisableTutorialObjectiveUI());
                break;
            // deflect
            case 8:
                tutorialObjective8Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                objectiveText.text = tutorialObjective8Loc.GetLocalizedString();

                if (amountOfDetectorsPassed == 1)
                    StartCoroutine(DisableTutorialObjectiveUI());
                break;
            // goalkeeper dive
            case 9:
                tutorialObjective9Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                objectiveText.text = tutorialObjective9Loc.GetLocalizedString();

                if (amountOfDetectorsPassed == 3)
                    StartCoroutine(DisableTutorialObjectiveUI());
                break;
            // goalkeeper catch, drop kick, roll
            case 10:
                if (amountOfDetectorsPassed == 0)
                {
                    tutorialObjective10Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                    objectiveText.text = tutorialObjective10Loc.GetLocalizedString();
                }

                else if (amountOfDetectorsPassed == 1)
                {
                    tutorialObjective11Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                    objectiveText.text = tutorialObjective11Loc.GetLocalizedString();
                }

                else
                {
                    tutorialObjective12Loc["amountOfDetectorsPassed"] = new IntVariable { Value = amountOfDetectorsPassed };
                    objectiveText.text = tutorialObjective12Loc.GetLocalizedString();
                }

                if (amountOfDetectorsPassed == 3)
                    StartCoroutine(DisableTutorialObjectiveUI());

                break;
        }
    }
} 
