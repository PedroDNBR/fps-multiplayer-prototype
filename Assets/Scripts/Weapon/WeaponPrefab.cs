using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.VFX;

public class WeaponPrefab : MonoBehaviour
{
    public Transform recoilPivot;
    public Transform aimPivot;
    public Transform pointAwayFromWallsPivot;
    public Transform weaponTransform;

    public VisualEffect muzzleFire;
    public Transform muzzle;
    public Transform wallDetector;
    public Transform closestWallDetector;

    public Transform gunIkTransform;
    public Transform leftHandIkTransform;
    public Transform rightHandIkTransform;

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

    private void Start()
    {
        if (ocularCamera == null || objectiveCamera == null) return;

        Color whiteColor = new Color(255, 255, 255);

        ocularRenderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        ocularRenderTexture.Create();

        ocularRenderTexture.Release();

        ocularCamera.targetTexture = ocularRenderTexture;

        ocularMaterial = new Material(Shader.Find("Sprites/Default"));
        //ocularMaterial = new Material(baseMaterial);
        ocularMaterial.SetTexture("_MainTex", ocularRenderTexture);
        // ocularMaterial.SetTexture("_UnlitColorMap", ocularRenderTexture);
        //ocularMaterial.SetInt("_UseEmissiveIntensity", 1);
        //ocularMaterial.SetColor("_EmissiveColor", whiteColor);
        //ocularMaterial.SetTexture("_EmissiveColorMap", ocularRenderTexture);

        ocularRenderer.material = ocularMaterial;


        objectiveRenderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        objectiveRenderTexture.Create();

        objectiveRenderTexture.Release();

        objectiveCamera.targetTexture = objectiveRenderTexture;

        objectiveMaterial = new Material(Shader.Find("Sprites/Default"));
        //objectiveMaterial = new Material(baseMaterial);
        objectiveMaterial.SetTexture("_MainTex", objectiveRenderTexture);
        //objectiveMaterial.SetTexture("_UnlitColorMap", ocularRenderTexture);
        //objectiveMaterial.SetInt("_UseEmissiveIntensity", 1);
        //objectiveMaterial.SetColor("_EmissiveColor", whiteColor);
        //objectiveMaterial.SetTexture("_EmissiveColorMap", ocularRenderTexture);

        objectiveRenderer.material = objectiveMaterial;
    }


    // Update is called once per frame
    void Update()
    {
        if (playerCamera == null || ocularLens == null) return;
        Vector3 localPlayer = ocularLens.InverseTransformPoint(playerCamera.position);

        Vector3 lookAtScope = ocularLens.TransformPoint(new Vector3(-localPlayer.x, -localPlayer.y, -localPlayer.z));
        ocularCamera.transform.LookAt(lookAtScope);
    }
}
