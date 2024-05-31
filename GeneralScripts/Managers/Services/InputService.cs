using System;
using Godot;

public class InputService : Service
{
    public PlayerInput inputActions { get; private set; }

    // Vars for the control scheme
    public Action<string> onChangedControlScheme;
    private string lastControlScheme;

    public InputService()
    {
        inputActions = GameObject.FindObjectOfType<PlayerInput>();
    }

    private void OnActionsTriggered(InputAction.CallbackContext context)
    {
        // Check if the controlscheme has changed
        if(lastControlScheme != context.action.activeControl.device.displayName)
        {
            lastControlScheme = context.action.activeControl.device.displayName;
            onChangedControlScheme?.Invoke(lastControlScheme);
        }
    }

    public override void OnSceneLoad()
    {
        inputActions.onActionTriggered += OnActionsTriggered;
        inputActions.enabled = true;
    }

    public void OnDestroy()
    {
        inputActions.onActionTriggered -= OnActionsTriggered;
    }

    /// <summary>
    /// Toggles the PlayerInput mapping to disable or enable the player interaction.
    /// </summary>
    /// <param name="isActionable"></param>
    public void ToggleInput(bool isActionable)
    {
        inputActions.enabled = isActionable;
    }

    /// <summary>
    /// Returns the player's Action map to bind to events with.
    /// </summary>
    /// <returns></returns>
    public InputActionAsset GetActionMap()
    {
        return inputActions.actions;
    }
}

