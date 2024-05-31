using Godot;

public class VectorInputBinding : InputBinding
{
    private readonly string leftAction;
    private readonly string rightAction;
    private readonly string upAction;
    private readonly string downAction;

    public VectorInputBinding(string leftAction, string rightAction, string upAction, string downAction)
    {
        this.leftAction = leftAction;
        this.rightAction = rightAction;
        this.upAction = upAction;
        this.downAction = downAction;
    }

    public override void Update()
    {
        Vector2 compositeVector = Vector2.Zero;

        if (Input.IsActionPressed(leftAction))
        {
            compositeVector.X -= 1;
        }
        if (Input.IsActionPressed(rightAction))
        {
            compositeVector.X += 1;
        }
        if (Input.IsActionPressed(upAction))
        {
            compositeVector.Y -= 1;
        }
        if (Input.IsActionPressed(downAction))
        {
            compositeVector.Y += 1;
        }

        compositeVector = compositeVector.Normalized();

        bool wasActive = Active;
        Active = compositeVector.LengthSquared() > 0.01f;

        valueRaw = compositeVector;

        if (Active && !wasActive)
        {
            InvokePerformed();
        }
        else if (!Active && wasActive)
        {
            InvokeCanceled();
        }

        if (!Active)
        {
            value = Vector2.Zero;
        }
        else
        {
            value = Input.GetVector(leftAction, rightAction, upAction, downAction);
        }
    }
}