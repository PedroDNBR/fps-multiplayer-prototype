using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace RFC
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerStateMachine : StateMachine
    {
        public InputManager inputManager;
        public CharacterController characterController;

        public float speed = 3;
        public float jumpHeight = 3;
        public Transform groundCheck;
        public Vector3 velocity;
        public Vector3 globalMove;

        public float mouseX;

        public float groundDistance = 0.4f;
        public LayerMask groundMask;

        public override void UpdateMachine()
        {
            characterController.Move(globalMove * speed * inputManager.deltaTime);
        }

        void Awake()
        {
            inputManager = GetComponent<InputManager>();
            characterController = GetComponent<CharacterController>();

            CurrentState = new PlayerGroundedState(this);

            Debug.Log(CurrentState);
        }

        
    }
}
