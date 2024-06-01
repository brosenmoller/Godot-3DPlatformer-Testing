using System.Collections.Generic;
using Godot;

public class InputService : Service
{
    public readonly Dictionary<string, InputBinding> Bindings = new();

    public InputService() 
    {
        GD.Print("SetupInput");
        Godot.Collections.Array<StringName> actions = InputMap.GetActions();

        for (int i = 0; i < actions.Count; i++)
        {
            string action = actions[i];

            if (!action.StartsWith("ui_"))
            {
                Bindings.Add(action, new SingleInputBinding(action));
            }
        }

        Bindings.Add("movement", new VectorInputBinding("left", "right", "up", "down"));
    }

    public override void OnUpdate(float delta)
    {
        foreach (var binding in Bindings)
        {
            binding.Value.Update();
        }
    }
}
