using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalkingState : PlayerMovementState
{
    public PlayerWalkingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
    }

    #region IState Methods
    public override void Enter()
    {
        base.Enter();
        
        //set speed Modifier to walking speed
        speedModifier = 0.225f;
    }
    #endregion
    
    #region Resuable Methods
    protected override void AddInputActionCallBacks()
    {
        base.AddInputActionCallBacks();

        _stateMachine.Player.Input.PlayerActions.Move.canceled += OnMovementCanceled;
    }

    protected override void RemoveInputActionCallbacks()
    {
        base.RemoveInputActionCallbacks();

        _stateMachine.Player.Input.PlayerActions.Move.canceled -= OnMovementCanceled;
    }
    #endregion

    #region Input Methods
    protected override void OnWalkToggleStarted(InputAction.CallbackContext context)
    {
        base.OnWalkToggleStarted(context);
        
        _stateMachine.ChangeState(_stateMachine.RunningState);
    }
    
    protected void OnMovementCanceled(InputAction.CallbackContext context)
    {
        _stateMachine.ChangeState(_stateMachine.IdlingState);
    }
    #endregion
}
