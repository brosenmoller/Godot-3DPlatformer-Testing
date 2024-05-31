using Godot;

namespace PlayerStates
{
    public class SlideEnd : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.SpeedClamps();
        }

        public override void OnExit()
        {
            ctx.velocityLibrary[PlayerVelocitySource.slide] = Mathf.Clamp(ctx.velocityLibrary[PlayerVelocitySource.slide] - Slide.slideMaxVelocity,0,20);
        }
    }

}