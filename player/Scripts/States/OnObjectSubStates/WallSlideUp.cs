using UnityEngine;

namespace PlayerStates
{
    public class WallSlideUp : State<PlayerController>
    {
        private const float minClimbSpeed = 2;
        private const float maxClimbSpeed = 7;

        public override void OnEnter()
        {
            ctx.InvokeOnWallUpEnter();

            ctx.Rigidbody.useGravity = false;
            ctx.Velocity = ctx.ClimbDirection * ctx.Velocity.Length() * 2;
            ctx.climbRotateCoroutine = ctx.StartCoroutine(ctx.RotateTo(-ctx.WallNormal));
            ctx.climbingTimer.Restart();
        }

        public override void OnPhysicsUpdate()
        {
            float mag = Mathf.Clamp(ctx.Velocity.Length(), minClimbSpeed, maxClimbSpeed);
            mag *= 0.9f;
            ctx.Velocity = ctx.ClimbDirection * mag;

            //Is all of this needed? IDK it works though
            Vector3 cross = Vector3.Cross(ctx.WallNormal, Vector3.Up);
            //determine if the projected input is going in a left or right direction
            float leftRightInput = Vector3.Dot(ctx.InputDirection, cross);
            //determine if the projected input is going in a forward or backwards direction
            float forwardBackInput = Vector3.Dot(ctx.InputDirection, ctx.Transform.Forward());

            if (leftRightInput < -0.4f)
            {
                ctx.GlobalPosition += 4 * Time.fixedDeltaTime * -cross / ctx.Rigidbody.mass;
                return;
            }
            if (leftRightInput > 0.4f)
            {
                ctx.GlobalPosition += 4 * Time.fixedDeltaTime * cross / ctx.Rigidbody.mass;
                return;
            }
            if (forwardBackInput < -0.4f)
            {
                ctx.wallMoveBackTime += Time.fixedDeltaTime;
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
            ctx.Rigidbody.useGravity = true;
            if (ctx.climbRotateCoroutine != null)
            {
                ctx.StopCoroutine(ctx.climbRotateCoroutine);
                ctx.climbRotateCoroutine = null;
            }
        }

    }
}
