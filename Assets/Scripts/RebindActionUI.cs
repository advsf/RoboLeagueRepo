using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// <summary>
/// A reusable component with a self-contained UI for rebinding a single action.
/// Modified to work with Generated C# Classes via PlayerInputReference singleton.
/// </summary>
public class RebindActionUI : MonoBehaviour
{
    /// <summary>
    /// Reference to the action that is to be rebound.
    /// NOTE: When using generated classes, this is used strictly to get the Name and Map Name
    /// to look up the actual runtime action in PlayerInputReference.
    /// </summary>
    public InputActionReference actionReference
    {
        get => m_Action;
        set
        {
            m_Action = value;
            UpdateActionLabel();
            UpdateBindingDisplay();
        }
    }

    /// <summary>
    /// ID (in string form) of the binding that is to be rebound on the action.
    /// </summary>
    public string bindingId
    {
        get => m_BindingId;
        set
        {
            m_BindingId = value;
            UpdateBindingDisplay();
        }
    }

    public InputBinding.DisplayStringOptions displayStringOptions
    {
        get => m_DisplayStringOptions;
        set
        {
            m_DisplayStringOptions = value;
            UpdateBindingDisplay();
        }
    }

    public TMPro.TextMeshProUGUI actionLabel
    {
        get => m_ActionLabel;
        set
        {
            m_ActionLabel = value;
            UpdateActionLabel();
        }
    }

    public TMPro.TextMeshProUGUI bindingText
    {
        get => m_BindingText;
        set
        {
            m_BindingText = value;
            UpdateBindingDisplay();
        }
    }

    public TMPro.TextMeshProUGUI rebindPrompt
    {
        get => m_RebindText;
        set => m_RebindText = value;
    }

    public GameObject rebindOverlay
    {
        get => m_RebindOverlay;
        set => m_RebindOverlay = value;
    }

    public UpdateBindingUIEvent updateBindingUIEvent
    {
        get
        {
            if (m_UpdateBindingUIEvent == null)
                m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
            return m_UpdateBindingUIEvent;
        }
    }

    public InteractiveRebindEvent startRebindEvent
    {
        get
        {
            if (m_RebindStartEvent == null)
                m_RebindStartEvent = new InteractiveRebindEvent();
            return m_RebindStartEvent;
        }
    }

    public InteractiveRebindEvent stopRebindEvent
    {
        get
        {
            if (m_RebindStopEvent == null)
                m_RebindStopEvent = new InteractiveRebindEvent();
            return m_RebindStopEvent;
        }
    }

    public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

    /// <summary>
    /// Helper to find the actual action instance.
    /// If playing, it fetches from the Singleton. If in Editor, it uses the Asset.
    /// </summary>
    private InputAction GetSelectedAction()
    {
        // If no reference is set in inspector, we can't do anything
        if (m_Action == null || m_Action.action == null)
            return null;

        // 1. Try to find the runtime instance from the singleton
        if (Application.isPlaying && PlayerInputReference.instance != null && PlayerInputReference.instance.controls != null)
        {
            string mapName = m_Action.action.actionMap.name;
            string actionName = m_Action.action.name;

            // Look for the action in the generated class asset using "Map/Action" format
            var runtimeAction = PlayerInputReference.instance.controls.asset.FindAction($"{mapName}/{actionName}");

            if (runtimeAction != null)
                return runtimeAction;
        }

        // 2. Fallback: Return the asset reference (works in Editor mode for labels)
        return m_Action.action;
    }

    public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
    {
        bindingIndex = -1;
        action = GetSelectedAction();

        if (action == null)
            return false;

        if (string.IsNullOrEmpty(m_BindingId))
            return false;

        // Look up binding index.
        var bindingId = new Guid(m_BindingId);
        bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
        if (bindingIndex == -1)
        {
            Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
            return false;
        }

        return true;
    }

    public void UpdateBindingDisplay()
    {
        var displayString = string.Empty;
        var deviceLayoutName = default(string);
        var controlPath = default(string);

        // Get display string from action.
        var action = GetSelectedAction();
        if (action != null)
        {
            var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
            if (bindingIndex != -1)
                displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, displayStringOptions);
        }

        // Set on label (if any).
        if (m_BindingText != null)
            m_BindingText.text = displayString;

        // Give listeners a chance to configure UI in response.
        m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
    }

    public void ResetToDefault()
    {
        if (!ResolveActionAndBinding(out var action, out var bindingIndex))
            return;

        if (action.bindings[bindingIndex].isComposite)
        {
            for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                action.RemoveBindingOverride(i);
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }

        UpdateBindingDisplay();

        if (Application.isPlaying && PlayerInputReference.instance != null)
            PlayerInputReference.instance.SaveBindingOverrides();
    }

    public void StartInteractiveRebind()
    {
        if (!ResolveActionAndBinding(out var action, out var bindingIndex))
            return;

        if (action.bindings[bindingIndex].isComposite)
        {
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
        }
        else
        {
            PerformInteractiveRebind(action, bindingIndex);
        }
    }

    private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
    {
        m_RebindOperation?.Cancel();

        void CleanUp()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;

            // Re-enable the action map of the action we just bound
            action.actionMap.Enable();

            // Re-enable the UI if we disabled it
            if (Application.isPlaying && PlayerInputReference.instance != null)
            {
                // Assuming your UI input is in the same generated class or handled separately
                // If you have a specific UI action map in your generated class, enable it here.
                // PlayerInputReference.instance.controls.UI.Enable(); 
            }
        }

        // Disable the action map so we don't trigger game actions while rebinding
        action.actionMap.Disable();

        // Configure the rebind.
        m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnCancel(operation =>
            {
                m_RebindStopEvent?.Invoke(this, operation);
                if (m_RebindOverlay != null)
                    m_RebindOverlay.SetActive(false);
                UpdateBindingDisplay();
                CleanUp();
            })
            .OnComplete(operation =>
            {
                if (m_RebindOverlay != null)
                    m_RebindOverlay.SetActive(false);
                m_RebindStopEvent?.Invoke(this, operation);
                UpdateBindingDisplay();
                CleanUp();

                if (Application.isPlaying && PlayerInputReference.instance != null)
                    PlayerInputReference.instance.SaveBindingOverrides();

                if (allCompositeParts)
                {
                    var nextBindingIndex = bindingIndex + 1;
                    if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                        PerformInteractiveRebind(action, nextBindingIndex, true);
                }
            });

        // If it's a part binding, show the name of the part in the UI.
        var partName = default(string);
        if (action.bindings[bindingIndex].isPartOfComposite)
            partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

        m_RebindOverlay?.SetActive(true);
        if (m_RebindText != null)
        {
            var text = !string.IsNullOrEmpty(m_RebindOperation.expectedControlType)
                ? $"{partName}Waiting for {m_RebindOperation.expectedControlType} input..."
                : $"{partName}Waiting for input...";
            m_RebindText.text = text;
        }

        if (m_RebindOverlay == null && m_RebindText == null && m_RebindStartEvent == null && m_BindingText != null)
            m_BindingText.text = "<Waiting...>";

        m_RebindStartEvent?.Invoke(this, m_RebindOperation);
        m_RebindOperation.Start();
    }

    protected void OnEnable()
    {
        if (s_RebindActionUIs == null)
            s_RebindActionUIs = new List<RebindActionUI>();
        s_RebindActionUIs.Add(this);
        if (s_RebindActionUIs.Count == 1)
            InputSystem.onActionChange += OnActionChange;

        // load saved rebinds
        var rebinds = FBPP.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            PlayerInputReference.instance.controls.asset.LoadBindingOverridesFromJson(rebinds);
        }

        UpdateBindingDisplay();
    }

    protected void OnDisable()
    {
        m_RebindOperation?.Dispose();
        m_RebindOperation = null;

        s_RebindActionUIs.Remove(this);
        if (s_RebindActionUIs.Count == 0)
        {
            s_RebindActionUIs = null;
            InputSystem.onActionChange -= OnActionChange;
        }
    }

    private static void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.BoundControlsChanged)
            return;

        var action = obj as InputAction;
        var actionMap = action?.actionMap ?? obj as InputActionMap;
        var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

        for (var i = 0; i < s_RebindActionUIs.Count; ++i)
        {
            var component = s_RebindActionUIs[i];
            var referencedAction = component.GetSelectedAction();
            if (referencedAction == null)
                continue;

            if (referencedAction == action ||
                referencedAction.actionMap == actionMap ||
                referencedAction.actionMap?.asset == actionAsset)
                component.UpdateBindingDisplay();
        }
    }

    // We keep this variable so you can still use the Inspector to select which action 
    // this UI element represents, but the logic now uses GetSelectedAction() to
    // redirect that to the runtime instance.
    [Tooltip("Reference to action that is to be rebound from the UI.")]
    [SerializeField]
    private InputActionReference m_Action;

    [SerializeField]
    private string m_BindingId;

    [SerializeField]
    private InputBinding.DisplayStringOptions m_DisplayStringOptions;

    [SerializeField]
    private TMPro.TextMeshProUGUI m_ActionLabel;

    [SerializeField]
    private TMPro.TextMeshProUGUI m_BindingText;

    [SerializeField]
    private GameObject m_RebindOverlay;

    [SerializeField]
    private TMPro.TextMeshProUGUI m_RebindText;

    [SerializeField]
    private UpdateBindingUIEvent m_UpdateBindingUIEvent;

    [SerializeField]
    private InteractiveRebindEvent m_RebindStartEvent;

    [SerializeField]
    private InteractiveRebindEvent m_RebindStopEvent;

    private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

    private static List<RebindActionUI> s_RebindActionUIs;

#if UNITY_EDITOR
    protected void OnValidate()
    {
        UpdateActionLabel();
        UpdateBindingDisplay();
    }
#endif

    private void UpdateActionLabel()
    {
        if (m_ActionLabel != null)
        {
            var action = m_Action?.action;
            m_ActionLabel.text = action != null ? action.name : string.Empty;
        }
    }

    [Serializable]
    public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
    {
    }

    [Serializable]
    public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
    {
    }
}
