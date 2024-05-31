using System.Collections;
using UnityEngine;

namespace PlayerStates
{
    public class PoleJump : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.jumpCoroutine = ctx.StartCoroutine(PoleJumpRoutine());
            ctx.jumpPressCanTrigger = false;
        }


        private IEnumerator PoleJumpRoutine()
        {
            float maxTime = ctx.maxJumpTime;
            float Internal.Timer = 0;
            Vector3 direction = Vector3.Up + -ctx.Transform.Forward();
            ctx.ZeroVerticalVelocity();
            ctx.Transform.Forward() = -ctx.Transform.Forward();
            ctx.AddForceImmediate(direction, ctx.jumpForce);
            float minimumJump = 0.2f;
            while ((ctx.jumpPressed || Internal.Timer < minimumJump) && Internal.Timer < maxTime)
            {
                Internal.Timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            Internal.Timer = 0;
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
