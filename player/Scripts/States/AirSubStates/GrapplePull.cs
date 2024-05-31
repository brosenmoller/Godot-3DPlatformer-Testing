using UnityEngine;

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
            ctx.Rigidbody.useGravity = false;
            ctx.lastGrappleObject = ctx.activeGrapplePoint;
            direction = (ctx.activeGrapplePoint.position - ctx.GlobalPosition).Normalized();
            ctx.Transform.Forward() = direction;
            ctx.visuals.forward = direction;
            ctx.VisualsDirection = direction;
            ctx.velocityLibrary[PlayerVelocitySource.grapple] = 9;
            ctx.maintainVelocityLibrary[PlayerVelocitySource.grapple] = true;
            startPoint = ctx.GlobalPosition;
            time = 0;
            float dist = Vector3.Distance(ctx.GlobalPosition, ctx.activeGrapplePoint.position);
            desiredTime = Mathf.Lerp(0.2f, ctx.desiredTimeToReachGrapple, Mathf.InverseLerp(ctx.grappleMinDistance, ctx.grappleMaxDistance, dist));
        }

        public override void OnPhysicsUpdate()
        {
            time += Time.fixedDeltaTime;
            ctx.GlobalPosition = Vector3.Lerp(startPoint, ctx.activeGrapplePoint.position, time / desiredTime);
            if(time >= desiredTime)
            {
                ctx.GlobalPosition = ctx.activeGrapplePoint.position;
                ctx.grapplePullHasReachedDestination = true;
            }
        }

        public override void OnExit()
        {
            ctx.Rigidbody.useGravity = true;
            ctx.Velocity = direction * 12;
            ctx.velocityLibrary[PlayerVelocitySource.grapple] = 3;
            ctx.maintainVelocityLibrary[PlayerVelocitySource.grapple] = false;
            ctx.velocityLibrary[PlayerVelocitySource.normal] = ctx.defaultMaxVelocity;
            ctx.swingLockTimer.Restart();
        }
    }
}
