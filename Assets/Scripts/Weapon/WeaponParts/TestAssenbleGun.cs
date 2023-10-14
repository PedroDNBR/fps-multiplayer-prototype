using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAssenbleGun : MonoBehaviour
{
    public WeaponItem weapon;
    [SerializeField] List<WeaponPartSlots> weaponAttachmentPoints;

    Dictionary<WeaponPartType, WeaponSlot> weaponAttachmentPointsDictionary;

    private void Awake()
    {
        weaponAttachmentPointsDictionary = new Dictionary<WeaponPartType, WeaponSlot>();

        foreach (var attachmentPoint in weaponAttachmentPoints)
        {
            weaponAttachmentPointsDictionary[attachmentPoint.weaponPartType] = new WeaponSlot
            {
                anchorPoint = attachmentPoint.anchorPoint
            };
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SetWeaponParts();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SetWeaponParts();
        }
    }

    void SetWeaponParts()
    {
        if (weapon == null) return;

        foreach (WeaponPartsInGun weaponAttachmentPoint in weapon.weaponAttachmentPoints)
        {
            SetWeaponPart(weaponAttachmentPoint.weaponPart);
        }
    }

    void SetWeaponPart(WeaponPart weaponPart)
    {
        WeaponSlot weaponSlot = weaponAttachmentPointsDictionary[weaponPart.weaponPartType];
        if (weaponSlot.spawnedTransform != null)
            Destroy(weaponSlot.spawnedTransform.gameObject);

        GameObject weaponPartPrefab = Instantiate(weaponPart.prefab, weaponSlot.anchorPoint);
        weaponSlot.spawnedTransform = weaponPartPrefab.transform;

        weaponAttachmentPointsDictionary[weaponPart.weaponPartType] = weaponSlot;
        LoadSubAttachments(weaponPart);
    }

    void LoadSubAttachments(WeaponPart weaponPart)
    {
        if (
            weaponPart.weaponPartType == WeaponPartType.HandGuard ||
            weaponPart.weaponPartType == WeaponPartType.Adapter ||
            weaponPart.weaponPartType == WeaponPartType.ScopeMount
            )
        {
            SetModularWeaponPartAttachments(weaponPart as ModularWeaponPartSocket);
        }
    }

    void SetModularWeaponPartAttachments(ModularWeaponPartSocket weaponPart)
    {
        WeaponSlot parentWeaponSlot = weaponAttachmentPointsDictionary[weaponPart.weaponPartType];
        int i = 0;
        foreach(Transform child in parentWeaponSlot.spawnedTransform)
        {
            weaponPart.weaponAttachmentPoints[i].anchorPoint = child;
            i++;
        }
        foreach (var weaponAttachmentPoint in weaponPart.weaponAttachmentPoints)
        {
            if (weaponAttachmentPoint.weaponPart == null) continue;
            GameObject weaponPartPrefab = Instantiate(weaponAttachmentPoint.weaponPart.prefab, weaponAttachmentPoint.anchorPoint);
            weaponAttachmentPointsDictionary[weaponAttachmentPoint.weaponPart.weaponPartType] = new WeaponSlot
            {
                spawnedTransform = weaponPartPrefab.transform,
            };
            LoadSubAttachments(weaponAttachmentPoint.weaponPart);
        }
    }
}