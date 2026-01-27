using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class HandleAbilities : NetworkBehaviour
{
    public static HandleAbilities instance;

    [Header("Ability Slots")]
    [SerializeField] private Abilities leftAbility;
    [SerializeField] private Abilities rightAbility;

    [Header("Left Ability UI References")]
    [SerializeField] private GameObject leftAbilityObj;
    [SerializeField] private Image leftAbilityBackground;
    [SerializeField] private TextMeshProUGUI leftAbilityName;
    [SerializeField] private TextMeshProUGUI leftAbilityKeyForeground;
    [SerializeField] private TextMeshProUGUI leftAbilityKeyBackground;
    [SerializeField] private GameObject leftAbilityCooldownObj;
    [SerializeField] private TextMeshProUGUI leftAbilityCooldownText;

    [Header("Right Ability UI References")]
    [SerializeField] private GameObject rightAbilityObj;
    [SerializeField] private Image rightAbilityBackground;
    [SerializeField] private TextMeshProUGUI rightAbilityName;
    [SerializeField] private TextMeshProUGUI rightAbilityKeyForeground;
    [SerializeField] private TextMeshProUGUI rightAbilityKeyBackground;
    [SerializeField] private GameObject rightAbilityCooldownObj;
    [SerializeField] private TextMeshProUGUI rightAbilityCooldownText;

    [Header("Animation References")]
    [SerializeField] private Animator leftAnimator;
    [SerializeField] private Animator rightAnimator;

    [Header("Ability Slots")]
    [SerializeField] private Abilities powerKickAbility;
    [SerializeField] private Abilities speedsterAbility;
    [SerializeField] private Abilities trapAbility;
    [SerializeField] private Abilities rouletteAbility;
    [SerializeField] private Abilities deflectAbility;
    [SerializeField] private Abilities killAbility;

    [Header("Other References")]
    [SerializeField] private float maxBackgroundAlpha;
    [SerializeField] private HandleThrowIn throwInScript;

    private float leftCooldownTimer;
    private float rightCooldownTimer;

    private float leftCurrentCooldownDuration;
    private float rightCurrentCooldownDuration;

    private bool isLeftAbilityTriggered;
    private bool isRightAbilityTriggered;

    // ui button double click


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            leftAbilityObj.SetActive(false);
            rightAbilityObj.SetActive(false);
            return;
        }

        instance = this;

        isLeftAbilityTriggered = false;
        isRightAbilityTriggered = false;

        leftAbilityCooldownObj.SetActive(false);
        rightAbilityCooldownObj.SetActive(false);
    }

    private void OnEnable()
    {
        if (!IsOwner)
            return;

        PlayerInputReference.instance.controls.Gameplay.LeftAbility.Enable();
        PlayerInputReference.instance.controls.Gameplay.RightAbility.Enable();

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        HandleKBMSupport.OnInputChanged += HandleUI;
    }

    private void OnDisable()
    {
        if (!IsOwner)
            return;

        PlayerInputReference.instance.controls.Gameplay.LeftAbility.Disable();
        PlayerInputReference.instance.controls.Gameplay.RightAbility.Disable();

        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        HandleKBMSupport.OnInputChanged -= HandleUI;
    }

    private void OnLocaleChanged(Locale locale)
    {
        HandleAbilityNameText();
    }

    public void SetPositionAbilities(string position)
    {
        if (position.Equals("CF"))
        {
            leftAbility = powerKickAbility;
            rightAbility = speedsterAbility;
        }

        else if (position.Equals("RMF") || position.Equals("LMF"))
        {
            leftAbility = trapAbility;
            rightAbility = rouletteAbility;
        }

        else if (position.Equals("CB"))
        {
            leftAbility = killAbility;
            rightAbility = deflectAbility;
        }

        else
        {
            leftAbility = null;
            rightAbility = null;
        }

        // set up the animator component to the UI animators (which allows the rainbow display thing when activiated)
        if (leftAbility != null)
            leftAbility.animator = leftAnimator;
        
        if (rightAbility != null)
            rightAbility.animator = rightAnimator;

        HandleUI();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        HandleCooldown();

        if (PlayerMovement.instance.isMovementDisabled || HandleCursorSettings.instance.IsUIOn() || !ServerManager.instance.isAbilityEnabled.Value || throwInScript.isPickedUp)
            return;

        if (PlayerInputReference.instance.controls.Gameplay.LeftAbility.IsPressed() && leftAbility != null)
            PerformLeftAbility();

        if (PlayerInputReference.instance.controls.Gameplay.RightAbility.IsPressed() && rightAbility != null)
            PerformRightAbility();
    }

    private void HandleUI()
    {
        HandleAbilityNameText();

        if (!Application.isMobilePlatform || HandleKBMSupport.instance.IsUsingKBM)
        {
            leftAbilityKeyForeground.text = PlayerInputReference.instance.controls.Gameplay.LeftAbility.GetBindingDisplayString();
            leftAbilityKeyBackground.text = PlayerInputReference.instance.controls.Gameplay.LeftAbility.GetBindingDisplayString();
            leftAbilityBackground.fillAmount = 1;

            rightAbilityKeyForeground.text = PlayerInputReference.instance.controls.Gameplay.RightAbility.GetBindingDisplayString();
            rightAbilityKeyBackground.text = PlayerInputReference.instance.controls.Gameplay.RightAbility.GetBindingDisplayString();
            rightAbilityBackground.fillAmount = 1;
        }

        else
        {
            leftAbilityKeyForeground.text = "L";
            leftAbilityKeyBackground.text = "L";
            leftAbilityBackground.fillAmount = 1;

            rightAbilityKeyForeground.text = "R";
            rightAbilityKeyBackground.text = "R";
            rightAbilityBackground.fillAmount = 1;
        }
    }

    private void HandleAbilityNameText()
    {
        if (rightAbility != null)
            rightAbilityName.text = new LocalizedString("Table1", rightAbility.abilityName).GetLocalizedString();
        else
            rightAbilityName.text = new LocalizedString("Table1", "None").GetLocalizedString();

        if (leftAbility != null)
            leftAbilityName.text = new LocalizedString("Table1", leftAbility.abilityName).GetLocalizedString();
        else
            leftAbilityName.text = new LocalizedString("Table1", "None").GetLocalizedString();
    }

    private void HandleCooldown()
    {
        // left ability
        if (leftCooldownTimer > 0)
        {
            leftCooldownTimer -= Time.deltaTime;

            if (leftCurrentCooldownDuration > 0)
            {
                float elapsedTime = leftCurrentCooldownDuration - leftCooldownTimer;
                float calculatedFillAmount = elapsedTime / leftCurrentCooldownDuration;
                leftAbilityBackground.fillAmount = Mathf.Clamp01(calculatedFillAmount);

                leftAbilityCooldownText.text = Mathf.RoundToInt(leftCooldownTimer).ToString();
            }
        }

        else if (leftAbilityCooldownObj.activeInHierarchy)
            leftAbilityCooldownObj.SetActive(false);

        // right ability
        if (rightCooldownTimer > 0)
        {
            rightCooldownTimer -= Time.deltaTime;

            if (rightCurrentCooldownDuration > 0)
            {
                float elapsedTime = rightCurrentCooldownDuration - rightCooldownTimer;
                float calculatedFillAmount = elapsedTime / rightCurrentCooldownDuration;
                rightAbilityBackground.fillAmount = Mathf.Clamp01(calculatedFillAmount);

                rightAbilityCooldownText.text = Mathf.RoundToInt(rightCooldownTimer).ToString();
            }
        }

        else if (rightAbilityCooldownObj.activeInHierarchy)
            rightAbilityCooldownObj.SetActive(false);
    }

    public void TriggerCooldown(Abilities abilityUsed)
    {
        if (!ServerManager.instance.doAbilityCD.Value)
        {
            SetCooldownToOne(abilityUsed); // we need 1 seconds to avoid breaking any abilities
            return;
        }

        // skip cooldown
        if (ServerManager.instance.isTutorialServer || ServerManager.instance.isPracticeServer || !ServerManager.instance.didStartGame.Value)
        {
            if (abilityUsed == leftAbility)
            {
                leftCurrentCooldownDuration = 1;
                leftCooldownTimer = leftCurrentCooldownDuration;
                leftAbilityBackground.fillAmount = 0;
                isLeftAbilityTriggered = false;
                leftAbility.StopAnimation();

                leftAbilityCooldownObj.SetActive(true);
            }

            else if (abilityUsed == rightAbility)
            {
                rightCurrentCooldownDuration = 1;
                rightCooldownTimer = rightCurrentCooldownDuration;
                rightAbilityBackground.fillAmount = 0;
                isRightAbilityTriggered = false;
                rightAbility.StopAnimation();

                rightAbilityCooldownObj.SetActive(true);
            }

            return;
        }

        if (abilityUsed == leftAbility)
        {
            leftCurrentCooldownDuration = leftAbility.cooldownTime;
            leftCooldownTimer = leftCurrentCooldownDuration;
            leftAbilityBackground.fillAmount = 0; 
            isLeftAbilityTriggered = false;
            leftAbility.StopAnimation();

            leftAbilityCooldownObj.SetActive(true);
        }

        else if (abilityUsed == rightAbility)
        {
            rightCurrentCooldownDuration = rightAbility.cooldownTime;
            rightCooldownTimer = rightCurrentCooldownDuration;
            rightAbilityBackground.fillAmount = 0;
            isRightAbilityTriggered = false;
            rightAbility.StopAnimation();

            rightAbilityCooldownObj.SetActive(true);
        }
    }

    public void TriggerCooldown(Abilities abilityUsed, float newCooldown)
    {
        if (!ServerManager.instance.doAbilityCD.Value)
        {
            SetCooldownToOne(abilityUsed); // we need 1 seconds to avoid breaking any abilities
            return;
        }

        // skip cooldown
        if (ServerManager.instance.isTutorialServer || ServerManager.instance.isPracticeServer || !ServerManager.instance.didStartGame.Value)
        {
            if (abilityUsed == leftAbility)
            {
                leftCurrentCooldownDuration = 1;
                leftCooldownTimer = leftCurrentCooldownDuration;
                leftAbilityBackground.fillAmount = 0;
                isLeftAbilityTriggered = false;
                leftAbility.StopAnimation();

                leftAbilityCooldownObj.SetActive(true);
            }

            else if (abilityUsed == rightAbility)
            {
                rightCurrentCooldownDuration = 1;
                rightCooldownTimer = rightCurrentCooldownDuration;
                rightAbilityBackground.fillAmount = 0;
                isRightAbilityTriggered = false;
                rightAbility.StopAnimation();

                rightAbilityCooldownObj.SetActive(true);
            }

            return;
        }

        // normal cooldown logic
        if (abilityUsed == leftAbility)
        {
            leftCurrentCooldownDuration = newCooldown;
            leftCooldownTimer = leftCurrentCooldownDuration;
            leftAbilityBackground.fillAmount = 0;
            isLeftAbilityTriggered = false;
            leftAbility.StopAnimation();

            leftAbilityCooldownObj.SetActive(true);
        }

        else if (abilityUsed == rightAbility)
        {
            rightCurrentCooldownDuration = newCooldown;
            rightCooldownTimer = rightCurrentCooldownDuration;
            rightAbilityBackground.fillAmount = 0;
            isRightAbilityTriggered = false;
            rightAbility.StopAnimation();

            rightAbilityCooldownObj.SetActive(true);
        }
    }

    private void SetCooldownToOne(Abilities abilityUsed)
    {
        if (ServerManager.instance.isTutorialServer || ServerManager.instance.isPracticeServer || !ServerManager.instance.didStartGame.Value)
        {
            if (abilityUsed == leftAbility)
            {
                leftCurrentCooldownDuration = 1;
                leftCooldownTimer = 1;
                leftAbilityBackground.fillAmount = 0;
                isLeftAbilityTriggered = false;
                leftAbility.StopAnimation();

                leftAbilityCooldownObj.SetActive(true);
            }

            else if (abilityUsed == rightAbility)
            {
                rightCurrentCooldownDuration = 1;
                rightCooldownTimer = 1;
                rightAbilityBackground.fillAmount = 0;
                isRightAbilityTriggered = false;
                rightAbility.StopAnimation();

                rightAbilityCooldownObj.SetActive(true);
            }

            return;
        }

        if (abilityUsed == leftAbility)
        {
            leftCurrentCooldownDuration = leftAbility.cooldownTime;
            leftCooldownTimer = 1;
            leftAbilityBackground.fillAmount = 0;
            isLeftAbilityTriggered = false;
            leftAbility.StopAnimation();

            leftAbilityCooldownObj.SetActive(true);
        }

        else if (abilityUsed == rightAbility)
        {
            rightCurrentCooldownDuration = rightAbility.cooldownTime;
            rightCooldownTimer = 1;
            rightAbilityBackground.fillAmount = 0;
            isRightAbilityTriggered = false;
            rightAbility.StopAnimation();

            rightAbilityCooldownObj.SetActive(true);
        }
    }

    private void PerformLeftAbility()
    {
        if (leftCooldownTimer > 0 || isLeftAbilityTriggered)
            return;

        leftAbility.Activate(gameObject);
        isLeftAbilityTriggered = true;
    }

    private void PerformRightAbility()
    {
        if (rightCooldownTimer > 0 || isRightAbilityTriggered)
            return;

        rightAbility.Activate(gameObject);
        isRightAbilityTriggered = true;
    }

    // called by mobile UI buttons
    public void ActiviateLeftAbilityThroughUI()
    {
        if (PlayerMovement.instance.isMovementDisabled || HandleCursorSettings.instance.IsUIOn() || !ServerManager.instance.isAbilityEnabled.Value || throwInScript.isPickedUp)
            return;

        PerformLeftAbility();

        // for tutorial progression
        if (ServerManager.instance.isTutorialServer)
        {
            TutorialPlayerDetectorListener tutorialListener = HandleTutorialPlayerDetectors.instance.GetCurrentTutorialDetector();
            tutorialListener.CheckIfAbilityIsPressedForMobile(leftAbility);
        }
    }

    public void ActiviateRightAbilityThroughUI()
    {
        if (PlayerMovement.instance.isMovementDisabled || HandleCursorSettings.instance.IsUIOn() || !ServerManager.instance.isAbilityEnabled.Value || throwInScript.isPickedUp)
            return;

        PerformRightAbility();

        // for tutorial progression
        if (ServerManager.instance.isTutorialServer)
        {
            TutorialPlayerDetectorListener tutorialListener = HandleTutorialPlayerDetectors.instance.GetCurrentTutorialDetector();
            tutorialListener.CheckIfAbilityIsPressedForMobile(rightAbility);
        }
    }

    public bool IsAbilityActivitated() => isLeftAbilityTriggered || isRightAbilityTriggered; 
}
