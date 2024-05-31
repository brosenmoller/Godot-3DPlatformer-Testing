using System;
using System.Collections.Generic;
using Godot;

public class ServiceLocator : IServiceLocator
{
    public static IServiceLocator Instance { get; protected set; }


    private readonly Dictionary<Type, Service> services = new();

    public ServiceLocator()
    {
        if (Instance != null)
        {
            GD.Print("Other instance of a service locator already exists.");
            return;
        }
        Instance = this;
    }

    public void Add(Service service, Type key = null)
    {
        if (key == null) { key = service.GetType(); }

        if (services.ContainsKey(key))
        {
            GD.Print($"Key: {key} already present in the service pool.");
            return;
        }
        services.Add(key, service);
    }

    public void Remove(Type key)
    {
        if (!services.ContainsKey(key))
        {
            GD.Print($"Key: {key} is not present in the service pool.");
            return;
        }
        services.Remove(key);
    }

    public T Get<T>() where T : Service
    {
        Type key = typeof(T);

        if (services.ContainsKey(key))
        {
            return (T)services[key];
        }
        else
        {
            GD.PrintErr($"Key: {key} did not return a valid service.");
            return default;
        }
    }

    public void OnSceneLoaded()
    {
        foreach (Service service in services.Values)
        {
            service.OnSceneLoad();
        }
    }

    public void PhysicsUpdate(float delta)
    {
        foreach (Service service in services.Values)
        {
            service.OnPhysicsUpdate(delta);
        }
    }

    public void Update(float delta)
    {
        foreach (Service service in services.Values)
        {
            service.OnUpdate(delta);
        }
    }
}
