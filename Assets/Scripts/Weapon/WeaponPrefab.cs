using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.VFX;

public class WeaponPrefab : MonoBehaviour
{
    public Transform recoilPivot;
    public Transform aimPivot;
    public Transform pointAwayFromWallsPivot;
    public Transform weaponTransform;
    public Transform magazineTransform;
    public Transform chargingHandleTransform;
    public Transform defaultSightTransform;
    public float defaultSightOffset;
    public List<Transform> sightTransforms;
    public List<float> sightOffsets;

    public VisualEffect muzzleFire;
    public Transform muzzle;
    public Transform wallDetector;
    public Transform closestWallDetector;

    public Transform gunIkTransform;
    public Transform leftHandIkTransform;
    public Transform rightHandIkTransform;
    public Transform magazineIkTransform;
    public Transform chargingHandleIkTransform;

    public Animator weaponAnimator;
    public Animator weaponMovementAnimator;

    public Renderer ocularRenderer;
    public Renderer objectiveRenderer;

    public Material baseMaterial;

    public Transform playerCamera;
    public Transform ocularLens;

    public Camera ocularCamera;
    public Camera objectiveCamera;

    [SerializeField] RenderTexture ocularRenderTexture;
    [SerializeField] RenderTexture objectiveRenderTexture;
    [SerializeField] Material ocularMaterial;
    [SerializeField] Material objectiveMaterial;

    public WeaponItem weapon;
    [SerializeField] List<WeaponPartSlots> weaponAttachmentPoints;

    Dictionary<WeaponPartType, WeaponSlot> weaponAttachmentPointsDictionary;

    public Vector3 leftHandPosition;
    public Quaternion leftHandRotation;

    public bool isIcon;

    private void Awake()
    {
        weaponAttachmentPointsDictionary = new Dictionary<WeaponPartType, WeaponSlot>();

        foreach (var attachmentPoint in weaponAttachmentPoints)
        {
            weaponAttachmentPointsDictionary[attachmentPoint.weaponPartType] = new WeaponSlot
            {
                anchorPoint = attachmentPoint.anchorPoint
            };
        }
    }

    private void Start()
    {
        if (ocularCamera == null || objectiveCamera == null) return;

        Color whiteColor = new Color(255, 255, 255);

        ocularRenderTexture = new RenderTexture(512, 512, 32);
        ocularRenderTexture.name = "Whatever";
        ocularRenderTexture.enableRandomWrite = true;
        ocularRenderTexture.Create();

        ocularRenderTexture.Release();


        ocularCamera.targetTexture = ocularRenderTexture;

        ocularMaterial = new Material(Shader.Find("Sprites/Default"));
        ocularMaterial.SetTexture("_MainTex", ocularRenderTexture);

        /*ocularMaterial = new Material(baseMaterial);
        ocularMaterial.SetTexture("_UnlitColorMap", ocularRenderTexture);
        ocularMaterial.SetTexture("_BaseColorMap", ocularRenderTexture);
        ocularMaterial.SetTexture("_EmissiveColorMap", ocularRenderTexture);*/
        //ocularMaterial.SetInt("_UseEmissiveIntensity", 1);
        //ocularMaterial.SetColor("_EmissiveColor", whiteColor);

        ocularRenderer.material = ocularMaterial;


        objectiveRenderTexture = new RenderTexture(512, 512, 32);
        objectiveRenderTexture.name = "Whatever";
        objectiveRenderTexture.enableRandomWrite = true;
        objectiveRenderTexture.Create();

        objectiveRenderTexture.Release();

        objectiveCamera.targetTexture = objectiveRenderTexture;

        objectiveMaterial = new Material(Shader.Find("Sprites/Default"));
        objectiveMaterial.SetTexture("_MainTex", objectiveRenderTexture);
        /*objectiveMaterial = new Material(baseMaterial);
        objectiveMaterial.SetTexture("_UnlitColorMap", ocularRenderTexture);
        objectiveMaterial.SetTexture("_BaseColorMap", ocularRenderTexture);
        objectiveMaterial.SetTexture("_EmissiveColorMap", ocularRenderTexture);*/
        //objectiveMaterial.SetInt("_UseEmissiveIntensity", 1);
        //objectiveMaterial.SetColor("_EmissiveColor", whiteColor);

        objectiveRenderer.material = objectiveMaterial;
    }


    // Update is called once per frame
    void Update()
    {
        if (playerCamera == null || ocularLens == null) return;
        Vector3 localPlayer = ocularLens.InverseTransformPoint(playerCamera.position);

        Vector3 lookAtScope = ocularLens.TransformPoint(new Vector3(-localPlayer.x, -localPlayer.y, -localPlayer.z));

        ocularCamera.transform.rotation = Quaternion.LookRotation(lookAtScope - ocularCamera.transform.position, objectiveCamera.transform.up);
    }

    public void SetWeaponParts(WeaponItem weapon)
    {
        if (weapon == null) return;
        this.weapon = weapon;

        foreach (WeaponPartsInGun weaponAttachmentPoint in weapon.weaponAttachmentPoints)
        {
            if (weaponAttachmentPoint.weaponPart == null) continue;
            SetWeaponPart(weaponAttachmentPoint.weaponPart);
        }
    }

    void SetWeaponPart(WeaponPart weaponPart)
    {
        WeaponSlot weaponSlot = weaponAttachmentPointsDictionary[weaponPart.weaponPartType];
        if (weaponSlot.spawnedTransform != null)
            Destroy(weaponSlot.spawnedTransform.gameObject);

        GameObject weaponPartPrefab = Instantiate(DatabaseSingleton.instance.prefabList.prefabs[weaponPart.prefabId], weaponSlot.anchorPoint);
        weaponSlot.spawnedTransform = weaponPartPrefab.transform;

        if (weaponPart.weaponPartType == WeaponPartType.Magazine)
        {
            magazineTransform = weaponPartPrefab.transform;
        }

        weaponAttachmentPointsDictionary[weaponPart.weaponPartType] = weaponSlot;
        LoadSubAttachments(weaponPart);
    }

    void LoadSubAttachments(WeaponPart weaponPart)
    {
        if (weaponPart.weaponPartType == WeaponPartType.HandGuard)
        {
            HandGuard handGuard = weaponPart as HandGuard;
            weaponMovementAnimator.CrossFade(handGuard.IdleAnimationName, .1f);
        }

        if (weaponPart.weaponPartType == WeaponPartType.HandGríp)
        {
            HandGríp handGríp = weaponPart as HandGríp;
            weaponMovementAnimator.CrossFade(handGríp.IdleAnimationName, .1f);
        }

        if (weaponPart.weaponPartType == WeaponPartType.Optics)
        {
            WeaponSlot parentWeaponSlot = weaponAttachmentPointsDictionary[weaponPart.weaponPartType];
            Optics optics = weaponPart as Optics;
            Transform child = parentWeaponSlot.spawnedTransform.GetChild(0);

            sightOffsets.Add(optics.sightOffset);
            sightTransforms.Add(child);
        }

        if (weaponPart.weaponPartType == WeaponPartType.LightingDevice)
        {
            WeaponSlot parentWeaponSlot = weaponAttachmentPointsDictionary[weaponPart.weaponPartType];
            LightingDevice optics = weaponPart as LightingDevice;
            Transform child = parentWeaponSlot.spawnedTransform.GetChild(0);

            GameObject test = new GameObject();
            GameObject instantiated = Instantiate(test, muzzle);
            instantiated.transform.localPosition = new Vector3(0, 0, 10);

            child.LookAt(instantiated.transform);

            Destroy(instantiated.gameObject);

            if (isIcon) Destroy(child.gameObject);
        }

        if (
            weaponPart.weaponPartType == WeaponPartType.HandGuard ||
            weaponPart.weaponPartType == WeaponPartType.Adapter ||
            weaponPart.weaponPartType == WeaponPartType.ScopeMount
            )
        {
            SetModularWeaponPartAttachments(weaponPart as ModularWeaponPartSocket);
        }
    }

    void SetModularWeaponPartAttachments(ModularWeaponPartSocket weaponPart)
    {
        WeaponSlot parentWeaponSlot = weaponAttachmentPointsDictionary[weaponPart.weaponPartType];
        int i = 0;
        foreach (Transform child in parentWeaponSlot.spawnedTransform)
        {
            weaponPart.weaponAttachmentPoints[i].anchorPoint = child;
            i++;
        }
        foreach (var weaponAttachmentPoint in weaponPart.weaponAttachmentPoints)
        {
            if (weaponAttachmentPoint.weaponPart == null) continue;
            GameObject weaponPartPrefab = Instantiate(DatabaseSingleton.instance.prefabList.prefabs[weaponAttachmentPoint.weaponPart.prefabId], weaponAttachmentPoint.anchorPoint);
            weaponAttachmentPointsDictionary[weaponAttachmentPoint.weaponPart.weaponPartType] = new WeaponSlot
            {
                spawnedTransform = weaponPartPrefab.transform,
            };
            LoadSubAttachments(weaponAttachmentPoint.weaponPart);
        }
    }
}

[Serializable]
public class WeaponPartSlots
{
    public WeaponPartType weaponPartType;
    public Transform anchorPoint;
}

public class WeaponSlot
{
    public Transform anchorPoint;
    public Transform spawnedTransform;
}