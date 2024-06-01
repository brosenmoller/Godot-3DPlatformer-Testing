using Godot;

public struct RaycastHit2D
{
    public Vector2 point;
    public Vector2 normal;
    public float distance;
    public ColliderInfo2D colliderInfo;
}

public struct RaycastHit3D
{
    public Vector3 point;
    public Vector3 normal;
    public float distance;
    public ColliderInfo3D colliderInfo;
}

public struct ShapecastHit3D
{
    public Vector3 lastSafeLocation;
    public float distance;
    public OverlapShape3D overlapInfo;
}
public struct OverlapShape3D
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

