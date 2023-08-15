using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class AnimatorManager : NetworkBehaviour
{
    Animator animator;
    InputManager inputManager;
    float xRotation;
    public NetworkVariable<int> rigWeight = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public Rig rig;

    public Animator weaponAnimator;
    public Animator weaponMovementAnimator;

    public void Init(Animator animator, InputManager inputManager)
    {
        this.animator = animator;
        this.inputManager = inputManager;
        rig = GetComponentInChildren<Rig>();

        rigWeight.OnValueChanged += (oldValue, newValue) =>
        {
            rig.weight = newValue;
        };
    }

    public void HandleMovementAnimation(float vertical, float horizontal)
    {
        HandleMovementAnimationServerRpc(vertical, horizontal);
    }

    [ServerRpc]
    void HandleMovementAnimationServerRpc(float vertical, float horizontal)
    {
        HandleMovementAnimationClientRpc(vertical, horizontal);
    }

    [ClientRpc]
    void HandleMovementAnimationClientRpc(float vertical, float horizontal)
    {
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Horizontal", horizontal);

        weaponMovementAnimator.SetFloat("Vertical", vertical);
    }

    public void HandleCrouchAnimation(float height)
    {
        animator.SetFloat("Height", height);
    }

    public float CrouchValue()
    {
        return animator.GetFloat("Height");
    }

    public void HandleSpineAim()
    {
        float mouseY = inputManager.mouseY * inputManager.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, 0, 1);
        animator.SetFloat("AimHeight", xRotation);
    }

    public void PlayTargetAnimation(string animation)
    {
        animator.CrossFade(animation, .1f);
    }

    public void SetRigWeight(float weight)
    {
        rig.weight = weight;
    }
    float value = 0;
    private void OnAnimatorIK()
    {
        Quaternion actualRotation = animator.GetBoneTransform(HumanBodyBones.Chest).localRotation;

        actualRotation.z = value;

        animator.SetBoneLocalRotation(HumanBodyBones.Chest, actualRotation);
    }

    public void SetValue(float value)
    {
        this.value = value;
    }
}
