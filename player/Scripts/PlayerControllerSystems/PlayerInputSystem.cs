using System.Collections.Generic;
using Godot;
using Godot.Collections;

[System.Serializable]
public enum PlayerAction
{
    Move = 0,
    Jump = 1,
    Camera = 2,
    GroundPound = 3,
    Slide = 4,
    Dive = 5,
    GrappleHook = 6,
    WallJump = 7,
    WallSlide = 8,
}

public partial class PlayerController
{
    [ExportCategory("Input")]
    [Export]
    private Array<PlayerAction> startingPlayerActions;

    public HashSet<PlayerAction> activePlayerActions;

    private InputService input;

    /// <summary>
    /// Add Bindings the player can perform in-game.
    /// </summary>
    /// <param name="playerActions"></param>
    public void AddPlayerAction(params PlayerAction[] playerActions)
    {
        foreach (PlayerAction playerAction in playerActions) 
        {
            activePlayerActions.Add(playerAction);
        }
    }

    /// <summary>
    /// Removes Bindings the player can perform in-game.
    /// </summary>
    /// <param name="playerActions"></param>
    public void RemovePlayerAction(params PlayerAction[] playerActions)
    {
        foreach (PlayerAction playerAction in playerActions)
        {
            activePlayerActions.Remove(playerAction);
        }
    }

    
    private void SetupCharacterInput()
    {
        input = ServiceLocator.Instance.Get<InputService>();
        activePlayerActions = new HashSet<PlayerAction>(startingPlayerActions);

        foreach (PlayerAction playerAction in activePlayerActions)
        {
            GD.Print("PlayerAction: " + playerAction.ToString());
        }
     
        input.Bindings["movement"].Hold += OnMovementHold;
        input.Bindings["movement"].Canceled += OnMovementCancelled;
        input.Bindings["jump"].Started += OnJumpPerformed;
        input.Bindings["jump"].Canceled += OnJumpCancelled;
        input.Bindings["ground_pound"].Started += OnGroundPoundPerformed;
        input.Bindings["ground_pound"].Canceled += OnGroundPoundCancelled;
        input.Bindings["slide"].Started += OnSlidePerformed;
        input.Bindings["slide"].Canceled += OnSlideCancelled;
        input.Bindings["grapple"].Started += OnGrappleHookPerformed;
        input.Bindings["grapple"].Canceled += OnGrappleHookCancelled;
    }

    private void DesubscribeCharacterInput()
    {
        input.Bindings["movement"].Hold -= OnMovementHold;
        input.Bindings["movement"].Canceled -= OnMovementCancelled;
        input.Bindings["jump"].Started -= OnJumpPerformed;
        input.Bindings["jump"].Canceled -= OnJumpCancelled;
        input.Bindings["ground_pound"].Started -= OnGroundPoundPerformed;
        input.Bindings["ground_pound"].Canceled -= OnGroundPoundCancelled;
        input.Bindings["slide"].Started -= OnSlidePerformed;
        input.Bindings["slide"].Canceled -= OnSlideCancelled;
        input.Bindings["grapple"].Started -= OnGrappleHookPerformed;
        input.Bindings["grapple"].Canceled -= OnGrappleHookCancelled;
    }


    private void OnMovementHold()
    {
            GD.Print("Hold");
        if (activePlayerActions.Contains(PlayerAction.Move))
        {
            MovementInput = input.Bindings["movement"].ReadValue<Vector2>();
        }
        else
        {
            MovementInput = Vector2.Zero;
        }
    }

    private void OnMovementCancelled()
    {
        MovementInput = Vector2.Zero;
    }

    private void OnJumpPerformed()
    {
        jumpPressed = true;
        jumpPressCanTrigger = true;
        jumpInputTimer.Restart();
    }

    private void OnJumpCancelled()
    {
        jumpPressed = false;
        jumpPressCanTrigger = false;
    }

    private void OnGroundPoundPerformed()
    {
        groundPoundPressCanTrigger = true;
        groundPoundPressed = true;
    }

    private void OnGroundPoundCancelled()
    {
        groundPoundPressed = false;
        groundPoundPressCanTrigger = false;
    }

    private void OnSlidePerformed()
    {
        slideDivePressed = true;
        slideDivePressCanTrigger = true;
        slideInputTimer.Restart();
    }

    private void OnSlideCancelled()
    {
        slideDivePressed = false;
        slideDivePressCanTrigger = false;
    }

    private void OnGrappleHookPerformed()
    {
        grapplePressed = true;
        grapplePressCanTrigger = true;
    }

    private void OnGrappleHookCancelled()
    {
        grapplePressed = false;
        grapplePressCanTrigger = false;
    }
}

