using System.Collections.Generic;
using Godot;

namespace PlayerStates
{
    public class SolarDive : State<PlayerController>
    {
        private float diveForce = 100f;
        private const float solarDiveRotationSpeed = 0.03f;

        public override void OnEnter()
        {
            ctx.solarDiveTimer.Restart();

            ctx.velocityLibrary[PlayerVelocitySource.solarDive] = CalculateDesiredVelocityAdd(10, 5, PlayerVelocitySource.solarDive);
            //make the sure the normal max isn't down
            ctx.velocityLibrary[PlayerVelocitySource.normal] = ctx.defaultMaxVelocity;

            ctx.SetForward(ctx.SolarDiveDirection);

            ctx.ZeroVerticalVelocity();
            ctx.Velocity += ctx.Transform.Forward() * diveForce;
            ctx.UseGravity = false;
        }

        public override void OnPhysicsUpdate()
        {
            ctx.AirSpeedClamps();

            // Adjust player input rotation speed
            ctx.InputRotation(solarDiveRotationSpeed);
            ctx.VelocityRotation(solarDiveRotationSpeed);
        }

        public override void OnExit()
        {
            ctx.UseGravity = true;
            ctx.divesLeft = 1;
        }
        private float CalculateDesiredVelocityAdd(float desiredVelocity, float maxForceToAdd, PlayerVelocitySource source)
        {
            float totalVelocity = 0;
            foreach (KeyValuePair<PlayerVelocitySource, float> data in ctx.velocityLibrary)
            {
                if (data.Key == PlayerVelocitySource.normal || data.Key == source) { continue; }
                totalVelocity += data.Value;
            }

            return Mathf.Clamp(desiredVelocity - totalVelocity, 0, maxForceToAdd);
        }
    }
}
