using System.Collections.Generic;
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
    ScoreboardManager scoreboardManager;

    public WeaponItem weaponItem;

    public ulong clientId;


    Dictionary<ulong, PlayerInfo> teste;


    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        characterController = GetComponent<CharacterController>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        weaponManager = GetComponent<WeaponManager>();
        animatorManager = GetComponentInChildren<AnimatorManager>();
        animator = GetComponentInChildren<Animator>();
        healthManager = GetComponent<HealthManager>();
        scoreboardManager = GetComponent<ScoreboardManager>();

    }

    void Start()
    {
        clientId = GetComponent<NetworkObject>().OwnerClientId;
        inputManager.Init();
        playerLocomotion.Init(inputManager, characterController, animatorManager);
        weaponManager.Init(inputManager, weaponItem, animatorManager, this);
        animatorManager.Init(animator, inputManager);
        scoreboardManager.Init(inputManager);
        healthManager.Init(this, inputManager);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        scoreboardManager.HandleScoreboardData();
        healthManager.ToggleInventory();

        animatorManager.HandleSpineAim();
    }

    public void ChangeColor(Color color)
    {
        ChangeColorServerRpc(color);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeColorServerRpc(Color color)
    {
        ChangeColorClientRpc(color);
    }

    [ClientRpc]
    public void ChangeColorClientRpc(Color color)
    {
        GetComponentInChildren<SkinnedMeshRenderer>().material.color = color;
    }
}
