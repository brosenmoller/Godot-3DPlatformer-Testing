using Godot;

namespace PlayerStates 
{
    public class GroundPoundLand : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.InvokeOnGroundPoundLand();

            ctx.groundPoundLandTimer.Restart();
            ctx.slopeJumpHeight = Mathf.Inf;
            ctx.ZeroVelocity();
        }
    }
}
