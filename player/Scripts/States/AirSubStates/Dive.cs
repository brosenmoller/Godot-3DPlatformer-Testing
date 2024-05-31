using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public class Dive : State<PlayerController>
    {
        private float diveForce = 80f;
        protected float diveUpForce = 0.07f;

        public override void OnEnter()
        {
            ctx.InvokeOnDive();

            ctx.slideDivePressCanTrigger = false;
            ctx.divesLeft--;

            ctx.velocityLibrary[PlayerVelocitySource.dive] = CalculateDesiredVelocityAdd(8, 3, PlayerVelocitySource.dive);
            //make the sure the normal max isn't down
            ctx.velocityLibrary[PlayerVelocitySource.normal] = ctx.defaultMaxVelocity;

            ctx.ZeroVerticalVelocity();
            ctx.Velocity += (ctx.Transform.Forward() + new Vector3(0, diveUpForce, 0)).Normalized() * diveForce;
            ctx.AirSpeedClamps();
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
