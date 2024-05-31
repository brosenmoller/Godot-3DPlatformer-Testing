using System;
using System.Linq;

namespace PlayerStates
{
    public class AirRoot : RootState<PlayerController>
    {
        public AirRoot()
        {
            subStates = new Type[]
            {
                typeof(AirMovement),
                typeof(Dive),
                typeof(SolarDive),
                typeof(GrapplePull),
                typeof(GrappleSwing),
                typeof(GroundPound),
                typeof(PostJump),
                typeof(Swing),
                typeof(AirNormalJump),
            };
        }

        public override void InitializeSubState()
        {
            if (((RootState<PlayerController>)stateOwner.stateDictionary[typeof(GroundedRoot)])
                .subStates.Contains(stateOwner.currentState.GetType()))
            {
                if (stateOwner.currentState.GetType() != typeof(SlopeSlide))
                {
                    ctx.coyoteTimer.Restart();
                }
            }

            if (stateOwner.currentState is Slide)
            {
                ctx.slideCoyoteTime = true;
            }

            ctx.fallingHeight = ctx.GlobalPosition.Y;
            stateOwner.ChangeState(typeof(AirMovement));
        }

        public override void OnEnter()
        {
            ctx.InvokeOnAir();
        }
    }
}
