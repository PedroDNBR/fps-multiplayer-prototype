using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Inventory")]
public class Inventory : ScriptableObject
{
    public WeaponItem primaryWeapon;
    public WeaponItem secondaryWeapon;
    public WeaponItem tertiaryWeapon;
}
