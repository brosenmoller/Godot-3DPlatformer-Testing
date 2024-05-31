namespace PlayerStates
{
    public class WallJumpLow : WallJump
    {
        public override void OnEnter()
        {
            ctx.InvokeOnWallJump();

            ctx.wallClimbsSinceGrounded++;
            ctx.canClimb = true;
            ctx.jumpCoroutine = ctx.StartCoroutine(WallJumpRoutine(ctx.maxJumpTime/2));
            ctx.jumpPressCanTrigger = false;
        }
    }
}
