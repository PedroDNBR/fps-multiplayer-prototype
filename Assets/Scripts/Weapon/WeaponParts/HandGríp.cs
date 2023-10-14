using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon/Hand Grip")]
public class HandGríp : WeaponPart
{
    public Vector3 leftHandPosition;
    public Quaternion leftHandRotation;
    public string IdleAnimationName;
}
