namespace PlayerStates
{
    public class SlopeSlideJump : NormalJump
    {
        public override void OnEnter()
        {
            ctx.slopeJumpHeight = ctx.GlobalPosition.Y;
            base.OnEnter();
        }
    }
}
