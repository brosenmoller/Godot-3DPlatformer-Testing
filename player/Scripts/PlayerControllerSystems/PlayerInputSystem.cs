using System.Collections.Generic;
using Godot;

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
    private List<PlayerAction> startingPlayerActions;

    public HashSet<PlayerAction> activePlayerActions;

    private PlayerInput gameActions;

    /// <summary>
    /// Add actions the player can perform in-game.
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
    /// Removes actions the player can perform in-game.
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
        gameActions = ServiceLocator.Instance.Get<InputService>().inputActions;
        activePlayerActions = new HashSet<PlayerAction>(startingPlayerActions);
     
        gameActions.actions["Movement"].performed += OnMovementPerformed;
        gameActions.actions["Movement"].canceled += OnMovementCancelled;
        gameActions.actions["Jump"].performed += OnJumpPerformed;
        gameActions.actions["Jump"].canceled += OnJumpCancelled;
        gameActions.actions["GroundPound"].performed += OnGroundPoundPerformed;
        gameActions.actions["GroundPound"].canceled += OnGroundPoundCancelled;
        gameActions.actions["Slide"].performed += OnSlidePerformed;
        gameActions.actions["Slide"].canceled += OnSlideCancelled;
        gameActions.actions["GrappleHook"].performed += OnGrappleHookPerformed;
        gameActions.actions["GrappleHook"].canceled += OnGrappleHookCancelled;
    }

    private void DesubscribeCharacterInput()
    {
        gameActions.actions["Movement"].performed -= OnMovementPerformed;
        gameActions.actions["Movement"].canceled -= OnMovementCancelled;
        gameActions.actions["Jump"].performed -= OnJumpPerformed;
        gameActions.actions["Jump"].canceled -= OnJumpCancelled;
        gameActions.actions["GroundPound"].performed -= OnGroundPoundPerformed;
        gameActions.actions["GroundPound"].canceled -= OnGroundPoundCancelled;
        gameActions.actions["Slide"].performed -= OnSlidePerformed;
        gameActions.actions["Slide"].canceled -= OnSlideCancelled;
        gameActions.actions["GrappleHook"].performed -= OnGrappleHookPerformed;
        gameActions.actions["GrappleHook"].canceled -= OnGrappleHookCancelled;
    }


    private void OnMovementPerformed(InputAction.CallbackContext ctx)
    {
        if (activePlayerActions.Contains(PlayerAction.Move))
        {
            MovementInput = ctx.ReadValue<Vector2>();
        }
        else
        {
            MovementInput = Vector2.Zero;
        }
    }

    private void OnMovementCancelled(InputAction.CallbackContext ctx)
    {
        MovementInput = Vector2.Zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        jumpPressed = true;
        jumpPressCanTrigger = true;
        jumpInputTimer.Restart();
    }

    private void OnJumpCancelled(InputAction.CallbackContext ctx)
    {
        jumpPressed = false;
        jumpPressCanTrigger = false;
    }

    private void OnGroundPoundPerformed(InputAction.CallbackContext ctx)
    {
        groundPoundPressCanTrigger = true;
        groundPoundPressed = true;
    }

    private void OnGroundPoundCancelled(InputAction.CallbackContext ctx)
    {
        groundPoundPressed = false;
        groundPoundPressCanTrigger = false;
    }

    private void OnSlidePerformed(InputAction.CallbackContext ctx)
    {
        slideDivePressed = true;
        slideDivePressCanTrigger = true;
        slideInputTimer.Restart();
    }

    private void OnSlideCancelled(InputAction.CallbackContext ctx)
    {
        slideDivePressed = false;
        slideDivePressCanTrigger = false;
    }

    private void OnGrappleHookPerformed(InputAction.CallbackContext ctx)
    {
        grapplePressed = true;
        grapplePressCanTrigger = true;
    }

    private void OnGrappleHookCancelled(InputAction.CallbackContext ctx)
    {
        grapplePressed = false;
        grapplePressCanTrigger = false;
    }
}

