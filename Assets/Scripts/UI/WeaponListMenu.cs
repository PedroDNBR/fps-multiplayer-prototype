using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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

    WeaponItem currentWeapon;

    List<WeaponItem> newWeaponList = new List<WeaponItem>();
    IconMaker iconMaker;

    private void Start()
    {
        iconMaker = GetComponent<IconMaker>();

        foreach (var weapon in weaponsList)
        {
            var newWeapon = Instantiate(weapon);
            newWeaponList.Add(newWeapon);
        }

        UpdateWeaponList();
    }

    void UpdateWeaponList()
    {
        Debug.Log("Updating list");
        foreach (Transform child in UiList.transform)
        {
            Debug.Log(child.name + " Destroyed");
            Destroy(child.gameObject);
        }

        foreach (var weapon in newWeaponList)
        {
            Debug.Log(weapon.name + " Added");
            GameObject UiItemInstantiated = Instantiate(UiItemPrefab, UiList);
            UiItemInstantiated.GetComponentInChildren<TMP_Text>().text = weapon.name;
            Image icon = UiItemInstantiated.GetComponentsInChildren<Image>()[1];
            iconMaker.SetItemInQueue(icon, weapon, true);

            UiItemInstantiated.SetActive(true);
            UiItemInstantiated.GetComponent<Button>().onClick.AddListener(() =>
            {
                currentWeapon = weapon;
                weaponNameInput.text = weapon.name;
                itemPreview.ChangeItemInPreviewTab(currentWeapon);
                GetAttachmentSlots();
            });
        }

        foreach (var file in Directory.EnumerateFiles(Application.persistentDataPath + "/custom-weapons", "*.json"))
        {
            Debug.Log(file + " Added");
            LoadWeaponFromLocal(file);
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

        string path = Application.persistentDataPath + $"/custom-weapons/{savedWeapon.name}.json";

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

            weaponsList.Add(weapon);

            GameObject UiItemInstantiated = Instantiate(UiItemPrefab, UiList);
            UiItemInstantiated.GetComponentInChildren<TMP_Text>().text = weapon.name;
            Image icon = UiItemInstantiated.GetComponentsInChildren<Image>()[1];
            iconMaker.SetItemInQueue(icon, weapon, true);

            UiItemInstantiated.SetActive(true);
            UiItemInstantiated.GetComponent<Button>().onClick.AddListener(() =>
            {
                currentWeapon = weapon;
                weaponNameInput.text = weapon.name;
                itemPreview.ChangeItemInPreviewTab(currentWeapon);
                GetAttachmentSlots();
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data due to: {e.Message} {e.StackTrace}");
        }
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

    void GetAttachmentSlots()
    {
        foreach (Transform child in uiAttachmentList)
        {
            Destroy(child.gameObject);
        }

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


            itemPreview.ChangeItemInPreviewTab(currentWeapon, true);
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
