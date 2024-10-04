using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using PlayerState;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Find other scripts & components")]
    public CinemachineVirtualCamera vCam;
    public PlayerData _playerData;
    public GameManager _gameManager;
    public CharacterController _controller;
    public Animator _animator;
    public PlayerInput _playerInput;
    public IPlayerState current;
    public IPlayerState last;

    public Dictionary<PlayerState.PlayerStateType, IPlayerState> states =
        new Dictionary<PlayerState.PlayerStateType, IPlayerState>();
    

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _playerInput.Enable();
        _gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        _animator = this.GetComponent<Animator>();
        _controller = this.GetComponent<CharacterController>();
        
        //add state
        states.Add(PlayerState.PlayerStateType.Idle, new PlayerState.Idle(this));
        states.Add(PlayerState.PlayerStateType.Move, new PlayerState.Move(this));
        TransState(PlayerState.PlayerStateType.Idle);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        current.OnUpdate();
        Debug.Log(vCam.transform.rotation.eulerAngles.y);
    }

    public void TransState(PlayerState.PlayerStateType type)
    {
        if (current != null)
        {
            current.OnExit();
            last = current;
        }
        current = states[type];
        current.OnEnter();
    }

    public Vector3 GetRelativeDirection(Vector3 input)
    {
        Quaternion rotation = Quaternion.Euler(0, vCam.transform.rotation.eulerAngles.y, 0);
        Vector3 direction = rotation * Vector3.forward * input.y + rotation * Vector3.right * input.x;
        return new Vector3(direction.x, 0, direction.z).normalized;
    }
}
