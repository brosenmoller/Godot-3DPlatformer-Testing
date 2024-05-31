using System.Collections.Generic;
using MEC;
using Godot;

namespace PlayerStates
{
    public class NormalJump : State<PlayerController>
    {
        protected float minimumJumpTime = 0.2f;

        public override void OnEnter()
        {
            ctx.InvokeOnJump();

            ctx.jumpCoroutine = Timing.RunCoroutine(NormalJumpRoutine(), Segment.PhysicsProcess);
            ctx.jumpPressCanTrigger = false;
        }

        private IEnumerator<double> NormalJumpRoutine()
        {
            float maxTime = ctx.maxJumpTime;
            float timer = 0;
            
            ctx.ZeroVerticalVelocity();
            
            ctx.AddForceImmediate(Vector3.Up, ctx.jumpForce);

            while ((ctx.jumpPressed || timer < minimumJumpTime) && timer < maxTime)
            {
                timer += (float)ctx.GetPhysicsProcessDeltaTime();
                yield return Timing.WaitForOneFrame;
            }

            timer = 0;

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
