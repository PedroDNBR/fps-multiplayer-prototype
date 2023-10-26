using System;
using System.Collections.Generic;
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

    WeaponItem currentWeapon;

    IconMaker iconMaker;

    private void Start()
    {
        iconMaker = GetComponent<IconMaker>();
        foreach (var weapon in weaponsList)
        {
            GameObject UiItemInstantiated = Instantiate(UiItemPrefab, UiList);
            UiItemInstantiated.GetComponentInChildren<TMP_Text>().text = weapon.name;
            Image icon = UiItemInstantiated.GetComponentsInChildren<Image>()[1];
            iconMaker.SetItemInQueue(icon, weapon, true);

            UiItemInstantiated.SetActive(true);
            UiItemInstantiated.GetComponent<Button>().onClick.AddListener(() => {
                currentWeapon = weapon;
                itemPreview.ChangeItemInPreviewTab(currentWeapon);
                GetAttachmentSlots();
            });
        }
    }

    void SetAttachmentInUI(GameObject instantiated, WeaponPart weaponPart, WeaponPartType weaponPartType)
    {
        Debug.Log(weaponPartType);
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
        foreach(Transform child in uiAttachmentList)
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
        if(weaponAttachmentPoint.canBeEmpty) compatibleListOptions.Add("remove");
        foreach (var compatibleAttachment in compatibleWeaponPartsList.compatibleWeaponItems)
        {
            compatibleListOptions.Add(compatibleAttachment.name);
        }

        TMP_Dropdown dropdown = instantiated.GetComponentInChildren<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(compatibleListOptions);
        if(weaponAttachmentPoint.weaponPart != null)
            dropdown.value = compatibleListOptions.IndexOf(weaponAttachmentPoint.weaponPart.name);

        dropdown.onValueChanged.AddListener((int index) =>
        {
            Debug.Log(index);
            if (weaponAttachmentPoint.canBeEmpty && index == 0)
                weaponAttachmentPoint.weaponPart = null;
            else
            {
                if(weaponAttachmentPoint.canBeEmpty) index--;
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
