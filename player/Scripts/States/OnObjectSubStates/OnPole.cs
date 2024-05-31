using UnityEngine;

namespace PlayerStates
{
    public class OnPole : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.InvokeOnPole();

            ctx.Rigidbody.useGravity = false;
            ctx.Velocity = Vector3.Zero;
            Vector3 poleXZ = ctx.Pole.position;
            poleXZ.Y = 0;
            Vector3 transfromXZ = ctx.GlobalPosition;
            transfromXZ.Y = 0;
            ctx.Transform.Forward() = (poleXZ - transfromXZ).Normalized();
            Vector3 direction = (transfromXZ - poleXZ).Normalized();
            Vector3 position = poleXZ + (direction * PlayerController.POLEDISTANCE);
            position.Y = ctx.PoleStartHeight;
            ctx.GlobalPosition = position;
        }

        public override void OnPhysicsUpdate()
        {
            Vector3 poleXZ = ctx.Pole.position;
            poleXZ.Y = 0;
            Vector3 transfromXZ = ctx.GlobalPosition;
            transfromXZ.Y = 0;
            ctx.Transform.Forward() = (poleXZ - transfromXZ).Normalized();

            Vector3 direction = (transfromXZ - poleXZ).Normalized();

            float yPos = ctx.GlobalPosition.Y;

            if (ctx.MovementInput.X > 0.3f)
            {
                direction = Quaternion.AngleAxis(-PlayerController.POLETURNSPEED, Vector3.Up) * direction;
            }
            if (ctx.MovementInput.X < -0.3f)
            {
                direction = Quaternion.AngleAxis(PlayerController.POLETURNSPEED, Vector3.Up) * direction;
            }
            if (ctx.MovementInput.Y > 0.3f)
            {
                if (this.RayCast3D(new Vector3(ctx.GlobalPosition.X, yPos, ctx.GlobalPosition.Z), ctx.Transform.Forward(), out var hit, 0.7f, ctx.PoleLayer, QueryTriggerInteraction.Collide))
                {
                    if (hit.colliderObject.tag == ctx.poleTag)
                    {
                        yPos += PlayerController.POLECLIMBSPEED * Time.fixedDeltaTime;
                    }
                }
            }
            if (ctx.MovementInput.Y < -0.3f)
            {
                if (this.RayCast3D(new Vector3(ctx.GlobalPosition.X, yPos - 0.1f, ctx.GlobalPosition.Z), ctx.Transform.Forward(), out var hit, 0.7f, ctx.PoleLayer, QueryTriggerInteraction.Collide))
                {
                    if (hit.colliderObject.tag == ctx.poleTag)
                    {
                        yPos -= PlayerController.POLECLIMBSPEED * Time.fixedDeltaTime;
                    }
                }

            }

            Vector3 position = poleXZ + (direction * PlayerController.POLEDISTANCE);
            position.Y = yPos;
            ctx.GlobalPosition = position;
        }

        public override void OnExit()
        {
            ctx.Rigidbody.useGravity = true;
            ctx.poleLockTimer.Reset();
            ctx.canClimb = true;
            ctx.oldWallNormal = new();
        }
    }
}
