using MEC;

namespace PlayerStates
{
    public class WallJumpLow : WallJump
    {
        public override void OnEnter()
        {
            ctx.InvokeOnWallJump();

            ctx.wallClimbsSinceGrounded++;
            ctx.canClimb = true;
            ctx.jumpCoroutine = Timing.RunCoroutine(WallJumpRoutine(ctx.maxJumpTime/2), Segment.PhysicsProcess);
            ctx.jumpPressCanTrigger = false;
        }
    }
}
