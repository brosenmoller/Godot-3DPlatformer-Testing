using System.Collections.Generic;
using Godot;

public class InputService : Service
{
    public Dictionary<string, InputBinding> Bindings { get; private set; }

    public InputService() 
    {
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