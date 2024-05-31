
namespace PlayerStates
{
    public class Idle : State<PlayerController>
    {
        public override void OnPhysicsUpdate()
        {
            ctx.steppedLastFrame = false;
            ctx.SpeedClamps();
        }
    }
}
