using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageContainer : MonoBehaviour
{
    StorageContainerItem storageContainerItem;

    [SerializeField] List<Item> items = new List<Item>();

    public void Init(StorageContainerItem storageContainerItem)
    {
        this.storageContainerItem = storageContainerItem;

    }
}
