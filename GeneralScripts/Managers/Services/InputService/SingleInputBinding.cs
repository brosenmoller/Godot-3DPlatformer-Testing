using Godot;

public class SingleInputBinding : InputBinding
{
    private readonly string actionName;

    public SingleInputBinding(string actionName)
    {
        this.actionName = actionName;
    }

    public override void Update()
    {
        bool wasActive = Active;
        Active = Input.IsActionPressed(actionName);
        value = Input.GetActionStrength(actionName);
        valueRaw = Input.GetActionRawStrength(actionName);

        if (Active && !wasActive)
        {
            InvokePerformed();
        }
        else if (!Active && wasActive)
        {
            InvokeCanceled();
        }

        if (Active)
        {
            InvokeHold();
        }
    }
}