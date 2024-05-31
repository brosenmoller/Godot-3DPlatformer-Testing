namespace PlayerStates
{
    public class OnHangingPoint : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.InvokeOnHangingPoint();
            base.OnEnter();
        }
    }
}
