using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  PlayerState
{
    public enum PlayerStateType
    {
        Idle,
        Attack,
        Jump,
        Fall,
        Interact,
        Move
    }

    public class Idle: IPlayerState
    {
        public Player self;

        public Idle(Player self)
        {
            this.self = self;
        }

        public void OnEnter()
        {
            self._animator.Play("Idle");
        }

        public void OnUpdate()
        {
            if (self._playerInput.Player.Move.ReadValue<Vector2>().magnitude >= 0.05f)
            {
                self.TransState(PlayerStateType.Move);
            }
        }

        public void OnExit()
        {
            
        }
    }
    
    public class Move: IPlayerState
    {
        public Player self;

        public Move(Player self)
        {
            this.self = self;
        }

        public void OnEnter()
        {
            self._animator.Play("Move");
        }

        public void OnUpdate()
        {
            if (self._playerInput.Player.Move.ReadValue<Vector2>().magnitude <= 0.05f)
            {
                self.TransState(PlayerStateType.Idle);
            }
            else
            {
                Debug.Log(self._playerInput.Player.Move.ReadValue<Vector2>());
                self._controller.Move(Time.deltaTime * self._playerData.speed * self.GetRelativeDirection(self._playerInput.Player.Move.ReadValue<Vector2>()));
                self.transform.LookAt(self.transform.position + self.GetRelativeDirection(self._playerInput.Player.Move.ReadValue<Vector2>()));
            }
        }

        public void OnExit()
        {
            
        }
    }
}
