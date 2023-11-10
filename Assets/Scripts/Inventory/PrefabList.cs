using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Prefab List")]
public class PrefabList : ScriptableObject
{
    public List<GameObject> prefabs;
}