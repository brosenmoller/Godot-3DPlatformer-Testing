using PlayerStates;
using Godot;
using System;
using System.Collections.Generic;
using MEC;

public partial class PlayerController
{
    private void SetupTransitions()
    {
        SetupJumpTransitions();
        SetupSlideTransitions();

        stateMachine.AddTransition(new Transition(typeof(Walk),             typeof(DirectionChange),    InputDirectionFlippedBack));
        stateMachine.AddTransition(new Transition(typeof(DirectionChange),  typeof(Walk),               () => InputDirectionNormal() && activePlayerActions.Contains(PlayerAction.Move)));
        stateMachine.AddTransition(new Transition(typeof(Walk),             typeof(Idle),               ShouldIdle));
        stateMachine.AddTransition(new Transition(typeof(Idle),             typeof(Walk),               () => !ShouldIdle() && activePlayerActions.Contains(PlayerAction.Move)));
        
        stateMachine.AddTransition(new Transition(typeof(Walk),             typeof(SlopeSlide),         OnTooBigSlopeRay));
        stateMachine.AddTransition(new Transition(typeof(GroundPoundLand),  typeof(SlopeSlide),         OnTooBigSlopeRay));

        stateMachine.AddTransition(new Transition(typeof(Walk),             typeof(AirRoot),            () => !GroundReCheck()));
        stateMachine.AddTransition(new Transition(typeof(Idle),             typeof(AirRoot),            () => !GroundReCheck()));
        stateMachine.AddTransition(new Transition(typeof(DirectionChange),  typeof(AirRoot),            () => !GroundReCheck()));
        stateMachine.AddTransition(new Transition(typeof(SlopeSlide),       typeof(AirRoot),            () => !GroundReCheck()));
        stateMachine.AddTransition(new Transition(typeof(SlopeSlide),       typeof(Walk),               () => !OnTooBigSlope() && activePlayerActions.Contains(PlayerAction.Move)));


        stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(GroundPound),        GroundPoundInputValid));
        stateMachine.AddTransition(new Transition(typeof(PostJump),         typeof(GroundPound),        GroundPoundInputValid));

        stateMachine.AddTransition(new Transition(typeof(GroundPound),      typeof(GroundedRoot),       GroundCheck));
        stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(GroundedRoot),       GroundCheck));

        stateMachine.AddTransition(new Transition(typeof(GroundPoundLand),  typeof(Idle),               () => groundPoundLandTimer.IsFinished));

        stateMachine.AddTransition(new Transition(typeof(PostJump),         typeof(Dive),               DiveInputValid));
        stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(Dive),               DiveInputValid));
        stateMachine.AddTransition(new Transition(typeof(GroundPound),      typeof(Dive),               DiveInputValid));

        stateMachine.AddTransition(new Transition(typeof(Dive),             typeof(AirMovement),        NoCondition));

        stateMachine.AddTransition(new Transition(typeof(SolarDive),        typeof(AirMovement),        SolarDiveActive));

        stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(LedgeGrab),          LedgeGrabChecker));
        stateMachine.AddTransition(new Transition(typeof(LedgeGrab),        typeof(AirMovement),        () => { return !LedgeGrabCheckerRaycast(Vector3.Zero, true); }));
        stateMachine.AddTransition(new Transition(typeof(LedgeGrab),        typeof(AirMovement),        LedgeLeaveInputValid ));

        stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(OnPoleTop),          PoleTopCheck));
        stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(OnPole),             PoleCheck));
        stateMachine.AddTransition(new Transition(typeof(PostJump),         typeof(OnPole),             PoleCheck));
        stateMachine.AddTransition(new Transition(typeof(Walk),             typeof(OnPole),             PoleCheck));
        stateMachine.AddTransition(new Transition(typeof(DirectionChange),  typeof(OnPole),             PoleCheck));
        stateMachine.AddTransition(new Transition(typeof(GrappleSwing),     typeof(OnPole),             PoleCheck));
        stateMachine.AddTransition(new Transition(typeof(OnPole),           typeof(OnPoleTop),          PoleTopCheck));
        stateMachine.AddTransition(new Transition(typeof(OnPole),           typeof(GroundedRoot),       GroundCheck));

        stateMachine.AddTransition(new Transition(typeof(WallSlideUp),      typeof(GroundedRoot),       GroundCheck));
        stateMachine.AddTransition(new Transition(typeof(WallSlideDown),    typeof(GroundedRoot),       GroundCheck));

        stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(WallSlideUp),        WallClimbChecker));
        stateMachine.AddTransition(new Transition(typeof(PostJump),         typeof(WallSlideUp),        WallClimbChecker));
        stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(WallSlideDown),      WallSlideChecker));
        stateMachine.AddTransition(new Transition(typeof(PostJump),         typeof(WallSlideDown),      WallSlideChecker));
        stateMachine.AddTransition(new Transition(typeof(WallSlideUp),      typeof(LedgeGrab),          LedgeGrabChecker));
        stateMachine.AddTransition(new Transition(typeof(WallSlideDown),    typeof(LedgeGrab),          LedgeGrabChecker));
        stateMachine.AddTransition(new Transition(typeof(WallSlideUp),      typeof(WallSlideDown),      () => { return climbingTimer.IsFinished; }));
        stateMachine.AddTransition(new Transition(typeof(WallSlideDown),    typeof(AirMovement),        WallSlideReValidate));
        stateMachine.AddTransition(new Transition(typeof(WallSlideDown),    typeof(AirMovement),        WallLeaveInputValid));
        stateMachine.AddTransition(new Transition(typeof(WallSlideUp),      typeof(AirMovement),        () => { return !canClimb || WallSlideReValidate(); }));


        //
        stateMachine.AddTransition(new Transition(typeof(Walk),             typeof(GrapplePull),        GrappleTransition));
        stateMachine.AddTransition(new Transition(typeof(Idle),             typeof(GrapplePull),        GrappleTransition));
        stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(GrapplePull),        GrappleTransition));
        stateMachine.AddTransition(new Transition(typeof(PostJump),         typeof(GrapplePull),        GrappleTransition));
        stateMachine.AddTransition(new Transition(typeof(GrappleSwing),     typeof(AirMovement),        () => !GrappleInput()));
        stateMachine.AddTransition(new Transition(typeof(GrapplePull),      typeof(AirMovement),        GrapplePullDone));

        //stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(Swing),              SwingPointOverlap));
        //stateMachine.AddTransition(new Transition(typeof(PostJump),         typeof(Swing),              SwingPointOverlap));
    }

    private void SetupJumpTransitions()
    {
        // TO Jump
        stateMachine.AddTransition(new Transition(typeof(Walk),             typeof(NormalJump),         NormalJumpInputValid));
        stateMachine.AddTransition(new Transition(typeof(Idle),             typeof(NormalJump),         NormalJumpInputValid));
        stateMachine.AddTransition(new Transition(typeof(DirectionChange),  typeof(BackJump),           JumpInputValid));
        stateMachine.AddTransition(new Transition(typeof(SlopeSlide),       typeof(SlopeSlideJump),     () => JumpInputValid() && slopeJumpHeight > GlobalPosition.Y));
        stateMachine.AddTransition(new Transition(typeof(GroundPoundLand),  typeof(GroundPoundJump),    JumpInputValid));
        stateMachine.AddTransition(new Transition(typeof(OnPoleTop),        typeof(NormalJump),         NormalJumpInputValid));
        stateMachine.AddTransition(new Transition(typeof(OnPole),           typeof(PoleJump),           JumpInputValid));
        stateMachine.AddTransition(new Transition(typeof(WallSlideUp),      typeof(WallJump),           WallJumpValid));
        stateMachine.AddTransition(new Transition(typeof(WallSlideDown),    typeof(WallJumpLow),        WallJumpValid));
        stateMachine.AddTransition(new Transition(typeof(LedgeGrab),        typeof(LedgeJump),          LedgeJumpInputValid));
        stateMachine.AddTransition(new Transition(typeof(AirMovement),      typeof(AirNormalJump),      CoyoteJumpValid));
        stateMachine.AddTransition(new Transition(typeof(Swing),            typeof(AirMovement),        JumpInputValid));

        // From Jump
        stateMachine.AddTransition(new Transition(typeof(NormalJump),       typeof(PostJump),           NoCondition));
        stateMachine.AddTransition(new Transition(typeof(AirNormalJump),    typeof(PostJump),           NoCondition));
        stateMachine.AddTransition(new Transition(typeof(GroundPoundJump),  typeof(PostJump),           NoCondition));
        stateMachine.AddTransition(new Transition(typeof(BackJump),         typeof(PostJump),           NoCondition));
        stateMachine.AddTransition(new Transition(typeof(WallJump),         typeof(PostJump),           NoCondition));
        stateMachine.AddTransition(new Transition(typeof(WallJumpLow),      typeof(PostJump),           NoCondition));
        stateMachine.AddTransition(new Transition(typeof(LedgeJump),        typeof(PostJump),           NoCondition));
        stateMachine.AddTransition(new Transition(typeof(SlopeSlideJump),   typeof(PostJump),           NoCondition));
        stateMachine.AddTransition(new Transition(typeof(SlideJump),        typeof(PostJump),           NoCondition));
        stateMachine.AddTransition(new Transition(typeof(PoleJump),         typeof(PostJump),           NoCondition));

        // Post Jump
        stateMachine.AddTransition(new Transition(typeof(PostJump),         typeof(AirMovement),        () => Timing.IsCoroutineRunning(jumpCoroutine)));
        stateMachine.AddTransition(new Transition(typeof(PostJump),         typeof(GroundedRoot),       () => GroundCheck() && postJumpGroundCheckDelayTimer.IsFinished));
    }

    private void SetupSlideTransitions()
    {
        stateMachine.AddTransition(new Transition(typeof(Idle),             typeof(Slide),              SlideInputValid));
        stateMachine.AddTransition(new Transition(typeof(Walk),             typeof(Slide),              SlideInputValid));
        stateMachine.AddTransition(new Transition(typeof(DirectionChange),  typeof(Slide),              SlideInputValid));
        stateMachine.AddTransition(new Transition(typeof(GroundPoundLand),  typeof(Slide),              SlideInputValid));

        // From Slide
        stateMachine.AddTransition(new Transition(typeof(Slide),            typeof(SlopeSlide),         OnTooBigSlopeRay));
        stateMachine.AddTransition(new Transition(typeof(Slide),            typeof(SlideEnd),           () => slideTimer.IsFinished && CanStandUpCheckerRaycast()));
        stateMachine.AddTransition(new Transition(typeof(SlideEnd),         typeof(Walk),               NoCondition));
        stateMachine.AddTransition(new Transition(typeof(Slide),            typeof(SlideJump),          JumpInputValid));
        stateMachine.AddTransition(new Transition(typeof(Slide),            typeof(AirRoot),            () => !GroundReCheck()));
    }

    public void OverrideState(Type stateType)
    {
        stateMachine.ChangeState(stateType);
    }

    public bool SolarDiveActive()
    {
        return !solarDiveTimer.IsRunning;
    }

    private Vector3[] groundCheckPoints = new[]
    {
        new(1, 0, 1),
        Vector3.Forward,
        new(-1, 0, 1),
        Vector3.Left,
        Vector3.Zero,
        Vector3.Right,
        new(1, 0, -1),
        Vector3.Back,
        new(-1, 0, -1)
    };

    private bool GroundCheck()
    {
        Vector3 middleStart = GlobalPosition + new Vector3(0f, -0.5f, 0f);
        float distance = 0.7f;

        for (int i = 0; i < groundCheckPoints.Length; i++)
        {
            Vector3 origin = middleStart + (groundCheckPoints[i] * 0.3f);

            //DebugDraw3D.DrawArrow(origin, origin + Vector3.Down * distance, Color.Color8(255, 0, 0), 0.1f, false, 0.5f);
            if (this.RayCast3D(origin, Vector3.Down, distance, GroundLayer))
            {
                return true;
            }
        }

        return false;
    }
    private bool GroundReCheck()
    {
        if (SlopeNormal == Vector3.Zero) { return false; }
        Vector3 middleStart = GlobalPosition + new Vector3(0f, -0.5f, 0f);

        for (int i = 0; i < groundCheckPoints.Length; i++)
        {
            Vector3 origin = middleStart + (groundCheckPoints[i] * 0.3f);
            float distance = 0.7f;

            if (this.RayCast3D(origin, Vector3.Down, distance, GroundLayer))
            {
                return true;
            }
        }

        return false;
    }

    //Longer Ground check to see if the input should be Slide or Dive
    private bool SlideGroundCheck()
    {
        Vector3 middleStart = GlobalPosition + new Vector3(0f, -0.5f, 0f);
        float distance = 1f;

        for (int i = 0; i < groundCheckPoints.Length; i++)
        {
            Vector3 origin = middleStart + (groundCheckPoints[i] * 0.3f);

            if (this.RayCast3D(origin, Vector3.Down, out var _, distance, GroundLayer))
            {
                if (!OnTooBigSlope())
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool JumpInputValid()
    {
        return jumpPressed && jumpPressCanTrigger && jumpInputTimer.IsRunning && activePlayerActions.Contains(PlayerAction.Jump);
    }

    private bool NormalJumpInputValid()
    {
        return JumpInputValid();
    }

    private bool CoyoteJumpValid()
    {
        return NormalJumpInputValid() && coyoteTimer.IsRunning;
    }

    private bool WallJumpValid()
    {
        return JumpInputValid() && activePlayerActions.Contains(PlayerAction.WallJump);
    }

    private bool GroundPoundInputValid()
    {
        return groundPoundPressed && groundPoundPressCanTrigger && activePlayerActions.Contains(PlayerAction.GroundPound);
    }

    private bool SlideInputValid()
    {
        return slideDivePressed && slideDivePressCanTrigger && slideInputTimer.IsRunning && SlideGroundCheck() && activePlayerActions.Contains(PlayerAction.Slide);
    }

    private bool DiveInputValid()
    {
        return !SlideGroundCheck() && slideDivePressed && slideDivePressCanTrigger && divesLeft > 0 && activePlayerActions.Contains(PlayerAction.Dive);
    }

    public bool InputDirectionFlippedBack()
    {
        return inputVelocityDotProduct < fullDirectionChangeCheck;
    }

    private bool InputDirectionNormal()
    {
        return !InputDirectionFlippedBack();
    }

    private bool OnTooBigSlope()
    {
        if (SlopeNormal == Vector3.Zero)
        {
            GD.PrintErr("No slope while on Slope"); 
            return false;
        }

        return SlopeAngle > maxSlopeAngle && SlopeAngle != 180;
    }

    private bool OnTooBigSlopeRay()
    {
        Vector3 DownPlane = ProjectOnSlope(Vector3.Down);
        if (this.RayCast3D(GlobalPosition + new Vector3(0f, 0.25f, 0f), DownPlane, out var _, 2.5f, GroundLayer)) return false;
        return OnTooBigSlope();
    }

    private bool NoCondition()
    {
        return true;
    }

    private bool CanStandUpCheckerRaycast()
    {
        Vector3 rayUpStart = GlobalPosition;
        return !this.RayCast3D(rayUpStart, Vector3.Up, out _, 0.8f, GroundLayer);
    }

    private bool LedgeGrabChecker()
    {
        if (ledgeGrabCoolDown.IsRunning) { return false; }
        //Check middle first
        if (LedgeGrabCheckerRaycast()) { return true; }
        //Check left
        if (LedgeGrabCheckerRaycast(-visuals.Transform.Right() * 0.3f)) { return true; }
        //Check right
        if (LedgeGrabCheckerRaycast(visuals.Transform.Right() * 0.3f)) { return true; }
        return false;
    }

    private bool LedgeGrabCheckerRaycast(Vector3 rayStartOffset = new(), bool checkOldPlane = false)
    {
        Vector3 rayDownStart = (GlobalPosition + Vector3.Up * 1.5f) + Transform.Forward() * 0.7f;

        if (this.RayCast3D(rayDownStart + rayStartOffset, Vector3.Down, out var downHit, 0.8f, GroundLayer))
        {
            if (checkOldPlane)
            {
                if (downHit.normal == ledgeDownHit.normal) { return true; }
            }

            if (VectorExtensions.Angle(downHit.normal, Vector3.Up) > maxSlopeAngle) { return false; }

            Vector3 rayForwardStart = new(GlobalPosition.X, downHit.point.Y - 0.1f, GlobalPosition.Z);

            if (this.RayCast3D(rayForwardStart + rayStartOffset, Transform.Forward(), out var forwardHit, 1, GroundLayer))
            {
                if (forwardHit.normal.Y != 0)
                {
                    return false;
                }
                //setting varriables based on this check so this can't be in the state it self
                //Checking if the player can fit
                Vector3 offset = forwardHit.normal * 0.64f + Vector3.Down;
                Vector3 pos = new Vector3(forwardHit.point.X, downHit.point.Y, forwardHit.point.Z) + offset;

                if (!this.OverlapCapsule3D(pos, 0.5f, 1.0f, out OverlapShape3D info, GroundLayer))
                {
                    return false;
                }

                if (forwardHit.colliderInfo.collider.IsInGroup("hangingPoint"))
                {
                    ledgeIsHangingPoint = true;
                }

                ledgeForwardHit = forwardHit;
                ledgeDownHit = downHit;
                ledgeDirection = forwardHit.normal.Cross(downHit.normal);

                return true;
            }
        }
        return false;
    }

    private bool LedgeLeaveInputValid()
    {
        return ledgeMoveBackTime > 0.2f;
    }

    private bool LedgeJumpInputValid()
    {
        return ledgeMoveForwardTime > 0.2f || JumpInputValid();
    }

    private bool ShouldIdle()
    {
        return GetFlatVelocity().Length() < 0.1f && MovementInput == Vector2.Zero;
    }

    private bool PoleTopCheck()
    {
        if (poleLockTimer.IsRunning) { return false; }

        Vector3 middle = GlobalPosition + new Vector3(0f, 0.25f, 0f);

        this.OverlapSphere3D(middle, 0.5f, out OverlapShape3D hitInfo, PoleLayer, true);

        if (hitInfo.allColliders.Length != 0)
        {
            foreach (ColliderInfo3D col in hitInfo.allColliders)
            {
                if (!col.collider.IsInGroup(poleTopTag)) { continue; }

                if (PoleTopOverLapCheck(col.collider))
                {
                    PoleTopCollider = col.collider;
                    return true;
                }
            }
        }

        return false;
    }

    private bool PoleTopOverLapCheck(CollisionObject3D collider)
    {
        Vector3 newPosition = collider.GlobalPosition;
        newPosition.Y += 0.5f;

        if (this.OverlapCapsule3D(newPosition, 0.5f, 1.0f, out _, PoleLayer, true))
        {
            return false;
        }
        
        return true;
    }


    private bool PoleCheck()
    {
        if (poleLockTimer.IsRunning) { return false; }

        if (!this.OverlapCapsule3D(GlobalPosition, 0.5f, 1.0f, out OverlapShape3D overlapInfo, PoleLayer, true))
        {
            return false;
        }

        
        foreach (ColliderInfo3D colliderInfo in overlapInfo.allColliders)
        {
            if (!colliderInfo.collider.IsInGroup(poleTag)) { continue; }

            float yPos = GlobalPosition.Y + POLECLIMBSPEED * this.PhysicsDelta();
            
            Vector3 polePos = colliderInfo.collider.GlobalPosition;
            polePos.Y = 0;
            Vector3 player = GlobalPosition;
            player.Y = 0;
            Vector3 direction = (polePos - player).Normalized();
            
            if (MoveUpTillPoleFound(colliderInfo.collider, direction, polePos + (-direction * POLEDISTANCE), yPos))
            {
                Pole = colliderInfo.collider;
                return true;
            }
            
            if (MoveDownTillPoleFound(colliderInfo.collider, direction, polePos, yPos))
            {
                Pole = colliderInfo.collider;
                return true;
            }
        }
        
        return false;
    }


    private bool MoveUpTillPoleFound(CollisionObject3D collider, Vector3 direction, Vector3 position, float yPos, int depth = 0)
    {
        if (this.RayCast3D(new Vector3(position.X, yPos, position.Z), direction, out var hit, 0.7f, PoleLayer, true))
        {
            if (hit.colliderInfo.collider != collider) { return false; }

            if (hit.colliderInfo.collider.IsInGroup(poleTag))
            {
                PoleStartHeight = yPos;

                return true;
            }
        }

        if (depth > 50) { return false; }
        
        yPos += POLECLIMBSPEED * this.PhysicsDelta();
        
        return MoveUpTillPoleFound(collider, direction, position, yPos, ++depth);
    }

    private bool MoveDownTillPoleFound(CollisionObject3D collider, Vector3 direction, Vector3 position, float yPos, int depth = 0)
    {
        if (this.RayCast3D(new Vector3(position.X, yPos, position.Z), direction, out var hit, 0.7f, PoleLayer, true))
        {
            if (hit.colliderInfo.collider != collider) { return false; }

            if (hit.colliderInfo.collider.IsInGroup(poleTag))
            {
                PoleStartHeight = yPos;

                return true;
            }
        }

        if (depth > 50) { return false; }
        
        yPos -= POLECLIMBSPEED * this.PhysicsDelta();
        
        return MoveDownTillPoleFound(collider, direction, position, yPos, ++depth);
    }

    private bool WallClimbChecker()
    {
        if (wallClimbsSinceGrounded >= maxClimbsSinceGrounded) { return false; }
        if (!canClimb) { return false; }

        if (!activePlayerActions.Contains(PlayerAction.WallSlide)) { return false; }

        //Check middle first
        if (WallClimbStartCheckerRaycast(true)) { return true; }
        //Check left
        if (WallClimbStartCheckerRaycast(true, -visuals.Transform.Right() * 0.3f)) { return true; }
        //Check right
        if (WallClimbStartCheckerRaycast(true, visuals.Transform.Right() * 0.3f)) { return true; }

        return false;
    }



    private bool WallSlideChecker()
    {
        if (wallClimbsSinceGrounded >= maxClimbsSinceGrounded) { return false; }
        if (!activePlayerActions.Contains(PlayerAction.WallSlide)) { return false; }
        //Check middle first
        if (WallClimbStartCheckerRaycast(false)) { return true; }
        //Check left
        if (WallClimbStartCheckerRaycast(false, -visuals.Transform.Right() * 0.3f)) { return true; }
        //Check right
        if (WallClimbStartCheckerRaycast(false, visuals.Transform.Right() * 0.3f)) { return true; }

        return false;
    }

    private bool WallSlideReValidate()
    {
        if (wallClimbsSinceGrounded >= maxClimbsSinceGrounded) { return true; }
        //Check middle first
        if (WallClimbRevalidCheckerRaycast()) { return false; }
        //Check left
        if (WallClimbRevalidCheckerRaycast(-visuals.Transform.Right() * 0.3f)) { return false; }
        //Check right
        if (WallClimbRevalidCheckerRaycast(visuals.Transform.Right() * 0.3f)) { return false; }
        return true;
    }

    private bool WallClimbStartCheckerRaycast(bool climbCheck, Vector3 rayStartOffset = new())
    {
        RaycastHit3D hit = WallRaychecks(0.5f, rayStartOffset);

        if (hit.colliderInfo.collider != null)
        {
            float angle = VectorExtensions.Angle(hit.normal, Vector3.Up);
            if (angle > 91 || angle < 89) { return false; };
            if (climbCheck)
            {
                if (hit.normal == oldWallNormal) { return false; }
            }

            Vector3 v = GetFlatVelocity();

            if (v.Length() < 3f) { return false; }

            Vector3 toProject = new Quaternion(Transform.Right().Normalized(), -45) * v;
            ClimbDirection = toProject.ProjectOntoPlane(hit.normal).Normalized();
            ClimbDirection = (ClimbDirection + Vector3.Up).Normalized();
            WallNormal = hit.normal;

            return true;
        }

        return false;
    }


    private bool WallClimbRevalidCheckerRaycast(Vector3 rayStartOffset = new())
    {
        RaycastHit3D hit = WallRaychecks(1.3f, rayStartOffset);

        if (hit.colliderInfo.collider != null)
        {
            float angle = VectorExtensions.Angle(hit.normal, Vector3.Up);
            if (angle > 91 || angle < 89) { return false; };
            return true;
        }

        return false;
    }

    private bool WallLeaveInputValid()
    {
        return wallMoveBackTime > 0.2f;
    }


    private RaycastHit3D WallRaychecks(float dist, Vector3 rayStartOffset)
    {
        Vector3 rayForwardStart = (GlobalPosition) + visuals.Transform.Forward() * 0.2f;
        rayForwardStart += rayStartOffset;
        //bottom ray
        if (!this.RayCast3D(rayForwardStart, Transform.Forward(), dist, GroundLayer)) { return new(); }
        //middel ray
        if (!this.RayCast3D(rayForwardStart + Vector3.Up * 0.5f, Transform.Forward(), dist, GroundLayer)) { return new(); }
        //Top ray
        if (this.RayCast3D(rayForwardStart + Vector3.Up * 1, Transform.Forward(), out var hit, dist, GroundLayer))
        {
            return hit;
        }
        return new();
    }


    // Temporary for testing -----------------------------------------------------------------------
    private bool GrappleTransition()
    {
        FindGrapplePoint();

        if (!GrappleInputValid()) { return false; }
        if (grappleCooldownTimer.IsRunning) { return false; }
        if (selectionGrapplePoint != null)
        {
            grapplePressCanTrigger = false;
            grappleCooldownTimer.Restart();
            Timing.RunCoroutine(StartGrappleWait(selectionGrapplePoint));
        }
        return false;
    }

    private bool GrappleInputValid()
    {
        return grapplePressed && grapplePressCanTrigger && activePlayerActions.Contains(PlayerAction.GrappleHook);
    }
    private bool GrappleInput()
    {
        return grapplePressed;
    }
    private bool GrapplePullDone()
    {
        return grapplePullHasReachedDestination;
    }

    private IEnumerator<double> StartGrappleWait(Node3D point)
    {
        float currentTime = 0;
        bool pull = false;

        while (currentTime < grappleDelayTime)
        {
            currentTime += this.ProcessDelta();

            if (!grapplePressed)
            {
                pull = true;
            }

            yield return Timing.WaitForOneFrame;
        }
        ExecuteGrapple(point, pull);
    }
    private void ExecuteGrapple(Node3D grapplePoint, bool pull)
    {
        activeGrapplePoint = grapplePoint;

        if (pull)
        {
            stateMachine.ChangeState(typeof(GrapplePull));
        }
        else
        {
            GD.Print("Grapple Swing not connected");
            //stateMachine.ChangeState(typeof(GrappleSwing));
        }
    }
    //--------------------------------------------------------------------------------------------------

    private bool SwingPointOverlap()
    {
        if (swingLockTimer.IsRunning) { return false; }

        if (!this.OverlapCapsule3D(GlobalPosition, 0.5f, 1.0f, out OverlapShape3D info, grappleable, true))
        {
            return false;
        }

        
        foreach (ColliderInfo3D col in info.allColliders)
        {
            if (!col.collider.IsInGroup(swingPointTag)) { continue; }

            currentGrapplePoint = col.collider;
            return true;
        }
        
        
        return false;
    }
}
