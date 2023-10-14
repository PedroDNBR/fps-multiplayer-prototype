using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon/Hand Guard")]
public class HandGuard : ModularWeaponPartSocket
{
    public Vector3 leftHandPosition;
    public Quaternion leftHandRotation;
    public string IdleAnimationName;
}
