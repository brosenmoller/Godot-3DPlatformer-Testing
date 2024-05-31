using System;
using UnityEngine;

namespace PlayerStates
{
    public class GroundedRoot : RootState<PlayerController>
    {
        public GroundedRoot()
        {
            subStates = new Type[]
            {
                typeof(DirectionChange),
                typeof(GroundPoundLand),
                typeof(Idle),
                typeof(OnPoleTop),
                typeof(Walk),
                typeof(Slide),
                typeof(SlopeSlide),
                typeof(BackJump),
                typeof(NormalJump),
                typeof(GroundPoundJump),
                typeof(SlopeSlideJump),
                typeof(SlideJump),
                typeof(SlideEnd)
            };
        }

        public override void InitializeSubState()
        {
            Type nextStateType = typeof(Idle);

            if (ctx.MovementInput != Vector2.Zero && ctx.activePlayerActions.Contains(PlayerAction.Move))
            {
                nextStateType = typeof(Walk);
            }

            if (stateOwner.currentState != null)
            {
                Type currentStateType = stateOwner.currentState.GetType();
                if (currentStateType == typeof(GroundPound)) { nextStateType = typeof(GroundPoundLand); }
            }

            if(ctx.SlopeAngle > ctx.maxSlopeAngle)
            {
                nextStateType = typeof(SlopeSlide);
            }
            //fail safe
            if(ctx.SlopeNormal == Vector3.Zero)
            {
                GD.PrintErr("Entered Grounded with no slope data, This should never happen");
                nextStateType = typeof(AirMovement);
            }

            stateOwner.ChangeState(nextStateType);
        }


        public override void OnEnter()
        {
            ctx.Velocity = ctx.ProjectOnSlope(ctx.Velocity);

            ctx.fallingHeight = 0;
            ctx.divesLeft = ctx.divesLeftDefault;
            ctx.wallClimbsSinceGrounded = 0;
            ctx.canClimb = true;
            ctx.oldWallNormal = new();
            ctx.lastGrappleObject = null;

            ctx.InvokeOnGrounded();
        }

        public override void OnUpdate()
        {
            if (ctx.velocityLibrary[PlayerVelocitySource.dive] > 0)
            {
                ctx.ReduceVelocity(PlayerVelocitySource.dive, 22);
            }
            if (ctx.velocityLibrary[PlayerVelocitySource.solarDive] > 0)
            {
                ctx.ReduceVelocity(PlayerVelocitySource.solarDive, 22);
            }
        }
    }
}
