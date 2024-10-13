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
        
        //speedModifier = 0f;
        
        //ResetVelocity();
    }

    public override void Update()
    {
        base.Update();
        
        speedModifier = Mathf.Lerp(speedModifier, 0f, .5f * Time.deltaTime);

        //movement
        if (movementInput == Vector2.zero)
        {
            return;
        }
        OnMove();   //change to other state
        
        //attack
        
        //jump
        
    }

    private void OnMove()
    {
        //run when if walk is false
        _stateMachine.ChangeState(_stateMachine.RunningState);
    }
    #endregion
}
