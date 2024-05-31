using System;
using UnityEngine;

namespace PlayerStates
{
    public class OnObjectRoot : RootState<PlayerController>
    {
        public OnObjectRoot()
        {
            subStates = new Type[]
            {
                typeof(LedgeGrab),
                typeof(OnHangingPoint),
                typeof(OnPole),
                typeof(WallSlideDown),
                typeof(WallSlideUp),
                typeof(WallJump),
                typeof(LedgeJump),
                typeof(PoleJump),
                typeof(WallJumpLow)
            };
        }

        public override void InitializeSubState()
        {

        }

        public override void OnUpdate()
        {
            if (ctx.velocityLibrary[PlayerVelocitySource.slide] > 0)
            {
                ctx.ReduceVelocity(PlayerVelocitySource.slide, 6);
            }
        }
    }
}
