using Unity.Netcode;
using UnityEngine;

public class PlayerLocomotion : NetworkBehaviour
{
    InputManager inputManager;
    CharacterController characterController;
    AnimatorManager animatorManager;

    public float speed = 3;
    Vector3 velocity;

    float xRotation;
    // Start is called before the first frame update
    public void Init(InputManager inputManager, CharacterController characterController, AnimatorManager animatorManager)
    {
        this.inputManager = inputManager;
        this.characterController = characterController;
        this.animatorManager = animatorManager;
    }

    public void HandleRotation()
    {
        float mouseX = inputManager.mouseX * 100 * inputManager.deltaTime;

        inputManager.myTransform.Rotate(Vector3.up * mouseX);
    }

    // Update is called once per frame
    public void HandleMovement()
    {
        float heightDivisor = Mathf.InverseLerp(1f,0f, animatorManager.CrouchValue()) + 1;
        float vertical = inputManager.vertical / heightDivisor;
        float horizontal = inputManager.horizontal / heightDivisor;

        Vector3 move = inputManager.myTransform.right * horizontal + inputManager.myTransform.forward * vertical;

        characterController.Move(move * speed * inputManager.deltaTime);

        velocity.y = Physics.gravity.y * inputManager.deltaTime;

        characterController.Move(velocity * inputManager.deltaTime);
    }

    public bool crouchUp = false;
    public bool crouchDown = false;

    public void HandleCrouch()
    {
        HandleCrouchInput(inputManager.deltaTime);

        if (controlableCrouch) { 
            if (inputManager.scroll != 0)
            {
                animatorManager.HandleCrouchAnimaion(animatorManager.CrouchValue() + (inputManager.deltaTime * 2 * inputManager.scroll));
                if (animatorManager.CrouchValue() > 1)
                    animatorManager.HandleCrouchAnimaion(1);

                if (animatorManager.CrouchValue() < 0)
                    animatorManager.HandleCrouchAnimaion(0);

                return;
            }
        }

        if(directCrouch) { 
            if (animatorManager.CrouchValue() > .5f)
            {
                crouchDown = true;
                crouchUp = false;
            }
            else if (animatorManager.CrouchValue() < .5f)
            {
                crouchUp = true;
                crouchDown = false;
            }
            directCrouch = false;
        }

        if (crouchUp) HandleCrouchUp();
        if (crouchDown) HandleCrouchDown();
    }

    public void HandleCrouchUp()
    {
        if (animatorManager.CrouchValue() <= 1)
            animatorManager.HandleCrouchAnimaion(animatorManager.CrouchValue() + (inputManager.deltaTime * 2));

        if (animatorManager.CrouchValue() > 1)
        {
            animatorManager.HandleCrouchAnimaion(1);
            crouchUp = false;
        }
    }

    public void HandleCrouchDown()
    {
        if (animatorManager.CrouchValue() >= 0)
            animatorManager.HandleCrouchAnimaion(animatorManager.CrouchValue() - (inputManager.deltaTime * 2));

        if (animatorManager.CrouchValue() < 0)
        {
            animatorManager.HandleCrouchAnimaion(0);
            crouchDown = false;
        }
    }


    float crouchInputTimer;
    bool controlableCrouch;
    bool directCrouch;
    private void HandleCrouchInput(float delta)
    {

        if (inputManager.c)
        {
            crouchInputTimer += delta;
            controlableCrouch = true;
        }
        else
        {
            if (crouchInputTimer > 0 && crouchInputTimer < 0.5f)
            {
                controlableCrouch = false;
                directCrouch = true;
            }

            crouchInputTimer = 0;
        }
    }

    public float value = 0;
    const float INCLINATION_VELOCITY = .0055f;

    public void HandleInclination()
    {
        if (inputManager.q)
        {
            LeanRightServerRpc();
        }
        else if (inputManager.e)
        {
            LeanLeftServerRpc();
        }
        else
        {
            LeanCenterServerRpc();
        }

        ApplyLeanServerRpc();
    }

    void ApplyLean()
    {
        animatorManager.SetValue(value);
    }

    void LeanRight()
    {
        value += INCLINATION_VELOCITY;

        if (value >= .2f) value = .2f;
    }

    void LeanLeft()
    {
        value -= INCLINATION_VELOCITY;

        if (value <= -.2f) value = -.2f;
    }

    void LeanCenter()
    {
        if (value > 0)
            value -= INCLINATION_VELOCITY;

        if (value < 0)
            value += INCLINATION_VELOCITY;

        if (value <= 0.01f && value >= -0.01f)
            value = 0;
    }

    [ServerRpc]
    void ApplyLeanServerRpc() { ApplyLeanClientRpc(); }
    [ServerRpc]
    void LeanRightServerRpc() { LeanRightClientRpc(); }
    [ServerRpc]
    void LeanLeftServerRpc() { LeanLeftClientRpc(); }
    [ServerRpc]
    void LeanCenterServerRpc() { LeanCenterClientRpc(); }


    [ClientRpc]
    void ApplyLeanClientRpc() { ApplyLean(); }
    [ClientRpc]
    void LeanRightClientRpc() { LeanRight(); }
    [ClientRpc]
    void LeanLeftClientRpc() { LeanLeft(); }
    [ClientRpc]
    void LeanCenterClientRpc() { LeanCenter(); }

    private void LateUpdate()
    {
        controlableCrouch = false;
        directCrouch = false;
    }
}
