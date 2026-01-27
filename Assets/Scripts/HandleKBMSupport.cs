using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;



public class HandleKBMSupport : MonoBehaviour
{
    public static HandleKBMSupport instance;
    public static event Action OnInputChanged;

    public bool IsUsingKBM;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        InputSystem.onActionChange += OnActionChange;

        CheckInitialDevices();
    }

    private void OnDestroy()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void CheckInitialDevices()
    {
        if (Keyboard.current != null || Mouse.current != null)
            IsUsingKBM = true;
    }
    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            if (obj is not InputAction action || action.activeControl == null) return;

            InputControl control = action.activeControl;
            InputDevice device = control.device;

            // ignore inputs that are essentally zero
            // since when we disable the mobile UI with the onscreen button or stick
            // they become zero
            if (!control.IsActuated())
                return;

            bool shouldSwitchToKBM = false;
            bool shouldSwitchToTouch = false;

            if (device is Keyboard)
            {
                shouldSwitchToKBM = true;
            }

            else if (device is Mouse)
            {
                //
                bool isButtonInput = control is ButtonControl;
                bool isButtonHeld = Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed;

                if (isButtonInput || isButtonHeld)
                {
                    shouldSwitchToKBM = true;
                }
            }

            else if (device is Touchscreen || device is Gamepad)
            {
                shouldSwitchToTouch = true;
            }

            if (shouldSwitchToKBM && !IsUsingKBM)
            {
                IsUsingKBM = true;

                // invoke the event to change UI and other kbm related things
                OnInputChanged?.Invoke();
            }

            else if (shouldSwitchToTouch && IsUsingKBM)
            {
                IsUsingKBM = false;

                // invoke the event to change UI and other kbm related things
                OnInputChanged?.Invoke();
            }
        }
    }
}
