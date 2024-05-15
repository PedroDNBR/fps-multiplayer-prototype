using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFC
{

    public class PlayerWalkState : BaseState
    {
        PlayerStateMachine context;

        public PlayerWalkState(PlayerStateMachine context)
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
        }

        public override void LateUpdateState()
        {
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
            HandleMovement();

            if (context.inputManager.horizontal == 0 && context.inputManager.vertical == 0)
                superState.SetSubState(new PlayerIdleState(context));
        }

        void HandleMovement()
        {            
            Vector3 move = context.inputManager.myTransform.right * context.inputManager.horizontal + context.inputManager.myTransform.forward * context.inputManager.vertical;
            move = Vector3.ClampMagnitude(move, 1f);

            context.globalMove = move;
        }
    }
}
