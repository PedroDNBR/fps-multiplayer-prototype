using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopeFirstLens : MonoBehaviour
{
    public Transform playerTarget;
    public Transform scope;

    public float scopeRange = 50f;

    public Renderer ocularRenderer;
    public Renderer objectiveRenderer;

    public Material baseMaterial;

    public GameObject test;

    public Camera ocularCamera;
    public Camera objectiveCamera;

    public Texture Circle;
    public Texture CrossHair;

    [SerializeField] RenderTexture ocularRenderTexture;
    [SerializeField] RenderTexture objectiveRenderTexture;
    [SerializeField] Material ocularMaterial;
    [SerializeField] Material objectiveMaterial;

    bool isLocalPlayer = true;

    private void Start()
    {
        if(transform.root.GetComponentInChildren<PlayerManager>() != null)
        {
            isLocalPlayer = transform.root.GetComponentInChildren<PlayerManager>().IsLocalPlayer;

            ocularCamera.gameObject.SetActive(isLocalPlayer);
        }

        if (!isLocalPlayer) return;

        // playerTarget = transform.root.GetComponentInChildren<Camera>().transform;
        playerTarget = transform.root.GetComponentInChildren<AimingCamera>().transform;
        if (playerTarget == null) playerTarget = Camera.main.transform;

        if (ocularCamera == null) return;

        WeaponPrefab weaponPrefab = GetComponentInParent<WeaponPrefab>();
        if (weaponPrefab != null)
        {
            Transform muzzle = weaponPrefab.muzzle;
            if (muzzle != null)
            {
                
                GameObject model = Instantiate(test, muzzle);
                model.transform.parent = muzzle;
                model.transform.localPosition = new Vector3(0, 0, scopeRange);
                ocularCamera.transform.LookAt(model.transform.position);

                Destroy(model);
            }
        }

        Color whiteColor = new Color(255, 255, 255);

        ocularRenderTexture = new RenderTexture(512, 512, 32);
        ocularRenderTexture.name = "OcularRenderTexture";
        ocularRenderTexture.enableRandomWrite = true;

        ocularRenderTexture.Create();

        ocularRenderTexture.Release();


        ocularCamera.targetTexture = ocularRenderTexture;

        ocularRenderer.material.SetTexture("_View", ocularRenderTexture);

        /*
           ocularMaterial = new Material(Shader.Find("Shader Graphs/ScopeTest"));
           ocularMaterial.SetTexture("_MainTexture", Circle);
           ocularMaterial.SetTexture("_Crosshair", CrossHair);
           ocularMaterial.SetTexture("_View", ocularRenderTexture);
           ocularMaterial.SetFloat("_Size", 2.0f);
           ocularMaterial.SetFloat("_AlphaClip", 1.0f);


           ocularRenderer.material = ocularMaterial;
      */
    }
}
