using Godot;

namespace PlayerStates
{
    public class GrapplePull : State<PlayerController>
    {
        Vector3 direction;
        Vector3 startPoint;
        float time;
        float desiredTime;

        public override void OnEnter()
        {
            ctx.InvokeOnGrapplePull();

            ctx.grapplePullHasReachedDestination = false;
            ctx.ZeroVelocity();

            ctx.UseGravity = false;

            ctx.lastGrappleObject = ctx.activeGrapplePoint;

            direction = (ctx.activeGrapplePoint.GlobalPosition - ctx.GlobalPosition).Normalized();

            ctx.SetForward(direction);
            ctx.visuals.SetForward(direction);

            ctx.VisualsDirection = direction;
            ctx.velocityLibrary[PlayerVelocitySource.grapple] = 9;
            ctx.maintainVelocityLibrary[PlayerVelocitySource.grapple] = true;
            startPoint = ctx.GlobalPosition;
            time = 0;
            float dist = ctx.GlobalPosition.DistanceTo(ctx.activeGrapplePoint.GlobalPosition);
            desiredTime = Mathf.Lerp(0.2f, ctx.desiredTimeToReachGrapple, Mathf.InverseLerp(ctx.grappleMinDistance, ctx.grappleMaxDistance, dist));
        }

        public override void OnPhysicsUpdate()
        {
            time += ctx.PhysicsDelta();

            ctx.GlobalPosition = startPoint.Lerp(ctx.activeGrapplePoint.GlobalPosition, time / desiredTime);

            if(time >= desiredTime)
            {
                ctx.GlobalPosition = ctx.activeGrapplePoint.GlobalPosition;
                ctx.grapplePullHasReachedDestination = true;
            }
        }

        public override void OnExit()
        {
            ctx.UseGravity = true;
            ctx.Velocity = direction * 12;
            ctx.velocityLibrary[PlayerVelocitySource.grapple] = 3;
            ctx.maintainVelocityLibrary[PlayerVelocitySource.grapple] = false;
            ctx.velocityLibrary[PlayerVelocitySource.normal] = ctx.defaultMaxVelocity;
            ctx.swingLockTimer.Restart();
        }
    }
}
