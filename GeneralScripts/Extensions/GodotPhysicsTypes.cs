using Godot;

public struct RayCastHitInfo2D
{
    public Vector2 point;
    public Vector2 normal;
    public ColliderInfo2D colliderInfo;
}

public struct RayCastHitInfo3D
{
    public Vector3 point;
    public Vector3 normal;
    public ColliderInfo3D colliderInfo;
}

public struct ShapeCastHitInfo3D
{
    public Vector3 lastSafeLocation;
    public OverlapShapeInfo3D overlapInfo;
}
public struct OverlapShapeInfo3D
{
    public Vector3 point;
    public Vector3 normal;
    public ColliderInfo3D[] allColliders;
}

public struct ColliderInfo3D
{
    public CollisionObject3D collider;
    public GodotObject colliderObject;
    public int colliderID;
    public Rid rid;
    public int shapeIndex;
}

public struct ColliderInfo2D
{
    public CollisionObject2D collider;
    public GodotObject colliderObject;
    public int colliderID;
    public Rid rid;
    public int shapeIndex;
}

