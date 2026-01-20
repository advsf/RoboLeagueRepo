using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Localization;

public class HandleTutorialUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button proceedButton;
    [SerializeField] private int currentStageNumber;

    [Header("Typewriter Setting")]
    [SerializeField] private string messageText;
    [SerializeField] private string mobileMessageText;
    [SerializeField] private float typeWriterDelay;

    [Header("Animation Setting")]
    [SerializeField] private float animationIntroDuration;

    private LocalizedString tutorialTextLoc;

    private void OnEnable()
    {
        tutorialTextLoc = new("Table1", (!Application.isMobilePlatform ? "PC_TUTORIAL_" : "MOBILE_TUTORIAL") + currentStageNumber);

        StartUI();
    }

    private void Update()
    {
        HandleCursorSettings.instance.EnableCursor(true, false);
    }

    
    private void StartUI()
    {
        proceedButton.interactable = false;

        text.text = "";

        if (!Application.isMobilePlatform)
            messageText = tutorialTextLoc.GetLocalizedString();
        else
            mobileMessageText = tutorialTextLoc.GetLocalizedString();

            StartCoroutine(StartTypeWriterEffect());
    }
    private IEnumerator StartTypeWriterEffect()
    {
        yield return new WaitForSeconds(animationIntroDuration);

        if (!Application.isMobilePlatform)
        {
            foreach (char letter in messageText)
            {
                text.text += letter;
                yield return new WaitForSeconds(typeWriterDelay);
            }
        }

        else
        {
            foreach (char letter in mobileMessageText)
            {
                text.text += letter;
                yield return new WaitForSeconds(typeWriterDelay);
            }
        }

        // wait a second before allowing the user to proceed
        yield return new WaitForSeconds(1);
        proceedButton.interactable = true;
    }
}
