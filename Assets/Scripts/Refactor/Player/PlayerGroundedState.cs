using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace RFC 
{
    public class PlayerGroundedState : BaseState
    {
        PlayerStateMachine context;

        public PlayerGroundedState(PlayerStateMachine context)
        {
            this.context = context;
            SetSubState(new PlayerIdleState(context));
        }

        public override void EnterState()
        {
        }

        public override void ExitState()
        {

        }

        public override void InitializeSubState()
        {
        }

        public override void LateUpdateState()
        {
        }

        public override void OnTriggerEnter(Collider collider)
        {

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
            base.UpdateState();
            CheckIfIsInAir();
            HandleFakeGravity();
            HandleRotation();
            subState.UpdateState();
            if (context.inputManager.spaceBar)
            {
                context.QueueNextState(new PlayerJumpState(context));
            }
        }

        void HandleRotation()
        {
            context.mouseX = context.inputManager.mouseX * 100 * context.inputManager.deltaTime;

            context.inputManager.myTransform.Rotate(Vector3.up * context.mouseX);
        }

        void HandleFakeGravity()
        {
            if (context.velocity.y < 0)
                context.velocity.y = Physics.gravity.y;

            context.characterController.Move(context.velocity * context.inputManager.deltaTime);
        }

        void CheckIfIsInAir()
        {
            if (!Physics.CheckSphere(context.groundCheck.position, context.groundDistance, context.groundMask))
                context.QueueNextState(new PlayerInAirState(context));
        }
    }
}
