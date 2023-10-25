using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModularWeaponPartSocket : WeaponPart
{
    [SerializeField] public List<WeaponAttachmentPoint> weaponAttachmentPoints;
}

[Serializable]
public class WeaponAttachmentPoint
{
    public Transform anchorPoint;
    public WeaponPart weaponPart;
    public CompatibleWeaponPartsList compatibleWeaponPartsList;
    public Vector3 position;
    public Quaternion rotation;
}

