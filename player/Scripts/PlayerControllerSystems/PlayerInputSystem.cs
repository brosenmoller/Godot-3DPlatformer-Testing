using System.Collections.Generic;
using Godot;
using Godot.Collections;

[System.Serializable]
public enum PlayerAction
{
    Move = 1,
    Jump = 2,
    Camera = 3,
    GroundPound = 4,
    Slide = 5,
    Dive = 6,
    GrappleHook = 7,
    WallJump = 8,
    WallSlide = 9,
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
     
        input.Bindings["movement"].Performed += OnMovementPerformed;
        input.Bindings["movement"].Canceled += OnMovementCancelled;
        input.Bindings["jump"].Performed += OnJumpPerformed;
        input.Bindings["jump"].Canceled += OnJumpCancelled;
        input.Bindings["ground_pound"].Performed += OnGroundPoundPerformed;
        input.Bindings["ground_pound"].Canceled += OnGroundPoundCancelled;
        input.Bindings["slide"].Performed += OnSlidePerformed;
        input.Bindings["slide"].Canceled += OnSlideCancelled;
        input.Bindings["grapple"].Performed += OnGrappleHookPerformed;
        input.Bindings["grapple"].Canceled += OnGrappleHookCancelled;
    }

    private void DesubscribeCharacterInput()
    {
        input.Bindings["movement"].Performed -= OnMovementPerformed;
        input.Bindings["movement"].Canceled -= OnMovementCancelled;
        input.Bindings["jump"].Performed -= OnJumpPerformed;
        input.Bindings["jump"].Canceled -= OnJumpCancelled;
        input.Bindings["ground_pound"].Performed -= OnGroundPoundPerformed;
        input.Bindings["ground_pound"].Canceled -= OnGroundPoundCancelled;
        input.Bindings["slide"].Performed -= OnSlidePerformed;
        input.Bindings["slide"].Canceled -= OnSlideCancelled;
        input.Bindings["grapple"].Performed -= OnGrappleHookPerformed;
        input.Bindings["grapple"].Canceled -= OnGrappleHookCancelled;
    }


    private void OnMovementPerformed()
    {
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

