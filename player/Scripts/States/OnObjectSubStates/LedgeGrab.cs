using Godot;

namespace PlayerStates
{
    public class LedgeGrab : State<PlayerController>
    {
        private float ledgeSpeed = 1;

        public override void OnEnter()
        {
            ctx.InvokeOnLedgeEnter();

            ctx.Velocity = Vector3.Zero;
            ctx.useGravity = false;

            ctx.SetForward(ctx.ledgeForwardHit.normal);
            ctx.visuals.SetForward(-ctx.ledgeForwardHit.normal);

            Vector3 offset = ctx.ledgeForwardHit.normal * 0.64f + Vector3.Down;
            Vector3 pos = new Vector3(ctx.ledgeForwardHit.point.X, ctx.ledgeDownHit.point.Y, ctx.ledgeForwardHit.point.Z) + offset;
            ctx.GlobalPosition = pos;
        }

        public override void OnPhysicsUpdate()
        {
            if (ctx.MovementInput == Vector2.Zero) { return; }

            ctx.InvokeOnLedgeMove();

            Vector3 rayDownStart = (ctx.GlobalPosition + Vector3.Up * 1.5f) + ctx.visuals.Transform.Forward() * 0.7f;
            //Is all of this needed? IDK it works though
            Vector3 inputProjectedToLedge = ctx.InputDirection.ProjectOntoPlane(ctx.ledgeDownHit.normal).Normalized();

            //determine if the projected input is going in a left or right direction
            float leftRightInput = inputProjectedToLedge.Dot(ctx.ledgeDirection);
            //determine if the projected input is going in a forward or backwards direction
            float forwardBackInput = inputProjectedToLedge.Dot(ctx.Transform.Forward());
            //right input and ledge is present to the right
            if (leftRightInput > 0.4f && ctx.RayCast3D(rayDownStart + ctx.visuals.Transform.Right() * 0.3f, Vector3.Down, out var downHit, 0.8f, ctx.GroundLayer))
            {
                ctx.GlobalPosition += ledgeSpeed * (float)ctx.GetPhysicsProcessDeltaTime() * ctx.ledgeDirection;
                return;
            }
            //left input and ledge is present to the left
            if (leftRightInput < -0.4f && ctx.RayCast3D(rayDownStart + -ctx.visuals.Transform.Right() * 0.3f, Vector3.Down, out var downHit2, 0.8f, ctx.GroundLayer))
            {
                ctx.GlobalPosition += ledgeSpeed * (float)ctx.GetPhysicsProcessDeltaTime() * -ctx.ledgeDirection;
                return;
            }
            if (ctx.ledgeIsHangingPoint) { return; }
            if (forwardBackInput < -0.4f)
            {
                ctx.ledgeMoveBackTime += (float)ctx.GetPhysicsProcessDeltaTime();
            }
            else
            {
                ctx.ledgeMoveBackTime = 0;
            }

            if (forwardBackInput > 0.4f)
            {
                //Check if there is a object above the ledge
                Vector3 rayUpStart = ctx.GlobalPosition + ctx.Transform.Up() + ctx.Transform.Forward() * 1;

                if (!ctx.RayCast3D(rayUpStart, Vector3.Up, 0.8f, ctx.GroundLayer))
                {
                    ctx.ledgeMoveForwardTime += (float)ctx.GetPhysicsProcessDeltaTime();
                }
                else
                {
                    ctx.ledgeMoveForwardTime = 0;
                }
            }
            else
            {
                ctx.ledgeMoveForwardTime = 0;
            }
        }

        public override void OnExit()
        {
            ctx.InvokeOnLedgeExit();

            ctx.ledgeIsHangingPoint = false;
            ctx.useGravity = true;
            ctx.ledgeMoveBackTime = 0;
            ctx.ledgeMoveForwardTime = 0;
            ctx.ledgeGrabCoolDown.Restart();
        }
    }
}
