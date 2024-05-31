using Godot;
using MEC;
using System.Collections.Generic;

namespace PlayerStates
{
    public class WallJump : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.InvokeOnWallJump();

            ctx.wallClimbsSinceGrounded++;
            ctx.canClimb = true;
            ctx.jumpCoroutine = Timing.RunCoroutine(WallJumpRoutine(ctx.maxJumpTime), Segment.PhysicsProcess);
            ctx.jumpPressCanTrigger = false;
        }

        protected IEnumerator<double> WallJumpRoutine(float maxTime)
        {
            Vector3 direction = Vector3.Up + ctx.WallNormal;

            ctx.oldWallNormal = ctx.WallNormal;

            ctx.velocityLibrary[PlayerVelocitySource.normal] = ctx.defaultMaxVelocity;
            //wallJumping = true;
            float timer = 0;
            ctx.ZeroVerticalVelocity();

            ctx.SetForward(-ctx.Transform.Forward());

            ctx.AddForceImmediate(direction,ctx.jumpForce);
            while (timer < maxTime)
            {
                timer += (float)ctx.GetPhysicsProcessDeltaTime();
                yield return Timing.WaitForOneFrame;
            }

            timer = 0;
            
            ///wallJumping = false;
            //keep the player in the air for a few frames after ending the up velocity of a jump
            float hangingTime = 0.1f;
            while (timer < hangingTime)
            {
                timer += (float)ctx.GetPhysicsProcessDeltaTime();
                if (ctx.Velocity.Y != 0)
                {
                    ctx.ZeroVerticalVelocity();
                }
                yield return Timing.WaitForOneFrame;
            }

            //make sure there isn't any upwards velocity
            if (ctx.Velocity.Y > 0)
            {
                ctx.ZeroVerticalVelocity();
            }
            ctx.jumpCoroutine = null;
        }
    }
}
