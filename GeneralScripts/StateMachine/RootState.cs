using Godot;
using System;

public abstract class RootState<T> : State<T> where T : Node3D
{
    public Type[] subStates;
    public abstract void InitializeSubState();
}