using Godot;

namespace PlayerStates
{
    public class GrappleSwing : State<PlayerController>
    {
        private SpringJoint swingJoint;
        private float grappleGravityAdjustment = 2;
        public override void OnEnter()
        {
            ctx.InvokeOnGrappleSwing();

            ctx.lastGrappleObject = ctx.activeGrapplePoint;
            swingJoint = ctx.gameObject.AddComponent<SpringJoint>();
            swingJoint.autoConfigureConnectedAnchor = false;
            swingJoint.connectedAnchor = ctx.activeGrapplePoint.position;

            float distanceFromPoint = Vector3.Distance(ctx.GlobalPosition, ctx.activeGrapplePoint.position);
            distanceFromPoint = Mathf.Clamp(distanceFromPoint, 1.5f, 3.5f);
            swingJoint.maxDistance = distanceFromPoint;
            swingJoint.minDistance = distanceFromPoint * 0.88f;
            ctx.velocityLibrary[PlayerVelocitySource.grapple] = 9;
            ctx.maintainVelocityLibrary[PlayerVelocitySource.grapple] = true;
            swingJoint.spring = 3.5f;
            swingJoint.damper = 3;
            swingJoint.massScale = 4.5f;
        }

        public override void OnPhysicsUpdate()
        {
            ctx.Velocity += PlayerController.GRAVITY * (grappleGravityAdjustment - 1) * (float)ctx.GetPhysicsProcessDeltaTime(); * Vector3.Up;
            ctx.Velocity += ctx.swingSpeed * (float)ctx.GetPhysicsProcessDeltaTime(); * ctx.InputDirection;
            Vector3 v = ctx.Velocity;
            v.Y = Mathf.Clamp(v.Y, -12, 39);
            ctx.Velocity = v;
            v.Y = 0;
            if (v != Vector3.Zero)
            {
                ctx.Transform.Forward() = Vector3.Slerp(ctx.Transform.Forward(), v.Normalized(), 0.1f);
                ctx.visuals.forward = Vector3.Lerp(ctx.visuals.forward, ctx.Transform.Forward(), 0.4f);
                ctx.VisualsDirection = ctx.visuals.forward;
            }
        }

        public override void OnExit()
        {
            ctx.InvokeOnGrappleSwingExit();

            ctx.maintainVelocityLibrary[PlayerVelocitySource.grapple] = false;
            ctx.velocityLibrary[PlayerVelocitySource.normal] = ctx.defaultMaxVelocity;
            Object.Destroy(swingJoint);
            swingJoint = null;
            ctx.divesLeft = 1;
        }
    }
}
