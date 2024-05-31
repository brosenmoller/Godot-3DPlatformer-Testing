using Godot;
using Godot.Collections;

public static class CameraExtension
{

    /// <summary>
    /// determines if a point with given bounds is visible by the camera's view frustum 
    /// </summary>
    //public static bool PointIsInFrustum(this Camera3D camera, Vector3 position, Vector3 boundSize)
    //{
    //    // Create an Axis-Aligned Bounding Box (AABB) from the position and size
    //    Aabb boundingBox = new(position - boundSize / 2, boundSize);

    //    // Get the frustum planes of the camera
    //    Array<Plane> frustumPlanes = camera.GetFrustum();

    //    // Check if the bounding box is inside the frustum planes
    //    foreach (Plane plane in frustumPlanes)
    //    {
    //        // Get the nearest and farthest points on the AABB relative to the plane normal
    //        Vector3 nearestPoint = boundingBox.GetSupport(plane.Normal);

    //        // Check if the nearest point is outside the plane
    //        if (plane.DistanceTo(nearestPoint) > 0)
    //        {
    //            // If the nearest point is outside the plane, the box is not inside the frustum
    //            return false;
    //        }
    //    }

    //    // If the AABB is inside all frustum planes, return true
     
    //    return true;
    //}

    public static bool PointIsInFrustum(this Camera3D camera, Vector3 position, Vector3 boundSize)
    {
        // Create an Axis-Aligned Bounding Box (AABB) from the position and size
        Aabb boundingBox = new(position - boundSize / 2, boundSize);

        // Get the frustum planes of the camera
        Array<Plane> frustumPlanes = camera.GetFrustum();

        // Check if the bounding box is inside the frustum planes
        foreach (Plane plane in frustumPlanes)
        {
            if (!IsAabbCornerInsideFrustum(boundingBox, plane))
            {
                return false;
            }
        }

        // If the AABB is inside all frustum planes, return true
        return true;
    }

    private static bool IsAabbCornerInsideFrustum(Aabb box, Plane plane)
    {
        // Check each corner of the AABB against the frustum plane
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new Vector3(
                (i & 1) == 0 ? box.Position.X - box.Size.X / 2 : box.Position.X + box.Size.X / 2,
                (i & 2) == 0 ? box.Position.Y - box.Size.Y / 2 : box.Position.Y + box.Size.Y / 2,
                (i & 4) == 0 ? box.Position.Z - box.Size.Z / 2 : box.Position.Z + box.Size.Z / 2
            );

            if (plane.DistanceTo(corner) > 0)
            {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// determines if a point with given bounds is visible by the camera's view frustum and not blocked by any colliders
    /// </summary>
    public static bool PointIsVisible(this Camera3D camera, Vector3 pos, Vector3 boundSize, bool collideWithAreas = true)
    {
        if (!camera.PointIsInFrustum(pos, boundSize))
        {
            return false;
        }

        return camera.RayCast3D(camera.GlobalPosition, pos, out _, collideWithAreas: collideWithAreas);
    }

    public static bool PointIsVisible(this Camera3D camera, Vector3 pos, Vector3 boundSize, uint layerMask, bool collideWithAreas = true)
    {
        if (!camera.PointIsInFrustum(pos, boundSize))
        {
            return false;
        }

        return camera.RayCast3D(camera.GlobalPosition, pos, out _, layerMask, collideWithAreas);
    }
}
