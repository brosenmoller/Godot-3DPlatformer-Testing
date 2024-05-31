using System.Collections.Generic;
using Godot;
using MEC;

namespace PlayerStates
{
    public class BackJump : State<PlayerController>
    {
        private const float hangingTime = 0.25f;
        private const float maxJumpTimeIncrease = -0.15f;

        public override void OnEnter()
        {
            ctx.jumpCoroutine = Timing.RunCoroutine(BackJumpRoutine(), Segment.PhysicsProcess);
            ctx.jumpPressCanTrigger = false;
        }

        private IEnumerator<double> BackJumpRoutine()
        {
            ctx.ZeroVerticalVelocity();
            ctx.lockRotation = true;
            float maxTime = ctx.maxJumpTime + maxJumpTimeIncrease;
            Vector3 direction = ctx.Transform.Forward() + Vector3.Up;

            float timer = 0;
            ctx.Velocity = ctx.jumpForce * direction;

            direction.Y = 0;

            ctx.SetForward(direction);
            ctx.VisualsDirection = ctx.Transform.Forward();
            ctx.visuals.SetForward(ctx.Transform.Forward());
            ctx.AirSpeedClamps();

            while (timer < maxTime)
            {
                timer += (float)ctx.GetPhysicsProcessDeltaTime();
                if (timer > 0.2f)
                {
                    ctx.lockRotation = false;
                }
                yield return Timing.WaitForOneFrame;
            }
            timer = 0;

            //keep the player in the air for a few frames after ending the up velocity of a jump
            while (timer < hangingTime)
            {
                timer += (float)ctx.GetProcessDeltaTime();
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
