using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class StateMachine<T> where T : Node3D
{
    public Dictionary<Type, State<T>> stateDictionary = new();
    private List<Transition> allTransitions = new();
    private List<Transition> activeTransitions = new();

    public RootState<T>[] rootStates;

    public State<T> currentState;
    public RootState<T> currentRootState;

    public bool DebugEnabled {  get; set; }

    public T Controller { get; private set; }

    public StateMachine(T owner, RootState<T>[] rootStates, params State<T>[] states)
    {
        this.rootStates = rootStates;
        Controller = owner;

        for (int i = 0; i < rootStates.Length; i++) 
        {
            stateDictionary.Add(rootStates[i].GetType(), rootStates[i]);
            rootStates[i].Setup(this);
        }

        foreach (State<T> state in states)
        {
            stateDictionary.Add(state.GetType(), state);
            state.Setup(this);
        }

        // --------- Remove For final release -----------
        foreach (State<T> state in states)
        {
            FindRootforSubState(state.GetType());
        }
        // end remove section
    }

    public void ChangeState(Type newStateType)
    {
        if (!stateDictionary.ContainsKey(newStateType))
        {
            GD.Print($"{newStateType.Name} is not a state in the current statemachine ({nameof(T)})");
            return;
        }

        State<T> newState = stateDictionary[newStateType];

        if (rootStates.Contains(newState) && (currentRootState == null || newStateType != currentRootState.GetType()))
        {
            ChangeRootState(newState);
        }
        else
        {
            ChangeSubState(newStateType);
        }
    }

    private void ChangeRootState(State<T> newState, bool InitializeSubState = true)
    {
        if (DebugEnabled && currentRootState != null) { GD.Print("StateMachine: Exit Rootstate " + currentRootState.GetType().ToString()); }
        
        currentRootState?.OnExit();
        currentRootState = (RootState<T>)newState;
        currentRootState.OnEnter();
        
        if (DebugEnabled) { GD.Print("StateMachine: Enter RootState " + currentRootState.GetType().ToString()); }

        if (InitializeSubState)
        {
            currentRootState.InitializeSubState();
        }
    }

    private void ChangeSubState(Type newStateType)
    {
        if (!currentRootState.subStates.Contains(newStateType))
        {
            ChangeRootState(FindRootforSubState(newStateType), false);
        }

        if (DebugEnabled && currentState != null) { GD.Print("StateMachine: Exit State " + currentState.GetType().ToString()); }

        currentState?.OnExit();
        currentState = stateDictionary[newStateType];
        currentState.OnEnter();

        if (DebugEnabled) { GD.Print("StateMachine: Enter State " + currentState.GetType().ToString()); }

        activeTransitions = allTransitions.FindAll(
            currentTransition => currentTransition.fromState == currentState.GetType() || currentTransition.fromState == null
        );
    }

    private RootState<T> FindRootforSubState(Type newStateType)
    {
        for (int i = 0; i < rootStates.Length; i++)
        {
            if (rootStates[i].subStates.Contains(newStateType))
            {
                return rootStates[i];
            }
        }

        GD.Print("Couldn't find RootState for Substate: " +  newStateType.ToString());

        return null;
    }

    public void OnUpdate()
    {
        foreach (Transition transition in activeTransitions)
        {
            if (transition.Evaluate())
            {
                ChangeState(transition.toState);
                return;
            }
        }

        currentRootState.OnUpdate();
        currentState.OnUpdate();
    }

    public void OnPhysicsUpdate()
    {
        currentRootState.OnPhysicsUpdate();
        currentState.OnPhysicsUpdate();
    }

    public void AddTransition(Transition transition)
    {
        allTransitions.Add(transition);
    }
}
