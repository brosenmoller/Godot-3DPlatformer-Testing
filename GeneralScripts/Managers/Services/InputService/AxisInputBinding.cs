using Godot;
using System;

public class AxisInputBinding : InputBinding
{
    private readonly string positiveAction;
    private readonly string negativeAction;

    public AxisInputBinding(string positiveAction, string negativeAction)
    {
        this.positiveAction = positiveAction;
        this.negativeAction = negativeAction;
    }

    public override void Update()
    {
        int rawValue = 0;
        if (Input.IsActionPressed(positiveAction))
        {
            rawValue += 1;
        }
        if (Input.IsActionPressed(negativeAction))
        {
            rawValue -= 1;
        }

        bool wasActive = Active;
        Active = rawValue != 0;

        valueRaw = rawValue;

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
            value = Input.GetAxis(negativeAction, positiveAction);
            InvokeHold();
        }
        else
        {
            value = 0f;
        }
    }
}
