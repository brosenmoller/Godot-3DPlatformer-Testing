using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    private ServiceLocator serviceLocator;

    public override void _Ready()
    {
        GD.Print("GameManager Start");
        serviceLocator = new ServiceLocator();
        ServiceSetup();

        Instance ??= this;
    }

    private void ServiceSetup()
    {
        serviceLocator.Add(new InputService());
        serviceLocator.Add(new TimerService());
    }

    public override void _PhysicsProcess(double delta)
    {
        serviceLocator.PhysicsUpdate((float)delta);
    
    }

    public override void _Process(double delta)
    {
        serviceLocator.Update((float)delta);
    }
}
