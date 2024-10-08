using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerIdlingState : PlayerMovementState
{
    public PlayerIdlingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
    }

    #region IState Methods
    public override void Enter()
    {
        base.Enter();
        
        speedModifier = 0f;
        
        ResetVelocity();
    }

    public override void Update()
    {
        base.Update();

        if (movementInput == Vector2.zero)
        {
            return;
        }

        OnMove();   //change to other state
    }

    private void OnMove()
    {
        _stateMachine.ChangeState(_stateMachine.RunningState);
    }
    #endregion
}
