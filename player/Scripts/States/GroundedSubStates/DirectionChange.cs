using Godot;

namespace PlayerStates 
{
    public class DirectionChange : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.slopeJumpHeight = Mathf.Inf;
        }

        public override void OnUpdate()
        {
            ctx.NormalVelocityReduction();
        }

        public override void OnPhysicsUpdate()
        {
            ctx.InputRotation(1);
            ctx.GroundedColliderRotation();

            ctx.visuals.SetForward(ctx.VisualsDirection);

            Vector3 velocity = ctx.Velocity;

            if (ctx.SlopeAngle > 0)
            {
                velocity = velocity.Length() / 1.1f * velocity.Normalized();
            }
            else
            {
                velocity.Y = 0;
                velocity = velocity.Length() / 1.1f * velocity.Normalized();
                velocity.Y = ctx.Velocity.Y;
            }

            ctx.Velocity = velocity;
            ctx.SpeedClamps();
        }
    }
}
