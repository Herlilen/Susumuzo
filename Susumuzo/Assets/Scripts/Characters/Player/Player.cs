using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    public Rigidbody Rigidbody { get; private set; }
    public Transform MainCameraTransform { get; private set; }      //cinemachine controls the main camera
    public PlayerInput Input { get; private set; }
    
    private PlayerMovementStateMachine _movementStateMachine;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Input = GetComponent<PlayerInput>();

        MainCameraTransform = Camera.main.transform;
        
        _movementStateMachine = new PlayerMovementStateMachine(this);
    }

    private void Start()
    {
        _movementStateMachine.ChangeState(_movementStateMachine.IdlingState);
    }

    private void Update()
    {
        _movementStateMachine.HandleInput();
        _movementStateMachine.Update();
    }

    private void FixedUpdate()
    {
        _movementStateMachine.PhysicsUpdate();
    }
}
