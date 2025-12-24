using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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

    [Header("Right Ability UI References")]
    [SerializeField] private GameObject rightAbilityObj;
    [SerializeField] private Image rightAbilityBackground;
    [SerializeField] private TextMeshProUGUI rightAbilityName;
    [SerializeField] private TextMeshProUGUI rightAbilityKeyForeground;
    [SerializeField] private TextMeshProUGUI rightAbilityKeyBackground;

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

    [Header("Double Tap Cooldown Settings")]
    [SerializeField] private float doublePressInterval = 0.3f;
    private float lastLeftAbilityPressTime;
    private float lastRightAbilityPressTime;

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
    }

    private void OnEnable()
    {
        if (!IsOwner)
            return;

        PlayerInputReference.instance.controls.Gameplay.LeftAbility.Enable();
        PlayerInputReference.instance.controls.Gameplay.RightAbility.Enable();
    }

    private void OnDisable()
    {
        if (!IsOwner)
            return;

        PlayerInputReference.instance.controls.Gameplay.LeftAbility.Disable();
        PlayerInputReference.instance.controls.Gameplay.RightAbility.Disable();
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

        if (PlayerMovement.instance.isMovementDisabled || HandleCursorSettings.instance.IsUIOn() || throwInScript.isPickedUp)
            return;

        if (PlayerInputReference.instance.controls.Gameplay.LeftAbility.IsPressed() && leftAbility != null)
            PerformLeftAbility();

        if (PlayerInputReference.instance.controls.Gameplay.RightAbility.IsPressed() && rightAbility != null)
            PerformRightAbility();
    }

    private void HandleUI()
    {
        if (rightAbility != null)
            rightAbilityName.text = rightAbility.abilityName;
        else
            rightAbilityName.text = "None";

        if (leftAbility != null)
            leftAbilityName.text = leftAbility.abilityName;
        else
            leftAbilityName.text = "None";

        leftAbilityKeyForeground.text = PlayerInputReference.instance.controls.Gameplay.LeftAbility.GetBindingDisplayString();
        leftAbilityKeyBackground.text = PlayerInputReference.instance.controls.Gameplay.LeftAbility.GetBindingDisplayString();
        leftAbilityBackground.fillAmount = 1;

        rightAbilityKeyForeground.text = PlayerInputReference.instance.controls.Gameplay.RightAbility.GetBindingDisplayString();
        rightAbilityKeyBackground.text = PlayerInputReference.instance.controls.Gameplay.RightAbility.GetBindingDisplayString();
        rightAbilityBackground.fillAmount = 1;
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
            }
        }

        // right ability
        if (rightCooldownTimer > 0)
        {
            rightCooldownTimer -= Time.deltaTime;

            if (rightCurrentCooldownDuration > 0)
            {
                float elapsedTime = rightCurrentCooldownDuration - rightCooldownTimer;
                float calculatedFillAmount = elapsedTime / rightCurrentCooldownDuration;
                rightAbilityBackground.fillAmount = Mathf.Clamp01(calculatedFillAmount);
            }
        }
    }

    public void TriggerCooldown(Abilities abilityUsed)
    {
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
            }

            else if (abilityUsed == rightAbility)
            {
                rightCurrentCooldownDuration = 1;
                rightCooldownTimer = rightCurrentCooldownDuration;
                rightAbilityBackground.fillAmount = 0;
                isRightAbilityTriggered = false;
                rightAbility.StopAnimation();
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
        }

        else if (abilityUsed == rightAbility)
        {
            rightCurrentCooldownDuration = rightAbility.cooldownTime;
            rightCooldownTimer = rightCurrentCooldownDuration;
            rightAbilityBackground.fillAmount = 0;
            isRightAbilityTriggered = false;
            rightAbility.StopAnimation();
        }
    }

    public void TriggerCooldown(Abilities abilityUsed, float newCooldown)
    {
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
            }

            else if (abilityUsed == rightAbility)
            {
                rightCurrentCooldownDuration = 1;
                rightCooldownTimer = rightCurrentCooldownDuration;
                rightAbilityBackground.fillAmount = 0;
                isRightAbilityTriggered = false;
                rightAbility.StopAnimation();
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
        }

        else if (abilityUsed == rightAbility)
        {
            rightCurrentCooldownDuration = newCooldown;
            rightCooldownTimer = rightCurrentCooldownDuration;
            rightAbilityBackground.fillAmount = 0;
            isRightAbilityTriggered = false;
            rightAbility.StopAnimation();
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
        if (Time.time - lastLeftAbilityPressTime <= doublePressInterval)
        {
            PerformLeftAbility();
            lastLeftAbilityPressTime = 0;
        }

        lastLeftAbilityPressTime = Time.time;
    }

    public void ActiviateRightAbilityThroughUI()
    {
        if (Time.time - lastRightAbilityPressTime <= doublePressInterval)
        {
            PerformRightAbility();
            lastRightAbilityPressTime = 0;
        }

        lastRightAbilityPressTime = Time.time;
    }

    public bool IsAbilityActivitated() => isLeftAbilityTriggered || isRightAbilityTriggered; 
}
