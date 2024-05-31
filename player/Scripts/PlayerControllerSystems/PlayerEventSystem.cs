using Godot;
using MEC;
using PlayerStates;
using System;
using System.Collections.Generic;

public partial class PlayerController
{
    public static event Action OnJump;

    public static event Action OnGrounded;

    public static event Action OnAir;
    public static event Action OnAirMovement;

    public static event Action OnMove;

    public static event Action OnSlide;
    public static event Action OnSlideJump;

    public static event Action OnDive;

    public static event Action OnLedgeEnter;
    public static event Action OnLedgeMove;
    public static event Action OnLedgeJump;
    public static event Action OnLedgeExit;

    public static event Action OnWallUpEnter;
    public static event Action OnWallDownEnter;
    public static event Action OnWallUpExit;
    public static event Action OnWallDownExit;
    public static event Action OnWallEnter;
    public static event Action OnWallExit;
    public static event Action OnWallJump;

    public static event Action OnGroundPound;
    public static event Action OnGroundPoundLand;
    public static event Action OnGroundPoundJump;

    public static event Action OnHangingPoint;

    public static event Action OnGrapplePull;
    public static event Action<Node3D> OnGrappleSwing;
    public static event Action OnGrappleSwingExit;

    public static event Action OnPole;
    public static event Action OnPoleTop;

    public void InvokeOnJump() => OnJump?.Invoke();

    public void InvokeOnGrounded() => OnGrounded?.Invoke();

    public void InvokeOnAir() => OnAir?.Invoke();
    public void InvokeOnAirMovement() => OnAirMovement?.Invoke();

    public void InvokeOnMove() => OnMove?.Invoke();

    public void InvokeOnSlide() => OnSlide?.Invoke();
    public void InvokeOnSlideJump() => OnSlideJump?.Invoke();

    public void InvokeOnDive() => OnDive?.Invoke();

    public void InvokeOnLedgeEnter() => OnLedgeEnter?.Invoke();
    public void InvokeOnLedgeExit() => OnLedgeExit?.Invoke();
    public void InvokeOnLedgeMove() => OnLedgeMove?.Invoke();
    public void InvokeOnLedgeJump() => OnLedgeJump?.Invoke();

    public void InvokeOnWallUpEnter() => OnWallUpEnter?.Invoke();
    public void InvokeOnWallUpExit() => OnWallUpExit?.Invoke();
    public void InvokeOnWallDownEnter() => OnWallDownEnter?.Invoke();
    public void InvokeOnWallDownExit() => OnWallDownExit?.Invoke();
    public void InvokeOnWallEnter() => OnWallEnter?.Invoke();
    public void InvokeOnWallExit() => OnWallExit?.Invoke();
    public void InvokeOnWallJump() => OnWallJump?.Invoke();

    public void InvokeOnGroundPound() => OnGroundPound?.Invoke();
    public void InvokeOnGroundPoundLand() => OnGroundPoundLand?.Invoke();
    public void InvokeOnGroundPoundJump() => OnGroundPoundJump?.Invoke();

    public void InvokeOnHangingPoint() => OnHangingPoint?.Invoke();

    public void InvokeOnGrapplePull() => OnGrapplePull?.Invoke();
    public void InvokeOnGrappleSwing() => OnGrappleSwing?.Invoke(selectionGrapplePoint);
    public void InvokeOnGrappleSwingExit() => OnGrappleSwingExit?.Invoke();

    public void InvokeOnPole() => OnPole?.Invoke();
    public void InvokeOnPoleTop() => OnPoleTop?.Invoke();


    // -- Ugly system to merge WallUp and WallDown enter and exit states
    private bool isOnWall;
    private CoroutineHandle onWallCheckRoutine;

    private void SetupPlayerEvents()
    {
        OnWallUpEnter += OnAnyWallEnter;
        OnWallDownEnter += OnAnyWallEnter;
        OnWallUpExit += OnAnyWallExit;
        OnWallDownExit += OnAnyWallExit;
    }

    private void OnAnyWallEnter()
    {
        if (!isOnWall)
        {
            isOnWall = true;
            InvokeOnWallEnter();
        }
    }

    private void OnAnyWallExit()
    {
        Timing.KillCoroutines(onWallCheckRoutine);
        onWallCheckRoutine = Timing.RunCoroutine(OnWallCheckRoutine());
    }

    private IEnumerator<double> OnWallCheckRoutine()
    {
        yield return Timing.WaitForSeconds(0.1f);

        Type currentStateType = stateMachine.currentState.GetType();
        if (currentStateType != typeof(WallSlideUp) && currentStateType != typeof(WallSlideDown))
        {
            InvokeOnWallDownExit();
            isOnWall = false;
        }
    }
}