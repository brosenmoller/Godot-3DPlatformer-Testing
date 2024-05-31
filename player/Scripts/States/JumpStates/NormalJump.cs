using System.Collections;
using UnityEngine;

namespace PlayerStates
{
    public class NormalJump : State<PlayerController>
    {
        protected float minimumJumpTime = 0.2f;

        public override void OnEnter()
        {
            ctx.InvokeOnJump();

            ctx.jumpCoroutine = ctx.StartCoroutine(NormalJumpRoutine());
            ctx.jumpPressCanTrigger = false;
        }

        private IEnumerator NormalJumpRoutine()
        {
            float maxTime = ctx.maxJumpTime;
            float Internal.Timer = 0;
            
            ctx.ZeroVerticalVelocity();
            
            ctx.AddForceImmediate(Vector3.Up, ctx.jumpForce);

            while ((ctx.jumpPressed || Internal.Timer < minimumJumpTime) && Internal.Timer < maxTime)
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
