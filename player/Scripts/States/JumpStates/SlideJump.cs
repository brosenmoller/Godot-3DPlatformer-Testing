using Godot;

namespace PlayerStates
{
    public class SlideJump : NormalJump
    {
        private float prevJumpForce;

        public override void OnEnter()
        {
            // Save previous jump force to restore it when exiting
            prevJumpForce = ctx.jumpForce;

            // Calculate jump force based on current slide velocity
            float currentVelocity = ctx.velocityLibrary[PlayerVelocitySource.slide];
            float i = 1 + Mathf.InverseLerp(7, 14, currentVelocity);
            ctx.jumpForce *= i;

            ctx.InvokeOnSlideJump();
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
            //ctx.velocityLibrary[PlayerVelocitySource.slide] = 0;

            // Reset jump force
            ctx.jumpForce = prevJumpForce;
        }
    }
}
