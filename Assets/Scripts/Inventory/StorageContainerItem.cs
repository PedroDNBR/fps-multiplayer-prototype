using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Storage Container")]
public class StorageContainerItem : Item
{
    [Header("Storage Dimensions (mm/g/ml)")]
    public int storageLenght;
    public int storageWidth;
    public int storageHeight;
    public int storageWeight;
    public int storageVolume;

    [Header("Storage Info")]
    public StorageContainer storageContainerObject;
    public bool hasNoModel = false;

    [Header("Stored Items")]
    [SerializeField] List<Item> items = new List<Item>();
}
