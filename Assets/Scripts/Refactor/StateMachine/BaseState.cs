using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFC
{
    public abstract class BaseState
    {
        public BaseState()
        {
        }

        public BaseState superState;
        public BaseState subState;

        private BaseState queuedSubState;

        public virtual void SetSuperState(BaseState newSuperState)
        {
            superState = newSuperState;
        }

        public virtual void SetSubState(BaseState newSubState)
        {
            Debug.Log(newSubState + "Parent " + this);
            subState = newSubState;
            newSubState.SetSuperState(this);
        }

        public abstract void EnterState();
        public abstract void ExitState();
        public virtual void UpdateState()
        {
            if (subState != null)
            {
                subState.UpdateState();
            }
        }
        public abstract void LateUpdateState();
        public abstract void InitializeSubState();
        public abstract void OnTriggerEnter(Collider collider);
        public abstract void OnTriggerStay(Collider collider);
        public abstract void OnTriggerExit(Collider collider);
    }
}
