using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

public class BallSync : NetworkBehaviour
{
    private Rigidbody ballRb;
    private NetworkObject _networkObject;

    private struct StateSnapshot : INetworkSerializable
    {
        public int Tick;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public float KickNetworkTime;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref AngularVelocity);
            serializer.SerializeValue(ref KickNetworkTime);
        }
    }

    public struct InputPayload : INetworkSerializable
    {
        public int Tick;
        public Vector3 Force;
        public Vector3 AngularImpulse;
        public bool StopBallFirst;
        public bool SlideKick;

        public Vector3 ClientBallPosition;
        public Vector3 ClientBallVelocity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Force);
            serializer.SerializeValue(ref AngularImpulse);
            serializer.SerializeValue(ref StopBallFirst);
            serializer.SerializeValue(ref SlideKick);
            serializer.SerializeValue(ref ClientBallPosition);
            serializer.SerializeValue(ref ClientBallVelocity);
        }
    }

    private struct ContestedKickRequest
    {
        public InputPayload Payload;
        public ulong SenderId;
    }

    private List<ContestedKickRequest> _contestedKicks;

    [Header("Visual Smoothing")]
    [SerializeField] private Transform visualBallTransform;
    private Vector3 _visualVelocity;
    private Quaternion _visualAngularVelocity;

    [Header("Interpolation Settings")]
    [SerializeField] private float interpolationTime = 0.045f;
    [SerializeField] private float ballInAnimationInterpolationTime = 0.045f;

    [Header("Ball Physics")]
    [SerializeField] private float magnusForceMultiplier = 0.05f;
    [SerializeField] private float downForceMultiplier = 1.0f;
    [SerializeField] private float magnusEffectDuration = 2.0f;
    [SerializeField] private float magnusDecayRate = 0.98f;
    private float _kickTime = -1f;

    [Header("Server Validation")]
    [SerializeField] private SphereCollider ballCollider;

    [Header("Server-Side Arbitration - Force Summation")]
    [SerializeField] private float kickAccumulationWindow = 0.1f;
    private int _lastProcessedTick = -1;

    // prevent lag switching (dirty little cheaters)
    private const double MAX_ACCEPTABLE_LAG = 0.800;

    [Header("Client Prediction - Ping Adaptive")]
    private bool _hasLocalPrediction = false;
    private float _localPredictionStartTime = -1f;
    [SerializeField] private float basePredictionBlendDuration = 1.5f; // Base blend duration at target ping
    [SerializeField] private AnimationCurve predictionBlendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Base correction parameters (tuned for target ping)
    [SerializeField] private float baseSmallErrorThreshold = 0.5f;
    [SerializeField] private float baseLargeErrorThreshold = 3.0f;
    [SerializeField] private float baseSmallErrorCorrectionSpeed = 0.12f;
    [SerializeField] private float baseLargeErrorCorrectionSpeed = 0.5f; // Reduced from 0.8

    // Velocity correction (separate from position - should remain accurate)
    [SerializeField] private float baseVelocityCorrectionSpeed = 0.35f; // Higher than position correction
    [SerializeField] private float velocitySnapThreshold = 0.15f; // Below this error, snap velocity immediately

    // Ping adaptation settings
    [SerializeField] private float targetPing = 90f; // Ping at which base parameters are tuned
    [SerializeField] private float maxPingForAdaptation = 500f;
    [SerializeField] private float highPingPositionDamping = 0.4f; // Reduce POSITION correction at high ping
    [SerializeField] private float highPingVelocityDamping = 0.8f; // Keep velocity corrections much stronger
    [SerializeField] private float pingErrorThresholdScale = 1.5f; // Scale error thresholds with ping
    [SerializeField] private float rttSmoothingFactor = 0.1f; // Smooth RTT changes

    private float _currentRTT = 0f;
    private float _smoothedRTT = 90f;

    // Adaptive thresholds and speeds (calculated each frame)
    private float _adaptiveSmallErrorThreshold;
    private float _adaptiveLargeErrorThreshold;
    private float _adaptiveSmallCorrectionSpeed;
    private float _adaptiveLargeCorrectionSpeed;
    private float _adaptiveVelocityCorrectionSpeed;
    private float _adaptiveBlendDuration;

    private NetworkVariable<StateSnapshot> _serverState = new NetworkVariable<StateSnapshot>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isBallPickedUp = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isIndirectFreekick = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isOutOfPlay = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isThrowIn = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isCornerKick = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isGoalKick = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isOffside = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> lastKickedClientId = new(-1);
    public NetworkVariable<int> secondLastKickedClientId = new(-1);
    public NetworkVariable<FixedString32Bytes> lastKickedTeam = new(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<FixedString64Bytes> scorerUsername = new(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString64Bytes> assisterUsername = new(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool isKickedFromIndirectKick = false;

    private float ballCurveMultiplier;

    public override void OnNetworkSpawn()
    {
        ballRb = GetComponent<Rigidbody>();
        _networkObject = GetComponent<NetworkObject>();

        BallManager.instance.RegisterBall(this);

        if (!IsServer)
        {
            _serverState.OnValueChanged += OnServerStateChanged;
        }
        else
        {
            _contestedKicks = new List<ContestedKickRequest>();
        }

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        BallManager.instance.UnregisterBall(this);

        if (!IsServer)
            _serverState.OnValueChanged -= OnServerStateChanged;

        base.OnNetworkDespawn();
    }

    private void Start()
    {
        if (!IsServer)
        {
            // client simulates its own physics for prediction
            ballRb.useGravity = true;
            // Client needs curve multiplier for local Magnus effect prediction
            ballCurveMultiplier = ServerManager.instance.ballCurveMultiplier.Value;
        }
        else
        {
            ballCurveMultiplier = ServerManager.instance.ballCurveMultiplier.Value;
        }
    }

    private void LateUpdate()
    {
        // no interpolation when the ball is kinematic or out of bounds (mainly for the throw in and goalkeeper animation)
        if (isOutOfPlay.Value || ballRb.isKinematic)
        {
            visualBallTransform.SetPositionAndRotation(transform.position, transform.rotation);
            _visualVelocity = Vector3.zero;
            return;
        }

        float t = Time.deltaTime / interpolationTime;
        visualBallTransform.position = Vector3.Lerp(visualBallTransform.position, transform.position, t);

        visualBallTransform.rotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (!IsSpawned)
            return;

        // server simulates physics
        if (IsServer)
        {
            ProcessContestedKicks();

            if (!ballRb.isKinematic)
            {
                HandleSpinDecay();
                ApplySharedPhysics();
                _serverState.Value = GetCurrentState();
            }
        }
        // client simulates its own physics when it has prediction active
        else if (_hasLocalPrediction && !ballRb.isKinematic)
        {
            HandleSpinDecay();
            ApplySharedPhysics();
        }

        // Update ping-adaptive parameters on clients
        if (!IsServer)
        {
            UpdatePingAdaptiveParameters();
        }
    }

    public void LocalKick(InputPayload kickPayload, int kickerId, bool countOffside)
    {
        // if we can't slide kick
        if (kickPayload.SlideKick && !CanSlideKick())
            return;

        double estimatedServerTime = NetworkManager.ServerTime.Time;

        // populate client's ball state for server validation
        kickPayload.ClientBallPosition = ballRb.position;
        kickPayload.ClientBallVelocity = ballRb.linearVelocity;

        // handle offsides
        if (!ServerManager.instance.isPracticeServer && !ServerManager.instance.isTutorialServer && !ServerManager.instance.isInHalftime.Value && !ServerManager.instance.didStartGame.Value && countOffside)
        {
            // if someone is offside
            if (HandleOffsides.instance.IsPlayerOffside(kickerId))
            {
                HandleOffsides.instance.StartIndirectionKickServerRpc(ballRb.position, PlayerInfo.instance.currentTeam.Value.ToString().Equals("Blue") ? "Red" : "Blue");
                return;
            }

            // start a new detection
            else
                HandleOffsides.instance.CheckForOffsidesServerRpc(kickerId);
        }

        // reset all potential offsides
        else if ((!ServerManager.instance.isPracticeServer && !ServerManager.instance.isTutorialServer && !ServerManager.instance.isInHalftime.Value && !ServerManager.instance.didStartGame.Value) || !countOffside)
            HandleOffsides.instance.ClearPotentialOffsidesIdListClientRpc();

        // CLIENT-SIDE PREDICTION: Apply kick locally immediately
        if (!IsServer)
        {
            ApplyKickMechanicsLocally(kickPayload);
            _hasLocalPrediction = true;
            _localPredictionStartTime = Time.time;
        }

        if (IsServer)
            HandleKickRequestServerSide(kickPayload, (ulong)kickerId, estimatedServerTime);
        else
            ApplyKickServerRpc(kickPayload, estimatedServerTime);

        if (!ServerManager.instance.didKickOffEnd.Value)
            ServerManager.instance.DisableKickOffBarriers();
    }

    public bool ShouldOffsideCountWhenKicking()
    {
        return !isOffside.Value || !isGoalKick.Value || !isThrowIn.Value || !isCornerKick.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyKickServerRpc(InputPayload input, double clientTimeStamp, ServerRpcParams rpcParams = default)
    {
        HandleKickRequestServerSide(input, rpcParams.Receive.SenderClientId, clientTimeStamp);
    }

    private void HandleKickRequestServerSide(InputPayload payload, ulong clientId, double clientTimeStamp)
    {
        // a very simple server arbitration system
        int currentServerTick = NetworkManager.ServerTime.Tick;

        // reject future ticks
        if (payload.Tick > currentServerTick + 2)
            return;

        // if another player kicks it after an indirect kick
        if (isKickedFromIndirectKick)
            isKickedFromIndirectKick = false;

        _contestedKicks.Add(new ContestedKickRequest
        {
            Payload = payload,
            SenderId = clientId
        });
    }

    #region State Synchronization

    private void UpdatePingAdaptiveParameters()
    {
        // Get current RTT in milliseconds
        // Calculate round-trip time by comparing local time to server time
        double localTime = NetworkManager.Singleton.LocalTime.Time;
        double serverTime = NetworkManager.Singleton.ServerTime.Time;
        _currentRTT = (float)((localTime - serverTime) * 2000f); // *2 for round trip, *1000 for milliseconds

        // Smooth the RTT to avoid jitter
        _smoothedRTT = Mathf.Lerp(_smoothedRTT, _currentRTT, rttSmoothingFactor);

        // Clamp to reasonable values
        _smoothedRTT = Mathf.Clamp(_smoothedRTT, 0f, maxPingForAdaptation);

        // Calculate ping factor (1.0 at target ping, higher at higher ping)
        float pingFactor = Mathf.Clamp01(_smoothedRTT / targetPing);

        // Scale error thresholds with ping - allow larger errors at high ping
        float errorScale = 1f + (pingFactor * pingErrorThresholdScale);
        _adaptiveSmallErrorThreshold = baseSmallErrorThreshold * errorScale;
        _adaptiveLargeErrorThreshold = baseLargeErrorThreshold * errorScale;

        // Reduce POSITION correction aggressiveness at high ping to prevent rubber banding
        float positionDamping = Mathf.Lerp(1f, highPingPositionDamping, Mathf.Pow(pingFactor, 2f));
        _adaptiveSmallCorrectionSpeed = baseSmallErrorCorrectionSpeed * positionDamping;
        _adaptiveLargeCorrectionSpeed = baseLargeErrorCorrectionSpeed * positionDamping;

        // Keep VELOCITY corrections stronger - ball should maintain natural motion
        float velocityDamping = Mathf.Lerp(1f, highPingVelocityDamping, Mathf.Pow(pingFactor, 1.5f));
        _adaptiveVelocityCorrectionSpeed = baseVelocityCorrectionSpeed * velocityDamping;

        // Extend blend duration at high ping to give more time for smooth convergence
        _adaptiveBlendDuration = basePredictionBlendDuration * (1f + pingFactor * 0.5f);
    }

    private void OnServerStateChanged(StateSnapshot previous, StateSnapshot serverState)
    {
        if (ballRb.isKinematic)
            return;

        // Smooth blending system with adaptive correction based on error magnitude AND ping
        if (_hasLocalPrediction)
        {
            float timeSincePrediction = Time.time - _localPredictionStartTime;

            // Calculate blend factor using adaptive duration
            float blendFactor = Mathf.Clamp01(timeSincePrediction / _adaptiveBlendDuration);
            blendFactor = predictionBlendCurve.Evaluate(blendFactor);

            // Gently guide the client toward server state
            if (blendFactor < 1.0f)
            {
                // Calculate the difference between client and server
                Vector3 positionError = serverState.Position - ballRb.position;
                Vector3 _velocityError = serverState.Velocity - ballRb.linearVelocity;

                float positionErrorMagnitude = positionError.magnitude;
                float velocityErrorMagnitude = _velocityError.magnitude;

                // Use adaptive correction speeds (ping-aware)
                float positionCorrectionSpeed = GetAdaptiveCorrectionSpeed(positionErrorMagnitude);

                // Apply corrections with adaptive speed and blend factor
                // At high ping, POSITION corrections are less aggressive, reducing rubber banding
                if (positionErrorMagnitude > 0.01f)
                {
                    // Use a more gentle correction formula at high ping
                    float correctionAmount = positionCorrectionSpeed * blendFactor * Time.fixedDeltaTime * 50f;
                    ballRb.position = Vector3.Lerp(ballRb.position, serverState.Position, correctionAmount);
                }

                // VELOCITY correction uses separate logic - keep ball moving naturally
                if (velocityErrorMagnitude > velocitySnapThreshold)
                {
                    // Use stronger velocity correction to maintain natural ball motion
                    float velocityCorrectionAmount = _adaptiveVelocityCorrectionSpeed * blendFactor * Time.fixedDeltaTime * 60f;
                    ballRb.linearVelocity = Vector3.Lerp(ballRb.linearVelocity, serverState.Velocity, velocityCorrectionAmount);
                }
                else if (velocityErrorMagnitude > 0.01f)
                {
                    // Small velocity errors - snap immediately to prevent slow-down
                    ballRb.linearVelocity = serverState.Velocity;
                }

                // Smoothly blend rotation and angular velocity with ping awareness
                float rotationBlendSpeed = Mathf.Lerp(0.05f, 0.4f, blendFactor) * (_adaptiveSmallCorrectionSpeed / baseSmallErrorCorrectionSpeed);
                ballRb.rotation = Quaternion.Slerp(ballRb.rotation, serverState.Rotation, rotationBlendSpeed);
                ballRb.angularVelocity = Vector3.Lerp(ballRb.angularVelocity, serverState.AngularVelocity, rotationBlendSpeed * 0.5f);

                // Update kick time with blend
                _kickTime = Mathf.Lerp(_kickTime, serverState.KickNetworkTime, blendFactor * 0.5f);

                return;
            }

            // Blend complete, disable prediction
            _hasLocalPrediction = false;
        }

        // Full server authority (no prediction active)
        // Still use gentle corrections to avoid snapping
        float snapCorrectionSpeed = 0.6f * (_adaptiveSmallCorrectionSpeed / baseSmallErrorCorrectionSpeed);
        ballRb.position = Vector3.Lerp(ballRb.position, serverState.Position, snapCorrectionSpeed);
        ballRb.rotation = Quaternion.Slerp(ballRb.rotation, serverState.Rotation, snapCorrectionSpeed);

        // Velocity should be more accurate - use stronger correction to prevent slow-down
        Vector3 velocityError = serverState.Velocity - ballRb.linearVelocity;
        if (velocityError.magnitude < velocitySnapThreshold)
        {
            // Small error - snap immediately
            ballRb.linearVelocity = serverState.Velocity;
        }
        else
        {
            // Larger error - use stronger correction than position
            float velocitySnapSpeed = Mathf.Max(snapCorrectionSpeed * 1.5f, 0.3f);
            ballRb.linearVelocity = Vector3.Lerp(ballRb.linearVelocity, serverState.Velocity, velocitySnapSpeed);
        }

        ballRb.angularVelocity = Vector3.Lerp(ballRb.angularVelocity, serverState.AngularVelocity, snapCorrectionSpeed * 0.7f);
        _kickTime = serverState.KickNetworkTime;
    }

    private float GetAdaptiveCorrectionSpeed(float errorMagnitude)
    {
        // Use adaptive thresholds that scale with ping
        if (errorMagnitude < _adaptiveSmallErrorThreshold)
            return _adaptiveSmallCorrectionSpeed;

        if (errorMagnitude > _adaptiveLargeErrorThreshold)
            return _adaptiveLargeCorrectionSpeed;

        // Medium errors: interpolate between gentle and aggressive
        float t = (errorMagnitude - _adaptiveSmallErrorThreshold) / (_adaptiveLargeErrorThreshold - _adaptiveSmallErrorThreshold);
        return Mathf.Lerp(_adaptiveSmallCorrectionSpeed, _adaptiveLargeCorrectionSpeed, t);
    }

    #endregion

    #region Physics Networking

    private void HandleSpinDecay()
    {
        if (_kickTime <= 0f)
            return;

        float currentTime = (float)NetworkManager.ServerTime.Time;

        if (currentTime > _kickTime + magnusEffectDuration)
        {
            ballRb.angularVelocity *= Mathf.Pow(magnusDecayRate, Time.fixedDeltaTime);

            if (ballRb.angularVelocity.sqrMagnitude < 0.01f)
            {
                ballRb.angularVelocity = Vector3.zero;
                _kickTime = -1f;
            }
        }
    }

    private void ApplyKickMechanicsLocally(InputPayload input)
    {
        if (input.SlideKick && !CanSlideKick())
            return;

        if (input.StopBallFirst)
            StopBallVelocity();

        ApplyKickForces(input);

        if (input.AngularImpulse.sqrMagnitude > 0)
            _kickTime = (float)NetworkManager.ServerTime.Time;
    }

    private void ApplyKickMechanics(InputPayload input, ulong clientId)
    {
        if (input.SlideKick && !CanSlideKick())
            return;

        if (input.StopBallFirst)
            StopBallVelocity();

        ApplyKickForces(input);
        HandleTrackingKickers((int)clientId);

        if (input.AngularImpulse.sqrMagnitude > 0)
            _kickTime = (float)NetworkManager.ServerTime.Time;
    }

    private void ApplyKickForces(InputPayload input)
    {
        ballRb.AddForce(input.Force, ForceMode.Impulse);
        ballRb.AddTorque(input.AngularImpulse, ForceMode.Impulse);
    }

    private void ApplySharedPhysics()
    {
        if (ballRb.linearVelocity.sqrMagnitude > 1f && ballRb.angularVelocity.sqrMagnitude > 1f)
        {
            // Both client and server apply Magnus effect with the same multiplier
            Vector3 magnusForce = magnusForceMultiplier * ballCurveMultiplier * Vector3.Cross(ballRb.angularVelocity, ballRb.linearVelocity);
            ballRb.AddForce(magnusForce, ForceMode.Force);
        }

        ballRb.AddForce(downForceMultiplier * Vector3.down, ForceMode.Force);
    }

    private void StopBallVelocity()
    {
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
    }

    private void ProcessContestedKicks()
    {
        if (_contestedKicks.Count == 0)
            return;

        // Sort by tick to process in chronological order
        _contestedKicks.Sort((a, b) => a.Payload.Tick.CompareTo(b.Payload.Tick));

        // FORCE SUMMATION SYSTEM:
        // Find all kicks within the accumulation window and sum their forces
        int earliestTick = _contestedKicks[0].Payload.Tick;
        int accumulationWindowTicks = Mathf.CeilToInt(kickAccumulationWindow * NetworkManager.ServerTime.TickRate);

        // Only process kicks if we haven't processed this tick range yet
        if (_lastProcessedTick >= earliestTick)
        {
            _contestedKicks.Clear();
            return;
        }

        Vector3 totalForce = Vector3.zero;
        Vector3 totalTorque = Vector3.zero;
        bool anyStopBall = false;
        ulong lastKickerId = 0;
        int processedCount = 0;

        // Accumulate all forces from kicks in the window
        foreach (var req in _contestedKicks)
        {
            int tickDelta = req.Payload.Tick - earliestTick;

            // Include all kicks within the accumulation window
            if (tickDelta <= accumulationWindowTicks)
            {
                totalForce += req.Payload.Force;
                totalTorque += req.Payload.AngularImpulse;

                if (req.Payload.StopBallFirst)
                    anyStopBall = true;

                lastKickerId = req.SenderId;
                processedCount++;
            }
        }

        _contestedKicks.Clear();

        // Apply the summed forces if any kicks were accumulated
        if (processedCount > 0)
        {
            _lastProcessedTick = earliestTick + accumulationWindowTicks;

            // Stop ball if any kick requested it
            if (anyStopBall)
                StopBallVelocity();

            // Apply summed forces
            ballRb.AddForce(totalForce, ForceMode.Impulse);
            ballRb.AddTorque(totalTorque, ForceMode.Impulse);

            // Track the last kicker for game logic
            HandleTrackingKickers((int)lastKickerId);

            // Update kick time if there was any torque
            if (totalTorque.sqrMagnitude > 0)
                _kickTime = (float)NetworkManager.ServerTime.Time;
        }
    }

    private void HandleTrackingKickers(int clientId)
    {
        if (clientId < -1)
        {
            if (clientId == -2)
                lastKickedClientId.Value = -1;
            secondLastKickedClientId.Value = -1;
        }
        else
        {
            if (lastKickedClientId.Value != clientId)
                secondLastKickedClientId.Value = lastKickedClientId.Value;
            lastKickedClientId.Value = clientId;
            lastKickedTeam.Value = NetworkManager.ConnectedClients[(ulong)clientId].PlayerObject.GetComponent<PlayerInfo>().currentTeam.Value;
        }
    }

    private bool CanSlideKick() => !isOutOfPlay.Value;

    public void Teleport(Vector3 newPosition, Quaternion newRotation)
    {
        StopBallVelocity();

        visualBallTransform.SetPositionAndRotation(newPosition, newRotation);

        transform.SetPositionAndRotation(newPosition, newRotation);
        ballRb.position = newPosition;
        ballRb.rotation = newRotation;

        // Clear prediction on teleport
        _hasLocalPrediction = false;
    }

    private StateSnapshot GetCurrentState()
    {
        return new StateSnapshot
        {
            Tick = NetworkManager.ServerTime.Tick,
            Position = ballRb.position,
            Rotation = ballRb.rotation,
            Velocity = ballRb.linearVelocity,
            AngularVelocity = ballRb.angularVelocity,
            KickNetworkTime = _kickTime
        };
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyKickServerRpc(InputPayload input, ServerRpcParams rpcParams = default)
    {
        _contestedKicks.Add(new ContestedKickRequest
        {
            Payload = input,
            SenderId = rpcParams.Receive.SenderClientId
        });
    }

    public void StopBall()
    {
        StopBallVelocity();

        // Clear prediction when manually stopping
        _hasLocalPrediction = false;

        if (IsServer)
            StopBallClientRpc();
        else
            StopBallServerRpc();
    }

    public void EnableKinematics(bool condition)
    {
        ballRb.isKinematic = condition;

        // Clear prediction when kinematics change
        if (condition)
            _hasLocalPrediction = false;

        if (IsServer)
            EnableKinematicsClientRpc(condition);
        else
            EnableKinematicsServerRpc(condition);
    }

    public void EnableCollider(bool condition)
    {
        ballCollider.enabled = condition;

        if (IsServer)
            EnableColliderClientRpc(condition);
        else
            EnableColliderServerRpc(condition);
    }

    public Rigidbody GetRigidbody() => ballRb;


    [ServerRpc(RequireOwnership = false)]
    public void ResetBallServerRpc(Vector3 newPosition)
    {
        Teleport(newPosition, Quaternion.identity);
        SendBallResetToClientsClientRpc(newPosition);
    }

    [ClientRpc]
    private void SendBallResetToClientsClientRpc(Vector3 newPosition)
    {
        if (!IsServer) Teleport(newPosition, Quaternion.identity);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopBallServerRpc()
    {
        StopBallClientRpc();
    }

    [ClientRpc]
    private void StopBallClientRpc()
    {
        StopBallVelocity();
        _hasLocalPrediction = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnableKinematicsServerRpc(bool condition)
    {
        EnableKinematicsClientRpc(condition);
    }

    [ClientRpc]
    private void EnableKinematicsClientRpc(bool condition)
    {
        ballRb.isKinematic = condition;
        if (condition)
            _hasLocalPrediction = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnableColliderServerRpc(bool condition)
    {
        EnableColliderClientRpc(condition);
    }

    [ClientRpc]
    private void EnableColliderClientRpc(bool condition)
    {
        if (!IsServer)
            ballCollider.enabled = condition;
    }

    #endregion

    #region Out of Play Handling

    [ServerRpc(RequireOwnership = false)]
    public void EndBallOutOfPlayServerRpc()
    {
        if (isIndirectFreekick.Value)
            isKickedFromIndirectKick = true;

        isOutOfPlay.Value = false;
        isThrowIn.Value = false;
        isCornerKick.Value = false;
        isBallPickedUp.Value = false;
        isGoalKick.Value = false;
        isIndirectFreekick.Value = false;
        isOffside.Value = false;

        ServerManager.instance.isBallOutOfBounds = false;
        ServerManager.instance.DisableGoalkickBoundaries();
        ServerManager.instance.SetAllDetectorsToInactive();
        ServerManager.instance.ResetOutOfBoundsTimer();
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndOutOfPlayServerRpc(float delay)
    {
        isBallPickedUp.Value = false;
        Invoke(nameof(EndOutOfPlay), delay);
    }

    private void EndOutOfPlay()
    {
        if (isIndirectFreekick.Value)
            isKickedFromIndirectKick = true;

        isOutOfPlay.Value = false;
        isThrowIn.Value = false;
        isCornerKick.Value = false;
        isBallPickedUp.Value = false;
        isGoalKick.Value = false;
        isIndirectFreekick.Value = false;
        isOffside.Value = false;

        ServerManager.instance.isBallOutOfBounds = false;
        ServerManager.instance.SetAllDetectorsToInactive();
        ServerManager.instance.ResetOutOfBoundsTimer();
    }

    #endregion
}