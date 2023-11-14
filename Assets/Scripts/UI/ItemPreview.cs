using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemPreview : MonoBehaviour, IDragHandler//, IScrollHandler
{
    public Transform itemPivot;
    public string currentItemName;

    public void OnDrag(PointerEventData eventData)
    {
        itemPivot.eulerAngles += new Vector3(-eventData.delta.y, -eventData.delta.x);
    }

    public void SetCurrentItemName(string name)
    {
        currentItemName = name;
    }

    public void ChangeItemInPreviewTab(Item item, bool keepRotation = false)
    {
        ClearPreviewTab(keepRotation);
        var instantitedItem = Instantiate(DatabaseSingleton.instance.prefabList.prefabs[item.prefabId], itemPivot);

        instantitedItem.GetComponent<WeaponPrefab>().isIcon = true;
        instantitedItem.GetComponent<WeaponPrefab>().SetWeaponParts(item as WeaponItem);
    }

    public void ClearPreviewTab(bool keepRotation)
    {
        if (!keepRotation) itemPivot.eulerAngles = new Vector3(0, 90);
        if (itemPivot.childCount > 0)
        {
            Transform firstChild = itemPivot.GetChild(0);
            if (firstChild != null) Destroy(firstChild.gameObject);
        }
        currentItemName = null;
    }

    public bool CheckIfItemIsBeingPreviewed(string name)
    {
        return currentItemName == name;
    }

    // public void OnScroll(PointerEventData eventData)
    // {
    //      itemPivot.position += new Vector3(0, 0, -eventData.scrollDelta.y / 2);
    // }
}
