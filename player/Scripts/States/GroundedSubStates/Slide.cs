using System.Collections;
using Godot;
using MEC;

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
            ctx.visuals.SetForward(ctx.Transform.Forward());
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
            //ctx.PlayerCollider.height = 1;
            //ctx.PlayerCollider.center = Vector3.Zero;
        }

        public override void OnPhysicsUpdate()
        {
            bool downSlope = ctx.Velocity.Dot(ctx.ProjectOnSlope(Vector3.Down)) > 0.5f;

            //Keep Sliding if it's down slope
            if (ctx.slideDivePressed && ctx.SlopeAngle > 15 && downSlope)
            {
                ctx.slideTimer.Restart();
            }


            ctx.velocityLibrary[PlayerVelocitySource.slide] += (downSlope ? 1 : -1) * ctx.SlopeAngle * 0.2f * ctx.PhysicsDelta();
            if (ctx.velocityLibrary[PlayerVelocitySource.slide] < 0) { ctx.velocityLibrary[PlayerVelocitySource.slide] = 0; }
            if (ctx.velocityLibrary[PlayerVelocitySource.slide] > 14) { ctx.velocityLibrary[PlayerVelocitySource.slide] = 14; }

            ctx.AddForceImmediate(ctx.Transform.Forward(), slideForce);
            
            ctx.bottomVisualsPivot.Basis = new Basis(Quaternion.FromEuler(new Vector3(-90, 0, 0)));

            ctx.VisualsDirection = ctx.Transform.Forward();
            ctx.InputRotation(slidingRotationSpeed);
            ctx.GroundedColliderRotation();
            ctx.VelocityRotation(slidingRotationSpeed);
            ctx.SpeedClamps();
        }

        public override void OnExit()
        {
            //ctx.PlayerCollider.height = 2;
            //ctx.PlayerCollider.center = new Vector3(0, 0.5f, 0);
            ctx.bottomVisualsPivot.Basis = new Basis(Quaternion.FromEuler(Vector3.Zero));

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
        private IEnumerator<double> SlideRotation()
        {
            float t = 0;

            Quaternion startRotation = Quaternion.Identity;
            Quaternion endRotation = Quaternion.FromEuler(new Vector3(-90, 0, 0));

            float duration = ctx.slideTimer.EndTime / 4;
            while (t < duration)
            {
                t += ctx.ProcessDelta();
                Quaternion currentRotation = startRotation.Slerp(endRotation, t / duration);

                ctx.bottomVisualsPivot.Basis = new Basis(currentRotation);

                yield return null;
            }
            yield return Timing.WaitForSeconds(duration * 2);
        }

        private IEnumerator<double> SlideRotationBack()
        {
            float t = 0;
            Quaternion startRotation = ctx.bottomVisualsPivot.Basis.GetRotationQuaternion();
            Quaternion endRotation = Quaternion.Identity;

            float duration = ctx.slideTimer.EndTime / 3;
            while (t < duration)
            {
                t += ctx.ProcessDelta();
                Quaternion currentRotation = startRotation.Slerp(endRotation, t / duration);
                ctx.bottomVisualsPivot.Basis = new Basis(currentRotation);
                yield return null;
            }
        }
    }

}