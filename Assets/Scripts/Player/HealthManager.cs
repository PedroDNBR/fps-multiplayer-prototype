using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : NetworkBehaviour
{
    [Header("Max Health Points")]
    public NetworkVariable<int> totalMaxLifePoints = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> headMaxLifePoints = new(40, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> thoraxMaxLifePoints = new(90, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> legsMaxLifePoints = new(150, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> armsMaxLifePoints = new(150, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Current Health Points")]
    public NetworkVariable<int> totalLifePoints = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> headLifePoints = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> thoraxLifePoints = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> legsLifePoints = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> armsLifePoints = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public Canvas canvas;

    public Slider headSlider;
    public TMP_Text headLifePointsUI;

    public Slider thoraxSlider;
    public TMP_Text thoraxLifePointsUI;
    
    public Slider armsSlider;
    public TMP_Text armsLifePointsUI;
    
    public Slider legsSlider;
    public TMP_Text legsLifePointsUI;

    public TMP_Text totalLifePointsUI;

    PlayerManager playerManager;
    InputManager inputManager;

    ulong lastPlayerToDamage;

    public void Init(PlayerManager playerManager, InputManager inputManager)
    {
        this.playerManager = playerManager;
        this.inputManager = inputManager;
    }

    public override void OnNetworkSpawn()
    {
        headLifePoints.Value = headMaxLifePoints.Value;
        thoraxLifePoints.Value = thoraxMaxLifePoints.Value;
        armsLifePoints.Value = armsMaxLifePoints.Value;
        legsLifePoints.Value = legsMaxLifePoints.Value;

        totalMaxLifePoints.Value = headLifePoints.Value + thoraxLifePoints.Value + legsLifePoints.Value + armsMaxLifePoints.Value;

        totalLifePoints.Value = totalMaxLifePoints.Value;

        SetUIValues();

        canvas.gameObject.SetActive(false);

        base.OnNetworkSpawn(); // Not sure if this is needed though, but good to have it.
    }

    void GetUIComponents()
    {
        headSlider = gameObject.transform.Find("HeadHealthBox").GetComponentInChildren<Slider>();
        headLifePointsUI = gameObject.transform.Find("HeadHealthBox/Container/HealthBar/HealthBarSlider").GetComponentInChildren<TMP_Text>();

        thoraxSlider = gameObject.transform.Find("ThoraxHealthBox").GetComponentInChildren<Slider>();
        thoraxLifePointsUI = gameObject.transform.Find("ThoraxHealthBox/Container/HealthBar/HealthBarSlider").GetComponentInChildren<TMP_Text>();

        armsSlider = gameObject.transform.Find("ArmsHealthBox").GetComponentInChildren<Slider>();
        armsLifePointsUI = gameObject.transform.Find("ArmsHealthBox/Container/HealthBar/HealthBarSlider").GetComponentInChildren<TMP_Text>();

        legsSlider = gameObject.transform.Find("LegsHealthBox").GetComponentInChildren<Slider>();
        legsLifePointsUI = gameObject.transform.Find("LegsHealthBox/Container/HealthBar/HealthBarSlider").GetComponentInChildren<TMP_Text>();

        totalLifePointsUI = gameObject.transform.Find("FullHealthPoints").GetComponent<TMP_Text>();
        
    }

    void SetUIValues()
    {
        headSlider.maxValue = headMaxLifePoints.Value;

        thoraxSlider.maxValue = thoraxMaxLifePoints.Value;

        armsSlider.maxValue = armsMaxLifePoints.Value;

        legsSlider.maxValue = legsMaxLifePoints.Value;

        UpdateUiValues();
    }

    [ClientRpc]
    void UpdateUiValuesClientRpc()
    {
        UpdateUiValues();
    }

    void UpdateUiValues()
    {
        headSlider.value = headLifePoints.Value;
        headLifePointsUI.text = headLifePoints.Value + "/" + headMaxLifePoints.Value;

        thoraxSlider.value = thoraxLifePoints.Value;
        thoraxLifePointsUI.text = thoraxLifePoints.Value + "/" + thoraxMaxLifePoints.Value;

        armsSlider.value = armsLifePoints.Value;
        armsLifePointsUI.text = armsLifePoints.Value + "/" + armsMaxLifePoints.Value;

        legsSlider.value = legsLifePoints.Value;
        legsLifePointsUI.text = legsLifePoints.Value + "/" + legsMaxLifePoints.Value;

        totalLifePointsUI.text = totalLifePoints.Value + "/" + totalMaxLifePoints.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageHeadServerRpc(int damage, ulong lastPlayerToDamage)
    {
        this.lastPlayerToDamage = lastPlayerToDamage;
        headLifePoints.Value -= damage;
        totalLifePoints.Value -= damage;
        checkDeath();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageThoraxServerRpc(int damage, ulong lastPlayerToDamage)
    {
        this.lastPlayerToDamage = lastPlayerToDamage;
        thoraxLifePoints.Value -= (int)Mathf.Round(damage / 1.5f);
        totalLifePoints.Value -= (int)Mathf.Round(damage / 1.5f);
        checkDeath();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageLegsServerRpc(int damage, ulong lastPlayerToDamage)
    {
        this.lastPlayerToDamage = lastPlayerToDamage;
        legsLifePoints.Value -= (int)Mathf.Round(damage / 1.75f);
        if (legsLifePoints.Value < 1) legsLifePoints.Value = 0;
        totalLifePoints.Value -= (int)Mathf.Round(damage / 1.75f);
        checkDeath();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageArmsServerRpc(int damage, ulong lastPlayerToDamage)
    {
        this.lastPlayerToDamage = lastPlayerToDamage;
        armsLifePoints.Value -= (int)Mathf.Round(damage / 1.75f);
        if (armsLifePoints.Value < 1) armsLifePoints.Value = 0;
        totalLifePoints.Value -= (int)Mathf.Round(damage / 1.75f);
        checkDeath();
    }

    void checkDeath()
    {
        UpdateUiValuesClientRpc();
        if (totalLifePoints.Value <= 0 || thoraxLifePoints.Value <= 0 || headLifePoints.Value <= 0)
        {
            PlayerInfo playerinfo = GameMulitiplayerManager.Instance.playersConnected[lastPlayerToDamage];
            playerinfo.killCount += 1;
            GameMulitiplayerManager.Instance.playersConnected[lastPlayerToDamage] = playerinfo;
            DieServerRpc();
        }
    }



    [ServerRpc(RequireOwnership = false)]
    void DieServerRpc()
    {
        GameMulitiplayerManager.Instance.SpawnNewPlayerServerRpc(playerManager.clientId);
        Destroy(this.gameObject);
    }

    public void ToggleInventory()
    {
        if (!IsOwner) return;

        if(inputManager.tab)
            canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
    }
}
