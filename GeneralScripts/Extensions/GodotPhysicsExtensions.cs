using Godot.Collections;
using Godot;

public static class GodotPhysicsExtensions
{
    public static bool RayCast2D(this CanvasItem node, Vector2 startPosition, Vector2 direction, out RaycastHit2D hitInfo, float distance, uint layermask = 0xffffffff, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        PhysicsRayQueryParameters2D query = new()
        {
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies,
            HitFromInside = false,
            From = startPosition,
            To = startPosition + direction * distance,
            CollisionMask = layermask,
        };

        return RayCast2D(node, query, out hitInfo);
    }

    public static bool RayCast2D(this CanvasItem node, Vector2 startPosition, Vector2 endPosition, out RaycastHit2D hitInfo, uint layermask = 0xffffffff, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        PhysicsRayQueryParameters2D query = new()
        {
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies,
            HitFromInside = false,
            From = startPosition,
            To = endPosition,
            CollisionMask = layermask,
        };

        return RayCast2D(node, query, out hitInfo);
    }

    public static bool RayCast2D(this CanvasItem node, PhysicsRayQueryParameters2D query, out RaycastHit2D hitInfo)
    {
        PhysicsDirectSpaceState2D spaceState = node.GetWorld2D().DirectSpaceState;
        Dictionary result = spaceState.IntersectRay(query);

        hitInfo = new();

        if (result == null || result.Count <= 0)
        {
            return false;
        }

        foreach (Variant key in result.Keys)
        {
            switch (key.ToString())
            {
                case "position": hitInfo.point = (Vector2)result[key]; break;
                case "normal": hitInfo.normal = (Vector2)result[key]; break;
                case "collider": hitInfo.colliderInfo.colliderObject = (GodotObject)result[key]; break;
                case "collider_id": hitInfo.colliderInfo.colliderID = (int)result[key]; break;
                case "rid": hitInfo.colliderInfo.rid = (Rid)result[key]; break;
                case "shape": hitInfo.colliderInfo.shapeIndex = (int)result[key]; break;
            }
        }

        try
        {
            hitInfo.colliderInfo.collider = (CollisionObject2D)hitInfo.colliderInfo.colliderObject;
        }
        catch { }

        return true;
    }

    public static bool RayCast3D(this Node3D node, Vector3 startPosition, Vector3 direction, float distance, uint collisionMask = 0xffffffff, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        PhysicsRayQueryParameters3D query = new()
        {
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies,
            HitFromInside = false,
            From = startPosition,
            To = startPosition + direction * distance,
            CollisionMask = collisionMask,
        };

        return RayCast3D(node, query, out _);
    }

    public static bool RayCast3D(this Node3D node, Vector3 startPosition, Vector3 direction, out RaycastHit3D hitInfo, float distance, uint collisionMask = 0xffffffff, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        PhysicsRayQueryParameters3D query = new()
        {
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies,
            HitFromInside = false,
            From = startPosition,
            To = startPosition + direction * distance,
            CollisionMask = collisionMask,
        };

        return RayCast3D(node, query, out hitInfo);
    }

    public static bool RayCast3D(this Node3D node, Vector3 startPosition, Vector3 endPosition, out RaycastHit3D hitInfo, uint collisionMask = 0xffffffff, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        PhysicsRayQueryParameters3D query = new()
        {
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies,
            HitFromInside = false,
            From = startPosition,
            To = endPosition,
            CollisionMask = collisionMask,
        };

        return RayCast3D(node, query, out hitInfo);
    }

    public static bool RayCast3D(this Node3D node, PhysicsRayQueryParameters3D query, out RaycastHit3D hitInfo)
    {
        PhysicsDirectSpaceState3D spaceState = node.GetWorld3D().DirectSpaceState;
        Dictionary result = spaceState.IntersectRay(query);

        hitInfo = new();

        if (result == null || result.Count <= 0)
        {
            return false;
        }

        foreach (Variant key in result.Keys)
        {
            switch (key.ToString())
            {
                case "position": hitInfo.point = (Vector3)result[key]; break;
                case "normal": hitInfo.normal = (Vector3)result[key]; break;
                case "collider": hitInfo.colliderInfo.colliderObject = (GodotObject)result[key]; break;
                case "collider_id": hitInfo.colliderInfo.colliderID = (int)result[key]; break;
                case "rid": hitInfo.colliderInfo.rid = (Rid)result[key]; break;
                case "shape": hitInfo.colliderInfo.shapeIndex = (int)result[key]; break;
            }
        }

        try
        {
            hitInfo.colliderInfo.collider = (CollisionObject3D)hitInfo.colliderInfo.colliderObject;
        }
        catch
        {
            GD.PrintErr($"{node.Name} raycast's ColliderObject is Not CollisionObject3D");
        }

        return true;
    }

    public static bool SphereCast3D(this Node3D node, Vector3 startPosition, float radius, Vector3 endPosition, out ShapecastHit3D hitInfo, uint collisionMask = 0xffffffff, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        Rid shapeRid = PhysicsServer3D.SphereShapeCreate();
        PhysicsServer3D.ShapeSetData(shapeRid, radius);

        return StartShapeCast3D(shapeRid, node, startPosition, endPosition, out hitInfo, collisionMask, collideWithAreas, collideWithBodies);
    }

    public static bool CapsuleCast3D(this Node3D node, Vector3 startPosition, float radius, float height, Vector3 endPosition, out ShapecastHit3D hitInfo, uint collisionMask = 0xffffffff, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        Rid shapeRid = PhysicsServer3D.CapsuleShapeCreate();
        Dictionary capsuleDict = new()
        {
            { "radius", radius },
            { "height", height }
        };

        PhysicsServer3D.ShapeSetData(shapeRid, capsuleDict);

        return StartShapeCast3D(shapeRid, node, startPosition, endPosition, out hitInfo, collisionMask, collideWithAreas, collideWithBodies);
    }

    private static bool StartShapeCast3D(Rid shapeRid, Node3D node, Vector3 startPosition, Vector3 endPosition, out ShapecastHit3D hitInfo, uint collisionMask = 0xffffffff, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        Vector3 motion = endPosition - startPosition;
        Transform3D transform = new(new Basis(1, 0, 0, 0, 1, 0, 0, 0, 1), startPosition);

        PhysicsShapeQueryParameters3D query = new()
        {
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies,
            Motion = motion,
            Transform = transform,
            CollisionMask = collisionMask,
            ShapeRid = shapeRid,
        };

        bool result = ShapeCast3D(node, query, out hitInfo);

        PhysicsServer3D.FreeRid(shapeRid);

        return result;
    }

    public static bool ShapeCast3D(this Node3D node, PhysicsShapeQueryParameters3D query, out ShapecastHit3D hitInfo)
    {
        PhysicsDirectSpaceState3D spaceState = node.GetWorld3D().DirectSpaceState;
        float[] motionResult = spaceState.CastMotion(query);

        hitInfo = new();

        if (motionResult == null || motionResult.Length <= 0 || motionResult.Length > 2)
        {
            return false;
        }

        float safeProportion = motionResult[0];
        float unSafeProportion = motionResult[1];

        if (safeProportion > 0.99f && unSafeProportion > 0.99f)
        {
            return false;
        }

        Vector3 startPosition = query.Transform.Origin;
        float distance = query.Motion.Length();
        Vector3 direction = query.Motion / distance;

        hitInfo.lastSafeLocation = startPosition + distance * safeProportion * direction;

        PhysicsShapeQueryParameters3D restInfoQuery = query;
        restInfoQuery.Transform = new Transform3D(query.Transform.Basis, hitInfo.lastSafeLocation + direction * 0.01f);

        if (OverlapShape3D(node, restInfoQuery, out OverlapShape3D info))
        {
            hitInfo.overlapInfo = info;
        }

        return true;
    }

    public static bool OverlapSphere3D(this Node3D node, Vector3 position, float radius, out OverlapShape3D hitInfo, uint collisionMask = 0xffffffff, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        Rid shapeRid = PhysicsServer3D.SphereShapeCreate();
        PhysicsServer3D.ShapeSetData(shapeRid, radius);

        Transform3D transform = new(new Basis(1, 0, 0, 0, 1, 0, 0, 0, 1), position);

        PhysicsShapeQueryParameters3D query = new()
        {
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies,
            Transform = transform,
            CollisionMask = collisionMask,
            ShapeRid = shapeRid,
        };

        bool result = OverlapShape3D(node, query, out hitInfo);

        PhysicsServer3D.FreeRid(shapeRid);

        return result;
    }

    public static bool OverlapCapsule3D(this Node3D node, Vector3 position, float radius, float height, out OverlapShape3D hitInfo, uint collisionMask = 0xffffffff, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        Rid shapeRid = PhysicsServer3D.CapsuleShapeCreate();
        Dictionary capsuleDict = new()
        {
            { "radius", radius },
            { "height", height }
        };

        PhysicsServer3D.ShapeSetData(shapeRid, capsuleDict);

        Transform3D transform = new(new Basis(1, 0, 0, 0, 1, 0, 0, 0, 1), position);

        PhysicsShapeQueryParameters3D query = new()
        {
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies,
            Transform = transform,
            CollisionMask = collisionMask,
            ShapeRid = shapeRid,
        };

        bool result = OverlapShape3D(node, query, out hitInfo);

        PhysicsServer3D.FreeRid(shapeRid);

        return result;
    }

    public static bool OverlapShape3D(this Node3D node, PhysicsShapeQueryParameters3D query, out OverlapShape3D info)
    {
        PhysicsDirectSpaceState3D spaceState = node.GetWorld3D().DirectSpaceState;
        Dictionary restInfoResult = spaceState.GetRestInfo(query);
        Array<Dictionary> intersectResult = spaceState.IntersectShape(query);

        info = new();

        if (restInfoResult == null || restInfoResult.Count <= 0 || intersectResult == null || intersectResult.Count <= 0)
        {
            return false;
        }

        int closestColliderID = -1;

        foreach (Variant key in restInfoResult.Keys)
        {
            switch (key.ToString())
            {
                case "point": info.point = (Vector3)restInfoResult[key]; break;
                case "normal": info.normal = (Vector3)restInfoResult[key]; break;
                case "collider_id": closestColliderID = (int)restInfoResult[key]; break;
            }
        }

        info.allColliders = new ColliderInfo3D[intersectResult.Count];

        for (int i = 0; i < intersectResult.Count; i++)
        {
            Dictionary currentDict = intersectResult[i];

            foreach (Variant key in currentDict.Keys)
            {
                switch (key.ToString())
                {
                    case "collider": info.allColliders[i].colliderObject = (GodotObject)currentDict[key]; break;
                    case "collider_id": info.allColliders[i].colliderID = (int)currentDict[key]; break;
                    case "rid": info.allColliders[i].rid = (Rid)currentDict[key]; break;
                    case "shape": info.allColliders[i].shapeIndex = (int)currentDict[key]; break;
                }
            }

            try
            {
                info.allColliders[i].collider = (CollisionObject3D)info.allColliders[i].colliderObject;
            }
            catch 
            {
                GD.PrintErr($"{node.Name} raycast's ColliderObject is Not CollisionObject3D");
            }
        }

        int closestItemID = System.Array.FindIndex(info.allColliders, item => item.colliderID == closestColliderID);

        // Make sure the closest Collider is always the first in the list
        ColliderInfo3D currentFront = info.allColliders[0];
        if (currentFront.colliderID != closestColliderID)
        {
            info.allColliders[0] = info.allColliders[closestItemID];
            info.allColliders[closestItemID] = currentFront;
        }

        return true;
    }
}

