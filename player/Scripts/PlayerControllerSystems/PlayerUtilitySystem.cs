using Godot;

public partial class PlayerController
{
    public Vector3 ProjectOnSlope(Vector3 direction)
    {
        return direction.ProjectOntoPlane(SlopeNormal);
    }

    public void AddForceImmediate(Vector3 direction, float force)
    {
        Velocity += force * this.PhysicsDelta() * direction;
    }

    public Vector3 GetFlatVelocity()
    {
        Vector3 velocity = Velocity;
        velocity.Y = 0;
        return velocity;
    }

    public void ZeroVerticalVelocity()
    {
        Velocity = GetFlatVelocity();
    }

    public void ZeroVelocity()
    {
        Velocity = Vector3.Zero;
    }
}

