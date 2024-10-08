using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine
{
    protected IState currentState;      //current state of context we inherit from

    public void ChangeState(IState newState)
    {
        currentState?.Exit();

        currentState = newState;
        
        currentState.Enter();
    }

    public void HandleInput()
    {
        currentState?.HandleInput();
    }
    
    public void Update()
    {
        currentState?.Update();
    }
    
    public void PhysicsUpdate()
    {
        currentState?.PhysicsUpdate();
    }
}
