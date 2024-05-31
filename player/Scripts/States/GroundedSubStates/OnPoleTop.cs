using UnityEngine;
using System.Collections;
namespace PlayerStates
{
    public class OnPoleTop : State<PlayerController>
    {
        private CoroutineHandle MoveToPoleTop;
        public override void OnEnter()
        {
            ctx.InvokeOnPoleTop();

            ctx.Rigidbody.useGravity = false;
            MoveToPoleTop = ctx.StartCoroutine(SmoothMoveToPoleTop());
        }

        public override void OnExit()
        {
            ctx.Rigidbody.useGravity = true;
            if(MoveToPoleTop != null)
            {
                ctx.StopCoroutine(MoveToPoleTop);
                MoveToPoleTop = null;
            }
            ctx.poleLockTimer.Reset();
        }

        private IEnumerator SmoothMoveToPoleTop()
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
                time += Time.fixedDeltaTime;
                ctx.GlobalPosition = Vector3.Lerp(startPosition, newPosition, time / duration);
                yield return new WaitForFixedUpdate();
            }
            MoveToPoleTop = null;
        }
    }
}
