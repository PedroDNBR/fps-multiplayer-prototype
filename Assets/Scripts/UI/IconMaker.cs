using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconMaker : MonoBehaviour
{
    public Image icon;
    public Camera cam;
    public Transform objectHolder;

    List<IconQueueItems> iconList = new List<IconQueueItems>();

    public Sprite GetIcon(Item item, bool isWeapon)
    {
        cam.orthographicSize = item.orthographicSize;

        var instantiatedObjectToCreateIcon = Instantiate(DatabaseSingleton.instance.prefabList.prefabs[item.prefabId], objectHolder);

        if(isWeapon)
        {
            instantiatedObjectToCreateIcon.GetComponent<WeaponPrefab>().isIcon = true;
            instantiatedObjectToCreateIcon.GetComponent<WeaponPrefab>().SetWeaponParts(item as WeaponItem);
        }

        instantiatedObjectToCreateIcon.transform.localPosition = item.iconOffsetPosition;

        Texture2D texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        RenderTexture renderTexture = new RenderTexture(512, 512, 24);
        cam.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;

        cam.Render();
        texture.ReadPixels(new Rect(0,0, 512, 512), 0, 0);
        texture.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        foreach (Transform child in objectHolder)
        {
            Destroy(child.gameObject);
        }

        return Sprite.Create(texture, new Rect(0,0, texture.width, texture.height), new Vector2(0,0));
    }

    public void UpdateScriptAsync(Image image, Item item, bool isWeapon)
    {
        image.sprite = GetIcon(item, isWeapon);
    }

    private void LateUpdate()
    {
        if(iconList.Count > 0)
        {
            SetIcons(0);
            iconList.Remove(iconList[0]);
        }

    }

    void SetIcons(int index)
    {
        iconList[index].image.sprite = GetIcon(iconList[index].item, iconList[index].isWeapon);
    }

    public void SetItemInQueue(Image image, Item item, bool isWeapon = false)
    {
        iconList.Add(new IconQueueItems
        {
            image = image,
            item = item,
            isWeapon = isWeapon
        });
    }
}

public class IconQueueItems
{
    public Image image;
    public Item item;
    public bool isWeapon;
}
