using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementState : IState
{
    protected PlayerMovementStateMachine _stateMachine;

    protected Vector2 movementInput;

    protected float baseSpeed = 5f;
    protected float speedModifier = 1f;

    protected Vector3 currentTargetRotation;
    protected Vector3 timeToReachTargetRotation;
    protected Vector3 dampedTargetRotationCurrentVelocity;
    protected Vector3 dampedTargetRotationPassedTime;
    
    public PlayerMovementState(PlayerMovementStateMachine playerMovementStateMachine)
    {
        _stateMachine = playerMovementStateMachine;

        InitializeData();
    }

    private void InitializeData()
    {
        timeToReachTargetRotation.y = .14f;
    }
    
    #region IState Methods
    public virtual void Enter()
    {
        Debug.Log("State: " + GetType().Name);

        AddInputActionCallBacks();
    }
    
    public virtual void Exit()
    {
        RemoveInputActionCallbacks();
    }
    
    public virtual void HandleInput()
    {
        ReadMovementInput();
    }

    public virtual void Update()
    {
    }

    public virtual void PhysicsUpdate()
    {
        Move();
    }
    
    #endregion

    #region Main Methods
    private void ReadMovementInput()
    {
        movementInput = _stateMachine.Player.Input.PlayerActions.Move.ReadValue<Vector2>();
    }

    private void Move()
    {
        if (movementInput == Vector2.zero || speedModifier == 0f)  //return if not moving
        {
            return;
        }

        Vector3 movementDirection = GetMovementInputDirection();

        float targetRotationYAngle = Rotate(movementDirection);

        Vector3 targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle);
        
        //get movement speed
        float movementSpeed = getMovementSpeed();
        //get current velocity
        Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();
        _stateMachine.Player.Rigidbody.AddForce(targetRotationDirection * movementSpeed - currentPlayerHorizontalVelocity, ForceMode.VelocityChange);
    }

    private float Rotate(Vector3 direction)
    {
        float directionAngle = UpdateTargetRotation(direction);

        RotateTowardsTargetRotation();

        return directionAngle;
    }
    
    private void UpdateTargetRotationData(float targetAngle)
    {
        currentTargetRotation.y = targetAngle;

        dampedTargetRotationPassedTime.y = 0f;
    }
    
    private static float DirectionAngle(Vector3 direction)
    {
        float directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        //set negative angle to positive
        if (directionAngle < 0f)
        {
            directionAngle += 360f;
        }

        return directionAngle;
    }

    private float AddCameraRotationToAngle(float angle)
    {
        angle += _stateMachine.Player.MainCameraTransform.eulerAngles.y;

        if (angle > 360f)
        {
            angle -= 360f;
        }

        return angle;
    }
    #endregion


    #region Resuable Methods
    protected Vector3 GetMovementInputDirection()
    {
        return new Vector3(movementInput.x, 0f, movementInput.y);
    }
    
    protected float getMovementSpeed()
    {
        return baseSpeed * speedModifier;
    }
    
    protected Vector3 GetPlayerHorizontalVelocity()
    {
        Vector3 playerHorizontalVelocity = _stateMachine.Player.Rigidbody.velocity;

        playerHorizontalVelocity.y = 0f;

        return playerHorizontalVelocity;
    }
    
    protected void RotateTowardsTargetRotation()
    {
        float currentYAngle = _stateMachine.Player.Rigidbody.rotation.eulerAngles.y;

        if (currentYAngle == currentTargetRotation.y)
        {
            return;
        }

        float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, currentTargetRotation.y,
            ref dampedTargetRotationCurrentVelocity.y, timeToReachTargetRotation.y - dampedTargetRotationPassedTime.y);

        dampedTargetRotationPassedTime.y += Time.deltaTime;
        
        //rotate player
        Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);
        
        _stateMachine.Player.Rigidbody.MoveRotation(targetRotation);
    }

    protected float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraRotation = true)
    {
        float directionAngle = DirectionAngle(direction);

        if (shouldConsiderCameraRotation)
        {
            directionAngle = AddCameraRotationToAngle(directionAngle);
        }
        
        //set current time
        if (directionAngle != currentTargetRotation.y)
        {
            UpdateTargetRotationData(directionAngle);
        }

        return directionAngle;
    }
    
    protected Vector3 GetTargetRotationDirection(float targetAngle)
    {
        return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
    }

    protected void ResetVelocity()
    {
        _stateMachine.Player.Rigidbody.velocity = Vector3.zero;
    }
    
    protected virtual void AddInputActionCallBacks()
    {
    }
    
    protected virtual void RemoveInputActionCallbacks()
    {
    }

    #endregion

    #region Input Methods
    
    

    #endregion
}
