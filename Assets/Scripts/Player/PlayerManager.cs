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
    public WeaponItem weaponItemSecondary;
    public WeaponItem weaponItemTertiary;

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
        animatorManager.Init(animator, inputManager);
        weaponManager.Init(inputManager, animatorManager, this);
        playerLocomotion.Init(inputManager, characterController, animatorManager);
        scoreboardManager.Init(inputManager);
        healthManager.Init(this, inputManager);

        SetupPrimaryWeaponServerRpc();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        weaponManager.WeaponExecutionServer();
        if (!IsOwner)
            return;

        inputManager.HandleInputs();
        playerLocomotion.HandleRotation();
        playerLocomotion.HandleMovement();
        playerLocomotion.HandleCrouch();
        playerLocomotion.HandleInclination();
        weaponManager.WeaponExecutionLocal();
        scoreboardManager.HandleScoreboardData();
        healthManager.ToggleInventory();

        animatorManager.HandleSpineAim();

        if (inputManager.numberOne) SetupPrimaryWeaponServerRpc();
        if (inputManager.numberTwo) SetupSecondaryWeaponServerRpc();
        if (inputManager.numberThree) SetupTertiaryWeaponServerRpc();

    }

    [ServerRpc(RequireOwnership = false)]
    void SetupPrimaryWeaponServerRpc()
    {
        SetupPrimaryWeaponClientRpc();
    }

    [ClientRpc]
    void SetupPrimaryWeaponClientRpc() {
        weaponManager.currentWeapon = weaponItem;
        weaponManager.SetupWeaponServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void SetupSecondaryWeaponServerRpc()
    {
        SetupSecondaryWeaponClientRpc();
    }

    [ClientRpc]
    void SetupSecondaryWeaponClientRpc()
    {
        weaponManager.currentWeapon = weaponItemSecondary;
        weaponManager.SetupWeaponServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void SetupTertiaryWeaponServerRpc()
    {
        SetupTertiaryWeaponClientRpc();
    }

    [ClientRpc]
    void SetupTertiaryWeaponClientRpc()
    {
        weaponManager.currentWeapon = weaponItemTertiary;
        weaponManager.SetupWeaponServerRpc();
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
