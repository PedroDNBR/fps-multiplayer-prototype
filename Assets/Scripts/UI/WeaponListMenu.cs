using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponListMenu : MonoBehaviour
{
    public List<WeaponItem> weaponsList;
    public GameObject UiItemPrefab;
    public Transform UiList;

    public ItemPreview itemPreview;

    public Transform uiAttachmentList;
    public GameObject attachmentUiItemPrefab;
    public TMP_InputField weaponNameInput;

    public Toggle equipPrimary;
    public Toggle equipSecondary;
    public Toggle equipTertiary;

    public WeaponItem currentWeapon;

    public List<WeaponItem> newWeaponsList = new List<WeaponItem>();
    IconMaker iconMaker;

    public Inventory playerInventory;

    private void Start()
    {
        GetPlayerInventory();

        iconMaker = GetComponent<IconMaker>();

        UpdateWeaponList();

        equipPrimary.onValueChanged.AddListener((bool isSelected) =>
        {
            if (currentWeapon == null) return;

            if (isSelected)
            {
                playerInventory.primaryWeapon = currentWeapon;
                SaveInventoryLocally();
            }
            else
                if (playerInventory.primaryWeapon != null && currentWeapon.name == playerInventory.primaryWeapon.name) playerInventory.primaryWeapon = null;

        });

        equipSecondary.onValueChanged.AddListener((bool isSelected) =>
        {
            if (currentWeapon == null) return;


            if (isSelected)
            {
                playerInventory.secondaryWeapon = currentWeapon;
                SaveInventoryLocally();
            }
            else
                if (playerInventory.secondaryWeapon != null && currentWeapon.name == playerInventory.secondaryWeapon.name) playerInventory.secondaryWeapon = null;
        });

        equipTertiary.onValueChanged.AddListener((bool isSelected) =>
        {
            if (currentWeapon == null) return;

            if (isSelected)
            {
                playerInventory.tertiaryWeapon = currentWeapon;
                SaveInventoryLocally();
            }
            else
                if (playerInventory.tertiaryWeapon != null && currentWeapon.name == playerInventory.tertiaryWeapon.name) playerInventory.tertiaryWeapon = null;
        });
    }

    void MarkIfEquiped()
    {
        if (playerInventory == null)
        {
            Debug.Log("Sem Inventário");
            return;
            }
        if (playerInventory.primaryWeapon != null && playerInventory.primaryWeapon.name == currentWeapon.name) equipPrimary.isOn = true; else equipPrimary.isOn = false;
        if (playerInventory.secondaryWeapon != null && playerInventory.secondaryWeapon.name == currentWeapon.name) equipSecondary.isOn = true; else equipSecondary.isOn = false;
        if (playerInventory.tertiaryWeapon != null && playerInventory.tertiaryWeapon.name == currentWeapon.name) equipTertiary.isOn = true; else equipTertiary.isOn = false;
    }

    public Inventory emptyInventory;

    void GetPlayerInventory()
    {
        string dirName = "player-inventory";

        Debug.Log(Path.Combine(Application.persistentDataPath, dirName));

        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, dirName)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, dirName));
        }

        string path = Path.Combine(Application.persistentDataPath, dirName) + "/inventory.json";

        if (!File.Exists(path))
        {
            Debug.Log($"Cannot load file at {path}. File does not exist!");
            SaveInventoryLocally();
            GetPlayerInventory();
        }
        else
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.All,
            };
            Inventory inventory = JsonConvert.DeserializeObject<Inventory>(File.ReadAllText(path), settings);
            Debug.Log("Inventory from JSON");
            Debug.Log(inventory);
            playerInventory = inventory;
        }

        if(playerInventory == null)
        {
            playerInventory = new Inventory();
        }
    }

    void UpdateWeaponList()
    {
        newWeaponsList.Clear();
        foreach (var weapon in weaponsList)
        {
            var newWeapon = Instantiate(weapon);
            newWeaponsList.Add(newWeapon);
        }

        foreach (Transform child in UiList.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var weapon in newWeaponsList)
        {
            GameObject UiItemInstantiated = Instantiate(UiItemPrefab, UiList);
            UiItemInstantiated.GetComponentInChildren<TMP_Text>().text = weapon.name;
            Image icon = UiItemInstantiated.GetComponentsInChildren<Image>()[1];
            iconMaker.SetItemInQueue(icon, weapon, true);

            UiItemInstantiated.SetActive(true);
            UiItemInstantiated.GetComponent<Button>().onClick.AddListener(() =>
            {
                currentWeapon = weapon;
                weaponNameInput.text = weapon.name;
                itemPreview.ChangeItemInPreviewTab(weapon);
                itemPreview.SetCurrentItemName(weapon.name);
                MarkIfEquiped();
                GetAttachmentSlots();
            });

            UiItemInstantiated.GetComponentsInChildren<Button>()[1].gameObject.SetActive(false);
        }

        /*foreach (var file in Directory.EnumerateFiles(Application.persistentDataPath + "/custom-weapons", "*.json"))
        {
            Debug.Log(file + " Added");
            LoadWeaponFromLocal(file);
        }*/
        try
        {
            foreach (string file in Directory.GetFiles(Application.persistentDataPath + "/custom-weapons", "*.json"))
            {
                LoadWeaponFromLocal(file);
            }
        }
        catch(Exception e)
        {

        }
    }

    public void SaveCurrentWeaponLocally()
    {
        var savedWeapon = Instantiate(currentWeapon);
        savedWeapon.name = weaponNameInput.text;

        string dirName = "custom-weapons";

        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, dirName)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, dirName));
        }

        string path = Application.persistentDataPath + $"/{dirName}/{savedWeapon.name}.json";

        if (File.Exists(path))
        {
            File.Delete(path);
        }
        using FileStream stream = File.Create(path);
        stream.Close();

        File.WriteAllText(path, JsonConvert.SerializeObject(savedWeapon, Formatting.None,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.All,
            }));
        //File.WriteAllText(path, JsonUtility.ToJson(currentWeapon));

        SaveInventoryLocally();
        UpdateWeaponList();
    }

    public void LoadWeaponFromLocal(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"Cannot load file at {path}. File does not exist!");
            throw new FileNotFoundException($"{path} does not exist!");
        }

        try
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.All,
            };
            WeaponItem weapon = JsonConvert.DeserializeObject<WeaponItem>(File.ReadAllText(path), settings);

            newWeaponsList.Add(weapon);

            GameObject UiItemInstantiated = Instantiate(UiItemPrefab, UiList);
            UiItemInstantiated.GetComponentInChildren<TMP_Text>().text = weapon.name;
            Image icon = UiItemInstantiated.GetComponentsInChildren<Image>()[1];
            iconMaker.SetItemInQueue(icon, weapon, true);

            UiItemInstantiated.SetActive(true);
            UiItemInstantiated.GetComponent<Button>().onClick.AddListener(() =>
            {
                currentWeapon = weapon;
                weaponNameInput.text = weapon.name;
                itemPreview.ChangeItemInPreviewTab(weapon);
                itemPreview.SetCurrentItemName("custom_" + weapon.name);
                MarkIfEquiped();
                GetAttachmentSlots();
            });
            UiItemInstantiated.GetComponentsInChildren<Button>()[1].onClick.AddListener(() =>
            {
                DeleteWeaponLocally(weapon);
                if (itemPreview.CheckIfItemIsBeingPreviewed("custom_" + weapon.name))
                {
                    weaponNameInput.text = null;
                    itemPreview.ClearPreviewTab(false);
                    itemPreview.SetCurrentItemName("");
                    ClearAttachmentItemsInUI();
                }
                MarkIfEquiped();
                if (playerInventory.primaryWeapon != null && playerInventory.primaryWeapon.name == weapon.name) playerInventory.primaryWeapon = null;
                if (playerInventory.secondaryWeapon != null && playerInventory.secondaryWeapon.name == weapon.name) playerInventory.secondaryWeapon = null;
                if (playerInventory.tertiaryWeapon != null && playerInventory.tertiaryWeapon.name == weapon.name) playerInventory.tertiaryWeapon = null;
                UpdateWeaponList();
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data due to: {e.Message} {e.StackTrace}");
        }
    }

    public void DeleteWeaponLocally(WeaponItem weapon)
    {
        string dirName = "custom-weapons";

        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, dirName)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, dirName));
        }

        string path = Application.persistentDataPath + $"/{dirName}/{weapon.name}.json";
        Debug.Log($"{path} Weapon Deleted {weapon.name}");
        if (File.Exists(path))
        {
            File.Delete(path);
            SaveInventoryLocally();
        }
    }

    public void SaveInventoryLocally()
    {
        string dirName = "player-inventory";

        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, dirName)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, dirName));
        }

        string path = Path.Combine(Application.persistentDataPath, dirName) + "/inventory.json";

        if (File.Exists(path))
        {
            File.Delete(path);
        }
        using FileStream stream = File.Create(path);
        stream.Close();

        File.WriteAllText(path, JsonConvert.SerializeObject(playerInventory, Formatting.None,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.All,
            }));
        //File.WriteAllText(path, JsonUtility.ToJson(currentWeapon));
    }

    void SetAttachmentInUI(GameObject instantiated, WeaponPart weaponPart, WeaponPartType weaponPartType)
    {
        string partName = Enum.GetName(typeof(WeaponPartType), weaponPartType);
        if (weaponPart != null)
        {
            partName = weaponPart.name;
            Image icon = instantiated.GetComponentsInChildren<Image>()[1];
            iconMaker.SetItemInQueue(icon, weaponPart);
        }
        instantiated.GetComponentInChildren<TMP_Text>().text = partName;
    }

    void ClearAttachmentItemsInUI()
    {
        foreach (Transform child in uiAttachmentList)
        {
            Destroy(child.gameObject);
        }
    }

    void GetAttachmentSlots()
    {
        ClearAttachmentItemsInUI();

        foreach (var weaponAttachmentPoint in currentWeapon.weaponAttachmentPoints)
        {
            if (weaponAttachmentPoint == null) continue;

            var instantiated = Instantiate(attachmentUiItemPrefab, uiAttachmentList);

            SetAttachmentInUI(instantiated, weaponAttachmentPoint.weaponPart, weaponAttachmentPoint.weaponPartType);
            SetDropdownValues(weaponAttachmentPoint.compatibleWeaponPartsList, instantiated, weaponAttachmentPoint);

            LoadModularAttachments(weaponAttachmentPoint);
        }
    }

    void SetDropdownValues(CompatibleWeaponPartsList compatibleWeaponPartsList, GameObject instantiated, WeaponPartsInGun weaponAttachmentPoint)
    {
        List<string> compatibleListOptions = new List<string>();
        if (weaponAttachmentPoint.canBeEmpty) compatibleListOptions.Add("remove");
        foreach (var compatibleAttachment in compatibleWeaponPartsList.compatibleWeaponItems)
        {
            Debug.Log(compatibleAttachment.name);
            compatibleListOptions.Add(compatibleAttachment.name);
        }

        TMP_Dropdown dropdown = instantiated.GetComponentInChildren<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(compatibleListOptions);
        if (weaponAttachmentPoint.weaponPart != null)
            dropdown.value = compatibleListOptions.IndexOf(weaponAttachmentPoint.weaponPart.name);

        dropdown.onValueChanged.AddListener((int index) =>
        {
            if (weaponAttachmentPoint.canBeEmpty && index == 0)
                weaponAttachmentPoint.weaponPart = null;
            else
            {
                if (weaponAttachmentPoint.canBeEmpty) index--;
                weaponAttachmentPoint.weaponPart = compatibleWeaponPartsList.compatibleWeaponItems[index];
            }
            string currentWeaponName = itemPreview.currentItemName;
            itemPreview.ChangeItemInPreviewTab(currentWeapon, true);
            itemPreview.SetCurrentItemName(currentWeaponName);
            GetAttachmentSlots();
        });
    }

    void LoadModularAttachments(WeaponPartsInGun weaponPartsInGun)
    {
        if (
            weaponPartsInGun.weaponPartType == WeaponPartType.HandGuard ||
            weaponPartsInGun.weaponPartType == WeaponPartType.Adapter ||
            weaponPartsInGun.weaponPartType == WeaponPartType.ScopeMount
        )
        {
            if (weaponPartsInGun.weaponPart == null) return;
            ModularWeaponPartSocket modularWeaponPartSocket = weaponPartsInGun.weaponPart as ModularWeaponPartSocket;
            if (modularWeaponPartSocket != null && modularWeaponPartSocket.weaponAttachmentPoints != null)
            {
                foreach (var subAttachmentPoints in modularWeaponPartSocket.weaponAttachmentPoints)
                {
                    var instantiatedSubAttachment = Instantiate(attachmentUiItemPrefab, uiAttachmentList);

                    SetAttachmentInUI(instantiatedSubAttachment, subAttachmentPoints.weaponPart, subAttachmentPoints.weaponPartType);
                    SetDropdownValues(subAttachmentPoints.compatibleWeaponPartsList, instantiatedSubAttachment, subAttachmentPoints);

                    if (subAttachmentPoints.weaponPart == null) continue;

                    if (subAttachmentPoints.weaponPart.weaponPartType == WeaponPartType.ScopeMount)
                    {
                        ScopeMount scopeMount = subAttachmentPoints.weaponPart as ScopeMount;

                        var instantiatedSubAttachmentSub = Instantiate(attachmentUiItemPrefab, uiAttachmentList);

                        SetAttachmentInUI(instantiatedSubAttachmentSub, scopeMount.weaponAttachmentPoints[0].weaponPart, scopeMount.weaponAttachmentPoints[0].weaponPartType);
                        SetDropdownValues(scopeMount.weaponAttachmentPoints[0].compatibleWeaponPartsList, instantiatedSubAttachmentSub, scopeMount.weaponAttachmentPoints[0]);
                    }
                }
            }
        }
    }
}
