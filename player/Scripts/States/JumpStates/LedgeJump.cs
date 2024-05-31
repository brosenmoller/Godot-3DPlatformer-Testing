namespace PlayerStates
{
    public class LedgeJump : AirNormalJump
    {
        public override void OnEnter()
        {
            base.OnEnter();
            ctx.InvokeOnLedgeJump();
        }
    }
}
