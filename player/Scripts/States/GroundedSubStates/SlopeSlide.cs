using Godot;

namespace PlayerStates
{
    public class SlopeSlide : State<PlayerController>
    {
        public override void OnPhysicsUpdate()
        {
            SlopeSlideBehaviour();
            ctx.velocityLibrary[PlayerVelocitySource.slide] += 1 * ctx.SlopeAngle * 0.05f * ctx.PhysicsDelta();
            if (ctx.velocityLibrary[PlayerVelocitySource.slide] < 0) { ctx.velocityLibrary[PlayerVelocitySource.slide] = 0; }
            if (ctx.velocityLibrary[PlayerVelocitySource.slide] > 14) { ctx.velocityLibrary[PlayerVelocitySource.slide] = 14; }

            ctx.SpeedClamps();
        }

        private void SlopeSlideBehaviour()
        {
            ctx.Velocity += PlayerController.GRAVITY * 1.5f * ctx.PhysicsDelta() * Vector3.Up;
            //Add force that goes down the slope
            Vector3 DownPlane = ctx.ProjectOnSlope(80 * ctx.PhysicsDelta() * Vector3.Down);
            ctx.Velocity += DownPlane;

            //make sure the speed won't be clamped to 0
            if (ctx.maxVelocity < ctx.defaultMaxVelocity)
            {
                ctx.maxVelocity = ctx.defaultMaxVelocity;
            }

            //Project and check the players input against the down direction of the slope
            Vector3 input = ctx.InputDirection;
            input = ctx.ProjectOnSlope(input);

            float downAngleDot = Vector3.Dot(DownPlane.Normalized(), input);
            if (downAngleDot > -0.5f && downAngleDot < 0.5f)
            {
                //remove the downward force frome the input because we don't need to add extra down force
                input -= DownPlane.Normalized();
                ctx.Velocity += 2 * ctx.moveSpeed * ctx.PhysicsDelta() * input;
            }

            Vector3 flatVelocity = ctx.GetFlatVelocity();

            //Rotate the player root and visuals to match the velocity
            ctx.SetForward(flatVelocity);
            ctx.visuals.SetForward(ctx.Transform.Forward());
        }
    }
}
