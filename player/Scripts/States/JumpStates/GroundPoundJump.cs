using System.Collections;
using UnityEngine;

namespace PlayerStates
{
    public class GroundPoundJump : State<PlayerController>
    {
        private const float hangingTime = 0.25f;
        private const float maxJumpTimeIncrease = 0f;

        public override void OnEnter()
        {
            ctx.InvokeOnGroundPoundJump();

            ctx.jumpCoroutine = ctx.StartCoroutine(GroundPoundJumpRoutine());
            ctx.jumpPressCanTrigger = false;
        }

        private IEnumerator GroundPoundJumpRoutine()
        {
            float maxTime = ctx.maxJumpTime + maxJumpTimeIncrease;
            Vector3 direction = Vector3.Up;

            float Internal.Timer = 0;

            ctx.ZeroVerticalVelocity();

            ctx.Velocity = ctx.jumpForce * direction;
            ctx.AirSpeedClamps();

            while (timer < maxTime)
            {
                Internal.Timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            Internal.Timer = 0;

            //keep the player in the air for a few frames after ending the up velocity of a jump
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
