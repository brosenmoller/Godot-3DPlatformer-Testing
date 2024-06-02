using Godot;
using MEC;
using System.Collections.Generic;

public partial class PlayerController
{
    public void InputRotation(float rotationSpeed)
    {
        if (lockRotation) { return; }

        if (InputDirection.Length() >= 0.1f)
        {
            //rotate the player to the direction of the input.
            float x = Mathf.Lerp(Transform.Forward().X, InputDirection.Normalized().X, rotationSpeed);
            float z = Mathf.Lerp(Transform.Forward().Z, InputDirection.Normalized().Z, rotationSpeed);
            this.SetForward(new Vector3(x, Transform.Forward().Y, z).Normalized());
        }
    }

    public void VisualsRotation()
    {
        visuals.SetForward(-VisualsDirection);
        float x = Mathf.Lerp(visuals.Transform.Forward().X, Transform.Forward().X, 1.1f);
        float z = Mathf.Lerp(visuals.Transform.Forward().Z, Transform.Forward().Z, 1.1f);
        visuals.SetForward(-new Vector3(x, 0, z));

        VisualsDirection = visuals.Transform.Forward();
    }

    public float GroundRotationBySpeed()
    {
        //Vector3 flatVelocity = GetFlatVelocity();

        //if (flatVelocity.Length() > 6)
        //{
        //    return Mathf.Lerp(rotationSmoothMin, 0.05f, flatVelocity.Length() / defaultMaxVelocity);
        //}
        //return Mathf.Lerp(rotationSmoothMin, rotationSmoothMax, flatVelocity.Length() / defaultMaxVelocity);
        return 0.5f;
    }

    public float AirRotaionBySpeed()
    {
        //Vector3 flatVelocity = GetFlatVelocity();
        //return Mathf.Lerp(1, 0.15f, flatVelocity.Length() / defaultMaxVelocity);
        return 0.7f;
    }

    public void VelocityRotation(float rotationSpeed)
    {
        //Change the existing velocity to the turned direction
        //like a car would when you turn its wheels
        Vector3 flatVelocity = GetFlatVelocity();

        //uses Transform Forward() because the player always moves in the direction it is facing
        flatVelocity = flatVelocity.Lerp(Transform.Forward() * flatVelocity.Length(), rotationSpeed);
        flatVelocity.Y = Velocity.Y;
        Velocity = flatVelocity;
    }

    public void GroundedColliderRotation()
    {
        Vector3 slopeForward = ProjectOnSlope(Transform.Forward()).Normalized();

        if (SlopeAngle > 0 && slopeForward != Vector3.Zero)
        {
            if (SlopeAngle < maxSlopeAngle)
            {
                this.SetForward(slopeForward);
            }
        }
        else
        {
            AirColliderRotation();
        }
    }

    public void AirColliderRotation()
    {
        //clear of angles to prevent angle when not moving and jumping or falling
        this.SetForward(Transform.Forward().ProjectOntoPlane(Vector3.Up));
    }


    public IEnumerator<double> RotateTo(Vector3 direction)
    {
        Vector3 currentForward = Transform.Forward();
        float time = 0;
        while (time < 0.15f)
        {
            time += this.PhysicsDelta();
            Vector3 dir = currentForward.Lerp(direction, time / 0.15f);

            this.SetForward(dir);
            visuals.SetForward(dir);

            VisualsDirection = dir;

            yield return Timing.WaitForOneFrame;
        }

        this.SetForward(direction);
        visuals.SetForward(direction);

        VisualsDirection = direction;
    }
}

