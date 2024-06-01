using Godot;

namespace PlayerStates
{
    public class Swing : State<PlayerController>
    {
        //private HingeJoint hingeJoint;

        public override void OnEnter()
        {
            //float dot = Vector3.Dot(ctx.Transform.Forward(), ctx.currentGrapplePoint.forward);
            //if (dot > 0 || dot == 0)
            //{
            //    ctx.Transform.Forward() = ctx.currentGrapplePoint.forward;
                
            //}
            //else
            //{
            //    ctx.Transform.Forward() = -ctx.currentGrapplePoint.forward;
            //}
            //ctx.visuals.forward = ctx.Transform.Forward();
            //ctx.VisualsDirection = ctx.Transform.Forward();
            //ctx.Rigidbody.constraints = RigidbodyConstraints.None;
            //hingeJoint = ctx.gameObject.AddComponent<HingeJoint>();
            //hingeJoint.autoConfigureConnectedAnchor = false;
            //hingeJoint.anchor = new Vector3(0, 1.5f, 0);
            //hingeJoint.connectedAnchor = ctx.currentGrapplePoint.position;
            //hingeJoint.useLimits = true;
            //hingeJoint.limits = ctx.hingeLimits;
        }


        public override void OnPhysicsUpdate()
        {
            //float dot = Vector3.Dot(ctx.InputDirection, ctx.Transform.Forward());
            //if (dot > 0.5f)
            //{
            //    ctx.Velocity += 0.05f * ctx.Transform.Forward() / ctx.Rigidbody.mass;
            //}
            //if (dot < -0.5f)
            //{
            //    ctx.Velocity += -0.05f * ctx.Transform.Forward() / ctx.Rigidbody.mass;
            //}
        }


        public override void OnExit()
        {
            //ctx.currentGrapplePoint = null;
            //Vector3 forward = ctx.Transform.Forward();
            //forward.Y = Mathf.Abs(forward.Y);
            //float angle = VectorExtensions.Angle(forward, Vector3.Up);
            //Vector3 v = ctx.jumpForce / 1.3f * Mathf.InverseLerp(90, 0, angle) * ctx.PhysicsDelta(); * Vector3.Up;
            ////Destroy Immediatly to prevent hinge continuing to constrain the ridgidbody
            //Object.DestroyImmediate(hingeJoint);
            //ctx.Rigidbody.constraints = ctx.normalConstraints;

            //forward = ctx.Transform.Forward();
            //if (forward.Y < 0)
            //{
            //    forward = Quaternion.AngleAxis(180, Vector3.Up) * forward;
            //}
            //forward.Y = 0;
            //ctx.Transform.Forward() = forward;
            //v += ctx.jumpForce / 1.3f * Mathf.InverseLerp(90, 0, angle) * ctx.PhysicsDelta(); * ctx.Transform.Forward();
            //ctx.Velocity = v;
            //ctx.divesLeft = 1;
            //ctx.swingLockTimer.Restart();
        }
    }
}
