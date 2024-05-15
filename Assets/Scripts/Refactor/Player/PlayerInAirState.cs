using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFC
{
    public class PlayerInAirState : BaseState
    {
        PlayerStateMachine context;

        public PlayerInAirState(PlayerStateMachine context)
        {
            this.context = context;
        }

        public override void EnterState()
        {
        }

        public override void ExitState()
        {

        }

        public override void InitializeSubState()
        {
            throw new System.NotImplementedException();
        }

        public override void LateUpdateState()
        {
            CheckIfIsGrounded();
        }

        public override void OnTriggerEnter(Collider collider)
        {
            throw new System.NotImplementedException();
        }

        public override void OnTriggerExit(Collider collider)
        {
            throw new System.NotImplementedException();
        }

        public override void OnTriggerStay(Collider collider)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateState()
        {
            HandleRotation();
            ApplyGravity();
        }

        void CheckIfIsGrounded()
        {
            if (Physics.CheckSphere(context.groundCheck.position, context.groundDistance, context.groundMask))
                context.QueueNextState(new PlayerGroundedState(context));
        }

        void ApplyGravity()
        {
            context.velocity.y += Physics.gravity.y * context.inputManager.deltaTime;

            context.characterController.Move(context.velocity * context.inputManager.deltaTime);
        }

        void HandleRotation()
        {
            context.mouseX = context.inputManager.mouseX * 15 * context.inputManager.deltaTime;

            context.inputManager.myTransform.Rotate(Vector3.up * context.mouseX);
        }
    }
}