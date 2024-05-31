using Godot;

public abstract class State<T> where T : Node3D
{
    protected StateMachine<T> stateOwner;
    protected T ctx { get { return stateOwner.Controller; } }

    public void Setup(StateMachine<T> stateMachine)
    {
        stateOwner = stateMachine;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnPhysicsUpdate() { }
    public virtual void OnExit() { }
}
