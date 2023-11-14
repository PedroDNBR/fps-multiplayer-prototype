using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Color = UnityEngine.Color;

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

    public Inventory inventory;

    public ulong clientId;

    Dictionary<ulong, PlayerInfo> teste;

    public bool debugOffline = false;

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
        healthManager.Init(this, inputManager, animatorManager);

        if (debugOffline)
        {
            weaponManager.currentWeapon = inventory.primaryWeapon;
            weaponManager.TestSetupGun();
        }

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

    public async Task<Inventory> GetInventoryFromNameHttp(string name)
    {
        Debug.Log("http://server.pedrogom.es:23562/inventario/" + name);
        using var webRequest = UnityWebRequest.Get("http://server.pedrogom.es:23562/inventario/" + name);

        var operation = webRequest.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(webRequest.error);
        }

        var playerInventoryType = JsonConvert.DeserializeObject<PlayerInventoryType>(webRequest.downloadHandler.text);
        var settings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.All,
        };
        Inventory inventory = JsonConvert.DeserializeObject<Inventory>(playerInventoryType.inventario, settings);
        return inventory;
    }

    public void SetInventory(string name)
    {
        SetInventoryServerRpc(name);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetInventoryServerRpc(string name)
    {
        SetInventoryClientRpc(name);
    }

    [ClientRpc]
    public void SetInventoryClientRpc(string name)
    {
        SetInventoryAndWeapons(name);
    }

    async void SetInventoryAndWeapons(string name)
    {
        Inventory newInventory = null;
        if (GameMulitiplayerManager.Instance.playersInventory.ContainsKey(name))
        {
            Debug.Log("Contains key");
            newInventory = GameMulitiplayerManager.Instance.playersInventory[name];
        } else
        {
            Debug.Log("Do not contains key");
            newInventory = await GetInventoryFromNameHttp(name);
            GameMulitiplayerManager.Instance.playersInventory.Add(name, newInventory);
        }

        this.inventory = newInventory;
        SetupPrimaryWeaponServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void SetupPrimaryWeaponServerRpc()
    {
        SetupPrimaryWeaponClientRpc();
    }

    [ClientRpc]
    void SetupPrimaryWeaponClientRpc() {
        weaponManager.currentWeapon = inventory.primaryWeapon;
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
        weaponManager.currentWeapon = inventory.secondaryWeapon;
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
        weaponManager.currentWeapon = inventory.tertiaryWeapon;
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

public class PlayerInventoryType
{
    public string nome;
    public string inventario;
}
