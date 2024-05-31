using System.Collections.Generic;
using Godot;
using MEC;

namespace PlayerStates
{
    public class PoleJump : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.jumpCoroutine = Timing.RunCoroutine(PoleJumpRoutine(), Segment.PhysicsProcess);
            ctx.jumpPressCanTrigger = false;
        }

        private IEnumerator<double> PoleJumpRoutine()
        {
            float maxTime = ctx.maxJumpTime;
            float timer = 0;
            Vector3 direction = Vector3.Up + -ctx.Transform.Forward();
            ctx.ZeroVerticalVelocity();

            ctx.SetForward(-ctx.Transform.Forward());
            
            ctx.AddForceImmediate(direction, ctx.jumpForce);
            float minimumJump = 0.2f;
            while ((ctx.jumpPressed || timer < minimumJump) && timer < maxTime)
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
