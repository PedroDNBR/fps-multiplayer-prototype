using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeticalAimWithSpine : MonoBehaviour
{
    Animator animator;
    InputManager inputManager;

    float xRotation;

    float xRotationSpine;
    float xRotationChest;
    float xRotationUpperChest;

    // Start is called before the first frame update
    public void Init(Animator animator, InputManager inputManager)
    {
        this.animator = animator;
        this.inputManager = inputManager;
    }

    public void HandleRotation()
    {
        float mouseY = inputManager.mouseY * 100 * inputManager.deltaTime;

        xRotation -= mouseY;
        xRotationSpine = xRotation;
        xRotationChest = xRotation;
        // xRotationUpperChest = xRotation;
        Debug.Log(xRotation);
    }

    // Update is called once per frame
    void OnAnimatorIK()
    {
        animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Euler(xRotationSpine, 0, 0));
        animator.SetBoneLocalRotation(HumanBodyBones.Chest, Quaternion.Euler(xRotationChest, xRotationChest, xRotationChest));
        // animator.SetBoneLocalRotation(HumanBodyBones.UpperChest, Quaternion.Euler(xRotationUpperChest, 0, 0));
    }
}
