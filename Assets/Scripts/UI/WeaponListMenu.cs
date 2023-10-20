using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponListMenu : MonoBehaviour
{
    public List<WeaponItem> weaponsList;
    public GameObject UiItemPrefab;
    public Image icon;
    public TMP_Text text;
    public Button button;
    public Transform UiList;

    public ItemPreview itemPreview;

    public Transform uiAttachmentList;
    public GameObject attachmentUiItemPrefab;

    IconMaker iconMaker;

    private void Start()
    {
        iconMaker = GetComponent<IconMaker>();
        float timer = .01f;
        foreach (var weapon in weaponsList)
        {
            StartCoroutine(LoadIconsLazy(weapon, timer));
            timer += .01f;
        }
    }

    IEnumerator LoadIconsLazy(Item item, float timer)
    {
        yield return new WaitForEndOfFrame();
        text.text = item.name;

        Sprite newSprite = iconMaker.GetIcon(item.prefab, item.iconOffsetPosition, item.orthographicSize, item);
        icon.sprite = newSprite;
        GameObject UiItemInstantiated = Instantiate(UiItemPrefab, UiList);
        UiItemInstantiated.SetActive(true);
        UiItemInstantiated.GetComponent<Button>().onClick.AddListener(() => {
            itemPreview.ChangeItemInPreviewTab(item);
            GetAttachmentSlots(item as WeaponItem);
        });

    }

    void GetAttachmentSlots(WeaponItem weapon)
    {
        Debug.Log(weapon.weaponAttachmentPoints);
        foreach (var weaponAttachmentPoint in weapon.weaponAttachmentPoints)
        {
            if (weaponAttachmentPoint == null) continue;
            if (weaponAttachmentPoint.weaponPart == null) continue;

            var instantiated = Instantiate(attachmentUiItemPrefab, uiAttachmentList);
            instantiated.GetComponentInChildren<TMP_Text>().text = weaponAttachmentPoint.weaponPart.name;
            Sprite newSprite = iconMaker.GetIcon(weapon.prefab, weapon.iconOffsetPosition, weapon.orthographicSize);
            instantiated.GetComponentsInChildren<Image>()[1].sprite = newSprite;

            LoadModularAttachments(weaponAttachmentPoint);
            Debug.Log(weaponAttachmentPoint.weaponPartType);
        }
    }

    void LoadModularAttachments(WeaponPartsInGun weaponPartsInGun)
    {
        if (
                weaponPartsInGun.weaponPartType == WeaponPartType.HandGuard ||
                weaponPartsInGun.weaponPartType == WeaponPartType.Adapter ||
                weaponPartsInGun.weaponPartType == WeaponPartType.ScopeMount
                )
        {
            if (weaponPartsInGun.weaponPart == null) return;
            ModularWeaponPartSocket modularWeaponPartSocket = weaponPartsInGun.weaponPart as ModularWeaponPartSocket;
            foreach (var subAttachmentPoints in modularWeaponPartSocket.weaponAttachmentPoints)
            {
                Debug.Log("     " + subAttachmentPoints.weaponPart);
                if (subAttachmentPoints.weaponPart == null) continue;
                var instantiatedSubAttachment = Instantiate(attachmentUiItemPrefab, uiAttachmentList);
                instantiatedSubAttachment.GetComponentInChildren<TMP_Text>().text = subAttachmentPoints.weaponPart.name;
                Sprite newSprite = iconMaker.GetIcon(subAttachmentPoints.weaponPart.prefab, subAttachmentPoints.weaponPart.iconOffsetPosition, subAttachmentPoints.weaponPart.orthographicSize);
                instantiatedSubAttachment.GetComponentsInChildren<Image>()[1].sprite = newSprite;

                if (subAttachmentPoints.weaponPart.weaponPartType == WeaponPartType.ScopeMount)
                {
                    ScopeMount scopeMount = subAttachmentPoints.weaponPart as ScopeMount;
                    WeaponPart subAttach = scopeMount.weaponAttachmentPoints[0].weaponPart;
                    if (subAttach == null) continue;
                    Debug.Log("     " + subAttach);
                    var instantiatedSubAttachmentSub = Instantiate(attachmentUiItemPrefab, uiAttachmentList);
                    instantiatedSubAttachmentSub.GetComponentInChildren<TMP_Text>().text = subAttach.name;
                    Sprite newSpriteSub = iconMaker.GetIcon(scopeMount.weaponAttachmentPoints[0].weaponPart.prefab, scopeMount.weaponAttachmentPoints[0].weaponPart.iconOffsetPosition, scopeMount.weaponAttachmentPoints[0].weaponPart.orthographicSize);
                    instantiatedSubAttachmentSub.GetComponentsInChildren<Image>()[1].sprite = newSpriteSub;
                }
            }
        }
    }
 
}
