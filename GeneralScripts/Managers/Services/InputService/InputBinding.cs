﻿using Godot;
using System;

public abstract class InputBinding
{
    public bool Active { get; protected set; }

    public event Action Started;
    public event Action Canceled;
    public event Action Hold;

    protected object value;
    protected object valueRaw;

    public T ReadValue<T>()
    {
        try
        {
            return (T)value;
        }
        catch (InvalidCastException ex)
        {
            GD.PrintErr($"ReadValue was used with an incorrect type. Expected {typeof(T)}, but got {value?.GetType().Name}:\n{ex}");
            return default;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"An unexpected error occurred in ReadValue:\n{ex}");
            return default;
        }
    }

    public T ReadValueRaw<T>()
    {
        try
        {
            return (T)valueRaw;
        }
        catch (InvalidCastException ex)
        {
            GD.PrintErr($"ReadValue was used with an incorrect type. Expected {typeof(T)}, but got {value?.GetType().Name}:\n{ex}");
            return default;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"An unexpected error occurred in ReadValue:\n{ex}");
            return default;
        }
    }

    public abstract void Update();

    protected void InvokePerformed()
    {
        Started?.Invoke();
    }

    protected void InvokeCanceled()
    {
        Canceled?.Invoke();
    }

    protected void InvokeHold()
    {
        Hold?.Invoke();
    }
}