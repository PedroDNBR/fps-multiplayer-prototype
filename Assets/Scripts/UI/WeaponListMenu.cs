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
                itemPreview.ChangeItemInPreviewTab(weapon);
                GetAttachmentSlots(weapon);
            });
        }
    }

    void GetAttachmentSlots(WeaponItem weapon)
    {
        foreach (var weaponAttachmentPoint in weapon.weaponAttachmentPoints)
        {
            if (weaponAttachmentPoint == null) continue;
            if (weaponAttachmentPoint.weaponPart == null) continue;

            var instantiated = Instantiate(attachmentUiItemPrefab, uiAttachmentList);
            instantiated.GetComponentInChildren<TMP_Text>().text = weaponAttachmentPoint.weaponPart.name;
            SetDropdownValues(weaponAttachmentPoint.compatibleWeaponPartsList, instantiated, weaponAttachmentPoint.weaponPart);

            Image icon = instantiated.GetComponentsInChildren<Image>()[1];
            iconMaker.SetItemInQueue(icon, weaponAttachmentPoint.weaponPart);

            LoadModularAttachments(weaponAttachmentPoint);

        }
    }

    void SetDropdownValues(CompatibleWeaponPartsList compatibleWeaponPartsList, GameObject instantiated, WeaponPart currentPart)
    {
        List<string> compatibleListOptions = new List<string>();
        foreach (var compatibleAttachment in compatibleWeaponPartsList.compatibleWeaponItems)
        {
            compatibleListOptions.Add(compatibleAttachment.name);
        }

        TMP_Dropdown dropdown = instantiated.GetComponentInChildren<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(compatibleListOptions);
        dropdown.value = compatibleListOptions.IndexOf(currentPart.name);
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
                if (subAttachmentPoints.weaponPart == null) continue;

                var instantiatedSubAttachment = Instantiate(attachmentUiItemPrefab, uiAttachmentList);
                instantiatedSubAttachment.GetComponentInChildren<TMP_Text>().text = subAttachmentPoints.weaponPart.name;
                Image icon = instantiatedSubAttachment.GetComponentsInChildren<Image>()[1];
                iconMaker.SetItemInQueue(icon, subAttachmentPoints.weaponPart);

                SetDropdownValues(subAttachmentPoints.compatibleWeaponPartsList, instantiatedSubAttachment, subAttachmentPoints.weaponPart);


                if (subAttachmentPoints.weaponPart.weaponPartType == WeaponPartType.ScopeMount)
                {
                    ScopeMount scopeMount = subAttachmentPoints.weaponPart as ScopeMount;
                    WeaponPart subAttach = scopeMount.weaponAttachmentPoints[0].weaponPart;
                    if (subAttach == null) continue;

                    var instantiatedSubAttachmentSub = Instantiate(attachmentUiItemPrefab, uiAttachmentList);
                    instantiatedSubAttachmentSub.GetComponentInChildren<TMP_Text>().text = subAttach.name;
                    Image iconSu = instantiatedSubAttachmentSub.GetComponentsInChildren<Image>()[1];
                    iconMaker.SetItemInQueue(iconSu, scopeMount.weaponAttachmentPoints[0].weaponPart);

                    SetDropdownValues(scopeMount.weaponAttachmentPoints[0].compatibleWeaponPartsList, instantiatedSubAttachmentSub, scopeMount.weaponAttachmentPoints[0].weaponPart);

                }
            }
        }
    }
 
}
