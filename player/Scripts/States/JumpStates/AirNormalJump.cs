namespace PlayerStates
{
    public class AirNormalJump : NormalJump
    {
        public override void OnEnter()
        {
            if (ctx.slideCoyoteTime)
            {
                ctx.slideCoyoteTime = false;
                ctx.InvokeOnSlideJump();
            }

            base.OnEnter();
        }
    }
}
