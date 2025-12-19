using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class HandleKicking : NetworkBehaviour
{
    public static HandleKicking instance;

    // used for mobile
    public enum KickMode 
    { 
        Shooting, 
        Dribbling, 
        BicycleKick 
    }

    [Header("Mobile Settings")]
    public KickMode currentMobileKickMode = KickMode.Shooting;
    [SerializeField] private float mobileMinCurveMagnitude = 0.05f;
    [SerializeField] private float mobileUpwardInfluenceMultiplier = 2f;
    [SerializeField] private float mobileCurveMultiplier = 1500f;

    [Header("Dribbling Settings")]
    [SerializeField] private float dribblingMultipler;
    [SerializeField] private float dribblingStaminaLoss;
    [SerializeField] private float dribblingHeightMultiplier;
    [SerializeField] private float dribblingCooldown;

    [Header("Shooting Settings")]
    [SerializeField] private float kickingMultipler;
    [SerializeField] private float shootingStaminaLoss;
    [SerializeField] private float shootingHeightMultiplier;
    [SerializeField] private float shootingYHeightAddition;
    [SerializeField] private float shootingCooldown;

    [Header("Heading Settings")]
    [SerializeField] private float headingMultipler;
    [SerializeField] private float headingStaminaLoss;

    [Header("Bicycle Kick Settings")]
    [SerializeField] private float bicycleOnAirKickingMultipler;
    [SerializeField] private float bicycleOnGroundKickingMultipler;
    [SerializeField] private float bicycleKickStaminaLoss;
    [SerializeField] private float bicycleKickDuration;
    [SerializeField] private float bicycleKickSphereRadius;
    [SerializeField] private float timeBeforeEnablingMovementAfterBicycleKick;

    [Header("Bicycle Physics Settings")]
    [SerializeField] private float bicycleKickMinLift = 6f;
    [SerializeField] private float bicycleKickMaxLift = 14f;
    [SerializeField] private float bicycleKickTopSpin = 5f;

    [Header("Powershot Settings")]
    [SerializeField] private GameObject powerShotBarObj;
    [SerializeField] private Slider powerShotSlider;
    [SerializeField] private Image powerShotBarFill;
    [SerializeField] private float powerShotSliderIncrementValue;
    [SerializeField] private float perfectTimingSliderValue;
    [SerializeField] private float perfectTimingSliderValueWindow;
    [SerializeField] private float goodTimingSliderValueWindow;
    private bool canPowerShot = false;
    private float powerShotMultiplier;
    private Powershot powerShotAbility;

    [Header("Movement Debuff Settings")]
    [SerializeField] private float timeBeforeSlowMovement;
    [SerializeField] private float minWalkSpeed;
    [SerializeField] private float minSprintSpeed;
    [SerializeField] private float speedLerpMultiplier;
    private float currentWalkSpeed;
    private float currentSprintSpeed;
    private float sliderChargingStartTime;

    [Header("Slider Settings")]
    [SerializeField] private float sliderIncrementValue;

    [Header("Ball Physics")]
    [SerializeField] private float sideSpinMultiplier;
    [SerializeField] private float topSpinMultiplier;
    [SerializeField] private float minimumMagnusMouseThreshold = 5f;

    [Header("Shooting Boosts Settings")]
    [SerializeField] private float shotMultiplier;
    [SerializeField] private float nonSpinShotMultiplier;

    [Header("Shooting Bar References")]
    [SerializeField] private Slider shootingBarSlider;
    [SerializeField] private Image barFill;

    [Header("References")]
    [SerializeField] private AnimationStateController playerAnimation;
    [SerializeField] private HandleStaminaBar staminaBar;
    [SerializeField] private HandleGoalkeeperAsPlayer playerGK;
    [SerializeField] private HandleThrowIn throwInScript;
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerMovement player;

    [Header("Current Kicking Information")]
    public bool IsChargingKick {
        get
        {
            // get pc input
            bool pcInput = PlayerInputReference.instance.controls.Gameplay.Kick.ReadValue<float>() > 0 || PlayerInputReference.instance.controls.Gameplay.Dribble.ReadValue<float>() > 0;
            bool mobileInput = PlayerInputReference.instance.controls.Gameplay.MobileShooting.IsPressed();

            return mobileInput;
        }
    }

    public bool isDribbling;
    public bool isShooting;
    public bool didDribble;
    public bool didBicycleKick;
    public bool didShoot;
    public bool hasPossession = false;

    private bool canKick;
    private bool canHead;
    private bool isInCooldown;
    private bool isShotBarReset;
    private bool canStartKick = false;

    private GameObject ball;
    private Rigidbody ballRb;

    private BallSync nearestBallSync;

    // input buffering for better responsiveness
    private const float INPUT_BUFFER_TIME = 0.1f;
    private float lastInputTime;
    private bool hasBufferedInput;

    private GameObject localSpawnedBall;

    private float shotBarAmount;

    private Vector2 joystickVal;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        InitializeUI();

        instance = this;

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        instance = null;

        base.OnNetworkDespawn();
    }

    private void Start()
    {
        if (!IsOwner)
            return;

        ball = SceneReferenceManager.instance.ball;
        ballRb = SceneReferenceManager.instance.ballRb;

        if (PlayerMovement.instance != null)
        {
            currentWalkSpeed = PlayerMovement.instance.walkSpeeds.forwardSpeed;
            currentSprintSpeed = PlayerMovement.instance.sprintSpeeds.forwardSpeed;
        }
    }

    private void OnEnable()
    {
        if (!IsOwner)
            return;

        nearestBallSync = BallManager.instance.FindNearestBall(transform.position);

        if (nearestBallSync == null)
            nearestBallSync = BallManager.instance.mainBallSync;

        PlayerInputReference.instance.controls.Gameplay.Kick.Enable();
        PlayerInputReference.instance.controls.Gameplay.Dribble.Enable();
        PlayerInputReference.instance.controls.Gameplay.SpawnBall.Enable();

        PlayerInputReference.instance.controls.Gameplay.MobileCurve.Enable();
        PlayerInputReference.instance.controls.Gameplay.MobileShooting.Enable();
    }

    private void OnDisable()
    {
        if (!IsOwner)
            return;

        PlayerInputReference.instance.controls.Gameplay.Kick.Disable();
        PlayerInputReference.instance.controls.Gameplay.Dribble.Disable();
        PlayerInputReference.instance.controls.Gameplay.SpawnBall.Disable();

        PlayerInputReference.instance.controls.Gameplay.MobileCurve.Disable();
        PlayerInputReference.instance.controls.Gameplay.MobileShooting.Disable();
    }

    private void InitializeUI()
    {
        shootingBarSlider.value = 0;
        shootingBarSlider.gameObject.SetActive(false);
        powerShotSlider.value = 0;
        powerShotBarObj.SetActive(false);
        isInCooldown = false;
        isShotBarReset = false;
    }

    private void Update()
    {
        if (!IsOwner || HandleCursorSettings.instance.IsUIOn())
            return;

        UpdateInputState();
        HandleChargingLogic();
        HandleInputBuffer();

        if (nearestBallSync == null)
            nearestBallSync = BallManager.instance.mainBallSync;

        if (PlayerInputReference.instance.controls.Gameplay.MobileCurve.ReadValue<Vector2>().magnitude > mobileMinCurveMagnitude)
            joystickVal = PlayerInputReference.instance.controls.Gameplay.MobileCurve.ReadValue<Vector2>();

        // spawn the local ball
        if (PlayerInputReference.instance.controls.Gameplay.SpawnBall.WasPressedThisFrame() && !ServerManager.instance.didStartGame.Value)
        {
            // don't do anything if it's a tutorial server
            if (ServerManager.instance.isTutorialServer)
                return;

            // if it is a practice server, then just set the main ball's position to the player
            if (ServerManager.instance.isPracticeServer)
            {
                BallManager.instance.mainBallSync.Teleport(new(transform.position.x, transform.position.y + 1.5f, transform.position.z), Quaternion.identity);
                return;
            }

            // spawn the ball
            if (localSpawnedBall == null)
                HandleSpawningLocalBall();

            // reset the ball since the ball is already spawned
            else if (localSpawnedBall != null)
                ResetLocalBallPosition(transform.position);
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || nearestBallSync == null || HandleCursorSettings.instance.IsUIOn())
            return;

        if ((canStartKick || hasBufferedInput) && !playerGK.isHoldingBall && !PlayerMovement.instance.isMovementDisabled)
            ExecuteKick();
    }

    public void HandleSpawningLocalBall()
    {
        Vector3 spawnPosition = new(transform.position.x, transform.position.y + 1.5f, transform.position.z); // add a little bit of a y offset

        BallManager.instance.RequestBallSpawnServerRpc(spawnPosition);
    }

    public void SetLocalSpawnedBall(GameObject ball)
    {
        localSpawnedBall = ball;
    }

    public void ResetLocalBallPosition(Vector3 pos)
    {
        if (localSpawnedBall != null)
        {
            BallSync ballSync = localSpawnedBall.GetComponentInChildren<BallSync>();

            ballSync.ResetBallServerRpc(pos);
        }
    }

    private void UpdateInputState()
    {
        // get mobile joystick data
        bool isUsingMobileStick = PlayerInputReference.instance.controls.Gameplay.MobileShooting.IsPressed();

        // mobile logic
        if (isUsingMobileStick)
        {
            isShooting = currentMobileKickMode == KickMode.Shooting;
            isDribbling = currentMobileKickMode == KickMode.Dribbling;
        }
        
        // pc logic
        else
        {
            isDribbling = PlayerInputReference.instance.controls.Gameplay.Dribble.ReadValue<float>() > 0;
            isShooting = PlayerInputReference.instance.controls.Gameplay.Kick.ReadValue<float>() > 0;
        }

        if (!IsChargingKick)
            sliderChargingStartTime = Time.time;
    }

    private void HandleChargingLogic()
    {
        if (IsChargingKick && !isInCooldown && !isShotBarReset)
        {
            if (canPowerShot)
                HandlePowerShotCharging();

            else
                HandleNormalKickCharging();

            ApplyMovementDebuff();
        }

        // kick
        else if (!IsChargingKick && !isShotBarReset && (shootingBarSlider.value > 0 || powerShotSlider.value > 0))
            PrepareKick();
    }

    private void HandlePowerShotCharging()
    {
        didShoot = true;
        powerShotBarObj.SetActive(true);
        powerShotSlider.value += powerShotSliderIncrementValue * Time.deltaTime;
    }

    private void HandleNormalKickCharging()
    {
        if (isDribbling)
            didDribble = true;

        if (isShooting)
            didShoot = true;

        // pc bicycle kick logic
        bool pcBicycleKick = isDribbling && isShooting;

        // mobile bicycle kick lgoic
        bool mobileBicycle = (currentMobileKickMode == KickMode.BicycleKick) && (joystickVal.magnitude > mobileMinCurveMagnitude);

        // if the user is performing a bicycle kick
        if (pcBicycleKick || mobileBicycle)
        {
            didBicycleKick = true;

            didShoot = false;
            didDribble = false;
        }

        ChangeShootingBarColor();

        shootingBarSlider.gameObject.SetActive(true);
        shootingBarSlider.value += sliderIncrementValue * Time.deltaTime;
    }

    private void ApplyMovementDebuff()
    {
        // if we've been charging for too long, make the player move slower
        if (Time.time - sliderChargingStartTime >= timeBeforeSlowMovement)
        {
            currentWalkSpeed = Mathf.Lerp(currentWalkSpeed, minWalkSpeed, Time.deltaTime * speedLerpMultiplier);
            if (PlayerMovement.instance != null)
                PlayerMovement.instance.SetSpeed(currentWalkSpeed);
        }
    }

    private void PrepareKick()
    {
        canStartKick = true;
        lastInputTime = Time.time;
        hasBufferedInput = true;

        if (canPowerShot)
            HandlePowerShotSliderColor();
    }

    private void HandleInputBuffer()
    {
        if (hasBufferedInput && Time.time - lastInputTime > INPUT_BUFFER_TIME)
            hasBufferedInput = false;
    }

    private void ExecuteKick()
    {
        // find the nearest ball
        nearestBallSync = BallManager.instance.FindNearestBall(transform.position);

        canStartKick = false;
        hasBufferedInput = false;

        // stop emote
        playerAnimation.StopCurrentEmote();

        shotBarAmount = shootingBarSlider.value;

        // bicycle kicks should be played even if the ball isn't near
        if (didBicycleKick)
        {
            if (!ValidateKickConditionsForBicycleKick())
            {
                ResetKickState();
                return;
            }

            playerAnimation.PlayBicycleKickAnimation();

            StartCoroutine(DetectBallDuringBicycleKick());
            StartCooldown(1.5f);

            return;
        }

        // validate kick conditions
        if (!ValidateKickConditions())
        {
            ResetKickState();
            return;
        }

        PlayKickAnimation();

        // perform a dribble move
        if (didDribble)
        {
            HandleDribbling();
            StartCooldown(dribblingCooldown);
        }

        // perform a powerkick move
        else if (canPowerShot)
            HandlePowerShot();

        // perform heading
        else if (!PlayerMovement.instance.IsOnGround && !didDribble)
            HandleHeading();

        // perform shooting 
        else if (didShoot && canKick)
        {
            HandleShooting();
            StartCooldown(shootingCooldown);
        }

        FinalizeKick();
    }

    private bool ValidateKickConditions()
    {
        if (!canKick && !canHead)
            return false;

        // check possession during out of bounds situations
        if (nearestBallSync.isOutOfBounds.Value &&
            !ServerManager.instance.possessionTeam.Value.Equals(PlayerInfo.instance.currentTeam.Value))
            return false;

        // check throw-in restrictions
        if (nearestBallSync.isThrowIn.Value)
            return false;

        // check cooldown
        if (isInCooldown)
            return false;

        return true;
    }
    private bool ValidateKickConditionsForBicycleKick()
    {
        // check possession during out of bounds situations
        if (nearestBallSync.isOutOfBounds.Value &&
            !ServerManager.instance.possessionTeam.Value.Equals(PlayerInfo.instance.currentTeam.Value))
            return false;

        // check throw-in restrictions
        if (nearestBallSync.isThrowIn.Value)
            return false;

        // check cooldown
        if (isInCooldown)
            return false;

        return true;
    }

    private void PlayKickAnimation()
    {
        if (!PlayerMovement.instance.IsOnGround || throwInScript.isPickedUp)
            return;

        if (canPowerShot)
            playerAnimation.PlayPowerKickAnimation();

        else if (didShoot)
            playerAnimation.PlayShootAnimation();

        else if (didDribble)
            playerAnimation.PlayDribbleAnimation();
    }

    private void FinalizeKick()
    {
        // if we can still powerkick (meaning that the ball is still in our player hitbox
        if (canPowerShot)
        {
            canPowerShot = false;
            isShotBarReset = true;
            Invoke(nameof(ResetShootingBar), 1f);

            if (ServerManager.instance.didStartGame.Value)
                HandleAbilities.instance.TriggerCooldown(powerShotAbility);
            else
                HandleAbilities.instance.TriggerCooldown(powerShotAbility, 1);
        }

        // otherwise just reset
        else
            ResetShootingBar();

        if (nearestBallSync.isOutOfBounds.Value)
        {
            nearestBallSync.EndOutOfBoundsPlayServerRpc();
            hasPossession = false;
        }

        // reset movement speed
        ResetMovementSpeed();
    }

    private void ResetKickState()
    {
        if (canPowerShot)
        {
            powerShotSlider.value = 0;
            powerShotBarObj.SetActive(false);
            canPowerShot = false;

            ResetShootingBar();

            // normal cd if the game has started
            if (ServerManager.instance.didStartGame.Value)
                HandleAbilities.instance.TriggerCooldown(powerShotAbility, powerShotAbility.autoCancelCooldownDuration);

            // for practice servers or if the game hasn't started
            // no cooldown
            else
                HandleAbilities.instance.TriggerCooldown(powerShotAbility, 1);
        }
        else
            ResetShootingBar();

        ResetMovementSpeed();
    }

    private void StartCooldown(float duration)
    {
        isInCooldown = true;
        Invoke(nameof(ResetCooldown), duration);
    }

    private void ResetCooldown()
    {
        isInCooldown = false;
    }

    private void ResetMovementSpeed()
    {
        if (PlayerMovement.instance != null)
        {
            currentWalkSpeed = PlayerMovement.instance.walkSpeeds.forwardSpeed;
            currentSprintSpeed = PlayerMovement.instance.sprintSpeeds.forwardSpeed;
        }
    }

    #region Kick Implementations

    private float GetSpinCurveInput()
    {
        // the reason why we multiply 5000 with the mouse DPI is to ensure that it's the best curve setting that they can get and makes it consistent
        // same goes for the sensitivity
        // during testing those were the mouse specs i was using so we'll just make everyone use my own settings LOL

        if (joystickVal.magnitude > mobileMinCurveMagnitude)
            return joystickVal.x * mobileCurveMultiplier;

        // pc curve input
        float curve = Mouse.current.delta.ReadValue().x * (5000 / PlayerPrefs.GetFloat("MouseDPI"));
        return Mathf.Min(curve, 1500);
    }

    private void HandleDribbling()
    {
        SoundManager.instance?.PlayDribbleSoundEffect();
        PlayerMovement.instance?.ChangeStamina(dribblingStaminaLoss);

        Vector3 direction = GetPlayerDirection();

        // prevent shooting at the ground (causes weird issues)
        if (direction.y < 0)
            direction.y = 0;

        float mouseX = GetSpinCurveInput();
        float cameraLookY = Mathf.Clamp01(cam.transform.forward.y);

        float upwardInfluence;

        // mobile upward influnce logic
        if (joystickVal.magnitude > mobileMinCurveMagnitude)
        {
            float mobileVerticalInput = -Mathf.Clamp01(joystickVal.y);
            upwardInfluence = ((mobileVerticalInput * dribblingHeightMultiplier) + shootingYHeightAddition) * mobileUpwardInfluenceMultiplier;
        }

        // pc upward influnce logic
        else
            upwardInfluence = (cameraLookY * dribblingHeightMultiplier) + shootingYHeightAddition;


        float powerBoost = PlayerMovement.instance.IsSprinting ? shotMultiplier : 1f;

        CreateAndSendKick(
            dribblingMultipler,
            shotBarAmount,
            mouseX,
            direction,
            upwardInfluence,
            powerBoost
        );
    }

    private void HandleShooting()
    {
        SoundManager.instance?.PlayShootSoundEffect();
        PlayerMovement.instance?.ChangeStamina(shootingStaminaLoss);

        Ray ray = cam.ScreenPointToRay(new 
            Vector3(Screen.width / 2f, Screen.height / 2f));
        Vector3 direction = ray.direction.normalized;

        // prevent shooting at the ground (causes weird issues)
        if (direction.y < 0) 
            direction.y = 0;

        float mouseX = GetSpinCurveInput();
        float cameraLookY = Mathf.Clamp01(cam.transform.forward.y);

        float upwardInfluence;

        // mobile upward influnce logic
        if (joystickVal.magnitude > mobileMinCurveMagnitude)
        {
            float mobileVerticalInput = -Mathf.Clamp01(joystickVal.y);
            upwardInfluence = ((mobileVerticalInput * shootingHeightMultiplier) + shootingYHeightAddition) * mobileUpwardInfluenceMultiplier;
        }

        // pc upward influnce logic
        else
            upwardInfluence = (cameraLookY * shootingHeightMultiplier) + shootingYHeightAddition;

        float powerBoost = CalculateShootingPowerBoost(mouseX);

        CreateAndSendKick(
            kickingMultipler,
            shotBarAmount,
            mouseX,
            direction,
            upwardInfluence,
            powerBoost
        );
    }

    private void HandlePowerShot()
    {
        SoundManager.instance?.PlayPowerShotSoundEffect();
        PlayerMovement.instance?.ChangeStamina(shootingStaminaLoss);

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        Vector3 direction = ray.direction.normalized;

        // prevent shooting at the ground (causes weird issues)
        if (direction.y < 0)
            direction.y = 0;

        float mouseX = GetSpinCurveInput();
        float cameraLookY = Mathf.Clamp01(cam.transform.forward.y);

        float upwardInfluence;

        // mobile upward influnce logic
        if (joystickVal.magnitude > mobileMinCurveMagnitude)
        {
            float mobileVerticalInput = -Mathf.Clamp01(joystickVal.y);
            upwardInfluence = ((mobileVerticalInput * shootingHeightMultiplier) + shootingYHeightAddition) * mobileUpwardInfluenceMultiplier;
        }

        // pc upward influnce logic
        else
            upwardInfluence = (cameraLookY * shootingHeightMultiplier) + shootingYHeightAddition;

        float powerBoost = CalculatePowerShotBoost(ref upwardInfluence);

        CreateAndSendKick(
            powerBoost, 
            1f, 
            mouseX,
            direction,
            upwardInfluence,
            1f 
        );
    }

    private void HandleHeading()
    {
        SoundManager.instance?.PlayShootSoundEffect();
        PlayerMovement.instance?.ChangeStamina(headingStaminaLoss);

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        Vector3 direction = ray.direction.normalized;

        var headingPayload = new BallSync.InputPayload
        {
            Tick = NetworkManager.Singleton.ServerTime.Tick,
            Force = headingMultipler * shotBarAmount * direction,
            AngularImpulse = Vector3.zero, 
            StopBallFirst = false,
            SlideKick = false
        };

        nearestBallSync.LocalKick(headingPayload, (int)NetworkManager.LocalClientId);
    }

    private IEnumerator DetectBallDuringBicycleKick()
    {
        float startTime = Time.time;

        while (Time.time - startTime < bicycleKickDuration)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, bicycleKickSphereRadius);

            foreach (var hit in hits)
            {
                BallSync ball = hit.GetComponent<BallSync>();
                if (ball != null && ball.transform.position.y > 3)
                {
                    HandleBicycleKicking(ball);
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void HandleBicycleKicking(BallSync _nearestBallSync)
    {
        SoundManager.instance?.PlayShootSoundEffect();
        PlayerMovement.instance?.ChangeStamina(bicycleKickStaminaLoss);

        PlayerMovement.instance.DisableMovement(true);

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

        // reverse since we are bicycle kicking
        Vector3 direction = -ray.direction.normalized;

        float lift = Mathf.Lerp(bicycleKickMinLift, bicycleKickMaxLift, bicycleOnAirKickingMultipler);

        Vector3 force = (PlayerMovement.instance.IsOnGround ? bicycleOnGroundKickingMultipler : bicycleOnAirKickingMultipler) * shotBarAmount * direction + Vector3.up * lift;

        // creaate top spin
        Vector3 spinAxis = Vector3.Cross(Vector3.up, direction).normalized;
        Vector3 angularImpulse = spinAxis * bicycleKickTopSpin * bicycleOnAirKickingMultipler;

        var bicycleKickPayload = new BallSync.InputPayload
        {
            Tick = NetworkManager.Singleton.ServerTime.Tick,
            Force = force,
            AngularImpulse = angularImpulse,
            StopBallFirst = false,
            SlideKick = false
        };

        _nearestBallSync.LocalKick(bicycleKickPayload, (int)NetworkManager.LocalClientId);

        Invoke(nameof(EnableMovementAfterBicycleKick), timeBeforeEnablingMovementAfterBicycleKick);
    }

    private void EnableMovementAfterBicycleKick()
    {
        PlayerMovement.instance.DisableMovement(false);
    }

    private float CalculateShootingPowerBoost(float mouseX)
    {
        bool hasMinimalSpin = mouseX > -5 && mouseX < 5;
        float spinMultiplier = hasMinimalSpin ? nonSpinShotMultiplier : 1f;

        return shotMultiplier * spinMultiplier;
    }

    private float CalculatePowerShotBoost(ref float upwardInfluence)
    {
        float sliderVal = powerShotSlider.value;
        float perfectLowerBound = perfectTimingSliderValue - perfectTimingSliderValueWindow;
        float perfectUpperBound = perfectTimingSliderValue + perfectTimingSliderValueWindow;
        float goodLowerBound = perfectTimingSliderValue - goodTimingSliderValueWindow;
        float goodUpperBound = perfectTimingSliderValue + goodTimingSliderValueWindow;

        if (sliderVal >= perfectLowerBound && sliderVal <= perfectUpperBound)
        {
            return powerShotMultiplier;
        }
        else if (sliderVal >= goodLowerBound && sliderVal <= goodUpperBound)
        {
            upwardInfluence *= Random.Range(1.15f, 1.25f);
            return powerShotMultiplier * Random.Range(0.6f, 0.8f);
        }
        else
        {
            upwardInfluence *= 1 + powerShotSlider.value;
            return powerShotMultiplier * Random.Range(0.15f, 0.4f);
        }
    }

    private void CreateAndSendKick(float power, float sliderValue, float mouseX, Vector3 direction, float upwardInfluence, float powerBoost)
    {
        joystickVal = Vector2.zero;

        if (ServerManager.instance.isStartingGame.Value)
            return;

        // calculate base force
        Vector3 force = power * powerBoost * sliderValue * direction;

        // add height control
        Vector3 heightControlForce = kickingMultipler * sliderValue * upwardInfluence * transform.up;
        force += heightControlForce;

        // calculate spin
        Vector3 angularImpulse = Vector3.zero;
        bool hasSpin = Mathf.Abs(mouseX) > minimumMagnusMouseThreshold;

        if (hasSpin)
        {
            Vector3 localSpinAxis = new Vector3(topSpinMultiplier, mouseX * 0.5f, 0f).normalized;
            Vector3 worldSpinAxis = transform.TransformDirection(localSpinAxis);
            angularImpulse = sideSpinMultiplier * sliderValue * worldSpinAxis;

            Debug.Log(angularImpulse);
        }

        var kickPayload = new BallSync.InputPayload
        {
            Tick = NetworkManager.Singleton.ServerTime.Tick,
            Force = force,
            AngularImpulse = angularImpulse,
            StopBallFirst = true,
            SlideKick = false
        };

        nearestBallSync.LocalKick(kickPayload, (int)NetworkManager.LocalClientId);
    }

    #endregion

    #region UI and Visual Feedback

    private void HandlePowerShotSliderColor()
    {
        float sliderVal = powerShotSlider.value;
        float perfectLowerBound = perfectTimingSliderValue - perfectTimingSliderValueWindow;
        float perfectUpperBound = perfectTimingSliderValue + perfectTimingSliderValueWindow;
        float goodLowerBound = perfectTimingSliderValue - goodTimingSliderValueWindow;
        float goodUpperBound = perfectTimingSliderValue + goodTimingSliderValueWindow;

        if (sliderVal >= perfectLowerBound && sliderVal <= perfectUpperBound)
            powerShotBarFill.color = new Color(0, 1, 0.07011509f, 1); // green
        else if (sliderVal >= goodLowerBound && sliderVal <= goodUpperBound)
            powerShotBarFill.color = new Color(0.9702021f, 1, 0.259434f, 1); // yellow-green
        else
            powerShotBarFill.color = new Color(1, 0, 0.07710934f, 1); // red
    }

    private void ChangeShootingBarColor()
    {
        // bicycle kicking
        if (didBicycleKick)
            barFill.color = new Color(0.9702021f, 1, 0.259434f, 1); // yellow-green

        // dribbling
        else if (didDribble)
            barFill.color = new Color(0.06132078f, 0.7101388f, 1, 1); // blue
        
        // shooting
        else
            barFill.color = new Color(1, 0, 0.07710934f, 1); // red
    }

    public void ResetShootingBar()
    {
        // reset power shot UI
        powerShotBarFill.color = new Color(1, 0, 0.07710934f, 1);
        powerShotSlider.value = 0;
        powerShotBarObj.SetActive(false);

        // reset normal shooting UI
        shootingBarSlider.value = 0;
        shootingBarSlider.gameObject.SetActive(false);

        // reset flags
        didDribble = false;
        didShoot = false;
        didBicycleKick = false;
        isShotBarReset = false;
    }

    #endregion

    public void DeleteLocalSpawnedBall()
    {
        localSpawnedBall = null;
    }

    public bool IsLocalBallSpawned() => localSpawnedBall != null;

    #region Mobile UI Settings

    public void SetModeToShooting()
    {
        currentMobileKickMode = KickMode.Shooting;
    }

    public void SetModeToDribbling()
    {
        currentMobileKickMode = KickMode.Dribbling;
    }

    public void SetModeToBicycleKick()
    {
        currentMobileKickMode = KickMode.BicycleKick;
    }

    #endregion

    #region Utility Methods

    private Vector3 GetPlayerDirection()
    {
        Vector3 direction = Vector3.zero;

        if (player.IsWalkingForward) 
            direction += Vector3.forward;

        if (player.IsWalkingBackwards) 
            direction += Vector3.back;

        if (player.IsStrafingLeft) 
            direction += Vector3.left;

        if (player.IsStrafingRight) 
            direction += Vector3.right;

        // if we're not moving, then just get the forward vector of the camera
        if (direction == Vector3.zero)
            return cam.transform.forward; 

        direction.Normalize();
        return player.transform.TransformDirection(direction);
    }

    public void SetCanKickOrHead(bool isKick, bool condition)
    {
        if (isKick)
            canKick = condition;
        else
            canHead = condition;
    }

    public void EnablePowerShot(float powerShotIncreaseMultiplier, Powershot powerShotAbility, float durationBeforeAutomaticDisable)
    {
        canPowerShot = true;
        powerShotMultiplier = powerShotIncreaseMultiplier;
        this.powerShotAbility = powerShotAbility;

        if (!ServerManager.instance.isTutorialServer && !ServerManager.instance.isPracticeServer && !ServerManager.instance.didStartGame.Value)
            Invoke(nameof(DisablePowerShotAutomatically), durationBeforeAutomaticDisable);
    }

    public void DisablePowerShotAutomatically()
    {
        if (canPowerShot)
        {
            canPowerShot = false;
            canKick = false;
            ResetShootingBar();

            // normal cd if the game has started
            if (ServerManager.instance.didStartGame.Value)
                HandleAbilities.instance.TriggerCooldown(powerShotAbility, powerShotAbility.autoCancelCooldownDuration);

            // for practice servers or if the game hasn't started
            // no cooldown
            else
                HandleAbilities.instance.TriggerCooldown(powerShotAbility, 1);
        }
    }

    #endregion
}