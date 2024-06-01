using Godot;
using System.Collections.Generic;
using MEC;

namespace PlayerStates
{
    public class OnPoleTop : State<PlayerController>
    {
        private CoroutineHandle MoveToPoleTop;

        public override void OnEnter()
        {
            ctx.InvokeOnPoleTop();

            ctx.UseGravity = false;
            MoveToPoleTop = Timing.RunCoroutine(SmoothMoveToPoleTop(), Segment.PhysicsProcess);
        }

        public override void OnExit()
        {
            ctx.UseGravity = true;

            if(MoveToPoleTop != null)
            {
                ctx.StopCoroutine(MoveToPoleTop);
                MoveToPoleTop = null;
            }

            ctx.poleLockTimer.Reset();
        }

        private IEnumerator<double> SmoothMoveToPoleTop()
        {
            Vector3 newPosition = ctx.PoleTopCollider.GlobalPosition;
            Vector3 startPosition = ctx.GlobalPosition;
            newPosition.Y += 0.5f;
            float time = 0;
            float duration = 0.05f;
            ctx.Velocity = Vector3.Zero;
            while (time < duration)
            {
                ctx.Velocity = Vector3.Zero;
                time += ctx.PhysicsDelta();
                ctx.GlobalPosition = startPosition.Lerp(newPosition, time / duration);

                yield return Timing.WaitForOneFrame;
            }

            MoveToPoleTop = null;
        }
    }
}
