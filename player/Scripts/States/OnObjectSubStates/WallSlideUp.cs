using Godot;
using MEC;

namespace PlayerStates
{
    public class WallSlideUp : State<PlayerController>
    {
        private const float minClimbSpeed = 2;
        private const float maxClimbSpeed = 7;

        public override void OnEnter()
        {
            ctx.InvokeOnWallUpEnter();

            ctx.useGravity = false;
            ctx.Velocity = ctx.ClimbDirection * ctx.Velocity.Length() * 2;
            ctx.climbRotateCoroutine = Timing.RunCoroutine(ctx.RotateTo(-ctx.WallNormal), Segment.PhysicsProcess);
            ctx.climbingTimer.Restart();
        }

        public override void OnPhysicsUpdate()
        {
            float mag = Mathf.Clamp(ctx.Velocity.Length(), minClimbSpeed, maxClimbSpeed);
            mag *= 0.9f;
            ctx.Velocity = ctx.ClimbDirection * mag;

            //Is all of this needed? IDK it works though
            Vector3 cross = ctx.WallNormal.Cross(Vector3.Up);
            //determine if the projected input is going in a left or right direction
            float leftRightInput = ctx.InputDirection.Dot(cross);
            //determine if the projected input is going in a forward or backwards direction
            float forwardBackInput = ctx.InputDirection.Dot(ctx.Transform.Forward());

            if (leftRightInput < -0.4f)
            {
                ctx.GlobalPosition += 4 * (float)ctx.GetPhysicsProcessDeltaTime() * -cross;
                return;
            }
            if (leftRightInput > 0.4f)
            {
                ctx.GlobalPosition += 4 * (float)ctx.GetPhysicsProcessDeltaTime() * cross;
                return;
            }
            if (forwardBackInput < -0.4f)
            {
                ctx.wallMoveBackTime += (float)ctx.GetPhysicsProcessDeltaTime();
            }
            else
            {
                ctx.wallMoveBackTime = 0;
            }
        }

        public override void OnExit()
        {
            ctx.InvokeOnWallUpExit();

            ctx.canClimb = false;
            ctx.useGravity = true;
            if (ctx.climbRotateCoroutine != null)
            {
                ctx.StopCoroutine(ctx.climbRotateCoroutine);
                ctx.climbRotateCoroutine = null;
            }
        }
    }
}
