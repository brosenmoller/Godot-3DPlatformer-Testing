using Godot;

namespace PlayerStates
{
    public class OnPole : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.InvokeOnPole();

            ctx.useGravity = false;
            ctx.Velocity = Vector3.Zero;
            Vector3 poleXZ = ctx.Pole.GlobalPosition;
            poleXZ.Y = 0;
            Vector3 transfromXZ = ctx.GlobalPosition;
            transfromXZ.Y = 0;
            ctx.SetForward((poleXZ - transfromXZ).Normalized());
            Vector3 direction = (transfromXZ - poleXZ).Normalized();
            Vector3 position = poleXZ + (direction * PlayerController.POLEDISTANCE);
            position.Y = ctx.PoleStartHeight;
            ctx.GlobalPosition = position;
        }

        public override void OnPhysicsUpdate()
        {
            Vector3 poleXZ = ctx.Pole.GlobalPosition;
            poleXZ.Y = 0;
            Vector3 transfromXZ = ctx.GlobalPosition;
            transfromXZ.Y = 0;

            ctx.SetForward((poleXZ - transfromXZ).Normalized());

            Vector3 direction = (transfromXZ - poleXZ).Normalized();

            float yPos = ctx.GlobalPosition.Y;

            if (ctx.MovementInput.X > 0.3f)
            {
                direction = new Quaternion(Vector3.Up, -PlayerController.POLETURNSPEED) * direction;
            }
            if (ctx.MovementInput.X < -0.3f)
            {
                direction = new Quaternion(Vector3.Up, PlayerController.POLETURNSPEED) * direction;
            }
            if (ctx.MovementInput.Y > 0.3f)
            {
                if (ctx.RayCast3D(new Vector3(ctx.GlobalPosition.X, yPos, ctx.GlobalPosition.Z), ctx.Transform.Forward(), out var hit, 0.7f, ctx.PoleLayer, true))
                {
                    if (hit.colliderInfo.collider.IsInGroup(ctx.poleTag))
                    {
                        yPos += PlayerController.POLECLIMBSPEED * (float)ctx.GetPhysicsProcessDeltaTime();
                    }
                }
            }
            if (ctx.MovementInput.Y < -0.3f)
            {
                if (ctx.RayCast3D(new Vector3(ctx.GlobalPosition.X, yPos - 0.1f, ctx.GlobalPosition.Z), ctx.Transform.Forward(), out var hit, 0.7f, ctx.PoleLayer, true))
                {
                    if (hit.colliderInfo.collider.IsInGroup(ctx.poleTag))
                    {
                        yPos -= PlayerController.POLECLIMBSPEED * (float)ctx.GetPhysicsProcessDeltaTime();
                    }
                }

            }

            Vector3 position = poleXZ + (direction * PlayerController.POLEDISTANCE);
            position.Y = yPos;
            ctx.GlobalPosition = position;
        }

        public override void OnExit()
        {
            ctx.useGravity = true;
            ctx.poleLockTimer.Reset();
            ctx.canClimb = true;
            ctx.oldWallNormal = new();
        }
    }
}
