using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    InputManager inputManager;
    CharacterController characterController;
    // VeticalAimWithSpine veticalAimWithSpine;
    PlayerLocomotion playerLocomotion;
    WeaponManager weaponManager;
    HealthManager healthManager;
    AnimatorManager animatorManager;
    Animator animator;

    public WeaponItem weaponItem;

    public ulong clientId;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        characterController = GetComponent<CharacterController>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        weaponManager = GetComponent<WeaponManager>();
        animatorManager = GetComponentInChildren<AnimatorManager>();
        animator = GetComponentInChildren<Animator>();
        healthManager = GetComponent<HealthManager>();
    }

    void Start()
    {
        clientId = GetComponent<NetworkObject>().OwnerClientId;
        inputManager.Init();
        playerLocomotion.Init(inputManager, characterController, animatorManager);
        weaponManager.Init(inputManager, weaponItem, animatorManager);
        animatorManager.Init(animator, inputManager);
        healthManager.Init(this, inputManager);

        /*Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;*/
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
            return;

        inputManager.HandleInputs();
        playerLocomotion.HandleRotation();
        playerLocomotion.HandleMovement();
        playerLocomotion.HandleCrouch();
        playerLocomotion.HandleInclination();
        weaponManager.HandleShooting();
        weaponManager.ApplyMotion();
        weaponManager.HandleAim();
        weaponManager.HandleReload();
        healthManager.ToggleInventory();

        animatorManager.HandleSpineAim();
    }
}
