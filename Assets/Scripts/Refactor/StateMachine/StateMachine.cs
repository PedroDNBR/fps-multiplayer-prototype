using System.Collections.Generic;
using UnityEngine;
using System;

namespace RFC
{
    public abstract class StateMachine: MonoBehaviour
    {
        protected BaseState CurrentState;
        private BaseState queuedState;

        void Start()
        {
            CurrentState.EnterState();
        }

        void Update() 
        {
            if (queuedState == null || queuedState == CurrentState)
            {
                CurrentState.UpdateState();
                UpdateMachine();
            } 
            else
            {
                TransitionToStateQueuedState();
            }
        }

        public void QueueNextState(BaseState nextState)
        {
            queuedState = nextState;
        }

        public void TransitionToStateQueuedState()
        {
            Debug.Log("queued:" + queuedState);

            CurrentState.ExitState();
            CurrentState = queuedState;
            queuedState = null;
            CurrentState.EnterState();
        }

        void LateUpdate()
        {
            CurrentState.LateUpdateState();
        }

        public abstract void UpdateMachine();

        void OnTriggerEnter(Collider collider) { }

        void OnTriggerStay(Collider collider) { }

        void OnTriggerExit(Collider collider) { }

    }
}

