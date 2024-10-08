using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRunningState : PlayerMovementState
{
    public PlayerRunningState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
    }

    #region IState Methods
    public override void Enter()
    {
        base.Enter();

        speedModifier = 1f;
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
        
        _stateMachine.ChangeState(_stateMachine.WalkingState);
    }
    
    protected void OnMovementCanceled(InputAction.CallbackContext context)
    {
        _stateMachine.ChangeState(_stateMachine.IdlingState);
    }
    #endregion
}
