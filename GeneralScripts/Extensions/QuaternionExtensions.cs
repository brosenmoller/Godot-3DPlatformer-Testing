using Godot;

public static class QuaternionExtensions
{
    /// <summary>This is a slerp that mimics a camera operator's movement in that
    /// it chooses a path that avoids the lower hemisphere, as defined by
    /// the up param</summary>
    /// <param name="qA">First direction</param>
    /// <param name="qB">Second direction</param>
    /// <param name="t">Interpolation amount</param>
    /// <param name="up">Defines the up direction.  Must have a length of 1.</param>
    /// <returns>Interpolated quaternion</returns>
    public static Quaternion SlerpWithReferenceUp(
        Quaternion qA, Quaternion qB, float t, Vector3 up)
    {
        var dirA = (qA * Vector3.Forward).ProjectOntoPlane(up);
        var dirB = (qB * Vector3.Forward).ProjectOntoPlane(up);
        if (dirA.AlmostZero() || dirB.AlmostZero())
        {
            return qA.Slerp(qB, t);
        }
            

        // Work on the plane, in eulers
        var qBase = Transform3D.Identity.LookingAt(dirA, up).Basis.GetRotationQuaternion();
        var qBaseInv = qBase.Inverse();
        Quaternion qA1 = qBaseInv * qA;
        Quaternion qB1 = qBaseInv * qB;
        var eA = qA1.GetEuler();
        var eB = qB1.GetEuler();
        return qBase * Quaternion.FromEuler(new Vector3(
            Mathf.LerpAngle(eA.X, eB.X, t),
            Mathf.LerpAngle(eA.Y, eB.Y, t),
            Mathf.LerpAngle(eA.Z, eB.Z, t)
        ));
    }

    /// <summary>Normalize a quaternion</summary>
    /// <param name="q"></param>
    /// <returns>The normalized quaternion.  Unit length is 1.</returns>
    public static Quaternion Normalized(this Quaternion q)
    {
        Vector4 v = new Vector4(q.X, q.Y, q.Z, q.W).Normalized();
        return new Quaternion(v.X, v.Y, v.Z, v.W);
    }

    /// <summary>
    /// Get the rotations, first about world up, then about (travelling) local right,
    /// necessary to align the quaternion's forward with the target direction.
    /// This represents the tripod head movement needed to look at the target.
    /// This formulation makes it easy to interpolate without introducing spurious roll.
    /// </summary>
    /// <param name="orient"></param>
    /// <param name="lookAtDir">The worldspace target direction in which we want to look</param>
    /// <param name="worldUp">Which way is up.  Must have a length of 1.</param>
    /// <returns>Vector2.Y is rotation about worldUp, and Vector2.X is second rotation,
    /// about local right.</returns>
    public static Vector2 GetCameraRotationToTarget(
        this Quaternion orient, Vector3 lookAtDir, Vector3 worldUp)
    {
        if (lookAtDir.AlmostZero())
            return Vector2.Zero;  // degenerate

        // Work in local space
        Quaternion toLocal = orient.Inverse();
        Vector3 up = toLocal * worldUp;
        lookAtDir = toLocal * lookAtDir;

        // Align yaw based on world up
        float angleH = 0;
        {
            Vector3 targetDirH = lookAtDir.ProjectOntoPlane(up);
            if (!targetDirH.AlmostZero())
            {
                Vector3 currentDirH = Vector3.Forward.ProjectOntoPlane(up);
                if (currentDirH.AlmostZero())
                {
                    // We're looking at the north or south pole
                    if (currentDirH.Dot(up) > 0)
                        currentDirH = Vector3.Down.ProjectOntoPlane(up);
                    else
                        currentDirH = Vector3.Up.ProjectOntoPlane(up);
                }
                angleH = VectorExtensions.SignedAngle(currentDirH, targetDirH, up);
            }
        }
        Quaternion q = new(up, angleH);

        // Get local vertical angle
        float angleV = VectorExtensions.SignedAngle(q * Vector3.Forward, lookAtDir, q * Vector3.Right);

        return new Vector2(angleV, angleH);
    }

    /// <summary>
    /// Apply rotations, first about world up, then about (travelling) local right.
    /// rot.Y is rotation about worldUp, and rot.X is second rotation, about local right.
    /// </summary>
    /// <param name="orient"></param>
    /// <param name="rot">Vector2.Y is rotation about worldUp, and Vector2.X is second rotation,
    /// about local right.</param>
    /// <param name="worldUp">Which way is up</param>
    /// <returns>Result rotation after the input is applied to the input quaternion</returns>
    public static Quaternion ApplyCameraRotation(
        this Quaternion orient, Vector2 rot, Vector3 worldUp)
    {
        Quaternion q = new(Vector3.Right, rot.X);
        return (new Quaternion(worldUp, rot.Y) * orient) * q;
    }
}
