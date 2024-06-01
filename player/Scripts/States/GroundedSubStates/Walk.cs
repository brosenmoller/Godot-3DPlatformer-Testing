using Godot;

namespace PlayerStates
{
    public class Walk : State<PlayerController>
    {
        private const float stepHeight = 0.35f;
        private const float noInputVelocityReductionMultiplier = 5f;

        public override void OnEnter()
        {
            ctx.InvokeOnMove();

            ctx.slopeJumpHeight = Mathf.Inf;
        }

        public override void OnUpdate()
        {
            ctx.NormalVelocityReduction();
        }

        public override void OnPhysicsUpdate()
        {
            NoInputVelocityReduction();
            float rotationSpeed = ctx.GroundRotationBySpeed();
            ctx.InputRotation(rotationSpeed);
            ctx.GroundedColliderRotation();
            ctx.VisualsRotation();
            ctx.VelocityRotation(rotationSpeed);

            InputVelocityHandeling();
            ctx.SpeedClamps();
        }

        /// <summary>
        /// Adds and handles the velocity of the player based on the slope and Dot product
        /// </summary>
        private void InputVelocityHandeling()
        {
            if (ctx.SlopeAngle > 0)
            {
                if(ctx.MovementInput != Vector2.Zero)
                {
                    ctx.AddForceImmediate(Vector3.Down, 10);
                }
                ctx.AddForceImmediate(ctx.ProjectOnSlope(ctx.Transform.Forward()), ctx.moveSpeed * 5);
            }
            else
            {
                ctx.AddForceImmediate(ctx.Transform.Forward(), ctx.moveSpeed);
            }
                
            //ctx.ReflectVelocity(ctx.GetFlatVelocity());

            StepClimb();
        }

        private void NoInputVelocityReduction()
        {
            if (ctx.MovementInput.LengthSquared() < 0.0001f)
            {
                float newSpeed = ctx.Velocity.Length() - ctx.PhysicsDelta() * noInputVelocityReductionMultiplier;
                ctx.Velocity = newSpeed * ctx.Velocity.Normalized();
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
                    if (!ctx.RayCast3D(origin + stepHeight * Vector3.Up, direction, 0.3f))
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
