using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerSprintingState : PlayerMovementState
{
    public PlayerSprintingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {

    }
    
    #region IState Methods
    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        speedModifier = Mathf.Lerp(speedModifier, 5f, .5f * Time.deltaTime);

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
    protected void OnMovementCanceled(InputAction.CallbackContext context)
    {
        _stateMachine.ChangeState(_stateMachine.IdlingState);
    }

    protected override void OnSprintEnded(InputAction.CallbackContext context)
    {
        base.OnSprintEnded(context);
        _stateMachine.ChangeState(_stateMachine.RunningState);
    }

    #endregion
}
