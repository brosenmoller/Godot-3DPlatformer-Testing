using System.Collections;
using UnityEngine;

namespace PlayerStates
{
    public class Slide : State<PlayerController>
    {
        public const float slideMaxVelocity = 7;
        private const float slideForce = 200000f;
        private const float slidingRotationSpeed = 0.03f;

        private CoroutineHandle slideVisualsCoroutine;

        public override void OnEnter()
        {
            ctx.InvokeOnSlide();

            ctx.slideDivePressCanTrigger = false;
            ctx.visuals.forward = ctx.Transform.Forward();
            ctx.VisualsDirection = ctx.Transform.Forward();
            ctx.slideTimer.Restart();

            ctx.velocityLibrary[PlayerVelocitySource.slide] = slideMaxVelocity;
            ctx.velocityLibrary[PlayerVelocitySource.normal] = ctx.defaultMaxVelocity;

            ctx.maintainVelocityLibrary[PlayerVelocitySource.slide] = true;
            ctx.maintainVelocityLibrary[PlayerVelocitySource.normal] = true;

            if (slideVisualsCoroutine != null)
            {
                ctx.StopCoroutine(slideVisualsCoroutine);
            }

            slideVisualsCoroutine = ctx.StartCoroutine(SlideRotation());
            ctx.playerCollider.height = 1;
            ctx.playerCollider.center = Vector3.Zero;
        }

        public override void OnPhysicsUpdate()
        {
            bool downSlope = Vector3.Dot(ctx.ProjectOnSlope(Vector3.Down), ctx.Velocity) > 0.5f;
            //Keep Sliding if it's down slope
            if (ctx.slideDivePressed && ctx.SlopeAngle > 15 && downSlope)
            {
                ctx.slideTimer.Restart();
            }


            ctx.velocityLibrary[PlayerVelocitySource.slide] += (downSlope ? 1 : -1) * ctx.SlopeAngle * 0.2f * Time.fixedDeltaTime;
            if (ctx.velocityLibrary[PlayerVelocitySource.slide] < 0) { ctx.velocityLibrary[PlayerVelocitySource.slide] = 0; }
            if (ctx.velocityLibrary[PlayerVelocitySource.slide] > 14) { ctx.velocityLibrary[PlayerVelocitySource.slide] = 14; }

            ctx.AddForceImmediate(ctx.Transform.Forward(), slideForce);
            ctx.bottomVisualsPivot.localRotation = Quaternion.Euler(-90, 0, 0);
            ctx.VisualsDirection = ctx.Transform.Forward();
            ctx.InputRotation(slidingRotationSpeed);
            ctx.GroundedColliderRotation();
            ctx.VelocityRotation(slidingRotationSpeed);
            ctx.SpeedClamps();
        }

        public override void OnExit()
        {
            ctx.playerCollider.height = 2;
            ctx.playerCollider.center = new Vector3(0, 0.5f, 0);
            ctx.bottomVisualsPivot.localRotation = Quaternion.Euler(0, 0, 0);

            ctx.maintainVelocityLibrary[PlayerVelocitySource.slide] = false;
            ctx.ZeroVerticalVelocity();
            //ctx.velocityLibrary[PlayerVelocitySource.slide] = 0;

            if (slideVisualsCoroutine != null)
            {
                ctx.StopCoroutine(slideVisualsCoroutine);
                slideVisualsCoroutine = ctx.StartCoroutine(SlideRotationBack());
            }
        }

        //prototype Visuals
        private IEnumerator SlideRotation()
        {
            float t = 0;
            Quaternion startRotation = Quaternion.Euler(0, 0, 0);
            Quaternion endRotation = Quaternion.Euler(-90, 0, 0);
            float duration = ctx.slideTimer.EndTime / 4;
            while (t < duration)
            {
                t += Time.deltaTime;
                Quaternion currentRotation = Quaternion.Lerp(startRotation, endRotation, t / duration);
                ctx.bottomVisualsPivot.localRotation = currentRotation;
                yield return null;
            }
            yield return new WaitForSeconds(duration * 2);
        }

        private IEnumerator SlideRotationBack()
        {
            float t = 0;
            Quaternion startRotation = ctx.bottomVisualsPivot.localRotation;
            Quaternion endRotation = Quaternion.Euler(0, 0, 0);
            float duration = ctx.slideTimer.EndTime / 3;
            while (t < duration)
            {
                t += Time.deltaTime;
                Quaternion currentRotation = Quaternion.Lerp(startRotation, endRotation, t / duration);
                ctx.bottomVisualsPivot.localRotation = currentRotation;
                yield return null;
            }
        }
    }

}