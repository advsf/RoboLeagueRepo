using UnityEngine;
using Unity.Netcode;

public class HandleSpectatingMovement : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float flySpeed = 12f;
    [SerializeField] private float slowFlySpeed = 5f;

    [Header("References")]
    [SerializeField] private Rigidbody rb;

    private Vector2 inputDirection;
    private Vector3 moveDirection;

    private float desiredFlySpeed;

    private void OnEnable()
    {
        if (!IsOwner)
            return;

        PlayerInputReference.instance.controls.Gameplay.Move.Enable();
        PlayerInputReference.instance.controls.Gameplay.Sprint.Enable();
        PlayerInputReference.instance.controls.Gameplay.Jump.Enable();
        PlayerInputReference.instance.controls.Gameplay.Slide.Enable();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        GatherInput();
        ControlSpeed();
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        MoveSpectator();
        HandleCountermovement();
    }

    private void GatherInput()
    {
        // calculate the move direction based on the user input
        inputDirection = PlayerInputReference.instance.controls.Gameplay.Move.ReadValue<Vector2>();
    }

    private void ControlSpeed()
    {
        desiredFlySpeed = PlayerInputReference.instance.controls.Gameplay.Sprint.ReadValue<float>() > 0 ? flySpeed : slowFlySpeed;
    }

    private void MoveSpectator()
    {
        // multiply the desired speed by 10 so that we can handle countermovement more effectively

        // move
        moveDirection = transform.forward * inputDirection.y + transform.right * inputDirection.x;
        rb.AddForce(10 * desiredFlySpeed * moveDirection.normalized, ForceMode.Force);

        // move upwards
        if (PlayerInputReference.instance.controls.Gameplay.Jump.IsPressed())
            rb.AddForce(10 * desiredFlySpeed * Vector3.up, ForceMode.Force);

        // move downwards
        if (PlayerInputReference.instance.controls.Gameplay.Slide.IsPressed())
            rb.AddForce(10 * desiredFlySpeed * Vector3.down, ForceMode.Force);

        // stop moving if there's no more inputs
        if (inputDirection == Vector2.zero)
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new(0f, 0f, 0f), Time.deltaTime * 10f);
    }

    private void HandleCountermovement()
    {
        Vector3 flatVel = new(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > desiredFlySpeed)
        {
            Vector3 limitedVel = flatVel.normalized * desiredFlySpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, limitedVel.y, limitedVel.z);
        }
    }
}

