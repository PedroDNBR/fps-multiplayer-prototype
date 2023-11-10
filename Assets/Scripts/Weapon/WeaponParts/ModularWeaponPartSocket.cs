using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ModularWeaponPartSocket : WeaponPart
{
    [SerializeField] public List<WeaponAttachmentPoint> weaponAttachmentPoints;
}

[Serializable]
public class WeaponAttachmentPoint : WeaponPartsInGun
{
    [JsonIgnore] public Transform anchorPoint;
    public Vector3 position;
    public Quaternion rotation;
}

