using System.Collections.Generic;
using MEC;
using Godot;

namespace PlayerStates
{
    public class GroundPoundJump : State<PlayerController>
    {
        private const float hangingTime = 0.25f;
        private const float maxJumpTimeIncrease = 0f;

        public override void OnEnter()
        {
            ctx.InvokeOnGroundPoundJump();

            ctx.jumpCoroutine = Timing.RunCoroutine(GroundPoundJumpRoutine(), Segment.PhysicsProcess);
            ctx.jumpPressCanTrigger = false;
        }

        private IEnumerator<double> GroundPoundJumpRoutine()
        {
            float maxTime = ctx.maxJumpTime + maxJumpTimeIncrease;
            Vector3 direction = Vector3.Up;

            float timer = 0;

            ctx.ZeroVerticalVelocity();

            ctx.Velocity = ctx.jumpForce * direction;
            ctx.AirSpeedClamps();

            while (timer < maxTime)
            {
                timer += (float)ctx.GetPhysicsProcessDeltaTime();
                yield return Timing.WaitForOneFrame;
            }
            timer = 0;

            //keep the player in the air for a few frames after ending the up velocity of a jump
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
