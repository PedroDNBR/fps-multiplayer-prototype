using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon/Compatible Weapon Parts List")]
public class CompatibleWeaponPartsList : ScriptableObject
{
    public List<WeaponPart> compatibleWeaponItems;
}
