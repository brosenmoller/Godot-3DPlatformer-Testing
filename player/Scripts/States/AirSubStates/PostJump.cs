using Godot;
using MEC;

namespace PlayerStates
{
    public class PostJump : State<PlayerController>
    {
        private const float fallMultiplier = 0.75f;

        public override void OnEnter()
        {
            ctx.postJumpGroundCheckDelayTimer.Restart();
        }

        public override void OnExit()
        {
            ctx.fallingHeight = ctx.GlobalPosition.Y;

            Timing.KillCoroutines(ctx.jumpCoroutine);

            ctx.lockRotation = false;
        }

        public override void OnPhysicsUpdate()
        {
            //Gravity Mod
            ModifyGravity();
            float rotationSpeed = ctx.AirRotaionBySpeed();
            ctx.InputRotation(rotationSpeed);
            ctx.AirColliderRotation();
            ctx.VisualsRotation();
            ctx.VelocityRotation(rotationSpeed);

            InputVelocityHandeling();
            ctx.AirSpeedClamps();
        }

        private void ModifyGravity()
        {
            if (ctx.UseGravity)
            {
                ctx.AddForceImmediate(Vector3.Up, PlayerController.GRAVITY * fallMultiplier);
            }
        }

        private void InputVelocityHandeling()
        {
            if (ctx.MovementInput != Vector2.Zero)
            {
                Vector3 direction = ctx.Transform.Forward();
                Vector3 flatVelocity = ctx.GetFlatVelocity();

                if (!ctx.InputDirectionFlippedBack())
                {
                    ctx.AddForceImmediate(direction, ctx.moveSpeed);
                    //ctx.ReflectVelocity(flatVelocity);
                }
                else
                {
                    flatVelocity = flatVelocity.Length() / 1.05f * flatVelocity.Normalized();
                    flatVelocity.Y = ctx.Velocity.Y;
                    ctx.Velocity = flatVelocity;
                }
            }
        }
    }
}
