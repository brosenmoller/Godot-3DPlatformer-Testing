using System.Collections;
using UnityEngine;

namespace PlayerStates
{
    public class BackJump : State<PlayerController>
    {
        private const float hangingTime = 0.25f;
        private const float maxJumpTimeIncrease = -0.15f;

        public override void OnEnter()
        {
            ctx.jumpCoroutine = ctx.StartCoroutine(BackJumpRoutine());
            ctx.jumpPressCanTrigger = false;
        }

        private IEnumerator BackJumpRoutine()
        {
            ctx.ZeroVerticalVelocity();
            ctx.lockRotation = true;
            float maxTime = ctx.maxJumpTime + maxJumpTimeIncrease;
            Vector3 direction = ctx.Transform.Forward() + Vector3.Up;

            float Internal.Timer = 0;
            ctx.Velocity = ctx.jumpForce * direction;

            direction.Y = 0;

            ctx.Transform.Forward() = direction;
            ctx.VisualsDirection = ctx.Transform.Forward();
            ctx.visuals.forward = ctx.Transform.Forward();
            ctx.AirSpeedClamps();

            while (timer < maxTime)
            {
                Internal.Timer += Time.fixedDeltaTime;
                if (timer > 0.2f)
                {
                    ctx.lockRotation = false;
                }
                yield return new WaitForFixedUpdate();
            }
            Internal.Timer = 0;

            //keep the player in the air for a few frames after ending the up velocity of a jump
            while (timer < hangingTime)
            {
                Internal.Timer += Time.deltaTime;
                if (ctx.Velocity.Y != 0)
                {
                    ctx.ZeroVerticalVelocity();
                }
                yield return new WaitForEndOfFrame();
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
