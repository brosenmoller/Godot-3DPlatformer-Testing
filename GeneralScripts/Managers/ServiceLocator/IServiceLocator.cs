using System;

public interface IServiceLocator
{
    /// <summary>
    /// Add a service to the locator, using the passed in instance to generate a Type key.
    /// </summary>
    /// <param name="service">The instance to be added as service to the locator.</param>
    /// <param name="key">Optional type key overload. GetType() doesn't return superclasses or interfaces when passing in a subclass.</param>
    public void Add(Service service, Type key = null);

    /// <summary>
    /// Remove a service from the locator, using the Type key to identify which instance should be removed.
    /// </summary>
    public void Remove(Type key);

    /// <returns>A service attached to the locator, if an instance of the specified generic type is present.</returns>
    public T Get<T>() where T : Service;
}
