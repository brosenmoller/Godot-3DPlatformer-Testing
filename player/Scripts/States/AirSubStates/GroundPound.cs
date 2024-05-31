using UnityEngine;

namespace PlayerStates
{
    public class GroundPound : State<PlayerController>
    {
        private const float fallMultiplier = 10f;

        public override void OnEnter()
        {
            ctx.InvokeOnGroundPound();

            ctx.ZeroVelocity();
            ctx.groundPoundPressCanTrigger = false;
            if(ctx.fallingHeight == 0)
            {
                ctx.fallingHeight = ctx.GlobalPosition.Y;
            }
        }

        public override void OnPhysicsUpdate()
        {
            //Gravity Mod
            ModifyGravity();

            Collider[] colliders = Physics.OverlapBox(ctx.GlobalPosition, new Vector3(0.5f,1.2f,0.5f));
            foreach(Collider c in colliders)
            {
                if(c.tag == "Destructable")
                {
                    GameObject.Destroy(c.gameObject);
                }
            }


            float rotationSpeed = ctx.AirRotaionBySpeed();
            ctx.InputRotation(rotationSpeed);
            ctx.AirColliderRotation();
            ctx.VisualsRotation();
            ctx.VelocityRotation(rotationSpeed);
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
            if (ctx.Rigidbody.useGravity)
            {
                ctx.AddForceImmediate(Vector3.Up, Physics.gravity.Y * fallMultiplier);
            }
        }
    }
}
