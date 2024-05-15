using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace RFC { 
    public class PlayerJumpState : BaseState
    {
        PlayerStateMachine context;

        public PlayerJumpState(PlayerStateMachine context)
        {
            this.context = context;
        }

        public override void EnterState()
        {
            HandleJump();
        }

        public override void ExitState()
        {

        }

        public override void InitializeSubState()
        {

        }

        public override void LateUpdateState()
        {
            CheckIfIsInAir();
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
            base.UpdateState();
            HandleJump();
        }

        void HandleJump()
        {
            context.velocity.y = Mathf.Sqrt(context.jumpHeight * -2f * Physics.gravity.y);

            context.velocity.y += Physics.gravity.y * context.inputManager.deltaTime;

            context.characterController.Move(context.velocity * context.inputManager.deltaTime);
        }

        void CheckIfIsInAir()
        {
            if (!Physics.CheckSphere(context.groundCheck.position, context.groundDistance, context.groundMask))
                context.QueueNextState(new PlayerInAirState(context));
        }
    }
}
