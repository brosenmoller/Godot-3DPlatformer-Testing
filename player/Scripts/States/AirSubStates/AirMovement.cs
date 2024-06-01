using Godot;

namespace PlayerStates
{
    public class AirMovement : State<PlayerController>
    {
        private const float fallMultiplier = 1.5f;
        private const float stepHeight = 0.35f;

        public override void OnEnter()
        {
            ctx.InvokeOnAirMovement();
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
        public override void OnUpdate()
        {
            if (ctx.velocityLibrary[PlayerVelocitySource.slide] > 0)
            {
                ctx.ReduceVelocity(PlayerVelocitySource.slide, 6);
            }
        }

        private void ModifyGravity()
        {
            if (ctx.UseGravity)
            {
                if (ctx.Velocity.Y < 0)
                {
                    ctx.AddForceImmediate(Vector3.Up, PlayerController.GRAVITY * fallMultiplier);
                }
                else
                {
                    ctx.AddForceImmediate(Vector3.Up, PlayerController.GRAVITY * (fallMultiplier/2));
                }
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
                StepClimb();
            }
        }

        private void StepClimb()
        {
            Vector3 origin = ctx.GlobalPosition + new Vector3(0f, -ctx.playerHeight * 0.9f, 0f) + (0.2f * ctx.Transform.Forward());
            if (StepCheck(origin, ctx.Transform.Forward())) { return; }
            if (StepCheck(origin, new Quaternion(Vector3.Up, 45) * ctx.Transform.Forward())) { return; }
            if (StepCheck(origin, new Quaternion(Vector3.Up, -45) * ctx.Transform.Forward())) { return; }
            ctx.steppedLastFrame = false;
        }

        private bool StepCheck(Vector3 origin, Vector3 direction)
        {
            //check if there is a collider at the players feet
            if (ctx.RayCast3D(origin, direction, out var hit, 0.3f))
            {
                //make sure it is not a walkable slope that was hit
                if (VectorExtensions.Angle(hit.normal, Vector3.Up) > ctx.maxSlopeAngle)
                {
                    //check if the collider is low enough to be stepped on
                    if (!ctx.RayCast3D(origin + stepHeight * Vector3.Up, direction, 0.5f))
                    {
                        ExecuteStep(hit);
                        return true;
                    }
                }
            }
            return false;
        }

        private void ExecuteStep(RaycastHit3D hit)
        {
            ctx.ZeroVerticalVelocity();

            Vector3 origin2 = hit.point + new Vector3(0, stepHeight + 0.1f, 0);

            ctx.RayCast3D(origin2, Vector3.Down, out var hit2, stepHeight + 0.1f);

            float height = hit2.point.Y + ctx.playerHeight + 0.3f;
            if (height < ctx.GlobalPosition.Y)
            {
                return;
            }

            ctx.GlobalPosition = new Vector3(ctx.GlobalPosition.X, Mathf.Lerp(ctx.GlobalPosition.Y, height, 0.2f), ctx.GlobalPosition.Z);
            ctx.steppedLastFrame = true;
        }
    }

}