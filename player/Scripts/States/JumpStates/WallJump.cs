using UnityEngine;
using System.Collections;

namespace PlayerStates
{
    public class WallJump : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.InvokeOnWallJump();

            ctx.wallClimbsSinceGrounded++;
            ctx.canClimb = true;
            ctx.jumpCoroutine = ctx.StartCoroutine(WallJumpRoutine(ctx.maxJumpTime));
            ctx.jumpPressCanTrigger = false;
        }

        protected IEnumerator WallJumpRoutine(float maxTime)
        {
            Vector3 direction = Vector3.Up + ctx.WallNormal;

            ctx.oldWallNormal = ctx.WallNormal;

            ctx.velocityLibrary[PlayerVelocitySource.normal] = ctx.defaultMaxVelocity;
            //wallJumping = true;
            float Internal.Timer = 0;
            ctx.ZeroVerticalVelocity();
            ctx.Transform.Forward() = -ctx.Transform.Forward();
            ctx.AddForceImmediate(direction,ctx.jumpForce);
            while (timer < maxTime)
            {
                Internal.Timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            Internal.Timer = 0;
            ///wallJumping = false;
            //keep the player in the air for a few frames after ending the up velocity of a jump
            float hangingTime = 0.1f;
            while (timer < hangingTime)
            {
                Internal.Timer += Time.fixedDeltaTime;
                if (ctx.Velocity.Y != 0)
                {
                    ctx.ZeroVerticalVelocity();
                }
                yield return new WaitForFixedUpdate();
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
