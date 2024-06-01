using System;
using Godot;

public static class VectorExtensions
{
    /// <summary>A useful Epsilon</summary>
    public const float Epsilon = 0.0001f;

    /// <summary>
    /// Checks if the Vector2 contains NaN for x or y.
    /// </summary>
    /// <param name="v">Vector2 to check for NaN</param>
    /// <returns>True, if any components of the vector are NaN</returns>
    public static bool IsNaN(this Vector2 v)
    {
        return float.IsNaN(v.X) || float.IsNaN(v.Y);
    }

    /// <summary>
    /// Checks if the Vector2 contains NaN for x or y.
    /// </summary>
    /// <param name="v">Vector2 to check for NaN</param>
    /// <returns>True, if any components of the vector are NaN</returns>
    public static bool IsNaN(this Vector3 v)
    {
        return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
    }


    /// <summary>
    /// Get the closest point on a line segment.
    /// </summary>
    /// <param name="p">A point in space</param>
    /// <param name="s0">Start of line segment</param>
    /// <param name="s1">End of line segment</param>
    /// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
    public static float ClosestPointOnSegment(this Vector3 p, Vector3 s0, Vector3 s1)
    {
        Vector3 s = s1 - s0;
        float len2 = s.LengthSquared();

        if (len2 < Epsilon)
        {
            return 0; // degenrate segment
        }
            
        return Mathf.Clamp(s.Dot(p - s0) / len2, 0, 1);
    }

    /// <summary>
    /// Get the closest point on a line segment.
    /// </summary>
    /// <param name="p">A point in space</param>
    /// <param name="s0">Start of line segment</param>
    /// <param name="s1">End of line segment</param>
    /// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
    public static float ClosestPointOnSegment(this Vector2 p, Vector2 s0, Vector2 s1)
    {
        Vector2 s = s1 - s0;
        float len2 = s.LengthSquared();
        if (len2 < Epsilon)
            return 0; // degenrate segment
        return Mathf.Clamp(s.Dot(p - s0) / len2, 0, 1);
    }

    /// <summary>
    /// Returns a non-normalized projection of the supplied vector onto a plane
    /// as described by its normal
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="planeNormal">The normal that defines the plane.  Must have a length of 1.</param>
    /// <returns>The component of the vector that lies in the plane</returns>
    public static Vector3 ProjectOntoPlane(this Vector3 vector, Vector3 planeNormal)
    {
        return (vector - planeNormal.Dot(vector) * planeNormal);
    }

    /// <summary>
    /// Normalized the vector onto the unit square instead of the unit circle
    /// </summary>
    /// <param name="v">The vector to normalize</param>
    /// <returns>The normalized vector, or the zero vector if its magnitude 
    /// was too small to normalize</returns>
    public static Vector2 SquareNormalize(this Vector2 v)
    {
        var d = Mathf.Max(Mathf.Abs(v.X), Mathf.Abs(v.Y));
        return d < Epsilon ? Vector2.Zero : v / d;
    }

    /// <summary>
    /// Calculates the intersection point defined by line_1 [p1, p2], and line_2 [q1, q2].
    /// </summary>
    /// <param name="p1">line_1 is defined by (p1, p2)</param>
    /// <param name="p2">line_1 is defined by (p1, p2)</param>
    /// <param name="q1">line_2 is defined by (q1, q2)</param>
    /// <param name="q2">line_2 is defined by (q1, q2)</param>
    /// <param name="intersection">If lines intersect at a single point, 
    /// then this will hold the intersection point. 
    /// Otherwise, it will be Vector2.positiveInfinity.</param>
    /// <returns>
    ///     0 = no intersection, 
    ///     1 = lines intersect, 
    ///     2 = segments intersect, 
    ///     3 = lines are colinear, segments do not touch, 
    ///     4 = lines are colinear, segments touch (at one or at multiple points)
    /// </returns>
    public static int FindIntersection(
        in Vector2 p1, in Vector2 p2, in Vector2 q1, in Vector2 q2,
        out Vector2 intersection)
    {
        var p = p2 - p1;
        var q = q2 - q1;
        var pq = q1 - p1;
        var pXq = p.Cross(q);
        if (Mathf.Abs(pXq) < 0.00001f)
        {
            // The lines are parallel (or close enough to it)
            intersection = Vector2.Inf;
            if (Mathf.Abs(pq.Cross(p)) < 0.00001f)
            {
                // The lines are colinear.  Do the segments touch?
                var dotPQ = Vector2.Dot(q, p);

                if (dotPQ > 0 && (p1 - q2).LengthSquared() < 0.001f)
                {
                    // q points to start of p
                    intersection = q2;
                    return 4;
                }
                if (dotPQ < 0 && (p2 - q2).LengthSquared() < 0.001f)
                {
                    // p and q point at the same point
                    intersection = p2;
                    return 4;
                }

                var dot = p.Dot(pq);
                if (0 <= dot && dot <= p.Dot(p))
                {
                    if (dot < 0.0001f)
                    {
                        if (dotPQ <= 0 && (p1 - q1).LengthSquared() < 0.001f)
                            intersection = p1; // p and q start at the same point and point away
                    }
                    else if (dotPQ > 0 && (p2 - q1).LengthSquared() < 0.001f)
                        intersection = p2; // p points at start of q

                    return 4;   // colinear segments touch
                }

                dot = q.Dot(p1 - q1);
                if (0 <= dot && dot <= q.Dot(q))
                    return 4;   // colinear segments overlap

                return 3;   // colinear segments don't touch
            }
            return 0; // the lines are parallel and not colinear
        }

        var t = pq.Cross(q) / pXq;
        intersection = p1 + t * p;

        var u = pq.Cross(p) / pXq;
        if (0 <= t && t <= 1 && 0 <= u && u <= 1)
            return 2;   // segments touch

        return 1;   // segments don't touch but lines intersect
    }

    private static float Cross(this Vector2 v1, Vector2 v2) { return (v1.X * v2.Y) - (v1.Y * v2.X); }

    /// <summary>
    /// Component-wise absolute value
    /// </summary>
    /// <param name="v">Input vector</param>
    /// <returns>Component-wise absolute value of the input vector</returns>
    public static Vector2 Abs(this Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.X), Mathf.Abs(v.Y));
    }

    /// <summary>
    /// Component-wise absolute value
    /// </summary>
    /// <param name="v">Input vector</param>
    /// <returns>Component-wise absolute value of the input vector</returns>
    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.X), Mathf.Abs(v.Y), Mathf.Abs(v.Z));
    }

    /// <summary>
    /// Checks whether the vector components are the same value.
    /// </summary>
    /// <param name="v">Vector to check</param>
    /// <returns>True, if the vector elements are the same. False, otherwise.</returns>
    public static bool IsUniform(this Vector2 v)
    {
        return Math.Abs(v.X - v.Y) < Epsilon;
    }

    /// <summary>
    /// Checks whether the vector components are the same value.
    /// </summary>
    /// <param name="v">Vector to check</param>
    /// <returns>True, if the vector elements are the same. False, otherwise.</returns>
    public static bool IsUniform(this Vector3 v)
    {
        return Math.Abs(v.X - v.Y) < Epsilon && Math.Abs(v.X - v.Z) < Epsilon;
    }

    /// <summary>Is the vector within Epsilon of zero length?</summary>
    /// <param name="v"></param>
    /// <returns>True if the square magnitude of the vector is within Epsilon of zero</returns>
    public static bool AlmostZero(this Vector3 v)
    {
        return v.LengthSquared() < (Epsilon * Epsilon);
    }

    /// <summary>Much more stable for small angles than Unity's native implementation</summary>
    /// <param name="v1">The first vector</param>
    /// <param name="v2">The second vector</param>
    /// <returns>Angle between the vectors, in degrees</returns>
    public static float Angle(Vector3 v1, Vector3 v2)
    {
#if false // Maybe this version is better?  to test....
            float a = v1.Length();
            v1 *= v2.Length();
            v2 *= a;
            return Mathf.Atan2((v1 - v2).Length(), (v1 + v2).Length()) * Mathf.Rad2Deg * 2;
#else
        v1 = v1.Normalized();
        v2 = v2.Normalized();
        return Mathf.RadToDeg(Mathf.Atan2((v1 - v2).Length(), (v1 + v2).Length())) * 2;
#endif
    }

    /// <summary>Much more stable for small angles than Unity's native implementation</summary>
    /// <param name="v1">The first vector</param>
    /// <param name="v2">The second vector</param>
    /// <param name="up">Definition of up (used to determine the sign)</param>
    /// <returns>Signed angle between the vectors, in degrees</returns>
    public static float SignedAngle(Vector3 v1, Vector3 v2, Vector3 up)
    {
        float angle = Angle(v1, v2);
        if (Mathf.Sign(up.Dot(v1.Cross(v2))) < 0)
        {
            return -angle;
        }
            
        return angle;
    }

    /// <summary>Much more stable for small angles than Unity's native implementation</summary>
    /// <param name="v1">The first vector</param>
    /// <param name="v2">The second vector</param>
    /// <param name="up">Definition of up (used to determine the sign)</param>
    /// <returns>Rotation between the vectors</returns>
    public static Quaternion SafeFromToRotation(Vector3 v1, Vector3 v2, Vector3 up)
    {
        var axis = v1.Cross(v2);
        if (axis.AlmostZero())
        {
            axis = up; // in case they are pointing in opposite directions
        }
        return new Quaternion(axis, Angle(v1, v2));
    }

    /// <summary>This is a slerp that mimics a camera operator's movement in that
    /// it chooses a path that avoids the lower hemisphere, as defined by
    /// the up param</summary>
    /// <param name="vA">First direction</param>
    /// <param name="vB">Second direction</param>
    /// <param name="t">Interpolation amoun t</param>
    /// <param name="up">Defines the up direction</param>
    /// <returns>Interpolated vector</returns>
    public static Vector3 SlerpWithReferenceUp(
        Vector3 vA, Vector3 vB, float t, Vector3 up)
    {
        float dA = vA.Length();
        float dB = vB.Length();

        if (dA < Epsilon || dB < Epsilon)
        {
            return vA.Lerp(vB, t);
        }

        Vector3 dirA = vA / dA;
        Vector3 dirB = vB / dB;
        Quaternion qA = Transform3D.Identity.LookingAt(dirA, up).Basis.GetRotationQuaternion();
        Quaternion qB = Transform3D.Identity.LookingAt(dirB, up).Basis.GetRotationQuaternion();
        Quaternion q = QuaternionExtensions.SlerpWithReferenceUp(qA, qB, t, up);
        Vector3 dir = q * Vector3.Forward;
        return dir * Mathf.Lerp(dA, dB, t);
    }

    public static Vector2 GetPerpendicular(this Vector2 v)
    {
        return new Vector2(-v.Y, v.X);
    }

    public static bool ApproximatelyEqual(this Vector3 v1, Vector3 v2)
    {
        return Mathf.Abs(v1.X - v2.X) < Epsilon
            && Mathf.Abs(v1.Y - v2.Y) < Epsilon
            && Mathf.Abs(v1.Z - v2.Z) < Epsilon;
    }

    public static Vector2 XZ(this Vector3 v)
    {
        return new Vector2(v.X, v.Z);
    }

    public static Vector2 XY(this Vector3 v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static Vector2 YZ(this Vector3 v)
    {
        return new Vector2(v.Y, v.Z);
    }

    public static Vector3 ZeroX(this Vector3 v)
    {
        return new Vector3(0, v.Y, v.Z);
    }

    public static Vector3 ZeroY(this Vector3 v)
    {
        return new Vector3(v.X, 0, v.Z);
    }

    public static Vector3 ZeroZ(this Vector3 v)
    {
        return new Vector3(v.X, v.Y, 0);
    }

    public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = Mathf.Inf)
    {
        smoothTime = Mathf.Max(0.0001f, smoothTime);
        float num = 2f / smoothTime;
        float num2 = num * deltaTime;
        float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
        float num4 = current - target;
        float num5 = target;
        float num6 = maxSpeed * smoothTime;
        num4 = Mathf.Clamp(num4, -num6, num6);
        target = current - num4;
        float num7 = (currentVelocity + num * num4) * deltaTime;
        currentVelocity = (currentVelocity - num * num7) * num3;
        float num8 = target + (num4 + num7) * num3;
        if (num5 - current > 0f == num8 > num5)
        {
            num8 = num5;
            currentVelocity = (num8 - num5) / deltaTime;
        }
        return num8;
    }

    public static Vector3 SmoothDamp(this Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float deltaTime, float maxSpeed = Mathf.Inf)
    {
        return new Vector3(
            SmoothDamp(current.X, target.X, ref currentVelocity.X, smoothTime, deltaTime, maxSpeed),
            SmoothDamp(current.Y, target.Y, ref currentVelocity.Y, smoothTime, deltaTime, maxSpeed),
            SmoothDamp(current.Z, target.Z, ref currentVelocity.Z, smoothTime, deltaTime, maxSpeed)
        );
    }
}
