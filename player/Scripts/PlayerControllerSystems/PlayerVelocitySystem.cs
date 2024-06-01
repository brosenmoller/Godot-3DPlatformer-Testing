using System.Collections.Generic;
using Godot;

public partial class PlayerController
{
    private void InitializeVelocitySystem()
    {
        velocityLibrary.Add(PlayerVelocitySource.normal, 7);
        maintainVelocityLibrary.Add(PlayerVelocitySource.normal, true);
        velocityLibrary.Add(PlayerVelocitySource.roll, 0);
        maintainVelocityLibrary.Add(PlayerVelocitySource.roll, false);
        velocityLibrary.Add(PlayerVelocitySource.slide, 0);
        maintainVelocityLibrary.Add(PlayerVelocitySource.slide, false);
        velocityLibrary.Add(PlayerVelocitySource.dive, 0);
        maintainVelocityLibrary.Add(PlayerVelocitySource.dive, false);
        velocityLibrary.Add(PlayerVelocitySource.grapple, 0);
        maintainVelocityLibrary.Add(PlayerVelocitySource.grapple, false);
        velocityLibrary.Add(PlayerVelocitySource.solarDive, 0);
        maintainVelocityLibrary.Add(PlayerVelocitySource.solarDive, true);
    }

    public void SpeedClamps()
    {
        Vector3 v = Velocity;

        if (SlopeAngle > 0)
        {
            if (v.Length() > maxVelocity)
            {
                Velocity = maxVelocity * v.Normalized();
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(v.X, 0, v.Z);

            if (flatVel.Length() > maxVelocity)
            {
                flatVel = maxVelocity * flatVel.Normalized();
            }

            v.X = flatVel.X;
            v.Z = flatVel.Z;
            v.Y = Mathf.Clamp(v.Y, -16, 18);
            Velocity = v;
        }
    }
    public void AirSpeedClamps()
    {
        Vector3 v = Velocity;

        Vector3 flatVel = new(v.X, 0, v.Z);

        if (flatVel.Length() > maxVelocity)
        {
            flatVel = maxVelocity * flatVel.Normalized();
        }

        v.X = flatVel.X;
        v.Z = flatVel.Z;
        v.Y = Mathf.Clamp(v.Y, -12, 18);
        Velocity = v;
    }

    public void UpdateVelocitySystem()
    {
        if (MovementInput != Vector2.Zero)
        {
            float velocity = Mathf.Clamp(defaultMaxVelocity * MovementInput.Length(), 0, defaultMaxVelocity);
            if (velocityLibrary[PlayerVelocitySource.normal] < velocity)
            {
                velocityLibrary[PlayerVelocitySource.normal] = velocity;
            }

            if (velocityLibrary[PlayerVelocitySource.normal] == velocity)
            {
                maintainVelocityLibrary[PlayerVelocitySource.normal] = true;
            }
            else
            {
                maintainVelocityLibrary[PlayerVelocitySource.normal] = false;
            }

        }
        else
        {
            maintainVelocityLibrary[PlayerVelocitySource.normal] = false;
        }

        float totalVelocity = 0;
        foreach (KeyValuePair<PlayerVelocitySource, float> data in velocityLibrary)
        {
            totalVelocity += data.Value;
        }
        MaxVelocity = totalVelocity;


        if (!maintainVelocityLibrary[PlayerVelocitySource.grapple] && velocityLibrary[PlayerVelocitySource.grapple] > 0)
        {
            ReduceVelocity(PlayerVelocitySource.grapple, 10);
        }
    }

    //if (grounded || onTopPole || onPole || hangingGrapplePoint)
    public void NormalVelocityReduction()
    {
        if(MovementInput == Vector2.Zero)
        {
            for(int i= 1; i < 7; i++)
            {
                if (!maintainVelocityLibrary[(PlayerVelocitySource)i])
                {
                    ReduceVelocity((PlayerVelocitySource)i, 44);
                }
            }
            return;
        }



        float highestVelocity = 0;
        PlayerVelocitySource key = PlayerVelocitySource.none;
        foreach (KeyValuePair<PlayerVelocitySource, float> data in velocityLibrary)
        {
            if (!maintainVelocityLibrary[data.Key] && sourceToRemove == PlayerVelocitySource.none)
            {
                if (data.Value > highestVelocity)
                {
                    key = data.Key;
                    highestVelocity = data.Value;
                }
            }
        }

        if (sourceToRemove != PlayerVelocitySource.none) { key = sourceToRemove; }
        else if (key != PlayerVelocitySource.none && key != PlayerVelocitySource.normal) { sourceToRemove = key; }

        if (key != PlayerVelocitySource.none)
        {
            if (sourceToRemove != PlayerVelocitySource.none) { key = sourceToRemove; }
            else if (key != PlayerVelocitySource.none && key != PlayerVelocitySource.normal) {      sourceToRemove = key; }

            if (key != PlayerVelocitySource.none)
            {
                
                ReduceVelocity(key, 3);

                if (velocityLibrary[key] <= 0)
                {
                     sourceToRemove = PlayerVelocitySource.none;
                }
            }
        }
    }

    public void ReduceVelocity(PlayerVelocitySource key, float mult)
    {
        velocityLibrary[key] -= this.ProcessDelta() * mult;
        if (velocityLibrary[key] <= 0)
        {
            velocityLibrary[key] = 0;
        }
    }

    //public void ReflectVelocity(Vector3 direction)
    //{
    //    //TODO just replace this with actual Collide and Slide
    //    //Detect if we are going to hit something and add reflect velocity to prevent sticking and push the player around it slightly
    //    //Pushing around no longer works
    //    if (Physics.CapsuleCast(GlobalPosition + new Vector3(0, 0.26f, 0), GlobalPosition + new Vector3(0, 0.9f, 0), 0.49f, direction, out var cHit, 0.2f, GroundLayer))
    //    {
    //        //maybe wrong
    //        Vector3 normal = cHit.GetCorrectNormalForSphere(direction);
    //        if (this.RayCast3D(GlobalPosition, Quaternion.AngleAxis(-45, Vector3.Up) * direction, 1) && this.RayCast3D(GlobalPosition, Quaternion.AngleAxis(45, Vector3.Up) * direction, 1))
    //        {
    //            List<Vector3> normals = new();
    //            for (int i = -5; i < 6; i++)
    //            {
    //                Vector3 newVectorDirection = Quaternion.AngleAxis(9 * i, Vector3.Up) * direction;
    //                if (this.RayCast3D(GlobalPosition, newVectorDirection, out var hit, 1))
    //                {
    //                    normals.Add(hit.normal);
    //                }
    //            }

    //            if (normals.Count != 0)
    //            {
    //                Vector3 averageNormal = new();
    //                foreach (Vector3 v3 in normals)
    //                {
    //                    averageNormal += v3;
    //                }
    //                averageNormal /= normals.Count;
    //                averageNormal.Y = 0;
    //                normal = averageNormal.Normalized();
    //            }
    //        }
    //        Vector3 flatVelocity = GetFlatVelocity();
    //        float originalMag = flatVelocity.Length();

    //        float scale = 1 - Vector3.Dot(new Vector3(cHit.normal.X, 0, cHit.normal.Z).Normalized(), -flatVelocity.Normalized());

    //        Vector3 reflect = Vector3.Reflect(flatVelocity, normal);
    //        reflect.Y = 0;
    //        flatVelocity = originalMag * scale * reflect.Normalized();
    //        flatVelocity.Y = Velocity.Y;
    //        Velocity = flatVelocity;
    //    }
    //}
}

