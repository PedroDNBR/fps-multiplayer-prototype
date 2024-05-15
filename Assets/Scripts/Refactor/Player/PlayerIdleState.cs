using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFC
{

    public class PlayerIdleState : BaseState
    {
        PlayerStateMachine context;

        public PlayerIdleState(PlayerStateMachine context)
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

            if (context.inputManager.horizontal != 0 || context.inputManager.vertical != 0)
                superState.SetSubState(new PlayerWalkState(context));
        }
    }
}